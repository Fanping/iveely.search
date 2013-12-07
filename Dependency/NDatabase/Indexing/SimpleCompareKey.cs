using System;
using NDatabase.Api;

namespace NDatabase.Indexing
{
    /// <summary>
    ///   A simple compare key : an object that contains various values used for indexing query result <p></p>
    /// </summary>
    internal sealed class SimpleCompareKey : IOdbComparable
    {
        private readonly IComparable _key;

        public SimpleCompareKey(IComparable key)
        {
            _key = key;
        }

        public int CompareTo(object o)
        {
            if (o == null || !(o is SimpleCompareKey))
                return -1;

            var ckey = (SimpleCompareKey) o;

            if (_key == null && ckey._key == null)
                return 0;

            if (_key == null)
                return -1;

            return _key.CompareTo(ckey._key);
        }

        public override string ToString()
        {
            return _key.ToString();
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is SimpleCompareKey))
                return false;

            var ckey = (SimpleCompareKey) o;

            if (_key == null && ckey._key == null)
                return true;

            return _key != null && _key.Equals(ckey._key);
        }

        public override int GetHashCode()
        {
            return _key == null ? 0 : _key.GetHashCode();
        }
    }
}
