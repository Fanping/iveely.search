using NDatabase.Api;

namespace NDatabase.Btree
{
    /// <summary>
    ///   An iterator to iterate over NDatabase BTree.
    /// </summary>
    /// <remarks>
    ///   An iterator to iterate over NDatabase BTree.
    /// </remarks>
    internal sealed class BTreeIteratorSingleValuePerKey<T> : AbstractBTreeIterator<T>
    {
        public BTreeIteratorSingleValuePerKey(IBTree tree, OrderByConstants orderByType) : base(tree, orderByType)
        {
        }

        protected override object GetValueAt(IBTreeNode node, int currentIndex)
        {
            var n = (IBTreeNodeOneValuePerKey) node;
            return n.GetValueAt(currentIndex);
        }
    }
}
