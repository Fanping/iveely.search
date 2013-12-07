using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Meta;

namespace NDatabase.Core.Engine
{
    internal interface IFileSystemReader
    {
        void ReadDatabaseHeader();

        /// <summary>
        ///   Reads the number of classes in database file
        /// </summary>
        long ReadNumberOfClasses();

        /// <summary>
        ///   Reads the first class OID
        /// </summary>
        long ReadFirstClassOid();

        object ReadAtomicNativeObjectInfoAsObject(int odbTypeId);

        /// <summary>
        ///   Returns information about all OIDs of the database
        /// </summary>
        /// <param name="idType"> </param>
        /// <returns> @ </returns>
        IList<long> GetAllIds(byte idType);

        /// <summary>
        ///   Gets the real object position from its OID
        /// </summary>
        /// <param name="oid"> The oid of the object to get the position </param>
        /// <param name="useCache"> </param>
        /// <param name="throwException"> To indicate if an exception must be thrown if object is not found </param>
        /// <returns> The object position, if object has been marked as deleted then return StorageEngineConstant.DELETED_OBJECT_POSITION @ </returns>
        long GetObjectPositionFromItsOid(OID oid, bool useCache, bool throwException);

        /// <summary>
        ///   Read the class info header with the specific oid
        /// </summary>
        /// <returns> The read class info object @ </returns>
        ClassInfo ReadClassInfoHeader(OID classInfoOid);

        long ReadOidPosition(OID oid);
        void Close();
    }
}