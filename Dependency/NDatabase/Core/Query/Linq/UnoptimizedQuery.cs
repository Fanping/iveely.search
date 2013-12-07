using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class UnoptimizedQuery<T> : ILinqQueryInternal<T>
    {
        private readonly IEnumerable<T> _result;

        public UnoptimizedQuery(IEnumerable<T> result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            _result = result;
        }

        public IEnumerable<T> Result
        {
            get { return _result; }
        }

        #region ILinqQueryInternal<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> UnoptimizedThenBy<TKey>(Func<T, TKey> function)
        {
            return ((IOrderedEnumerable<T>) _result).ThenBy(function);
        }

        public IEnumerable<T> UnoptimizedThenByDescending<TKey>(Func<T, TKey> function)
        {
            return ((IOrderedEnumerable<T>) _result).ThenByDescending(function);
        }

        public IEnumerable<T> UnoptimizedWhere(Func<T, bool> func)
        {
            return _result.Where(func);
        }

        #endregion
    }
}