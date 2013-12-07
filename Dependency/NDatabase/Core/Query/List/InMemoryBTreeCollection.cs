using NDatabase.Api;
using NDatabase.Btree;

namespace NDatabase.Core.Query.List
{
    /// <summary>
    ///   An implementation of an ordered Collection based on a BTree implementation that holds all objects in memory
    /// </summary>
    internal sealed class InMemoryBTreeCollection<T> : AbstractBTreeCollection<T>
    {
        public InMemoryBTreeCollection(OrderByConstants orderByType) : base(orderByType)
        {
        }

        protected override IBTree BuildTree(int degree)
        {
            return new InMemoryBTreeMultipleValuesPerKey(degree);
        }
    }
}
