using Iveely.General.Persist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iveely.General.Collections
{
    public interface IOrderedSet<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        IComparer<TKey> Comparer { get; }
        IEqualityComparer<TKey> EqualityComparer { get; }

        void Add(KeyValuePair<TKey, TValue> kv);
        void Add(TKey key, TValue value);

        void UnsafeAdd(TKey key, TValue value);

        bool Remove(TKey key);
        bool Remove(TKey from, bool hasFrom, TKey to, bool hasTo);

        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);

        TValue this[TKey key] { get; set; }

        void Clear();

        bool IsInternallyOrdered { get; }
        IEnumerable<KeyValuePair<TKey, TValue>> InternalEnumerate();

        IOrderedSet<TKey, TValue> Split(int count);
        void Merge(IOrderedSet<TKey, TValue> set);

        void LoadFrom(KeyValuePair<TKey, TValue>[] array, int count, bool isOrdered);

        IEnumerable<KeyValuePair<TKey, TValue>> Forward(TKey from, bool hasFrom, TKey to, bool hasTo);
        IEnumerable<KeyValuePair<TKey, TValue>> Backward(TKey to, bool hasTo, TKey from, bool hasFrom);

        KeyValuePair<TKey, TValue> First { get; }
        KeyValuePair<TKey, TValue> Last { get; }

        int Count { get; }
    }
}