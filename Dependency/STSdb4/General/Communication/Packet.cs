using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Iveely.General.Communication
{
    ///--------------------- Packet Exchange Protocol
    ///
    ///--------------------- Comments-----------------------------------
    ///format           : binary
    ///byte style       : LittleEndian
    ///ID               : Unique ID's per Connection and Unique ID per Packet.
    ///
    ///------------------------------------------------------------------
    ///Packet           : long ID, int Size, byte[] buffer 
    ///  

    public class Packet
    {
        internal long ID;

        public readonly MemoryStream Request; // Request Message
        public MemoryStream Response; // Response Message

        public readonly ManualResetEventSlim ResultEvent;
        public Exception Exception;

        public Packet(MemoryStream request)
        {
            if (request == null)
                throw new ArgumentNullException("request == null");

            Request = request;

            ResultEvent = new ManualResetEventSlim(false);
        }

        public void Wait()
        {
            ResultEvent.Wait();

            if (Exception != null)
                throw Exception;
        }

        public void Write(BinaryWriter writer, MemoryStream memoryStream)
        {
            int size = (int)memoryStream.Length;

            writer.Write(ID);
            writer.Write(size);
            writer.Write(memoryStream.GetBuffer(), 0, size);
        }
    }
}