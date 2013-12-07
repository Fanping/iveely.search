using NDatabase.Api;
using NDatabase.Exceptions;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.List
{
    /// <summary>
    ///   A simple list to hold query result for Object Values API.
    /// </summary>
    /// <remarks>
    ///   A simple list to hold query result for Object Values API. It is used when no index and no order by is used and inMemory = true
    /// </remarks>
    internal sealed class SimpleListForValues : SimpleList<IObjectValues>, IInternalValues
    {
        public SimpleListForValues(int initialCapacity) : base(initialCapacity)
        {
        }

        #region IValues Members

        public IObjectValues NextValues()
        {
            return Next();
        }

        public override void AddWithKey(IOdbComparable key, IObjectValues @object)
        {
            throw new OdbRuntimeException(NDatabaseError.OperationNotImplemented.AddParameter("addWithKey"));
        }

        public new void AddOid(OID oid)
        {
            throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter("Add Oid not implemented "));
        }

        #endregion
    }
}
