using System.Collections;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class PlaceHolderQuery<T> : ILinqQuery<T>
    {
        private readonly IOdb _odb;

        public PlaceHolderQuery(IOdb odb)
        {
            _odb = odb;
        }

        public IOdb QueryFactory
        {
            get { return _odb; }
        }

        #region ILinqQuery<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            var query = _odb.Query<T>();
            return query.Execute<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}