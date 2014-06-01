using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Iveely.General.Extensions;

namespace Iveely.General.Persist
{
    public class StringIndexerPersist : IIndexerPersist<String>
    {
        public const byte VERSION = 40;

        private const int NULL_ID = -1;
        private const double PERCENT = 38.2;

        public void Store(BinaryWriter writer, Func<int, string> values, int count)
        {
            writer.Write(VERSION);
            
            int MAP_CAPACITY = (int)((PERCENT / 100) * count);
            Dictionary<string, int> map = new Dictionary<string, int>(/*MAP_CAPACITY*/); //optimistic variant

            int ID = 0;
            int[] indexes = new int[count];
            PersistMode mode = PersistMode.Dictionary;

            for (int i = 0; i < count; i++)
            {
                var value = values(i);
                if (value == null)
                {
                    indexes[i] = NULL_ID;
                    continue;
                }

                int id;
                if (map.TryGetValue(value, out id))
                {
                    indexes[i] = id;
                    continue;
                }

                if (map.Count == MAP_CAPACITY)
                {
                    mode = PersistMode.Raw;
                    break;
                }

                map.Add(value, ID);
                indexes[i] = ID;
                ID++;
            }

            writer.Write((byte)mode);

            switch (mode)
            {
                case PersistMode.Raw:
                    {
                        new Raw().Store(writer, values, count);
                    }
                    break;

                case PersistMode.Dictionary:
                    {
                        writer.Write(map.Count);
                        foreach (var kv in map.OrderBy(x => x.Value))
                            writer.Write(kv.Key);

                        new Int32IndexerPersist().Store(writer, (idx) => { return indexes[idx]; }, count);
                    }
                    break;

                default:
                    throw new NotSupportedException(mode.ToString());
            }
        }

        public void Load(BinaryReader reader, Action<int, string> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid StringIndexerPersist version.");

            PersistMode mode = (PersistMode)reader.ReadByte();

            switch (mode)
            {
                case PersistMode.Raw:
                    {
                        new Raw().Load(reader, values, count);
                    }
                    break;

                case PersistMode.Dictionary:
                    {
                        string[] map = new string[reader.ReadInt32()];
                        for (int i = 0; i < map.Length; i++)
                            map[i] = reader.ReadString();

                        new Int32IndexerPersist().Load(reader, (idx, value) => { values(idx, value == NULL_ID ? null : map[value]); }, count);
                    }
                    break;

                default:
                    throw new NotSupportedException(mode.ToString());
            }
        }

        public class Raw : IIndexerPersist<String>
        {
            public void Store(BinaryWriter writer, Func<int, string> values, int count)
            {
                byte[] buffer = new byte[(int)Math.Ceiling(count / 8.0)];

                string[] array = new string[count];
                int length = 0;

                for (int i = 0; i < count; i++)
                {
                    string value = values(i);
                    if (value != null)
                    {
                        buffer.SetBit(i, 1);
                        array[length++] = value;
                    }
                    //else
                    //    buffer.SetBit(i, 0);
                }

                writer.Write(buffer);

                for (int i = 0; i < length; i++)
                {
                    try
                    {
                        writer.Write(array[i]);
                    }
                    catch
                    {
                        // 无法识别的编码忽略
                    }
                }
            }

            public void Load(BinaryReader reader, Action<int, string> values, int count)
            {
                byte[] buffer = reader.ReadBytes((int)Math.Ceiling(count / 8.0));

                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        values(i, buffer.GetBit(i) == 1 ? reader.ReadString() : null);
                    }
                    catch 
                    {
                         
                    }
                }
                   
            }
        }

        private enum PersistMode : byte
        {
            Raw,
            Dictionary
        }
    }
}