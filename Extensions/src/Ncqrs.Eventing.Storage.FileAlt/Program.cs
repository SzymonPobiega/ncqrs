using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EventStoreProto
{
   class Program
   {
      private static readonly Random _random = new Random();
      private static List<Guid> _eventsSources;

      private const int EventSize = 60000;
      private const int TransactionPerEventSource = 5;
      private const int EventsPerTransaction = 2;
      private const int EventSources = 1000;
      private const int TransactionCount = TransactionPerEventSource*EventSources;

      private const string DataFilePath = "events.dat";
      private const string IndexFilePath = "events.idx";

      static void Main(string[] args)
      {
         _eventsSources = Enumerable.Range(0, EventSources).Select(x => Guid.NewGuid()).ToList();

         var eventData = new byte[EventSize];
         _random.NextBytes(eventData);

         if (File.Exists(DataFilePath))
         {
            File.Delete(DataFilePath);
         }
         if (File.Exists(IndexFilePath))
         {
            File.Delete(IndexFilePath);
         }

         var store = new EventStore(DataFilePath, IndexFilePath);

         var watch = new Stopwatch();
         watch.Start();
         for (int transaction = 0; transaction < TransactionCount; transaction++ )
         {
            var sourceId = GetRandomSource();
            var records = Enumerable.Range(0, EventsPerTransaction).Select(x => new Record() {Data = eventData});
            store.Store(sourceId, records);
         }
         watch.Stop();         
         Console.WriteLine("Writing {0} events {1} bytes each took {2} milliseconds", TransactionCount * EventsPerTransaction, EventSize, watch.ElapsedMilliseconds);
         watch.Reset();
         watch.Start();
         foreach (Guid eventsSource in _eventsSources)
         {
            var events = store.Get(eventsSource).ToList();
            //foreach (Record record in events)
            //{
            //   Debug.Assert(record.Data.SequenceEqual(eventData));
            //}
         }
         watch.Stop();   
         Console.WriteLine("Reading and veryfication of all events took {0} milliseconds", watch.ElapsedMilliseconds);
         Console.ReadKey();
      }

      private static Guid GetRandomSource()
      {
         return _eventsSources[_random.Next(EventSources)];
      }
   }
}
