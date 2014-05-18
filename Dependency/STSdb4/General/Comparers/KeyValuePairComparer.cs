using Iveely.General.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.General.Comparers
{
    public class KeyValuePairComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
    {
        public static readonly KeyValuePairComparer<TKey, TValue> Instance = new KeyValuePairComparer<TKey, TValue>(Comparer<TKey>.Default);

        public IComparer<TKey> Comparer { get; private set; }

        public KeyValuePairComparer(IComparer<TKey> comparer)
        {
            Comparer = comparer;
        }

        public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            return Comparer.Compare(x.Key, y.Key);
        }
    }
}
