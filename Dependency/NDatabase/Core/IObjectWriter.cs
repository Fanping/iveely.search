using System;
using NDatabase.Api;
using NDatabase.Meta;
using NDatabase.Oid;
using NDatabase.Triggers;

namespace NDatabase.Core
{
    internal interface IObjectWriter : IDisposable
    {
        ClassInfoList AddClasses(ClassInfoList classInfoList);

        void UpdateClassInfo(ClassInfo classInfo, bool writeInTransaction);

        /// <param name="oid"> The Oid of the object to be inserted </param>
        /// <param name="nnoi"> The object meta representation The object to be inserted in the database </param>
        /// <param name="isNewObject"> To indicate if object is new </param>
        /// <returns> The position of the inserted object </returns>
        OID InsertNonNativeObject(OID oid, NonNativeObjectInfo nnoi, bool isNewObject);

        /// <summary>
        ///   Updates an object.
        /// </summary>
        /// <remarks>
        ///   Updates an object. Deletes the current object and creates a new at the end of the database file and updates
        ///                        OID object position.
        /// </remarks>
        /// <param name="nnoi"> The meta representation of the object to be updated </param>
        /// <param name="forceUpdate"> when true, no verification is done to check if update must be done. </param>
        /// <returns> The oid of the object, as a negative number </returns>
        OID UpdateNonNativeObjectInfo(NonNativeObjectInfo nnoi, bool forceUpdate);

        IIdManager GetIdManager();

        void Close();

        /// <summary>
        ///   Creates the header of the file
        /// </summary>
        /// <param name="creationDate"> The creation date </param>
        void CreateEmptyDatabaseHeader(long creationDate);

        OID Delete(ObjectInfoHeader header);

        /// <summary>
        ///   Updates the previous object position field of the object at objectPosition
        /// </summary>
        /// <param name="objectOID"> </param>
        /// <param name="previousObjectOID"> </param>
        /// <param name="writeInTransaction"> </param>
        void UpdatePreviousObjectFieldOfObjectInfo(OID objectOID, OID previousObjectOID, bool writeInTransaction);

        /// <summary>
        ///   Update next object oid field of the object at the specific position
        /// </summary>
        /// <param name="objectOID"> </param>
        /// <param name="nextObjectOID"> </param>
        /// <param name="writeInTransaction"> </param>
        void UpdateNextObjectFieldOfObjectInfo(OID objectOID, OID nextObjectOID, bool writeInTransaction);

        void AfterInit();

        void SetTriggerManager(IInternalTriggerManager triggerManager);

        /// <summary>
        ///   Mark a block as deleted
        /// </summary>
        /// <returns> The block size </returns>
        void MarkAsDeleted(long currentPosition, bool writeInTransaction);

        ClassInfo AddClass(ClassInfo newClassInfo, bool addDependentClasses);

        /// <summary>
        ///   Updates pointers of objects, Only changes uncommitted info pointers
        /// </summary>
        /// <param name="objectInfo"> The meta representation of the object being inserted </param>
        /// <param name="classInfo"> The class of the object being inserted </param>
        void ManageNewObjectPointers(NonNativeObjectInfo objectInfo, ClassInfo classInfo);

        /// <summary>
        ///   Store a meta representation of a native object(already as meta representation)in ODBFactory database.
        /// </summary>
        /// <remarks>
        ///   Store a meta representation of a native object(already as meta representation)in ODBFactory database. A Native object is an object that use native language type, String for example To detect if object must be updated or insert, we use the cache. To update an object, it must be first selected from the database. When an object is to be stored, if it exist in the cache, then it will be updated, else it will be inserted as a new object. If the object is null, the cache will be used to check if the meta representation is in the cache
        /// </remarks>
        /// <param name="noi"> The meta representation of an object </param>
        /// <returns> The object position @ </returns>
        long InternalStoreObject(NativeObjectInfo noi);

        /// <summary>
        ///   Insert the object in the index
        /// </summary>
        /// <param name="oid"> The object id </param>
        /// <param name="nnoi"> The object meta represenation </param>
        void ManageIndexesForInsert(OID oid, NonNativeObjectInfo nnoi);

        /// <summary>
        ///   Store a meta representation of an object(already as meta representation)in ODBFactory database.
        /// </summary>
        /// <remarks>
        ///   Store a meta representation of an object(already as meta representation)in ODBFactory database. To detect if object must be updated or insert, we use the cache. To update an object, it must be first selected from the database. When an object is to be stored, if it exist in the cache, then it will be updated, else it will be inserted as a new object. If the object is null, the cache will be used to check if the meta representation is in the cache
        /// </remarks>
        /// <param name="oid"> The oid of the object to be inserted/updates </param>
        /// <param name="nnoi"> The meta representation of an object </param>
        /// <returns> The object position </returns>
        OID StoreObject(OID oid, NonNativeObjectInfo nnoi);

        IFileSystemWriter FileSystemProcessor { get; }
    }
}
