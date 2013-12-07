using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Core.Session;
using NDatabase.IO;
using NDatabase.Meta;
using NDatabase.Oid;
using NDatabase.Services;
using NDatabase.Storage;
using NDatabase.Tool.Wrappers;
using NDatabase.Triggers;

namespace NDatabase.Core
{
    /// <summary>
    ///   The interface of all that a StorageEngine (Main concept in ODB) must do.
    /// </summary>
    internal interface IStorageEngine : ITriggersEngine, IClassInfoProvider, IQueryEngine
    {
        OID Store<T>(OID oid, T plainObject) where T : class;

        /// <summary>
        ///   Store an object in an database.
        /// </summary>
        /// <remarks>
        ///   Store an object in an database. To detect if object must be updated or insert, we use the cache. To update an object, it must be first selected from the database. When an object is to be stored, if it exist in the cache, then it will be updated, else it will be inserted as a new object. If the object is null, the cache will be used to check if the meta representation is in the cache
        /// </remarks>
        OID Store<T>(T plainObject) where T : class;

        void DeleteObjectWithOid(OID oid);

        OID Delete<T>(T plainObject) where T : class;

        void Close();

        IObjectReader GetObjectReader();

        IObjectWriter GetObjectWriter();

        IInternalTriggerManager GetTriggerManager();

        ISession GetSession();

        void Commit();

        void Rollback();

        ObjectInfoHeader GetObjectInfoHeaderFromOid(OID oid);

        void DefragmentTo(string newFileName);

        IList<long> GetAllObjectIds();

        bool IsClosed();

        void SetDatabaseId(IDatabaseId databaseId);

        void SetCurrentIdBlockInfos(CurrentIdBlockInfo currentIdBlockInfo);

        IDbIdentification GetBaseIdentification();

        /// <param name="className"> The class name on which the index must be created </param>
        /// <param name="name"> The name of the index </param>
        /// <param name="indexFields"> The list of fields of the index </param>
        /// <param name="acceptMultipleValuesForSameKey"> </param>
        void AddIndexOn(string className, string name, string[] indexFields, bool acceptMultipleValuesForSameKey);

        void AddCommitListener(ICommitListener commitListener);

        IOdbList<ICommitListener> GetCommitListeners();

        /// <summary>
        ///   Returns the object used to refactor the database
        /// </summary>
        IRefactorManager GetRefactorManager();

        void ResetCommitListeners();

        IDatabaseId GetDatabaseId();

        /// <summary>
        ///   Used to disconnect the object from the current session.
        /// </summary>
        /// <remarks>
        ///   Used to disconnect the object from the current session. The object is removed from the cache
        /// </remarks>
        void Disconnect<T>(T plainObject) where T : class;

        void RebuildIndex(string className, string indexName);

        void DeleteIndex(string className, string indexName);

        IIdManager GetIdManager();

        IInternalTriggerManager GetLocalTriggerManager();
    }
}
