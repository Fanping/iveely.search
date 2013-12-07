using NDatabase.Api;
using NDatabase.Btree;
using NDatabase.Exceptions;

namespace NDatabase.Core.Query.List
{
    /// <summary>
    ///   An ordered Collection to hold values (not objects) based on a BTree implementation.
    /// </summary>
    /// <remarks>
    ///   An ordered Collection to hold values (not objects) based on a BTree implementation. It holds all values in memory.
    /// </remarks>
    internal sealed class InMemoryBTreeCollectionForValues : AbstractBTreeCollection<IObjectValues>, IInternalValues
    {
        public InMemoryBTreeCollectionForValues(OrderByConstants orderByType) : base(orderByType)
        {
        }

        #region IValues Members

        public IObjectValues NextValues()
        {
            return Next();
        }

        public new void AddOid(OID oid)
        {
            throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter("Add Oid not implemented "));
        }

        #endregion

        protected override IBTree BuildTree(int degree)
        {
            return new InMemoryBTreeMultipleValuesPerKey(degree);
        }
    }
}
