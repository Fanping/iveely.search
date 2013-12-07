using System.Collections;
using System.Collections.Generic;
using System.Text;
using NDatabase.Api;
using NDatabase.Btree;
using NDatabase.Exceptions;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.List
{
    /// <summary>
    ///   A collection that uses a BTree as an underlying system to provide ordered by Collections <p></p>
    /// </summary>
    internal abstract class AbstractBTreeCollection<TItem> : IInternalObjectSet<TItem>
    {
        private readonly OrderByConstants _orderByType;
        private readonly IBTree _tree;

        [NonPersistent]
        private IEnumerator<TItem> _currentIterator;

        private int _size;

        protected AbstractBTreeCollection(OrderByConstants orderByType)
        {
            // TODO compute degree best value for the size value
            _tree = BuildTree(OdbConfiguration.GetIndexBTreeDegree());
            _orderByType = orderByType;
        }

        #region IInternalObjectSet<TItem> Members

        /// <summary>
        ///   Adds the object in the btree with the specific key
        /// </summary>
        public virtual void AddWithKey(IOdbComparable key, TItem o)
        {
            _tree.Insert(key, o);
            _size++;
        }

        protected virtual IEnumerator<TItem> Iterator(OrderByConstants newOrderByType)
        {
            return (IEnumerator<TItem>) _tree.Iterator<TItem>(newOrderByType);
        }

        public void AddOid(OID oid)
        {
            throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter("Add Oid not implemented "));
        }

        public void Add(TItem o)
        {
            _tree.Insert(_size, o);
            _size++;
        }

        public virtual void Clear()
        {
            _tree.Clear();
        }

        public virtual bool Contains(TItem o)
        {
            throw new OdbRuntimeException(NDatabaseError.OperationNotImplemented.AddParameter("contains"));
        }

        public virtual IEnumerator<TItem> GetEnumerator()
        {
            return Iterator(_orderByType);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tree.Iterator<TItem>(_orderByType);
        }

        public virtual bool Remove(TItem o)
        {
            throw new OdbRuntimeException(NDatabaseError.OperationNotImplemented.AddParameter("remove"));
        }

        public virtual int Count
        {
            get { return _size; }
        }

        public virtual bool IsReadOnly
        {
            get { return true; }
        }

        public void CopyTo(TItem[] ee, int arrayIndex)
        {
            throw new OdbRuntimeException(NDatabaseError.OperationNotImplemented.AddParameter("CopyTo"));
        }

        #endregion

        #region IObjects<TItem> Members

        public virtual TItem GetFirst()
        {
            return Iterator(_orderByType).Current;
        }

        public virtual bool HasNext()
        {
            if (_currentIterator == null)
                _currentIterator = Iterator(_orderByType);
            return _currentIterator.MoveNext();
        }

        public virtual TItem Next()
        {
            if (_currentIterator == null)
                _currentIterator = Iterator(_orderByType);
            return _currentIterator.Current;
        }

        public virtual void Reset()
        {
            _currentIterator = Iterator(_orderByType);
        }

        #endregion

        protected abstract IBTree BuildTree(int degree);

        protected IBTree GetTree()
        {
            return _tree;
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("size=").Append(_size).Append(" [");
            var iterator = GetEnumerator();
            while (iterator.MoveNext())
            {
                s.Append(iterator.Current);
                if (iterator.MoveNext())
                    s.Append(" , ");
            }
            s.Append("]");
            return s.ToString();
        }
    }
}
