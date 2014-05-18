using Iveely.Data;
using Iveely.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.WaterfallTree
{
    public partial class WTree
    {
        public struct FullKey : IComparable<FullKey>, IEquatable<FullKey>
        {
            public readonly Locator Locator;
            public readonly IData Key;

            public FullKey(Locator locator, IData key)
            {
                Locator = locator;
                Key = key;
            }

            public override string ToString()
            {
                return String.Format("Locator = {0}, Key = {1}", Locator, Key);
            }

            #region IComparable<Locator> Members

            public int CompareTo(FullKey other)
            {
                int cmp = Locator.CompareTo(other.Locator);
                if (cmp != 0)
                    return cmp;

                return Locator.KeyComparer.Compare(Key, other.Key);
            }

            #endregion

            #region IEquatable<Locator> Members

            public override int GetHashCode()
            {
                return Locator.GetHashCode() ^ Key.GetHashCode();
            }

            public bool Equals(FullKey other)
            {
                if (!Locator.Equals(other.Locator))
                    return false;

                return Locator.KeyEqualityComparer.Equals(Key, other.Key);
            }

            #endregion
        }
    }
}
