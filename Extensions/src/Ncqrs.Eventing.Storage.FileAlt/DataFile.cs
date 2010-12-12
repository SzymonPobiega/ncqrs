using System;
using System.IO;

namespace EventStoreProto
{
   public class DataFile : IDisposable
   {
      private readonly FileStream _dataFileStream;

      public DataFile(string filePath, long lastEventOffset)
      {
         _dataFileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
         if (lastEventOffset == long.MinValue)
         {
            return;
         }                  
         try
         {
            var lastRecord = Get(lastEventOffset);
            _dataFileStream.SetLength(lastEventOffset + DataRecord.BaseSize + lastRecord.Data.Length);
            _dataFileStream.Seek(0, SeekOrigin.End);
         }
         catch (Exception)
         {
            _dataFileStream.Dispose();
            throw;
         }        
      }

      public long Store(DataRecord data)
      {
         _dataFileStream.Seek(0, SeekOrigin.End);
         var offset = _dataFileStream.Position;
         data.Write(_dataFileStream);
         _dataFileStream.Flush();
         return offset;
      }

      public DataRecord Get(long offset)
      {
         _dataFileStream.Seek(offset, SeekOrigin.Begin);
         return new DataRecord(_dataFileStream);
      }

      public void Dispose()
      {
         _dataFileStream.Dispose();
      }
   }
}