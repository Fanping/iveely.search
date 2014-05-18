using System;
using System.IO;
using System.Collections.Generic;
using Iveely.General.Compression;

namespace Iveely.General.Persist
{
    public class Int64IndexerPersist : IIndexerPersist<Int64>
    {
        public const byte VERSION = 40;

        private long[] factors;

        /// <summary>
        /// This contructor gets the factors in ascending order
        /// </summary>
        public Int64IndexerPersist(long[] factors)
        {
            this.factors = factors;
        }

        public Int64IndexerPersist()
            : this(new long[0])
        {
        }

        public void Store(BinaryWriter writer, Func<int, long> values, int count)
        {
            writer.Write(VERSION);
            
            long[] array = new long[count];

            int index = factors.Length - 1;
            for (int i = 0; i < count; i++)
            {
                long value = values(i);
                array[i] = value;

                while (index >= 0)
                {
                    if (value % factors[index] == 0)
                        break;
                    else
                        index--;
                }
            }

            long factor = index >= 0 ? factors[index] : 1;

            DeltaCompression.Helper helper = new DeltaCompression.Helper();
            for (int i = 0; i < count; i++)
            {
                array[i] /= factor;
                helper.Add(array[i]);
            }

            CountCompression.Serialize(writer, checked((ulong)factor));
            DeltaCompression.Compress(writer, array, 0, count, helper);
        }

        public void Load(BinaryReader reader, Action<int, long> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid Int64IndexerPersist version.");
            
            long factor = (long)CountCompression.Deserialize(reader);

            DeltaCompression.Decompress(reader, (idx, val) => values(idx, factor * val), count);
        }
    }

    public class UInt64IndexerPersist : IIndexerPersist<UInt64>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();
        
        public void Store(BinaryWriter writer, Func<int, ulong> values, int count)
        {
            writer.Write(VERSION);
            
            persist.Store(writer, (i) => { return (long)values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, ulong> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid UInt64IndexerPersist version.");
            
            persist.Load(reader, (i, v) => { values(i, (ulong)v); }, count);
        }
    }

    public class Int32IndexerPersist : IIndexerPersist<Int32>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, int> values, int count)
        {
            writer.Write(VERSION);
            
            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, int> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid Int32IndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (int)v); }, count);
        }
    }

    public class UInt32IndexerPersist : IIndexerPersist<UInt32>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, uint> values, int count)
        {
            writer.Write(VERSION);
            
            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, uint> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid UInt32IndexerPersist version.");
            
            persist.Load(reader, (i, v) => { values(i, (uint)v); }, count);
        }
    }

    public class Int16IndexerPersist : IIndexerPersist<Int16>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, short> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, short> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid Int16IndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (short)v); }, count);
        }
    }

    public class UInt16IndexerPersist : IIndexerPersist<UInt16>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, ushort> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, ushort> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid UInt16IndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (ushort)v); }, count);
        }
    }

    public class ByteIndexerPersist : IIndexerPersist<Byte>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, byte> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, byte> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid ByteIndexerPersist version.");
            
            persist.Load(reader, (i, v) => { values(i, (byte)v); }, count);
        }
    }

    public class SByteIndexerPersist : IIndexerPersist<SByte>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, sbyte> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, sbyte> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid SByteIndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (sbyte)v); }, count);
        }
    }

    public class CharIndexerPersist : IIndexerPersist<Char>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, char> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, char> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid CharIndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (char)v); }, count);
        }
    }
}
