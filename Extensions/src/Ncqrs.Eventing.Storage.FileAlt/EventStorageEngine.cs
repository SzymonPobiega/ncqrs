using System;
using System.Collections.Generic;

namespace EventStoreProto
{
   public class EventStorageEngine : IDisposable
   {
      private bool _disposed;
      private readonly DataFile _dataFile;
      private readonly Index _index;
      
      public EventStorageEngine(string dataFilePath, string indexFilePath)
      {
         _index = new Index(indexFilePath);
         _dataFile = new DataFile(dataFilePath, _index.GetLastEventPosition());
      }

      public void Store(Guid sourceId, IEnumerable<Record> data)
      {
         var offset = _index.Get(sourceId);
         foreach (var record in data)
         {
            offset = _dataFile.Store(new DataRecord
                                        {
                                           Data = record.Data,
                                           SourceId = sourceId,
                                           PreviousOffset = offset
                                        });
         }
         _index.AddOrUpdate(sourceId, offset);
      }

      public IEnumerable<Record> Get(Guid sourceId)
      {
         var offset = _index.Get(sourceId);         
         while (offset != long.MinValue)
         {
            var lastRecord = _dataFile.Get(offset);
            offset = lastRecord.PreviousOffset;
            yield return new Record
                            {
                               Data = lastRecord.Data                               
                            };
         }
      }

      public void Dispose()
      {
         if (_disposed)
         {
            return;
         }
         _dataFile.Dispose();
         _index.Dispose();
         _disposed = true;
      }
   }
}