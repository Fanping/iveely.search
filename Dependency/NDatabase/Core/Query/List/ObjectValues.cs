using System.Text;
using NDatabase.Api;
using NDatabase.Exceptions;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.List
{
    internal sealed class ObjectValues : IObjectValues
    {
        /// <summary>
        ///   key=alias,value=value
        /// </summary>
        private readonly OdbHashMap<string, object> _valuesByAlias;

        private readonly object[] _valuesByIndex;

        public ObjectValues(int size)
        {
            _valuesByIndex = new object[size];
            _valuesByAlias = new OdbHashMap<string, object>();
        }

        #region IObjectValues Members

        public object GetByAlias(string alias)
        {
            var valueByAlias = _valuesByAlias[alias];

            if (valueByAlias == null && !_valuesByAlias.ContainsKey(alias))
            {
                throw new OdbRuntimeException(
                    NDatabaseError.ValuesQueryAliasDoesNotExist.AddParameter(alias).AddParameter(_valuesByAlias.Keys));
            }

            return valueByAlias;
        }

        public object GetByIndex(int index)
        {
            return _valuesByIndex[index];
        }

        public object[] GetValues()
        {
            return _valuesByIndex;
        }

        #endregion

        public void Set(int index, string alias, object value)
        {
            _valuesByIndex[index] = value;
            _valuesByAlias.Add(alias, value);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            foreach (var alias in _valuesByAlias.Keys)
            {
                var @object = _valuesByAlias[alias];
                buffer.Append(alias).Append("=").Append(@object).Append(",");
            }

            return buffer.ToString();
        }
    }
}
