using System;
using System.Text;

namespace NDatabase.Btree
{
    internal sealed class KeyAndValue : IKeyAndValue
    {
        private readonly IComparable _key;
        private readonly object _value;

        public KeyAndValue(IComparable key, object value)
        {
            _key = key;
            _value = value;
        }

        #region IKeyAndValue Members

        public override string ToString()
        {
            return new StringBuilder("(").Append(_key).Append("=").Append(_value).Append(") ").ToString();
        }

        public IComparable GetKey()
        {
            return _key;
        }

        public object GetValue()
        {
            return _value;
        }

        #endregion
    }
}
