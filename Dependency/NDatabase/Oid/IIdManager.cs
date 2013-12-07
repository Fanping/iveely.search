using NDatabase.Api;

namespace NDatabase.Oid
{
    internal interface IIdManager
    {
        /// <summary>
        ///   Gets an id for an object (instance)
        /// </summary>
        /// <param name="objectPosition"> the object position (instance) </param>
        /// <returns> The id </returns>
        OID GetNextObjectId(long objectPosition);

        /// <summary>
        ///   Gets an id for a class
        /// </summary>
        /// <param name="objectPosition"> the object position (class) </param>
        /// <returns> The id </returns>
        OID GetNextClassId(long objectPosition);

        void UpdateObjectPositionForOid(OID oid, long objectPosition, bool writeInTransaction);

        void UpdateClassPositionForId(OID classId, long objectPosition, bool writeInTransaction);

        void UpdateIdStatus(OID id, byte newStatus);

        long GetObjectPositionWithOid(OID oid, bool useCache);

        void Clear();

        /// <summary>
        ///   To check if the id block must shift: that a new id block must be created
        /// </summary>
        /// <returns> a boolean value to check if block of id is full </returns>
        bool MustShift();
    }
}
