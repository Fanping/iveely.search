using System.Globalization;
using System.Linq;
using System.Text;
using NDatabase.Api;

namespace NDatabase.Oid
{
    internal sealed class DatabaseId : IDatabaseId
    {
        private readonly long[] _ids;

        internal DatabaseId(long[] ids)
        {
            _ids = ids;
        }

        #region IDatabaseId Members

        public long[] GetIds()
        {
            return _ids;
        }

        #endregion

        public override string ToString()
        {
            var buffer = new StringBuilder();

            for (var i = 0; i < _ids.Length; i++)
            {
                if (i != 0)
                    buffer.Append("-");

                buffer.Append(_ids[i].ToString(CultureInfo.InvariantCulture));
            }

            return buffer.ToString();
        }

        public override bool Equals(object @object)
        {
            if (@object == null || @object.GetType() != typeof (DatabaseId))
                return false;

            var dbId = (DatabaseId) @object;

            for (var i = 0; i < _ids.Length; i++)
            {
                if (_ids[i] != dbId._ids[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var sum = _ids.Sum(val => (val ^ (UrShift(val, 32))));

            return (int)(sum ^ (UrShift(sum, 32)));
        }

        private static long UrShift(long number, int bits)
        {
            if (number >= 0)
                return number >> bits;

            return (number >> bits) + (2L << ~bits);
        }
    }
}
