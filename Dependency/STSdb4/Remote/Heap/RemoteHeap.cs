using Iveely.General.Communication;
using Iveely.WaterfallTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Remote.Heap
{
    public class RemoteHeap : IHeap
    {
        public ClientConnection Client { get; private set; }

        public RemoteHeap(string host, int port)
        {
            Client = new ClientConnection(host, port);
            Client.Start();
        }

        #region IHeap members

        public long ObtainNewHandle()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            ObtainHandleCommand.WriteRequest(writer);

            Packet packet = new Packet(ms);
            Client.Send(packet);
            packet.Wait();

            return ObtainHandleCommand.ReadResponse(new BinaryReader(packet.Response)).Handle;
        }

        public void Release(long handle)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            ReleaseHandleCommand.WriteRequest(writer, handle);

            Packet packet = new Packet(ms);
            Client.Send(packet);
        }

        public bool Exists(long handle)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            HandleExistCommand.WriteRequest(writer, handle);

            Packet packet = new Packet(ms);
            Client.Send(packet);
            packet.Wait();

            return HandleExistCommand.ReadResponse(new BinaryReader(packet.Response)).Exist;
        }

        public void Write(long handle, byte[] buffer, int index, int count)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            WriteCommand.WriteRequest(writer, handle, index, count, buffer);

            Packet packet = new Packet(ms);
            Client.Send(packet);
        }

        public byte[] Read(long handle)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            ReadCommand.WriteRequest(writer, handle);

            Packet packet = new Packet(ms);
            Client.Send(packet);
            packet.Wait();

            return ReadCommand.ReadResponse(new BinaryReader(packet.Response)).Buffer;
        }

        public void Commit()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            CommitCommand.WriteRequest(writer);

            Packet packet = new Packet(ms);
            Client.Send(packet);
        }

        public void Close()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            CloseCommand.WriteRequest(writer);

            Packet packet = new Packet(ms);
            Client.Send(packet);
        }

        public byte[] Tag
        {
            get
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                GetTagCommand.WriteRequest(writer);

                Packet packet = new Packet(ms);
                Client.Send(packet);
                packet.Wait();

                return GetTagCommand.ReadResponse(new BinaryReader(packet.Response)).Tag;
            }
            set
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                SetTagCommand.WriteRequest(writer, value);

                Packet packet = new Packet(ms);
                Client.Send(packet);
            }
        }

        public long DataSize
        {
            get
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                DataBaseSizeCommand.WriteRequest(writer);

                Packet packet = new Packet(ms);
                Client.Send(packet);
                packet.Wait();

                return DataBaseSizeCommand.ReadResponse(new BinaryReader(packet.Response)).DataBaseSize;
            }
        }

        public long Size
        {
            get
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                SizeCommand.WriteRequest(writer);

                Packet packet = new Packet(ms);
                Client.Send(packet);
                packet.Wait();

                return SizeCommand.ReadResponse(new BinaryReader(packet.Response)).DataBaseSize;
            }
        }

        #endregion
    }
}