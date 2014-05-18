using Iveely.Data;
using Iveely.Database;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Iveely.WaterfallTree
{
    public class Scheme : IEnumerable<KeyValuePair<long, Locator>>
    {
        public const byte VERSION = 40;

        private long locatorID = Locator.MIN.ID;

        private ConcurrentDictionary<long, Locator> map = new ConcurrentDictionary<long, Locator>();

        private long ObtainPathID()
        {
            return Interlocked.Increment(ref locatorID);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(VERSION);

            writer.Write(locatorID);
            writer.Write(map.Count);

            foreach (var kv in map)
            {
                Locator locator = (Locator)kv.Value;
                locator.Serialize(writer);
            }
        }

        public static Scheme Deserialize(BinaryReader reader)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid Scheme version.");

            Scheme scheme = new Scheme();

            scheme.locatorID = reader.ReadInt64();
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var locator = Locator.Deserialize(reader);

                scheme.map[locator.ID] = locator;

                //Do not prepare the locator yet
            }

            return scheme;
        }

        public Locator this[long id]
        {
            get { return map[id]; }
        }

        public Locator Create(string name, int structureType, DataType keyDataType, DataType recordDataType, Type keyType, Type recordType)
        {
            var id = ObtainPathID();

            var locator = new Locator(id, name, structureType, keyDataType, recordDataType, keyType, recordType);

            map[id] = locator;

            return locator;
        }

        public int Count
        {
            get { return map.Count; }
        }

        public IEnumerator<KeyValuePair<long, Locator>> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}