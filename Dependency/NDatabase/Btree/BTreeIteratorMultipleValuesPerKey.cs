using System.Collections;
using NDatabase.Api;

namespace NDatabase.Btree
{
    /// <summary>
    ///   An iterator to iterate over NDatabase BTree that accept more than one value per key.
    /// </summary>
    /// <remarks>
    ///   An iterator to iterate over NDatabase BTree that accept more than one value per key. This is used for non unique index and collection that return ordered by results
    /// </remarks>
    internal class BTreeIteratorMultipleValuesPerKey<T> : AbstractBTreeIterator<T>
    {
        /// <summary>
        ///   The index in the list of the current value, Here values of a key are lists!
        /// </summary>
        private int _currenListIndex;

        /// <summary>
        ///   The current value(List) of the current key being read.
        /// </summary>
        /// <remarks>
        ///   The current value(List) of the current key being read.
        /// </remarks>
        private IList _currentValue;

        /// <param name="tree"> </param>
        /// <param name="orderByType"> </param>
        public BTreeIteratorMultipleValuesPerKey(IBTree tree, OrderByConstants orderByType) : base(tree, orderByType)
        {
            _currenListIndex = 0;
            _currentValue = null;
        }

        public override T Current
        {
            get
            {
                // Here , the value of a specific key is a list, so we must iterate
                // through the list before going
                // to the next node
                if (CurrentNode != null && _currentValue != null)
                {
                    var listSize = _currentValue.Count;
                    if (listSize > _currenListIndex)
                    {
                        var value = _currentValue[_currenListIndex];
                        _currenListIndex++;
                        NbReturnedElements++;
                        return (T) value;
                    }
                    // We have reached the end of the list or the list is empty
                    // We must continue iterate in the current node / btree
                    _currenListIndex = 0;
                    _currentValue = null;
                }
                return base.Current;
            }
        }

        protected override object GetValueAt(IBTreeNode node, int currentIndex)
        {
            if (_currentValue == null)
                _currentValue = (IList) node.GetValueAsObjectAt(currentIndex);

            var listSize = _currentValue.Count;
            if (listSize > _currenListIndex)
            {
                var value = _currentValue[_currenListIndex];
                _currenListIndex++;
                return value;
            }

            // We have reached the end of the list or the list is empty
            // We must continue iterate in the current node / btree
            _currenListIndex = 0;
            _currentValue = null;

            return null;
        }
    }
}
