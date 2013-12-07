using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Exceptions;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.List
{
    /// <summary>
    ///   A simple list to hold query result.
    /// </summary>
    /// <remarks>
    ///   A simple list to hold query result. It is used when no index and no order by is used and inMemory = true
    /// </remarks>
    internal class SimpleList<TItem> : List<TItem>, IInternalObjectSet<TItem>
    {
        private int _currentPosition;

        public SimpleList()
        {
        }

        protected SimpleList(int initialCapacity) : base(initialCapacity)
        {
        }

        #region IObjects<E> Members

        public virtual void AddWithKey(IOdbComparable key, TItem o)
        {
            Add(o);
        }

        public virtual TItem GetFirst()
        {
            return Count == 0 ? default(TItem) : this[0];
        }

        public virtual bool HasNext()
        {
            return _currentPosition < Count;
        }

        public TItem Next()
        {
            return !HasNext() 
                ? default(TItem) 
                : this[_currentPosition++];
        }

        public virtual void Reset()
        {
            _currentPosition = 0;
        }

        public void AddOid(OID oid)
        {
            throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter("Add Oid not implemented "));
        }

        #endregion
    }
}
