using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Exceptions;
using NDatabase.Meta;

namespace NDatabase.Cache
{
    /// <summary>
    ///   A temporary cache of objects.
    /// </summary>
    internal sealed class ReadObjectsCache : IReadObjectsCache
    {
        /// <summary>
        ///   To resolve cyclic reference, keep track of objects being read
        /// </summary>
        private readonly IDictionary<OID, NonNativeObjectInfo> _readingObjectInfo;

        public ReadObjectsCache()
        {
            _readingObjectInfo = new Dictionary<OID, NonNativeObjectInfo>();
        }

        #region IReadObjectsCache Members

        public bool IsReadingObjectInfoWithOid(OID oid)
        {
            return oid != null && _readingObjectInfo.ContainsKey(oid);
        }

        public NonNativeObjectInfo GetObjectInfoByOid(OID oid)
        {
            if (oid == null)
                throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

            NonNativeObjectInfo value;
            _readingObjectInfo.TryGetValue(oid, out value);

            return value;
        }

        public void StartReadingObjectInfoWithOid(OID oid, NonNativeObjectInfo objectInfo)
        {
            if (oid == null)
                throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

            NonNativeObjectInfo nnoi;
            var success = _readingObjectInfo.TryGetValue(oid, out nnoi);

            if (!success)
                _readingObjectInfo[oid] = objectInfo;
        }

        public void ClearObjectInfos()
        {
            _readingObjectInfo.Clear();
        }

        #endregion
    }
}
