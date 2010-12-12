using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStoreProto
{
   public class Index : IDisposable
   {
      private bool _disposed;
      private readonly Dictionary<Guid, IndexEntry> _index = new Dictionary<Guid, IndexEntry>();
      private readonly IndexFile _indexFile;
      private long _lastEventOffset = long.MinValue;

      public Index(string filePath)
      {
         _indexFile = new IndexFile(filePath);
         var entries = _indexFile.ReadAll();
         foreach (var indexEntry in entries)
         {
            AddToIndex(indexEntry);
         }
      }

      private void AddToIndex(IndexEntry indexEntry)
      {
         _index[indexEntry.SourceId] = indexEntry;
         if (_lastEventOffset < indexEntry.LastEventOffset)
         {
            _lastEventOffset = indexEntry.LastEventOffset;
         }
      }

      public void AddOrUpdate(Guid sourceId, long offset)
      {
         IndexEntry indexEntry;
         if (!_index.TryGetValue(sourceId, out indexEntry))
         {
            indexEntry = _indexFile.Add(sourceId, offset);
            AddToIndex(indexEntry);
         }
         else
         {
            indexEntry.LastEventOffset = offset;
            _indexFile.Update(indexEntry);
         }
      }

      public long Get(Guid sourceId)
      {
         IndexEntry indexEntry;
         if (_index.TryGetValue(sourceId, out indexEntry))
         {
            return indexEntry.LastEventOffset;
         }
         return long.MinValue;
      }

      public long GetLastEventPosition()
      {
         return _lastEventOffset;
      }

      public void Dispose()
      {
         if (_disposed)
         {
            return;
         }
         _indexFile.Dispose();
         _disposed = true;
      }
   }
}