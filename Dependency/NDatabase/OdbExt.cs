using NDatabase.Api;
using NDatabase.Core;
using NDatabase.Exceptions;
using NDatabase.Oid;

namespace NDatabase
{
    internal sealed class OdbExt : IOdbExt
    {
        private readonly IStorageEngine _storageEngine;

        internal OdbExt(IStorageEngine storageEngine)
        {
            _storageEngine = storageEngine;
        }

        #region IOdbExt Members

        public IExternalOID ConvertToExternalOID(OID oid)
        {
            return new ExternalObjectOID(oid, _storageEngine.GetDatabaseId());
        }

        public IDatabaseId GetDatabaseId()
        {
            return _storageEngine.GetDatabaseId();
        }

        public IExternalOID GetObjectExternalOID<T>(T plainObject) where T : class 
        {
            return ConvertToExternalOID(_storageEngine.GetObjectId(plainObject, true));
        }

        public int GetObjectVersion(OID oid)
        {
            var objectInfoHeader = _storageEngine.GetObjectInfoHeaderFromOid(oid);
            if (objectInfoHeader == null)
                throw new OdbRuntimeException(NDatabaseError.ObjectWithOidDoesNotExistInCache.AddParameter(oid));

            return objectInfoHeader.GetObjectVersion();
        }

        public long GetObjectCreationDate(OID oid)
        {
            var objectInfoHeader = _storageEngine.GetObjectInfoHeaderFromOid(oid);
            if (objectInfoHeader == null)
                throw new OdbRuntimeException(NDatabaseError.ObjectWithOidDoesNotExistInCache.AddParameter(oid));

            return objectInfoHeader.GetCreationDate();
        }

        public long GetObjectUpdateDate(OID oid)
        {
            var objectInfoHeader = _storageEngine.GetObjectInfoHeaderFromOid(oid);
            if (objectInfoHeader == null)
                throw new OdbRuntimeException(NDatabaseError.ObjectWithOidDoesNotExistInCache.AddParameter(oid));

            return objectInfoHeader.GetUpdateDate();
        }

        public string GetDbId()
        {
            return _storageEngine.GetBaseIdentification().Id;
        }

        #endregion
    }
}
