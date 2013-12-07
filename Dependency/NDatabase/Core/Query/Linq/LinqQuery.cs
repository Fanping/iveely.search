using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NDatabase.Api;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class LinqQuery<T> : ILinqQueryInternal<T>
    {
        private readonly IOdb _odb;
        private readonly IQueryBuilderRecord _record;

        public LinqQuery(IOdb odb)
        {
            if (odb == null)
                throw new ArgumentNullException("odb");

            _odb = odb;
            _record = NullQueryBuilderRecord.Instance;
        }

        public LinqQuery(LinqQuery<T> parent, IQueryBuilderRecord record)
        {
            _odb = parent._odb;
            _record = new CompositeQueryBuilderRecord(parent._record, record);
        }

        public int Count
        {
            get
            {
                var query = _odb.Query<T>();
                _record.Playback(query);

                return (int) query.Count();
            }
        }

        #region ILinqQueryInternal<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            var query = _odb.Query<T>();
            _record.Playback(query);
            return query.Execute<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> UnoptimizedThenBy<TKey>(Func<T, TKey> function)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<T> UnoptimizedThenByDescending<TKey>(Func<T, TKey> function)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<T> UnoptimizedWhere(Func<T, bool> func)
        {
            return _odb.Query<T>().Execute<T>().Where(func);
        }

        #endregion
    }
}