using System;
using System.IO;

namespace EventStoreProto
{
   public class DataRecord
   {
      public static long BaseSize = sizeof (long) + 16 + sizeof (int);

      public Guid SourceId { get; set; }
      public long PreviousOffset { get; set; }
      public byte[] Data { get; set; }

      public DataRecord()
      { }

      public DataRecord(Stream inputStream)
      {
         var reader = new BinaryReader(inputStream);
         PreviousOffset = reader.ReadInt64();
         byte[] sourceId = new byte[16];
         reader.Read(sourceId, 0, 16);
         SourceId = new Guid(sourceId);
         int dataLength = reader.ReadInt32();
         Data = new byte[dataLength];
         reader.Read(Data, 0, dataLength);
      }

      public void Write(Stream outputStream)
      {
         var writer = new BinaryWriter(outputStream);
         writer.Write(PreviousOffset);
         writer.Write(SourceId.ToByteArray());
         writer.Write(Data.Length);
         writer.Write(Data);
         writer.Flush();
      }
   }
}