using Iveely.STSdb4.Data;
using Iveely.STSdb4.WaterfallTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.Database
{
    public class XTable<TKey, TRecord> : ITable<TKey, TRecord>
    {
        public ITable<IData, IData> Table { get; private set; }

        public XTable(ITable<IData, IData> table)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            
            Table = table;
        }

        #region ITable<TKey, TRecord> Membres

        public TRecord this[TKey key]
        {
            get
            {
                IData ikey = new Data<TKey>(key);
                IData irec = Table[ikey];

                return ((Data<TRecord>)irec).Value;
            }
            set
            {
                IData ikey = new Data<TKey>(key);
                IData irec = new Data<TRecord>(value);

                Table[ikey] = irec;
            }
        }

        public void Replace(TKey key, TRecord record)
        {
            IData ikey = new Data<TKey>(key);
            IData irec = new Data<TRecord>(record);

            Table.Replace(ikey, irec);
        }

        public void InsertOrIgnore(TKey key, TRecord record)
        {
            IData ikey = new Data<TKey>(key);
            IData irec = new Data<TRecord>(record);

            Table.InsertOrIgnore(ikey, irec);
        }

        public void Delete(TKey key)
        {
            IData ikey = new Data<TKey>(key);

            Table.Delete(ikey);
        }

        public void Delete(TKey fromKey, TKey toKey)
        {
            IData ifrom = new Data<TKey>(fromKey);
            IData ito = new Data<TKey>(toKey);

            Table.Delete(ifrom, ito);
        }

        public void Clear()
        {
            Table.Clear();
        }

        public bool Exists(TKey key)
        {
            IData ikey = new Data<TKey>(key);

            return Table.Exists(ikey);
        }

        public bool TryGet(TKey key, out TRecord record)
        {
            IData ikey = new Data<TKey>(key);

            IData irec;
            if (!Table.TryGet(ikey, out irec))
            {
                record = default(TRecord);
                return false;
            }

            record = ((Data<TRecord>)irec).Value;

            return true;
        }

        public TRecord Find(TKey key)
        {
            IData ikey = new Data<TKey>(key);

            IData irec = Table.Find(ikey);
            if (irec == null)
                return default(TRecord);

            TRecord record = ((Data<TRecord>)irec).Value;

            return record;
        }

        public TRecord TryGetOrDefault(TKey key, TRecord defaultRecord)
        {
            IData ikey = new Data<TKey>(key);
            IData idefaultRec = new Data<TRecord>(defaultRecord);
            IData irec = Table.TryGetOrDefault(ikey, idefaultRec);

            TRecord record = ((Data<TRecord>)irec).Value;

            return record;
        }

        public KeyValuePair<TKey, TRecord>? FindNext(TKey key)
        {
            IData ikey = new Data<TKey>(key);

            KeyValuePair<IData, IData>? kv = Table.FindNext(ikey);
            if (!kv.HasValue)
                return null;

            TKey k = ((Data<TKey>)kv.Value.Key).Value;
            TRecord r = ((Data<TRecord>)kv.Value.Value).Value;

            return new KeyValuePair<TKey, TRecord>(k, r);
        }

        public KeyValuePair<TKey, TRecord>? FindAfter(TKey key)
        {
            IData ikey = new Data<TKey>(key);

            KeyValuePair<IData, IData>? kv = Table.FindAfter(ikey);
            if (!kv.HasValue)
                return null;

            TKey k = ((Data<TKey>)kv.Value.Key).Value;
            TRecord r = ((Data<TRecord>)kv.Value.Value).Value;

            return new KeyValuePair<TKey, TRecord>(k, r);
        }

        public KeyValuePair<TKey, TRecord>? FindPrev(TKey key)
        {
            IData ikey = new Data<TKey>(key);

            KeyValuePair<IData, IData>? kv = Table.FindPrev(ikey);
            if (!kv.HasValue)
                return null;

            TKey k = ((Data<TKey>)kv.Value.Key).Value;
            TRecord r = ((Data<TRecord>)kv.Value.Value).Value;

            return new KeyValuePair<TKey, TRecord>(k, r);
        }

        public KeyValuePair<TKey, TRecord>? FindBefore(TKey key)
        {
            IData ikey = new Data<TKey>(key);

            KeyValuePair<IData, IData>? kv = Table.FindBefore(ikey);
            if (!kv.HasValue)
                return null;

            TKey k = ((Data<TKey>)kv.Value.Key).Value;
            TRecord r = ((Data<TRecord>)kv.Value.Value).Value;

            return new KeyValuePair<TKey, TRecord>(k, r);
        }

        public IEnumerable<KeyValuePair<TKey, TRecord>> Forward()
        {
            foreach (var kv in Table.Forward())
            {
                TKey key = ((Data<TKey>)kv.Key).Value;
                TRecord rec = default(TRecord);
                if (kv.Value != null)
                {
                    rec = ((Data<TRecord>)kv.Value).Value;
                }
                yield return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TRecord>> Forward(TKey from, bool hasFrom, TKey to, bool hasTo)
        {
            IData ifrom = hasFrom ? new Data<TKey>(from) : null;
            IData ito = hasTo ? new Data<TKey>(to) : null;

            foreach (var kv in Table.Forward(ifrom, hasFrom, ito, hasTo))
            {
                TKey key = ((Data<TKey>)kv.Key).Value;
                TRecord rec = ((Data<TRecord>)kv.Value).Value;

                yield return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TRecord>> Backward()
        {
            foreach (var kv in Table.Backward())
            {
                TKey key = ((Data<TKey>)kv.Key).Value;
                TRecord rec = ((Data<TRecord>)kv.Value).Value;

                yield return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TRecord>> Backward(TKey to, bool hasTo, TKey from, bool hasFrom)
        {
            IData ito = hasTo ? new Data<TKey>(to) : null;
            IData ifrom = hasFrom ? new Data<TKey>(from) : null;

            foreach (var kv in Table.Backward(ito, hasTo, ifrom, hasFrom))
            {
                TKey key = ((Data<TKey>)kv.Key).Value;
                TRecord rec = ((Data<TRecord>)kv.Value).Value;

                yield return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public KeyValuePair<TKey, TRecord> FirstRow
        {
            get
            {
                KeyValuePair<IData, IData> kv = Table.FirstRow;

                TKey key = ((Data<TKey>)kv.Key).Value;
                TRecord rec = ((Data<TRecord>)kv.Value).Value;

                return new KeyValuePair<TKey, TRecord>(key, rec);
            }
        }

        public KeyValuePair<TKey, TRecord> LastRow
        {
            get
            {
                KeyValuePair<IData, IData> kv = Table.LastRow;

                TKey key = ((Data<TKey>)kv.Key).Value;
                TRecord rec = ((Data<TRecord>)kv.Value).Value;

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
