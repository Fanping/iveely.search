using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Iveely.STSdb4.General.Collections
{
    public class Cache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        //where TKey : IEquatable<TKey>//, IComparable<TKey>
    {
        private readonly IDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> Mapping;//mapping between link and element in Items
        private readonly LinkedList<KeyValuePair<TKey, TValue>> Items = new LinkedList<KeyValuePair<TKey, TValue>>();//The newer and/or most used elements emerges on top(begining)
        private int capacity;

        public readonly object SyncRoot = new object();
        public event OverflowDelegate Overflow;

        //Comparer<TKey>.Default
        public Cache(int capacity, IComparer<TKey> comparer)
        {
            this.capacity = capacity;
            Mapping = new SortedDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
        }

        //EqualityComparer<TKey>.Default
        public Cache(int capacity, IEqualityComparer<TKey> comparer)
        {
            this.capacity = capacity;
            Mapping = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
        }

        public Cache(int capacity)
            : this(capacity, EqualityComparer<TKey>.Default)
        {
        }

        public TValue this[TKey key]
        {
            get { return Retrieve(key); }
            set { Packet(key, value); }
        }

        public TValue Packet(TKey key, TValue value)
        {
            TValue result;
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (Mapping.TryGetValue(key, out node))
            {
                result = node.Value.Value;
                node.Value = new KeyValuePair<TKey, TValue>(key, value);
                Refresh(node);
            }
            else
            {
                result = default(TValue);
                Mapping[key] = Items.AddFirst(new KeyValuePair<TKey, TValue>(key, value));
                ClearOverflowItems();
            }

            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (!Mapping.TryGetValue(key, out node))
            {
                value = default(TValue);
                return false;
            }

            Refresh(node);
            value = node.Value.Value;
            return true;
        }

        public TValue Retrieve(TKey key)
        {
            TValue value;
            TryGetValue(key, out value);

            return value;
        }

        public TValue Exclude(TKey key)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (!Mapping.TryGetValue(key, out node))
                return default(TValue);

            Mapping.Remove(key);
            Items.Remove(node);

            return node.Value.Value;
        }

        public void Clear()
        {
            Mapping.Clear();
            Items.Clear();
        }

        private void Refresh(LinkedListNode<KeyValuePair<TKey, TValue>> node)
        {
            if (node != Items.First)
            {
                Items.Remove(node);
                Items.AddFirst(node);
            }
        }

        public int Capacity
        {
            get { return capacity; }
            set
            {
                if (capacity == value)
                    return;

                capacity = value;
                ClearOverflowItems();
            }
        }

        public int Count
        {
            get { return Items.Count; }
        }

        public bool IsOverflow
        {
            get { return (Items.Count > Capacity); }
        }

        public KeyValuePair<TKey, TValue> ExcludeLastItem()
        {
            KeyValuePair<TKey, TValue> item = Items.Last.Value;
            Mapping.Remove(Items.Last.Value.Key);
            Items.RemoveLast();
            return item;
        }

        private void ClearOverflowItems()
        {
            while (IsOverflow)
            {
                KeyValuePair<TKey, TValue> item = ExcludeLastItem();
                if (Overflow != null)
                    Overflow(item);
            }
        }

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> node = Items.Last;
            while (node != null)
            {
                yield return node.Value;

                node = node.Previous;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public delegate void OverflowDelegate(KeyValuePair<TKey, TValue> item);
    }
}
