using System;
using System.IO;

namespace EventStoreProto
{
   public class IndexEntry
   {
      public Guid SourceId { get; set; }
      public long LastEventOffset { get; set; }
      public long IndexOffset { get; set; }

      public static bool TryRead(Stream inputStream, out IndexEntry newEntry)
      {
         newEntry = new IndexEntry();
         newEntry.IndexOffset = inputStream.Position;
         var reader = new BinaryReader(inputStream);
         try
         {
            newEntry.LastEventOffset = reader.ReadInt64();
         }
         catch (EndOfStreamException)
         {
            newEntry = null;
            return false;
         }
         var sourceId = new byte[16];
         if(reader.Read(sourceId, 0, 16) != 16)
         {
            newEntry = null;
            return false;
         }
         newEntry.SourceId = new Guid(sourceId);
         return true;
      }

      public void Write(Stream outputStream)
      {
         var writer = new BinaryWriter(outputStream);
         writer.Write(LastEventOffset);
         writer.Write(SourceId.ToByteArray());
         writer.Flush();   
      }
   }
}