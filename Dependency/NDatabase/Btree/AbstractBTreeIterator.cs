using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Exceptions;

namespace NDatabase.Btree
{
    internal abstract class AbstractBTreeIterator<T> : IterarorAdapter, IEnumerator<T>
    {
        private readonly IBTree _btree;
        private readonly OrderByConstants _orderByType;

        /// <summary>
        ///   The current node where the iterator is
        /// </summary>
        protected IBTreeNode CurrentNode;

        /// <summary>
        ///   The number of returned elements ; it may be different from the number of keys in the case f multileValues btree where a key can contain more than one value
        /// </summary>
        protected int NbReturnedElements;

        /// <summary>
        ///   The current key in the current node where the iterator is
        /// </summary>
        private int _currentKeyIndex;

        protected AbstractBTreeIterator(IBTree tree, OrderByConstants orderByType)
        {
            _btree = tree;
            CurrentNode = tree.GetRoot();
            _orderByType = orderByType;

            _currentKeyIndex = orderByType.IsOrderByDesc()
                                   ? CurrentNode.GetNbKeys()
                                   : 0;
        }

        #region IEnumerator<T> Members

        public override bool MoveNext()
        {
            return NbReturnedElements < _btree.GetSize();
        }

        public new virtual T Current
        {
            get
            {
                if (_currentKeyIndex > CurrentNode.GetNbKeys() || NbReturnedElements >= _btree.GetSize())
                    throw new OdbRuntimeException(NDatabaseError.NoMoreObjectsInCollection);

                return _orderByType.IsOrderByDesc()
                           ? NextDesc()
                           : NextAsc();
            }
        }

        public override void Reset()
        {
            CurrentNode = _btree.GetRoot();
            _currentKeyIndex = _orderByType.IsOrderByDesc()
                                   ? CurrentNode.GetNbKeys()
                                   : 0;
            NbReturnedElements = 0;
        }

        public virtual void Dispose()
        {
        }

        #endregion

        protected abstract object GetValueAt(IBTreeNode node, int currentIndex);

        protected override object GetCurrent()
        {
            return Current;
        }

        private T NextAsc()
        {
            // Try to go down till a leaf
            while (!CurrentNode.IsLeaf())
            {
                CurrentNode = CurrentNode.GetChildAt(_currentKeyIndex, true);
                _currentKeyIndex = 0;
            }

            // If leaf has more keys
            if (_currentKeyIndex < CurrentNode.GetNbKeys())
            {
                NbReturnedElements++;
                var nodeValue = GetValueAt(CurrentNode, _currentKeyIndex);
                _currentKeyIndex++;
                return (T) nodeValue;
            }

            // else go up till a node with keys
            while (_currentKeyIndex >= CurrentNode.GetNbKeys())
            {
                var child = CurrentNode;
                CurrentNode = CurrentNode.GetParent();
                _currentKeyIndex = IndexOfChild(CurrentNode, child);
            }

            NbReturnedElements++;
            
            var value = GetValueAt(CurrentNode, _currentKeyIndex);
            _currentKeyIndex++;
            return (T) value;
        }

        private T NextDesc()
        {
            // Try to go down till a leaf
            while (!CurrentNode.IsLeaf())
            {
                CurrentNode = CurrentNode.GetChildAt(_currentKeyIndex, true);
                _currentKeyIndex = CurrentNode.GetNbKeys();
            }

            // If leaf has more keys
            if (_currentKeyIndex > 0)
            {
                NbReturnedElements++;
                
                _currentKeyIndex--;
                var nodeValue = GetValueAt(CurrentNode, _currentKeyIndex);
                return (T) nodeValue;
            }

            // else go up till a node will keys
            while (_currentKeyIndex == 0)
            {
                var child = CurrentNode;
                CurrentNode = CurrentNode.GetParent();
                _currentKeyIndex = IndexOfChild(CurrentNode, child);
            }
            NbReturnedElements++;
            
            _currentKeyIndex--;
            var value = GetValueAt(CurrentNode, _currentKeyIndex);
            return (T) value;
        }

        private static int IndexOfChild(IBTreeNode parent, IBTreeNode child)
        {
            for (var i = 0; i < parent.GetNbChildren(); i++)
            {
                if (parent.GetChildAt(i, true).GetId().Equals(child.GetId()))
                    return i;
            }

            var errorMessage = string.Format("parent {0} does not have the specified child : {1}", parent, child);
            throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter(errorMessage));
        }
    }
}
