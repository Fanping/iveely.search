using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.General.Comparers
{
    public class ComparerInvertor<T> : IComparer<T>
    {
        public readonly IComparer<T> Comparer;

        public ComparerInvertor(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return -Comparer.Compare(x, y);
        }
    }
}
