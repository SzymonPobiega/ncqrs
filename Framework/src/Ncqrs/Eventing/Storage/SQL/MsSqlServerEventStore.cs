﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Text;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage.Serialization;
using Newtonsoft.Json.Linq;

namespace Ncqrs.Eventing.Storage.SQL
{
    /// <summary>
    /// Stores events for a SQL database.
    /// </summary>
    public class MsSqlServerEventStore : IEventStore, ISnapshotStore
    {
        private static int FirstVersion = 0;
        private readonly String _connectionString;

        private IEventFormatter<JObject> _formatter;
        private IEventTranslator<string> _translator;
        private IEventConverter _converter;

        public MsSqlServerEventStore(String connectionString) : this(connectionString, null, null)
        {}

        public MsSqlServerEventStore(String connectionString, IEventTypeResolver typeResolver, IEventConverter converter)
        {
            if (String.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");

            _connectionString = connectionString;
            _converter = converter ?? new NullEventConverter();
            _formatter = new JsonEventFormatter(typeResolver ?? new SimpleEventTypeResolver());
            _translator = new StringEventTranslator();
        }

        /// <summary>
        /// Get all event for a specific event provider.
        /// </summary>
        /// <param name="id">The id of the event provider.</param>
        /// <returns>All events for the specified event provider.</returns>
        public IEnumerable<ISourcedEvent> GetAllEvents(Guid id)
        {
            return GetAllEventsSinceVersion(id, FirstVersion);
        }

        /// <summary>
        /// Get all events provided by an specified event source.
        /// </summary>
        /// <param name="eventSourceId">The id of the event source that owns the events.</param>
        /// <returns>All the events from the event source.</returns>
        public IEnumerable<ISourcedEvent> GetAllEventsSinceVersion(Guid id, long version)
        {
            var result = new List<ISourcedEvent>();

            // Create connection and command.
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(Queries.SelectAllEventsQuery, connection))
            {
                // Add EventSourceId parameter and open connection.
                command.Parameters.AddWithValue("EventSourceId", id);
                command.Parameters.AddWithValue("EventSourceVersion", version);
                connection.Open();

                // Execute query and create reader.
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StoredEvent<string> rawEvent = ReadEvent(reader);

                        var document = _translator.TranslateToCommon(rawEvent);
                        _converter.Upgrade(document);

                        var evnt = (ISourcedEvent) _formatter.Deserialize(document);
                        evnt.InitializeFrom(rawEvent);
                        result.Add(evnt);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Saves all events from the stream.
        /// </summary>
        /// <param name="events">The events.</param>
        public void Save(IEnumerable<ISourcedEvent> events)
        {
 
            // Create new connection.
            using (var connection = new SqlConnection(_connectionString))
            {
                // Open connection and begin a transaction so we can
                // commit or rollback all the changes that has been made.
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var eventsGrouppedById = events.GroupBy(x => x.EventSourceId);           

                        foreach (var eventsFromSource in eventsGrouppedById)
                        {
                            var initialVersion = eventsFromSource.First().EventSequence - 1;
                            var version = eventsFromSource.Last().EventSequence;

                            // Get the current version of the event provider.
                            Guid eventSourceId = eventsFromSource.Key;
                            int? currentVersion = GetVersion(eventSourceId, transaction);

                            // Create new event provider when it is not found.
                            if (currentVersion == null)
                            {
                                AddEventSource(eventSourceId, version, transaction);
                            }
                            else if (currentVersion.Value != initialVersion)
                            {
                                throw new ConcurrencyException(eventSourceId, version);
                            }

                            // Save all events to the store.
                            SaveEvents(events, transaction);

                            // Update the version of the provider.
                            UpdateEventSourceVersion(eventSourceId, version, transaction);
                        }

                        // Everything is handled, commint transaction.
                        transaction.Commit();
                    }
                    catch
                    {
                        // Something went wrong, rollback transaction.
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Saves a snapshot of the specified event source.
        /// </summary>
        public void SaveShapshot(ISnapshot snapshot)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Open connection and begin a transaction so we can
                // commit or rollback all the changes that has been made.
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var dataStream = new MemoryStream())
                        {
                            var formatter = new BinaryFormatter();
                            formatter.Serialize(dataStream, snapshot);
                            byte[] data = dataStream.ToArray();

                            using (var command = new SqlCommand(Queries.InsertSnapshot, transaction.Connection))
                            {
                                command.Transaction = transaction;
                                command.Parameters.AddWithValue("EventSourceId", snapshot.EventSourceId);
                                command.Parameters.AddWithValue("Version", snapshot.EventSourceVersion);
                                command.Parameters.AddWithValue("Type", snapshot.GetType().AssemblyQualifiedName);
                                command.Parameters.AddWithValue("Data", data);
                                command.ExecuteNonQuery();
                            }
                        }

                        // Everything is handled, commint transaction.
                        transaction.Commit();
                    }
                    catch
                    {
                        // Something went wrong, rollback transaction.
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a snapshot of a particular event source, if one exists. Otherwise, returns <c>null</c>.
        /// </summary>
        public ISnapshot GetSnapshot(Guid eventSourceId)
        {
            ISnapshot theSnapshot = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                // Open connection and begin a transaction so we can
                // commit or rollback all the changes that has been made.
                connection.Open();

                using (var command = new SqlCommand(Queries.SelectLatestSnapshot, connection))
                {
                    command.Parameters.AddWithValue("@EventSourceId", eventSourceId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var snapshotData = (byte[]) reader["Data"];
                            using (var buffer = new MemoryStream(snapshotData))
                            {
                                var formatter = new BinaryFormatter();
                                theSnapshot = (ISnapshot) formatter.Deserialize(buffer);
                            }
                        }
                    }
                }
            }

            return theSnapshot;
        }        

        public void RemoveUnusedProviders()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(Queries.DeleteUnusedProviders, connection))
            {
                connection.Open();

                try
                {
                    command.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void UpdateEventSourceVersion(Guid eventSourceId, long version, SqlTransaction transaction)
        {
            using (var command = new SqlCommand(Queries.UpdateEventSourceVersionQuery, transaction.Connection))
            {
                command.Transaction = transaction;
                command.Parameters.AddWithValue("Id", eventSourceId);
                command.Parameters.AddWithValue("NewVersion", version);
                command.ExecuteNonQuery();
            }
        }

        private StoredEvent<string> ReadEvent(SqlDataReader reader)
        {
            var eventIdentifier = (Guid)reader["Id"];
            var eventTimeStamp = (DateTime)reader["TimeStamp"];
            var eventName = (string)reader["Name"];
            var eventVersion = Version.Parse((string)reader["Version"]);
            var eventSourceId = (Guid)reader["EventSourceId"];
            var eventSequence = (long)reader["Sequence"];
            var data = (String)reader["Data"];

            return new StoredEvent<string>(
                eventIdentifier,
                eventTimeStamp,
                eventName,
                eventVersion,
                eventSourceId,
                eventSequence,
                data);
        }

        /// <summary>
        /// Saves the events to the event store.
        /// </summary>
        /// <param name="evnts">The events to save.</param>
        /// <param name="eventSourceId">The event source id that owns the events.</param>
        /// <param name="transaction">The transaction.</param>
        private void SaveEvents(IEnumerable<ISourcedEvent> evnts, SqlTransaction transaction)
        {
            Contract.Requires<ArgumentNullException>(evnts != null, "The argument evnts could not be null.");
            Contract.Requires<ArgumentNullException>(transaction != null, "The argument transaction could not be null.");

            foreach (var sourcedEvent in evnts)
            {
                SaveEvent(sourcedEvent, transaction);
            }
        }

        /// <summary>
        /// Saves the event to the data store.
        /// </summary>
        /// <param name="evnt">The event to save.</param>
        /// <param name="eventSourceId">The id of the event source that owns the event.</param>
        /// <param name="transaction">The transaction.</param>
        private void SaveEvent(ISourcedEvent evnt, SqlTransaction transaction)
        {
            Contract.Requires<ArgumentNullException>(evnt != null, "The argument evnt could not be null.");
            Contract.Requires<ArgumentNullException>(transaction != null, "The argument transaction could not be null.");

            var document = _formatter.Serialize(evnt);
            var raw = _translator.TranslateToRaw(document);

            using (var command = new SqlCommand(Queries.InsertNewEventQuery, transaction.Connection))
            {
                command.Transaction = transaction;
                command.Parameters.AddWithValue("EventId", raw.EventIdentifier);
                command.Parameters.AddWithValue("TimeStamp", raw.EventTimeStamp);
                command.Parameters.AddWithValue("EventSourceId", raw.EventSourceId);
                command.Parameters.AddWithValue("Name", raw.EventName);
                command.Parameters.AddWithValue("Version", raw.EventVersion.ToString());
                command.Parameters.AddWithValue("Sequence", raw.EventSequence);
                command.Parameters.AddWithValue("Data", raw.Data);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds the event source to the event store.
        /// </summary>
        /// <param name="version">Current version of event source to add.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="eventSourceId">Id of event source to add.</param>
        private static void AddEventSource(Guid eventSourceId, long version, SqlTransaction transaction)
        {
            using (var command = new SqlCommand(Queries.InsertNewProviderQuery, transaction.Connection))
            {
                command.Transaction = transaction;
                command.Parameters.AddWithValue("Id", eventSourceId);
                command.Parameters.AddWithValue("Version", version);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the version of the provider from the event store.
        /// </summary>
        /// <param name="providerId">The provider id.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns>A <see cref="int?"/> that is <c>null</c> when no version was known ; otherwise,
        /// it contains the version number.</returns>
        private static int? GetVersion(Guid providerId, SqlTransaction transaction)
        {
            using (var command = new SqlCommand(Queries.SelectVersionQuery, transaction.Connection))
            {
                command.Transaction = transaction;
                command.Parameters.AddWithValue("id", providerId);
                return (int?)command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Gets the table creation queries that can be used to create the tables that are needed
        /// for a database that is used as an event store.
        /// </summary>
        /// <remarks>This returns the content of the TableCreationScript.sql that is embedded as resource.</remarks>
        /// <returns>Queries that contain the <i>create table</i> statements.</returns>
        public static IEnumerable<String> GetTableCreationQueries()
        {
            var currentAsm = Assembly.GetExecutingAssembly();

            const string resourcename = "Ncqrs.Eventing.Storage.SQL.TableCreationScript.sql";
            var resource = currentAsm.GetManifestResourceStream(resourcename);

            if (resource == null) throw new ApplicationException("Could not find the resource " + resourcename + " in assembly " + currentAsm.FullName);

            var result = new List<string>();
            
            using(var reader = new StreamReader(resource))
            {
                string line = null;
                while((line = reader.ReadLine()) != null)
                {
                    result.Add(line);
                }
            }

            return result;
        }
    }
}
