using System;
using System.Collections.Generic;

namespace EventStoreProto
{
   public class EventStore : IDisposable
   {
      private bool _disposed;
      private readonly string _dataFilePath;
      private readonly string _indexFilePath;
      private EventStorageEngine _engine;

      public EventStore(string dataFilePath, string indexFilePath)
      {
         _dataFilePath = dataFilePath;
         _indexFilePath = indexFilePath;
      }

      public void Store(Guid sourceId, IEnumerable<Record> data)
      {
         ThrowIfDisposed();
         TryStoreOnce(sourceId, data);
      }

      public IEnumerable<Record> Get(Guid sourceId)
      {
         ThrowIfDisposed();
         return _engine.Get(sourceId);
      }

      private void ThrowIfDisposed()
      {
         if (_disposed)
         {
            throw new ObjectDisposedException("Event store");
         }
      }

      private void TryStoreOnce(Guid sourceId, IEnumerable<Record> data)
      {
         EnsureEngineCreated();
         try
         {
            _engine.Store(sourceId, data);
         }
         catch (Exception)
         {
            KillEngine();
            throw;
         }
      }

      private void KillEngine()
      {
         _engine.Dispose();
         _engine = null;
      }

      private void EnsureEngineCreated()
      {
         if (_engine == null)
         {
            _engine = CreateEngine();
         }
      }

      private EventStorageEngine CreateEngine()
      {
         return new EventStorageEngine(_dataFilePath, _indexFilePath);
      }     

      public void Dispose()
      {
         if (_disposed)
         {
            return;
         }
         if (_engine != null)
         {
            _engine.Dispose();
         }
         _disposed = true;
      }
   }
}