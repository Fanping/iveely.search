using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Iveely.General.Extensions;
using Iveely.General.Comparers;
using System.IO;
using Iveely.General.Persist;
using System.Threading.Tasks;

namespace Iveely.General.Collections
{
    public class OrderedSet<TKey, TValue> : IOrderedSet<TKey, TValue>
    {
        protected List<KeyValuePair<TKey, TValue>> list;
        protected Dictionary<TKey, TValue> dictionary;
        protected SortedSet<KeyValuePair<TKey, TValue>> set;

        protected IComparer<TKey> comparer;
        protected IEqualityComparer<TKey> equalityComparer;
        protected KeyValuePairComparer<TKey, TValue> kvComparer;

        protected OrderedSet(IComparer<TKey> comparer, IEqualityComparer<TKey> equalityComparer, List<KeyValuePair<TKey, TValue>> list)
        {
            this.comparer = comparer;
            this.equalityComparer = equalityComparer;
            kvComparer = new KeyValuePairComparer<TKey, TValue>(comparer);

            this.list = list;
        }

        protected OrderedSet(IComparer<TKey> comparer, IEqualityComparer<TKey> equalityComparer, SortedSet<KeyValuePair<TKey, TValue>> set)
        {
            this.comparer = comparer;
            this.equalityComparer = equalityComparer;
            kvComparer = new KeyValuePairComparer<TKey, TValue>(comparer);

            this.set = set;
        }

        protected OrderedSet(IComparer<TKey> comparer, IEqualityComparer<TKey> equalityComparer, int capacity)
            : this(comparer, equalityComparer, new List<KeyValuePair<TKey, TValue>>(capacity))
        {
        }

        public OrderedSet(IComparer<TKey> comparer, IEqualityComparer<TKey> equalityComparer)
            : this(comparer, equalityComparer, 4)
        {
        }

        protected void TransformListToTree()
        {
            set = new SortedSet<KeyValuePair<TKey, TValue>>(kvComparer);
            set.ConstructFromSortedArray(list.GetArray(), 0, list.Count);
            list = null;
        }

        protected void TransformDictionaryToTree()
        {
            set = new SortedSet<KeyValuePair<TKey, TValue>>(dictionary, kvComparer);
            dictionary = null;
        }

        protected void TransformListToDictionary()
        {
            dictionary = new Dictionary<TKey, TValue>(list.Capacity, EqualityComparer);

            foreach (var kv in list)
                dictionary.Add(kv.Key, kv.Value);

            list = null;
        }

        /// <summary>
        /// clear all data and set ordered set to default list mode
        /// </summary>
        protected void Reset()
        {
            list = new List<KeyValuePair<TKey, TValue>>();
            dictionary = null;
            set = null;
        }

        private bool FindIndexes(KeyValuePair<TKey, TValue> from, bool hasFrom, KeyValuePair<TKey, TValue> to, bool hasTo, out int idxFrom, out int idxTo)
        {
            idxFrom = 0;
            idxTo = list.Count - 1;
            Debug.Assert(list.Count > 0);

            if (hasFrom)
            {
                int cmp = Comparer.Compare(from.Key, list[list.Count - 1].Key);
                if (cmp > 0)
                    return false;

                if (cmp == 0)
                {
                    idxFrom = idxTo;
                    return true;
                }
            }

            if (hasTo)
            {
                int cmp = Comparer.Compare(to.Key, list[0].Key);
                if (cmp < 0)
                    return false;

                if (cmp == 0)
                {
                    idxTo = idxFrom;
                    return true;
                }
            }

            if (hasFrom && Comparer.Compare(from.Key, list[0].Key) > 0)
            {
                idxFrom = list.BinarySearch(1, list.Count - 1, from, kvComparer);
                if (idxFrom < 0)
                    idxFrom = ~idxFrom;
            }

            if (hasTo && Comparer.Compare(to.Key, list[list.Count - 1].Key) < 0)
            {
                idxTo = list.BinarySearch(idxFrom, list.Count - idxFrom, to, kvComparer);
                if (idxTo < 0)
                    idxTo = ~idxTo - 1;
            }

            Debug.Assert(0 <= idxFrom);
            Debug.Assert(idxFrom <= idxTo);
            Debug.Assert(idxTo <= list.Count - 1);

            return true;
        }

        public IOrderedSet<TKey, TValue> Split(int count)
        {
            if (list != null)
            {
                var right = list.Split(count);

                return new OrderedSet<TKey, TValue>(Comparer, EqualityComparer, right);
            }
            else
            {
                if (dictionary != null)
                    TransformDictionaryToTree();

                var right = set.Split(count);

                return new OrderedSet<TKey, TValue>(Comparer, EqualityComparer, right);
            }
        }

        /// <summary>
        /// All keys in the input set must be less than all keys in the current set OR all keys in the input set must be greater than all keys in the current set.
        /// </summary>
        public void Merge(IOrderedSet<TKey, TValue> set)
        {
            if (set.Count == 0)
                return;

            if (this.Count == 0)
            {
                foreach (var x in set) //set.Forward()
                    list.Add(x);

                return;
            }

            //Debug.Assert(comparer.Compare(this.Last.Key, set.First.Key) < 0 || comparer.Compare(this.First.Key, set.Last.Key) > 0);

            if (list != null)
            {
                int idx = kvComparer.Compare(set.Last, list[0]) < 0 ? 0 : list.Count;
                list.InsertRange(idx, set);
            }
            else if (dictionary != null)
            {
                foreach (var kv in set.InternalEnumerate())
                    this.dictionary.Add(kv.Key, kv.Value); //there should be no exceptions
            }
            else //if (set != null)
            {
                foreach (var kv in set.InternalEnumerate())
                    this.set.Add(kv);
            }
        }

        #region IOrderedSet<TKey,TValue> Members

        public IComparer<TKey> Comparer
        {
            get { return comparer; }
        }

        public IEqualityComparer<TKey> EqualityComparer
        {
            get { return equalityComparer; }
        }

        public void Add(TKey key, TValue value)
        {
            KeyValuePair<TKey, TValue> kv = new KeyValuePair<TKey, TValue>(key, value);

            if (set != null)
            {
                set.Replace(kv);
                return;
            }

            if (dictionary != null)
            {
                dictionary[kv.Key] = kv.Value;
                return;
            }

            if (list.Count == 0)
                list.Add(kv);
            else
            {
                var last = list[list.Count - 1];
                int cmp = comparer.Compare(last.Key, kv.Key);

                if (cmp < 0)
                    list.Add(kv);
                else if (cmp > 0)
                {
                    TransformListToDictionary();
                    dictionary[kv.Key] = kv.Value;
                }
                else
                    list[list.Count - 1] = kv;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void UnsafeAdd(TKey key, TValue value)
        {
            KeyValuePair<TKey, TValue> kv = new KeyValuePair<TKey, TValue>(key, value);
            if (set != null)
            {
                set.Replace(kv);
                return;
            }

            if (dictionary != null)
            {
                dictionary[kv.Key] = kv.Value;
                return;
            }

            list.Add(kv);
        }

        public bool Remove(TKey key)
        {
            KeyValuePair<TKey, TValue> template = new KeyValuePair<TKey, TValue>(key, default(TValue));

            if (list != null)
                TransformListToDictionary();

            if (dictionary != null)
            {
                bool res = dictionary.Remove(key);
                if (dictionary.Count == 0)
                    Reset();

                return res;
            }
            else
            {
                bool res = set.Remove(template);
                if (set.Count == 0)
                    Reset();

                return res;
            }
        }

        public bool Remove(TKey from, bool hasFrom, TKey to, bool hasTo)
       {
            if (Count == 0)
                return false;

            if (!hasFrom && !hasTo)
            {
                Clear();
                return true;
            }

            if (list != null)
                TransformListToTree();
            else if (dictionary != null)
                TransformDictionaryToTree();

            KeyValuePair<TKey, TValue> fromKey = hasFrom ? new KeyValuePair<TKey, TValue>(from, default(TValue)) : set.Min;
            KeyValuePair<TKey, TValue> toKey = hasTo ? new KeyValuePair<TKey, TValue>(to, default(TValue)) : set.Max;

            bool res = set.Remove(fromKey, toKey);
            if (set.Count == 0)
                Reset();

            return res;
        }

        public bool ContainsKey(TKey key)
        {
            TValue value;
            return TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            KeyValuePair<TKey, TValue> template = new KeyValuePair<TKey, TValue>(key, default(TValue));

            if (list != null)
            {
                int idx = list.BinarySearch(template, kvComparer);
                if (idx >= 0)
                {
                    value = list[idx].Value;
                    return true;
                }
            }
            else if (dictionary != null)
                return dictionary.TryGetValue(template.Key, out value);
            else
            {
                KeyValuePair<TKey, TValue> kv;
                if (set.TryGetValue(template, out kv))
                {
                    value = kv.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                    throw new KeyNotFoundException("The key was not found.");

                return value;
            }
            set
            {
                Add(key, value);
            }
        }

        public void Clear()
        {
            Reset();
        }

        public bool IsInternallyOrdered
        {
            get { return dictionary == null; }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> InternalEnumerate()
        {
            if (list != null)
                return list;
            else if (dictionary != null)
                return dictionary;
            else //if (set != null)
                return set;
        }

        public void LoadFrom(KeyValuePair<TKey, TValue>[] array, int count, bool isOrdered)
        {
            if (isOrdered)
            {
                list = array.CreateList(count);
                dictionary = null;
                set = null;
            }
            else
            {
                list = null;
                dictionary = new Dictionary<TKey, TValue>(count, EqualityComparer);
                set = null;

                for (int i = 0; i < count; i++)
                {
                    if (array[i].Key!=null)
                    {
                        try
                        {
                            dictionary.Add(array[i].Key, array[i].Value);
                        }
                        catch 
                        {
                          
                        }
                    }
                }
                   
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Forward(TKey from, bool hasFrom, TKey to, bool hasTo)
        {
            if (hasFrom && hasTo && comparer.Compare(from, to) > 0)
                throw new ArgumentException("from > to");

            if (Count == 0)
                yield break;

            KeyValuePair<TKey, TValue> fromKey = new KeyValuePair<TKey, TValue>(from, default(TValue));
            KeyValuePair<TKey, TValue> toKey = new KeyValuePair<TKey, TValue>(to, default(TValue));

            if (list != null)
            {
                int idxFrom;
                int idxTo;
                if (!FindIndexes(fromKey, hasFrom, toKey, hasTo, out idxFrom, out idxTo))
                    yield break;

                for (int i = idxFrom; i <= idxTo; i++)
                    yield return list[i];
            }
            else
            {
                if (dictionary != null)
                    TransformDictionaryToTree();

                var enumerable = hasFrom || hasTo ? set.GetViewBetween(fromKey, toKey, hasFrom, hasTo) : set;

                foreach (var x in enumerable)
                    yield return x;
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Backward(TKey to, bool hasTo, TKey from, bool hasFrom)
        {
            if (hasFrom && hasTo && comparer.Compare(from, to) > 0)
                throw new ArgumentException("from > to");

            if (Count == 0)
                yield break;

            KeyValuePair<TKey, TValue> fromKey = new KeyValuePair<TKey, TValue>(from, default(TValue));
            KeyValuePair<TKey, TValue> toKey = new KeyValuePair<TKey, TValue>(to, default(TValue));

            if (list != null)
            {
                int idxFrom;
                int idxTo;
                if (!FindIndexes(fromKey, hasFrom, toKey, hasTo, out idxFrom, out idxTo))
                    yield break;

                for (int i = idxTo; i >= idxFrom; i--)
                    yield return list[i];
            }
            else
            {
                if (dictionary != null)
                    TransformDictionaryToTree();

                var enumerable = hasFrom || hasTo ? set.GetViewBetween(fromKey, toKey, hasFrom, hasTo) : set;

                foreach (var x in enumerable.Reverse())
                    yield return x;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Forward(default(TKey), false, default(TKey), false).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public KeyValuePair<TKey, TValue> First
        {
            get
            {
                if (Count == 0)
                    throw new InvalidOperationException("The set is empty.");

                if (list != null)
                    return list[0];

                if (dictionary != null)
                    TransformDictionaryToTree();

                return set.Min;
            }
        }

        public KeyValuePair<TKey, TValue> Last
        {
            get
            {
                if (Count == 0)
                    throw new InvalidOperationException("The set is empty.");

                if (list != null)
                    return list[list.Count - 1];

                if (dictionary != null)
                    TransformDictionaryToTree();

                return set.Max;
            }
        }

        public int Count
        {
            get
            {
                if (list != null)
                    return list.Count;

                if (dictionary != null)
                    return dictionary.Count;

                return set.Count;
            }
        }

        #endregion
    }
}
