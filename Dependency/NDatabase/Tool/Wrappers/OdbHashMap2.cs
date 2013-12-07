using System.Collections;
using System.Collections.Generic;

namespace NDatabase.Tool.Wrappers
{
    internal sealed class OdbHashMap<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        public OdbHashMap()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public OdbHashMap(int capacity)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public OdbHashMap(IDictionary<TKey, TValue> dic)
        {
            _dictionary = new Dictionary<TKey, TValue>();
            PutAll(dic);
        }

        #region IDictionary<TKey,TValue> Members

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                TryGetValue(key, out value);
                return value;
            }
            set { _dictionary[key] = value; }
        }

        public void Add(TKey key, TValue v)
        {
            TValue vnull;
            var success =TryGetValue(key, out vnull);
            if (success)
                Remove(key);

            _dictionary.Add(key, v);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue v)
        {
            return _dictionary.TryGetValue(key, out v);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Remove(item);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }

        public ICollection<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion

        private void PutAll(IDictionary<TKey, TValue> map)
        {
            var keys = map.Keys;
            foreach (var key in keys)
                Add(key, map[key]);
        }

        public TValue Remove2(TKey key)
        {
            TValue value;
            var success = TryGetValue(key, out value);
            if (success)
                Remove(key);

            return value;
        }
    }
}
