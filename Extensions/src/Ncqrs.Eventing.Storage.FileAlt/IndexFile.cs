using System;
using System.Collections.Generic;
using System.IO;

namespace EventStoreProto
{
   public class IndexFile : IDisposable
   {
      private bool _disposed;
      private readonly FileStream _indexFileStream;

      public IndexFile(string filePath)
      {
         _indexFileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
      }

      public IndexEntry Add(Guid sourceId, long offset)
      {
         var newEntry = new IndexEntry
                           {
                              SourceId = sourceId,
                              LastEventOffset = offset
                           };         
         _indexFileStream.Seek(0, SeekOrigin.End);
         newEntry.IndexOffset = _indexFileStream.Position;
         newEntry.Write(_indexFileStream);
         _indexFileStream.Flush();
         return newEntry;
      }

      public void Update(IndexEntry entry)
      {
         _indexFileStream.Seek(entry.IndexOffset, SeekOrigin.Begin);
         entry.Write(_indexFileStream);
         _indexFileStream.Flush();
      }

      public IEnumerable<IndexEntry> ReadAll()
      {
         _indexFileStream.Seek(0, SeekOrigin.Begin);
         IndexEntry entry;
         while (IndexEntry.TryRead(_indexFileStream, out entry))
         {
            yield return entry;
         }
      }

      public void Dispose()
      {
         if (_disposed)
         {
            return;
         }
         _indexFileStream.Dispose();
         _disposed = true;
      }
   }

}