using Iveely.STSdb4.WaterfallTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.Database
{
    public interface ITable
    {
    }

    public interface ITable<TKey, TRecord> : ITable, IEnumerable<KeyValuePair<TKey, TRecord>>
    {
        TRecord this[TKey key] { get; set; }

        void Replace(TKey key, TRecord record);
        void InsertOrIgnore(TKey key, TRecord record);
        void Delete(TKey key);
        void Delete(TKey fromKey, TKey toKey);
        void Clear();

        bool Exists(TKey key);
        bool TryGet(TKey key, out TRecord record);
        TRecord Find(TKey key);
        TRecord TryGetOrDefault(TKey key, TRecord defaultRecord);

        KeyValuePair<TKey, TRecord>? FindNext(TKey key);
        KeyValuePair<TKey, TRecord>? FindAfter(TKey key);
        KeyValuePair<TKey, TRecord>? FindPrev(TKey key);
        KeyValuePair<TKey, TRecord>? FindBefore(TKey key);

        IEnumerable<KeyValuePair<TKey, TRecord>> Forward();
        IEnumerable<KeyValuePair<TKey, TRecord>> Forward(TKey from, bool hasFrom, TKey to, bool hasTo);
        IEnumerable<KeyValuePair<TKey, TRecord>> Backward();
        IEnumerable<KeyValuePair<TKey, TRecord>> Backward(TKey to, bool hasTo, TKey from, bool hasFrom);

        KeyValuePair<TKey, TRecord> FirstRow { get; }
        KeyValuePair<TKey, TRecord> LastRow { get; }

        IDescriptor Descriptor { get; }

        long Count();
    }
}