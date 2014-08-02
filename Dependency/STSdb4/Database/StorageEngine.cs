using System;
using System.Collections.Concurrent;
using System.IO;
using Iveely.STSdb4.General.Extensions;
using Iveely.STSdb4.Data;
using Iveely.STSdb4.Storage;
using Iveely.STSdb4.Data;
using Iveely.STSdb4.WaterfallTree;
using System.Linq;
using Iveely.STSdb4.General.IO;
using System.Management;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Iveely.STSdb4.WaterfallTree;

namespace Iveely.STSdb4.Database
{
    public class StorageEngine : WTree, IStorageEngine
    {
        //user scheme
        private Dictionary<string, Item1> map = new Dictionary<string, Item1>();

        private readonly object SyncRoot = new object();

        public StorageEngine(IHeap heap)
            : base(heap)
        {
            foreach (var locator in GetAllLocators())
            {
                if (locator.IsDeleted)
                    continue;
                
                Item1 item = new Item1(locator, null);

                map[locator.Name] = item;
            }
        }

        private Item1 Obtain(string name, int structureType, DataType keyDataType, DataType recordDataType, Type keyType, Type recordType)
        {
            Debug.Assert(keyDataType != null);
            Debug.Assert(recordDataType != null);

            Item1 item;
            if (!map.TryGetValue(name, out item))
            {
                if (keyType == null)
                    keyType = DataTypeUtils.BuildType(keyDataType);
                if (recordType == null)
                    recordType = DataTypeUtils.BuildType(recordDataType);

                var locator = CreateLocator(name, structureType, keyDataType, recordDataType, keyType, recordType);
                XTablePortable table = new XTablePortable(this, locator);

                map[name] = item = new Item1(locator, table);
            }
            else
            {
                var locator = item.Locator;

                if (locator.StructureType != structureType)
                    throw new ArgumentException(String.Format("Invalid structure type for '{0}'", name));

                if (keyDataType != locator.KeyDataType)
                    throw new ArgumentException("keyDataType");

                if (recordDataType != locator.RecordDataType)
                    throw new ArgumentException("recordDataType");

                if (locator.KeyType == null)
                    locator.KeyType = DataTypeUtils.BuildType(keyDataType);
                else
                {
                    if (keyType != null && keyType != locator.KeyType)
                        throw new ArgumentException(String.Format("Invalid keyType for table '{0}'", name));
                }

                if (locator.RecordType == null)
                    locator.RecordType = DataTypeUtils.BuildType(recordDataType);
                else
                {
                    if (recordType != null && recordType != locator.RecordType)
                        throw new ArgumentException(String.Format("Invalid recordType for table '{0}'", name));
                }

                locator.AccessTime = DateTime.Now;
            }

            if (!item.Locator.IsReady)
                item.Locator.Prepare();

            if (item.Table == null)
                item.Table = new XTablePortable(this, item.Locator);

            return item;
        }

        #region IStorageEngine

        public ITable<IData, IData> OpenXTablePortable(string name, DataType keyDataType, DataType recordDataType)
        {
            lock (SyncRoot)
            {
                var item = Obtain(name, StructureType.XTABLE, keyDataType, recordDataType, null, null);

                return item.Table;
            }
        }

        public ITable<TKey, TRecord> OpenXTablePortable<TKey, TRecord>(string name, DataType keyDataType, DataType recordDataType, ITransformer<TKey, IData> keyTransformer, ITransformer<TRecord, IData> recordTransformer)
        {
            lock (SyncRoot)
            {
                var item = Obtain(name, StructureType.XTABLE, keyDataType, recordDataType, null, null);

                if (item.Portable == null)
                    item.Portable = new XTablePortable<TKey, TRecord>(item.Table, keyTransformer, recordTransformer);

                return (ITable<TKey, TRecord>)item.Portable;
            }
        }

        public ITable<TKey, TRecord> OpenXTablePortable<TKey, TRecord>(string name)
        {
            DataType keyDataType = DataTypeUtils.BuildDataType(typeof(TKey));
            DataType recordDataType = DataTypeUtils.BuildDataType(typeof(TRecord));

            return OpenXTablePortable<TKey, TRecord>(name, keyDataType, recordDataType, null, null);
        }

        public ITable<TKey, TRecord> OpenXTable<TKey, TRecord>(string name)
        {
            lock (SyncRoot)
            {
                Type keyType = typeof(TKey);
                Type recordType = typeof(TRecord);

                DataType keyDataType = DataTypeUtils.BuildDataType(keyType);
                DataType recordDataType = DataTypeUtils.BuildDataType(recordType);

                var item = Obtain(name, StructureType.XTABLE, keyDataType, recordDataType, keyType, recordType);

                if (item.Direct == null)
                    item.Direct = new XTable<TKey, TRecord>(item.Table);

                return (XTable<TKey, TRecord>)item.Direct;
            }
        }

        public XFile OpenXFile(string name)
        {
            lock (SyncRoot)
            {
                var item = Obtain(name, StructureType.XFILE, DataType.Int64, DataType.ByteArray, typeof(long), typeof(byte[]));

                if (item.File == null)
                    item.File = new XFile(item.Table);

                return item.File;
            }
        }

        public IDescriptor this[string name]
        {
            get
            {
                lock (SyncRoot)
                {
                    Item1 item;
                    if (!map.TryGetValue(name, out item))
                        return null;

                    return item.Locator;
                }
            }
        }

        public IDescriptor Find(long id)
        {
            lock (SyncRoot)
                return GetLocator(id);
        }

        public void Delete(string name)
        {
            lock (SyncRoot)
            {
                Item1 item;
                if (!map.TryGetValue(name, out item))
                    return;

                map.Remove(name);

                if (item.Table != null)
                {
                    item.Table.Clear();
                    item.Table.Flush();
                }

                item.Locator.IsDeleted = true;
            }
        }

        public void Rename(string name, string newName)
        {
            lock (SyncRoot)
            {
                if (map.ContainsKey(newName))
                    return;

                Item1 item;
                if (!map.TryGetValue(name, out item))
                    return;

                item.Locator.Name = newName;

                map.Remove(name);
                map.Add(newName, item);
            }
        }

        public bool Exists(string name)
        {
            lock (SyncRoot)
                return map.ContainsKey(name);
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                    return map.Count;
            }
        }

        public override void Commit()
        {
            lock (SyncRoot)
            {
                foreach (var kv in map)
                {
                    var table = kv.Value.Table;

                    if (table != null)
                    {
                        if (table.IsModified)
                            table.Locator.ModifiedTime = DateTime.Now;

                        table.Flush();
                    }
                }

                base.Commit();

                foreach (var kv in map)
                {
                    var table = kv.Value.Table;

                    if (table != null)
                        table.IsModified = false;
                }
            }
        }

        public override void Close()
        {
            base.Close();
        }

        public IEnumerator<IDescriptor> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return map.Select(x => (IDescriptor)x.Value.Locator).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private class Item1
        {
            public Locator Locator;
            public XTablePortable Table;

            public ITable Direct;
            public ITable Portable;
            public XFile File;            

            public Item1(Locator locator, XTablePortable table)
            {
                Locator = locator;
                Table = table;
            }
        }
    }
}
