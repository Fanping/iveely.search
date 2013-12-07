using NDatabase.Api;
using NDatabase.Meta;

namespace NDatabase.Cache
{
    /// <summary>
    ///   An interface for temporary cache
    /// </summary>
    internal interface IReadObjectsCache
    {
        NonNativeObjectInfo GetObjectInfoByOid(OID oid);

        bool IsReadingObjectInfoWithOid(OID oid);

        void StartReadingObjectInfoWithOid(OID oid, NonNativeObjectInfo objectInfo);

        void ClearObjectInfos();
    }
}
