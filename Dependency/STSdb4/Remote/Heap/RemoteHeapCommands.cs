using Iveely.General.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Remote.Heap
{
    public class HeapCommand
    {
    }

    public class ObtainHandleCommand : HeapCommand
    {
        public long Handle;

        public static void WriteRequest(BinaryWriter writer)
        {
            writer.Write((byte)RemoteHeapCommandCodes.ObtainHandle);
        }

        public static void WriteResponse(BinaryWriter writer, long handle)
        {
            CountCompression.Serialize(writer, (ulong)handle);
        }

        public static ObtainHandleCommand ReadResponse(BinaryReader reader)
        {
            return new ObtainHandleCommand()
            {
                Handle = (long)CountCompression.Deserialize(reader)
            };
        }
    }

    public class ReleaseHandleCommand : HeapCommand
    {
        public long Handle;

        public static void WriteRequest(BinaryWriter writer, long handle)
        {
            writer.Write((byte)RemoteHeapCommandCodes.ReleaseHandle);
            writer.Write(handle);
        }

        public static ReleaseHandleCommand ReadRequest(BinaryReader reader)
        {
            return new ReleaseHandleCommand()
            {
                Handle = (long)CountCompression.Deserialize(reader)
            };
        }
    }

    public class HandleExistCommand : HeapCommand
    {
        public bool Exist;
        public long Handle;

        public static void WriteRequest(BinaryWriter writer, long handle)
        {
            writer.Write((byte)RemoteHeapCommandCodes.HandleExist);
            CountCompression.Serialize(writer, (ulong)handle);
        }

        public static HandleExistCommand ReadRequest(BinaryReader reader)
        {
            return new HandleExistCommand()
            {
                Handle = (long)CountCompression.Deserialize(reader)
            };
        }

        public static void WriteResponse(BinaryWriter writer, bool exist)
        {
            writer.Write(exist);
        }

        public static HandleExistCommand ReadResponse(BinaryReader reader)
        {
            return new HandleExistCommand()
            {
                Exist = reader.ReadBoolean()
            };
        }
    }

    public class WriteCommand : HeapCommand
    {
        public long Handle;

        public int Index;
        public int Count;
        public byte[] Buffer;

        public static void WriteRequest(BinaryWriter writer, long handle, int index, int count, byte[] buffer)
        {
            writer.Write((byte)RemoteHeapCommandCodes.WriteCommand);
            CountCompression.Serialize(writer, (ulong)handle);

            CountCompression.Serialize(writer, (ulong)index);
            CountCompression.Serialize(writer, (ulong)count);

            CountCompression.Serialize(writer, (ulong)(count + index));
            writer.Write(buffer, 0, index + count);
        }

        public static WriteCommand ReadRequest(BinaryReader reader)
        {
            return new WriteCommand()
            {
                Handle = (long)CountCompression.Deserialize(reader),

                Index = (int)CountCompression.Deserialize(reader),
                Count = (int)CountCompression.Deserialize(reader),

                Buffer = reader.ReadBytes((int)CountCompression.Deserialize(reader))
            };
        }
    }

    public class ReadCommand : HeapCommand
    {
        public long Handle;
        public byte[] Buffer;

        public static void WriteRequest(BinaryWriter writer, long handle)
        {
            writer.Write((byte)RemoteHeapCommandCodes.ReadCommand);
            CountCompression.Serialize(writer, (ulong)handle);
        }

        public static ReadCommand ReadRequest(BinaryReader reader)
        {
            return new ReadCommand()
            {
                Handle = (long)CountCompression.Deserialize(reader)
            };
        }

        public static void WriteResponse(BinaryWriter writer, byte[] buffer)
        {
            CountCompression.Serialize(writer, (ulong)buffer.Length);
            writer.Write(buffer, 0, buffer.Length);
        }

        public static ReadCommand ReadResponse(BinaryReader reader)
        {
            return new ReadCommand()
            {
                Buffer = reader.ReadBytes((int)CountCompression.Deserialize(reader))
            };
        }
    }

    public class CommitCommand : HeapCommand
    {
        public static void WriteRequest(BinaryWriter writer)
        {
            writer.Write((byte)RemoteHeapCommandCodes.CommitCommand);
        }
    }

    public class CloseCommand : HeapCommand
    {
        public static void WriteRequest(BinaryWriter writer)
        {
            writer.Write((byte)RemoteHeapCommandCodes.CloseCommand);
        }
    }

    public class SetTagCommand : HeapCommand
    {
        public byte[] Tag;

        public static void WriteRequest(BinaryWriter writer, byte[] tag)
        {
            writer.Write((byte)RemoteHeapCommandCodes.SetTag);
            writer.Write(tag != null);
            if (tag != null)
            {
                CountCompression.Serialize(writer, (ulong)tag.Length);
                writer.Write(tag, 0, tag.Length);
            }
        }

        public static SetTagCommand ReadRequest(BinaryReader reader)
        {
            return new SetTagCommand()
            {
                Tag = reader.ReadBoolean() ? reader.ReadBytes((int)CountCompression.Deserialize(reader)) : null
            };
        }
    }

    public class GetTagCommand : HeapCommand
    {
        public byte[] Tag;

        public static void WriteRequest(BinaryWriter writer)
        {
            writer.Write((byte)RemoteHeapCommandCodes.GetTag);
        }

        public static void WriteResponse(BinaryWriter writer, byte[] tag)
        {
            writer.Write(tag != null);
            if (tag != null)
            {
                CountCompression.Serialize(writer, (ulong)tag.Length);
                writer.Write(tag, 0, tag.Length);
            }
        }

        public static GetTagCommand ReadResponse(BinaryReader reader)
        {
            return new GetTagCommand()
            {
                Tag = reader.ReadBoolean() ? reader.ReadBytes((int)CountCompression.Deserialize(reader)) : null
            };
        }
    }

    public class DataBaseSizeCommand : HeapCommand
    {
        public long DataBaseSize;

        public static void WriteRequest(BinaryWriter writer)
        {
            writer.Write((byte)RemoteHeapCommandCodes.DataBaseSize);
        }

        public static void WriteResponse(BinaryWriter writer, long size)
        {
            CountCompression.Serialize(writer, (ulong)size);
        }

        public static DataBaseSizeCommand ReadResponse(BinaryReader reader)
        {
            return new DataBaseSizeCommand()
             {
                 DataBaseSize = (long)CountCompression.Deserialize(reader)
             };
        }
    }

    public class SizeCommand : HeapCommand
    {
        public long Size;

        public static void WriteRequest(BinaryWriter writer)
        {
            writer.Write((byte)RemoteHeapCommandCodes.Size);
        }

        public static void WriteResponse(BinaryWriter writer, long size)
        {
            CountCompression.Serialize(writer, (ulong)size);
        }

        public static DataBaseSizeCommand ReadResponse(BinaryReader reader)
        {
            return new DataBaseSizeCommand()
            {
                DataBaseSize = (long)CountCompression.Deserialize(reader)
            };
        }
    }

    public enum RemoteHeapCommandCodes : byte
    {
        ObtainHandle = 1,
        ReleaseHandle = 2,
        HandleExist = 3,
        WriteCommand = 4,
        ReadCommand = 5,
        CommitCommand = 6,
        CloseCommand = 7,
        SetTag = 8,
        GetTag = 9,
        DataBaseSize = 10,
        Size = 11
    }
}
