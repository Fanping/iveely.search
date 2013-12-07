using NDatabase.Api;
using NDatabase.Meta;

namespace NDatabase.Cache
{
    internal interface IOdbCache
    {
        void AddObject(OID oid, object @object, ObjectInfoHeader objectInfoHeader);

        void StartInsertingObjectWithOid<T>(T plainObject, OID oid) where T : class;

        void UpdateIdOfInsertingObject<T>(T plainObject, OID oid) where T : class;

        void AddObjectInfoOfNonCommitedObject(ObjectInfoHeader objectInfoHeader);

        void RemoveObjectByOid(OID oid);

        void RemoveObject(object @object);

        bool Contains(object @object);

        object GetObject(OID oid);

        ObjectInfoHeader GetObjectInfoHeaderFromObject(object @object);

        ObjectInfoHeader GetObjectInfoHeaderByOid(OID oid, bool throwExceptionIfNotFound);

        OID GetOid(object @object);

        /// <summary>
        ///   To resolve uncommitted updates where the oid change and is not committed yet
        /// </summary>
        void SavePositionOfObjectWithOid(OID oid, long objectPosition);

        void MarkIdAsDeleted(OID oid);

        bool IsDeleted(OID oid);

        long GetObjectPositionByOid(OID oid);

        void ClearOnCommit();

        void Clear(bool setToNull);

        void ClearInsertingObjects();

        OID IdOfInsertingObject<T>(T plainObject) where T : class;

        bool IsInCommitedZone(OID oid);

        void AddOIDToUnconnectedZone(OID oid);
    }
}
