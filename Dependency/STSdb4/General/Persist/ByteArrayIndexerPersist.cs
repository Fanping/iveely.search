using System;
using System.IO;
using System.Collections.Generic;
using Iveely.STSdb4.General.Compression;

namespace Iveely.STSdb4.General.Persist
{
    public class ByteArrayIndexerPersist : IIndexerPersist<Byte[]>
    {
        public const byte VERSION = 40;

        public void Store(BinaryWriter writer, Func<int, byte[]> values, int count)
        {
            writer.Write(VERSION);

            int index = 0;
            byte[] value;

            if (writer.BaseStream is MemoryStream)
            {
                writer.Write((byte)1);
                if (count == 0)
                    return;

                int c = 1;
                long pos = writer.BaseStream.Position;
                writer.Write(count); //writer.Write(c);

                int length;
                value = values(index);
                if (value == null)
                {
                    length = -1;
                    writer.Write(length);
                }
                else
                {
                    length = value.Length;
                    writer.Write(length);
                    writer.Write(value);
                }
                index++;

                while (index < count)
                {
                    value = values(index);

                    if (value == null)
                    {
                        if (length != -1)
                            break;
                    }
                    else
                    {
                        if (length != value.Length)
                            break;

                        writer.Write(value);
                    }

                    index++;
                    c++;
                }

                if (c < count)
                {
                    var pos2 = writer.BaseStream.Position;
                    writer.BaseStream.Seek(pos, SeekOrigin.Begin);
                    writer.Write(c);
                    writer.BaseStream.Seek(pos2, SeekOrigin.Begin);
                }
                else
                    return;
            }
            else
                writer.Write((byte)0);

            for (int i = index; i < count; i++)
            {
                value = values(i);

                if (value == null)
                    CountCompression.Serialize(writer, 0);
                else
                {
                    CountCompression.Serialize(writer, (ulong)(value.Length + 1));
                    writer.Write(value);
                }
            }
        }

        public void Load(BinaryReader reader, Action<int, byte[]> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid ByteArrayIndexerPersist version.");

            int index = 0;

            byte format = reader.ReadByte();

            if (format == 1)
            {
                if (count == 0)
                    return;

                int c = reader.ReadInt32();
                int length = reader.ReadInt32();

                if (length < 0)
                {
                    for (int i = 0; i < c; i++)
                        values(index++, null);
                }
                else
                {
                    for (int i = 0; i < c; i++)
                        values(index++, reader.ReadBytes(length));
                }

                if (index == count)
                    return;
            }

            for (int i = index; i < count; i++)
            {
                int length = (int)CountCompression.Deserialize(reader);
                
                if (length == 0)
                    values(i, null);
                else
                    values(i, reader.ReadBytes(length - 1));
            }
        }
    }
}
