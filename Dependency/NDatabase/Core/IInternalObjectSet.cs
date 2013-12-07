using NDatabase.Api;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core
{
    internal interface IInternalObjectSet<TItem> : IObjectSet<TItem>
    {
        /// <summary>
        ///   Add an object into the collection using a specific ordering key
        /// </summary>
        /// <param name="key"> </param>
        /// <param name="item"> The object can be an OID, can o NNOI (NonNativeObjectInfo) or the object </param>
        void AddWithKey(IOdbComparable key, TItem item);

        void AddOid(OID oid);
    }
}