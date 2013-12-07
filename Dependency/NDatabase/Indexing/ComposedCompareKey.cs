using System.Text;
using NDatabase.Api;

namespace NDatabase.Indexing
{
    /// <summary>
    ///   A composed key : an object that contains various values used for indexing query result 
    ///   <p>This is an implementation that allows compare keys to contain more than one single value to be compared</p>
    /// </summary>
    internal sealed class ComposedCompareKey : IOdbComparable
    {
        private readonly IOdbComparable[] _keys;

        public ComposedCompareKey(IOdbComparable[] keys)
        {
            _keys = keys;
        }

        public int CompareTo(object o)
        {
            if (o == null || o.GetType() != typeof (ComposedCompareKey))
                return -1;
            var ckey = (ComposedCompareKey) o;

            for (var i = 0; i < _keys.Length; i++)
            {
                var key = _keys[i];

                if (key == null && ckey._keys[i] == null)
                    continue;

                if (key == null)
                    return -1;

                var result = key.CompareTo(ckey._keys[i]);
                if (result != 0)
                    return result;
            }

            return 0;
        }

        public override string ToString()
        {
            if (_keys == null)
                return "no keys defined";

            var buffer = new StringBuilder();
            for (var i = 0; i < _keys.Length; i++)
            {
                if (i != 0)
                    buffer.Append("|");

                buffer.Append(_keys[i]);
            }

            return buffer.ToString();
        }
    }
}
