using System.Collections;
using System.Collections.Generic;
using Iveely.Data;
using Iveely.WaterfallTree;
using System;

namespace Iveely.Database
{
    public class XTablePortable<TKey, TRecord> : ITable<TKey, TRecord>
    {
        public ITable<IData, IData> Table { get; private set; }
        public ITransformer<TKey, IData> KeyTransformer { get; private set; }
        public ITransformer<TRecord, IData> RecordTransformer { get; private set; }

        public XTablePortable(ITable<IData, IData> table, ITransformer<TKey, IData> keyTransformer = null, ITransformer<TRecord, IData> recordTransformer = null)
        {
            if (table == null)
                throw new ArgumentNullException("table");

            Table = table;

            if (keyTransformer == null)
                keyTransformer = new DataTransformer<TKey>(table.Descriptor.KeyType);

            if (recordTransformer == null)
                recordTransformer = new DataTransformer<TRecord>(table.Descriptor.RecordType);

            KeyTransformer = keyTransformer;
            RecordTransformer = recordTransformer;
        }

        #region ITable<TKey, TRecord> Membres

        public TRecord this[TKey key]
        {
            get
            {
                IData ikey = KeyTransformer.To(key);
                IData irec = Table[ikey];
                
                return RecordTransformer.From(irec);
            }
            set
            {
                IData ikey = KeyTransformer.To(key);
                IData irec = RecordTransformer.To(value);

                Table[ikey] = irec;
            }
        }

        public void Replace(TKey key, TRecord record)
        {
            IData ikey = KeyTransformer.To(key);
            IData irec = RecordTransformer.To(record);

            Table.Replace(ikey, irec);
        }

        public void InsertOrIgnore(TKey key, TRecord record)
        {
            IData ikey = KeyTransformer.To(key);
            IData irec = RecordTransformer.To(record);

            Table.InsertOrIgnore(ikey, irec);
        }

        public void Delete(TKey key)
        {
            IData ikey = KeyTransformer.To(key);

            Table.Delete(ikey);
        }

        public void Delete(TKey fromKey, TKey toKey)
        {
            IData ifrom = KeyTransformer.To(fromKey);
            IData ito = KeyTransformer.To(toKey);

            Table.Delete(ifrom, ito);
        }

        public void Clear()
        {
            Table.Clear();
        }

        public bool Exists(TKey key)
        {
            IData ikey = KeyTransformer.To(key);

            return Table.Exists(ikey);
        }

        public bool TryGet(TKey key, out TRecord record)
        {
            IData ikey = KeyTransformer.To(key);

            IData irec;
            if (!Table.TryGet(ikey, out irec))
            {
                record = default(TRecord);
                return false;
            }

            record = RecordTransformer.From(irec);

            return true;
        }

        public TRecord Find(TKey key)
        {
            IData ikey = KeyTransformer.To(key);

            IData irec = Table.Find(ikey);
            if (irec == null)
                return default(TRecord);

            TRecord record = RecordTransformer.From(irec);

            return record;
        }

        public TRecord TryGetOrDefault(TKey key, TRecord defaultRecord)
        {
            IData ikey = KeyTransformer.To(key);
            IData idefaultRec = RecordTransformer.To(defaultRecord);
            IData irec = Table.TryGetOrDefault(ikey, idefaultRec);

            TRecord record = RecordTransformer.From(irec);

            return record;
        }

        public KeyValuePair<TKey, TRecord>? FindNext(TKey key)
        {
            IData ikey = KeyTransformer.To(key);

            KeyValuePair<IData, IData>? kv = Table.FindNext(ikey);
            if (!kv.HasValue)
                return null;

            TKey k = KeyTransformer.From(kv.Value.Key);
            TRecord r = RecordTransformer.From(kv.Value.Value);

            return new KeyValuePair<TKey, TRecord>(k, r);
        }

        public KeyValuePair<TKey, TRecord>? FindAfter(TKey key)
        {
            IData ikey = KeyTransformer.To(key);

            KeyValuePair<IData, IData>? kv = Table.FindAfter(ikey);
            if (!kv.HasValue)
                return null;

            TKey k = KeyTransformer.From(kv.Value.Key);
            TRecord r = RecordTransformer.From(kv.Value.Value);

            return new KeyValuePair<TKey, TRecord>(k, r);
        }

        public KeyValuePair<TKey, TRecord>? FindPrev(TKey key)
        {
            IData ikey = KeyTransformer.To(key);

            KeyValuePair<IData, IData>? kv = Table.FindPrev(ikey);
            if (!kv.HasValue)
                return null;

            TKey k = KeyTransformer.From(kv.Value.Key);
            TRecord r = RecordTransformer.From(kv.Value.Value);

            return new KeyValuePair<TKey, TRecord>(k, r);
        }

        public KeyValuePair<TKey, TRecord>? FindBefore(TKey key)
        {
            IData ikey = KeyTransformer.To(key);

            KeyValuePair<IData, IData>? kv = Table.FindBefore(ikey);
            if (!kv.HasValue)
                return null;

            TKey k = KeyTransformer.From(kv.Value.Key);
            TRecord r = RecordTransformer.From(kv.Value.Value);

            return new KeyValuePair<TKey, TRecord>(k, r);
        }

        public IEnumerable<KeyValuePair<TKey, TRecord>> Forward()
        {
            foreach (var kv in Table.Forward())
            {
                TKey key = KeyTransformer.From(kv.Key);
                TRecord rec = RecordTransformer.From(kv.Value);

                yield return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TRecord>> Forward(TKey from, bool hasFrom, TKey to, bool hasTo)
        {
            IData ifrom = hasFrom ? KeyTransformer.To(from) : null;
            IData ito = hasTo ? KeyTransformer.To(to) : null;

            foreach (var kv in Table.Forward(ifrom, hasFrom, ito, hasTo))
            {
                TKey key = KeyTransformer.From(kv.Key);
                TRecord rec = RecordTransformer.From(kv.Value);

                yield return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TRecord>> Backward()
        {
            foreach (var kv in Table.Backward())
            {
                TKey key = KeyTransformer.From(kv.Key);
                TRecord rec = RecordTransformer.From(kv.Value);

                yield return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TRecord>> Backward(TKey to, bool hasTo, TKey from, bool hasFrom)
        {
            IData ito = hasTo ? KeyTransformer.To(to) : null;
            IData ifrom = hasFrom ? KeyTransformer.To(from) : null;
            
            foreach (var kv in Table.Backward(ito, hasTo, ifrom, hasFrom))
            {
                TKey key = KeyTransformer.From(kv.Key);
                TRecord rec = RecordTransformer.From(kv.Value);

                yield return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public KeyValuePair<TKey, TRecord> FirstRow
        {
            get
            {
                KeyValuePair<IData, IData> kv = Table.FirstRow;

                TKey key = KeyTransformer.From(kv.Key);
                TRecord rec = RecordTransformer.From(kv.Value);

                return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public KeyValuePair<TKey, TRecord> LastRow
        {
            get
            {
                KeyValuePair<IData, IData> kv = Table.LastRow;

                TKey key = KeyTransformer.From(kv.Key);
                TRecord rec = RecordTransformer.From(kv.Value);

                return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public long Count()
        {
            return Table.Count();
        }

        public IDescriptor Descriptor
        {
            get { return Table.Descriptor; }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey, TRecord>> Members

        public IEnumerator<KeyValuePair<TKey, TRecord>> GetEnumerator()
        {
            return Forward().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
