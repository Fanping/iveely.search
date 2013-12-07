using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NDatabase.Api;
using NDatabase.Api.Triggers;
using NDatabase.Container;
using NDatabase.Core.Query.Criteria;
using NDatabase.Core.Session;
using NDatabase.Exceptions;
using NDatabase.IO;
using NDatabase.Meta;
using NDatabase.Meta.Introspector;
using NDatabase.Oid;
using NDatabase.Services;
using NDatabase.Storage;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;
using NDatabase.Triggers;

namespace NDatabase.Core.Engine
{
    /// <summary>
    ///   The storage Engine. The Local Storage Engine class in the most important class in ODB.
    /// </summary>
    /// <remarks>
    ///   The storage Engine <pre>The Local Storage Engine class in the most important class in ODB. It manages reading, writing and querying objects.
    ///                        All write operations are delegated to the ObjectWriter class.
    ///                        All read operations are delegated to the ObjectReader class.
    ///                        All Id operations are delegated to the IdManager class.
    ///                        All Introspecting operations are delegated to the ObjectIntrospector class.
    ///                        All Trigger operations are delegated to the TriggerManager class.
    ///                        All session related operations are executed by The Session class. Session Class using the Transaction
    ///                        class are responsible for ACID behavior.</pre>
    /// </remarks>
    internal sealed class StorageEngine : AbstractStorageEngineReader
    {
        private readonly IOdbList<ICommitListener> _commitListeners = new OdbList<ICommitListener>();

        /// <summary>
        ///   This is a visitor used to execute some specific action(like calling 'Before Insert Trigger') when introspecting an object
        /// </summary>
        private readonly IIntrospectionCallback _introspectionCallbackForInsert;

        /// <summary>
        ///   This is a visitor used to execute some specific action when introspecting an object
        /// </summary>
        private readonly IIntrospectionCallback _introspectionCallbackForUpdate;

        private readonly IObjectWriter _objectWriter;
        private readonly IInternalTriggerManager _triggerManager;
        private CurrentIdBlockInfo _currentIdBlockInfo = new CurrentIdBlockInfo();

        private IDatabaseId _databaseId;
        private IObjectIntrospector _objectIntrospector;
        private readonly ISession _session;
        private readonly SessionDataProvider _objectIntrospectionDataProvider;

        private readonly IReflectionService _reflectionService;

        /// <summary>
        ///   The database file name
        /// </summary>
        public StorageEngine(IDbIdentification parameters)
        {
            _reflectionService = DependencyContainer.Resolve<IReflectionService>();

            try
            {
                var metaModelCompabilityChecker = DependencyContainer.Resolve<IMetaModelCompabilityChecker>();

                DbIdentification = parameters;
                IsDbClosed = false;

                var isNewDatabase = DbIdentification.IsNew();

                _session = DependencyContainer.Resolve<ISession>(this);

                // Object Writer must be created before object Reader
                _objectWriter = DependencyContainer.Resolve<IObjectWriter>(this);
                ObjectReader = DependencyContainer.Resolve<IObjectReader>(this);

                // Associate current session to the fsi -> all transaction writes
                // will be applied to this FileSystemInterface
                _session.SetFileSystemInterfaceToApplyTransaction(_objectWriter.FileSystemProcessor.FileSystemInterface);

                _objectIntrospectionDataProvider = new SessionDataProvider(_session);

                if (isNewDatabase)
                {
                    _objectWriter.CreateEmptyDatabaseHeader(OdbTime.GetCurrentTimeInTicks());
                }
                else
                {
                    GetObjectReader().ReadDatabaseHeader();
                }
                _objectWriter.AfterInit();
                _objectIntrospector = new ObjectIntrospector(GetClassInfoProvider());
                _triggerManager = GetLocalTriggerManager();
                // This forces the initialization of the meta model
                var metaModel = GetMetaModel();

                var shouldUpdate = metaModelCompabilityChecker.Check(ClassIntrospector.Instrospect(metaModel.GetAllClasses()), GetMetaModel());

                if (shouldUpdate)
                    UpdateMetaModel();

                _objectWriter.SetTriggerManager(_triggerManager);
                _introspectionCallbackForInsert = new InstrumentationCallbackForStore(_triggerManager, false);
                _introspectionCallbackForUpdate = new InstrumentationCallbackForStore(_triggerManager, true);
            }
            catch
            {
                if (parameters is FileIdentification)
                    Monitor.Exit(string.Intern(Path.GetFullPath(parameters.FileName)));
                throw;
            }
        }

        public override IObjectIntrospectionDataProvider GetClassInfoProvider()
        {
            return _objectIntrospectionDataProvider;
        }

        public override OID Store<T>(T plainObject)
        {
            if (IsDbClosed)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbIsClosed.AddParameter(DbIdentification.Id));
            }

            var newOid = InternalStore(null, plainObject);
            GetSession().GetCache().ClearInsertingObjects();

            return newOid;
        }

        public override OID Store<T>(OID oid, T plainObject)
        {
            if (IsDbClosed)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbIsClosed.AddParameter(DbIdentification.Id));
            }

            var newOid = InternalStore(oid, plainObject);
            GetSession().GetCache().ClearInsertingObjects();

            return newOid;
        }

        /// <summary>
        ///   Warning,
        /// </summary>
        public override void DeleteObjectWithOid(OID oid)
        {
            var cache = GetSession().GetCache();
            // Check if oih is in the cache
            var objectInfoHeader = cache.GetObjectInfoHeaderByOid(oid, false) ??
                                   ObjectReader.ReadObjectInfoHeaderFromOid(oid, true);

            _objectWriter.Delete(objectInfoHeader);
            // removes the object from the cache
            cache.RemoveObjectByOid(objectInfoHeader.GetOid());
        }

        /// <summary>
        ///   Actually deletes an object database
        /// </summary>
        public override OID Delete<T>(T plainObject)
        {
            var lsession = GetSession();
            if (lsession.IsRollbacked())
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbHasBeenRollbacked.AddParameter(DbIdentification.ToString()));
            }

            var cache = lsession.GetCache();

            // Get header of the object (position, previous object position, next
            // object position and class info position)
            // Header must come from cache because it may have been updated before.
            var header = cache.GetObjectInfoHeaderFromObject(plainObject);
            if (header == null)
            {
                var cachedOid = cache.GetOid(plainObject);

                if (cachedOid == null)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.ObjectDoesNotExistInCacheForDelete.AddParameter(plainObject.GetType().FullName).
                            AddParameter(plainObject.ToString()));
                }

                header = ObjectReader.ReadObjectInfoHeaderFromOid(cachedOid, false);
            }

            _triggerManager.ManageDeleteTriggerBefore(plainObject.GetType(), plainObject, header.GetOid());

            CascadeDelete(plainObject);

            var oid = _objectWriter.Delete(header);
            _triggerManager.ManageDeleteTriggerAfter(plainObject.GetType(), plainObject, oid);
            
            cache.RemoveObjectByOid(header.GetOid());

            return oid;
        }

        private void CascadeDelete<T>(T plainObject)
        {
            var fields = _reflectionService.GetFields(plainObject.GetType());

            fields = (from fieldInfo in fields
                      let attributes = fieldInfo.GetCustomAttributes(true)
                      where attributes.OfType<CascadeDeleteAttribute>().Any()
                      select fieldInfo).ToList();

            foreach (var fieldInfo in fields)
                Delete(fieldInfo.GetValue(plainObject));
        }

        public override IIdManager GetIdManager()
        {
            return new IdManager(GetObjectWriter(), GetObjectReader(), _currentIdBlockInfo);
        }

        public override void Close()
        {
            Commit();

            var lsession = GetSession();
            _objectWriter.FileSystemProcessor.WriteLastOdbCloseStatus(true, false);

            _objectWriter.FileSystemProcessor.Flush();
            if (lsession.TransactionIsPending())
                throw new OdbRuntimeException(NDatabaseError.TransactionIsPending.AddParameter(lsession.GetId()));

            IsDbClosed = true;
            ObjectReader.Close();
            _objectWriter.Close();
            
            lsession.Close();

            if (_objectIntrospector != null)
            {
                _objectIntrospector.Clear();
                _objectIntrospector = null;
            }

            // remove trigger manager
            RemoveLocalTriggerManager();
        }

        public override IObjectReader GetObjectReader()
        {
            return ObjectReader;
        }

        public override IObjectWriter GetObjectWriter()
        {
            return _objectWriter;
        }

        public override void Commit()
        {
            if (IsClosed())
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbIsClosed.AddParameter(DbIdentification.Id));
            }

            GetSession().Commit();
            _objectWriter.FileSystemProcessor.Flush();
        }

        public override void Rollback()
        {
            GetSession().Rollback();
        }

        public override OID GetObjectId<T>(T plainObject, bool throwExceptionIfDoesNotExist)
        {
            if (plainObject != null)
            {
                var oid = GetSession().GetCache().GetOid(plainObject);

                if (oid == null && throwExceptionIfDoesNotExist)
                    throw new OdbRuntimeException(
                        NDatabaseError.UnknownObjectToGetOid.AddParameter(plainObject.ToString()));

                return oid;
            }

            throw new OdbRuntimeException(NDatabaseError.OdbCanNotReturnOidOfNullObject);
        }

        public override object GetObjectFromOid(OID oid)
        {
            if (oid == null)
                throw new OdbRuntimeException(NDatabaseError.CanNotGetObjectFromNullOid);

            var nnoi = GetObjectReader().ReadNonNativeObjectInfoFromOid(null, oid, true, true);

            if (nnoi.IsDeletedObject())
                throw new OdbRuntimeException(NDatabaseError.ObjectIsMarkedAsDeletedForOid.AddParameter(oid));

            var objectFromOid = nnoi.GetObject() ??
                                GetObjectReader().GetInstanceBuilder().BuildOneInstance(nnoi,
                                                                                        GetSession().GetCache());

            var lsession = GetSession();
            // Here oid can be different from nnoi.getOid(). This is the case when
            // the oid is an external oid. That`s why we use
            // nnoi.getOid() to put in the cache
            lsession.GetCache().AddObject(nnoi.GetOid(), objectFromOid, nnoi.GetHeader());
            lsession.GetTmpCache().ClearObjectInfos();

            return objectFromOid;
        }

        public override ObjectInfoHeader GetObjectInfoHeaderFromOid(OID oid)
        {
            return GetObjectReader().ReadObjectInfoHeaderFromOid(oid, true);
        }

        public override IList<long> GetAllObjectIds()
        {
            return ObjectReader.GetAllIds(IdTypes.Object);
        }

        public override void SetDatabaseId(IDatabaseId databaseId)
        {
            _databaseId = databaseId;
        }

        public override void SetCurrentIdBlockInfos(CurrentIdBlockInfo currentIdBlockInfo)
        {
            _currentIdBlockInfo = currentIdBlockInfo;
        }

        public override IDatabaseId GetDatabaseId()
        {
            return _databaseId;
        }

        public override bool IsClosed()
        {
            return IsDbClosed;
        }

        public override IDbIdentification GetBaseIdentification()
        {
            return DbIdentification;
        }

        public override IValues GetValues(IInternalValuesQuery query, int startIndex, int endIndex)
        {
            if (IsDbClosed)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbIsClosed.AddParameter(DbIdentification.Id));
            }

            return ObjectReader.GetValues(query, startIndex, endIndex);
        }

        public override void AddCommitListener(ICommitListener commitListener)
        {
            _commitListeners.Add(commitListener);
        }

        public override IOdbList<ICommitListener> GetCommitListeners()
        {
            return _commitListeners;
        }

        public override IRefactorManager GetRefactorManager()
        {
            return new RefactorManager(GetSession().GetMetaModel(), GetObjectWriter());
        }

        public override void ResetCommitListeners()
        {
            _commitListeners.Clear();
        }

        public override void Disconnect<T>(T plainObject)
        {
            GetSession().RemoveObjectFromCache(plainObject);
        }

        public override IInternalTriggerManager GetTriggerManager()
        {
            return _triggerManager;
        }

        public override void AddDeleteTriggerFor(Type type, DeleteTrigger trigger)
        {
            _triggerManager.AddDeleteTriggerFor(type, trigger);
        }

        public override void AddInsertTriggerFor(Type type, InsertTrigger trigger)
        {
            _triggerManager.AddInsertTriggerFor(type, trigger);
        }

        public override void AddSelectTriggerFor(Type type, SelectTrigger trigger)
        {
            _triggerManager.AddSelectTriggerFor(type, trigger);
        }

        public override void AddUpdateTriggerFor(Type type, UpdateTrigger trigger)
        {
            _triggerManager.AddUpdateTriggerFor(type, trigger);
        }

        public override ISession GetSession()
        {
            return _session;
        }

        public override void DefragmentTo(string newFileName)
        {
            var start = OdbTime.GetCurrentTimeInMs();
            var totalNbObjects = 0L;

            var newStorageEngine = new StorageEngine(new FileIdentification(newFileName));
            var j = 0;

            var criteriaQuery = new SodaQuery(typeof(object));
            var defragObjects = GetObjects<object>(criteriaQuery, true, -1, -1);

            foreach (var defragObject in defragObjects)
            {
                newStorageEngine.Store(defragObject);
                totalNbObjects++;

                if (OdbConfiguration.IsLoggingEnabled())
                {
                    if (j % 10000 == 0)
                        DLogger.Info(string.Concat("\nStorageEngine: ", totalNbObjects.ToString(), " objects saved."));
                }

                j++;
            }

            newStorageEngine.Close();

            var time = OdbTime.GetCurrentTimeInMs() - start;

            if (!OdbConfiguration.IsLoggingEnabled())
                return;

            var nbObjectsAsString = totalNbObjects.ToString();
            var timeAsString = time.ToString();

            DLogger.Info(string.Format("StorageEngine: New storage {0} created with {1} objects in {2} ms.", newFileName,
                                       nbObjectsAsString, timeAsString));
        }

        private void UpdateMetaModel()
        {
            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Info("StorageEngine: Automatic refactoring : updating meta model");

            var metaModel = GetMetaModel();

            var storedClasses = metaModel.GetAllClasses().ToList();

            foreach (var userClass in storedClasses)
                _objectWriter.UpdateClassInfo(userClass, true);
        }

        /// <summary>
        ///   Store an object with the specific id
        /// </summary>
        /// <param name="oid"> </param>
        /// <param name="plainObject"> </param>
        private OID InternalStore<T>(OID oid, T plainObject) where T : class
        {
            if (GetSession().IsRollbacked())
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbHasBeenRollbacked.AddParameter(GetBaseIdentification().ToString()));
            }

            if (plainObject == null)
                throw new OdbRuntimeException(NDatabaseError.OdbCanNotStoreNullObject);

            var type = typeof (T);
            if (OdbType.IsNative(type))
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbCanNotStoreNativeObjectDirectly.AddParameter(type.FullName).AddParameter(
                        OdbType.GetFromClass(type).Name).AddParameter(type.FullName));
            }

            // first detects if we must perform an insert or an update
            // If object is in the cache, we must perform an update, else an insert
            var cache = GetSession().GetCache();

            var cacheOid = cache.IdOfInsertingObject(plainObject);
            if (cacheOid != null)
                return cacheOid;

            // throw new ODBRuntimeException("Inserting meta representation of
            // an object without the object itself is not yet supported");
            var mustUpdate = cache.Contains(plainObject);

            // The introspection callback is used to execute some specific task (like calling trigger, for example) while introspecting the object
            var callback = _introspectionCallbackForInsert;
            if (mustUpdate)
                callback = _introspectionCallbackForUpdate;

            // Transform the object into an ObjectInfo
            var nnoi =
                (NonNativeObjectInfo)
                _objectIntrospector.GetMetaRepresentation(plainObject, true, null, callback);

            // During the introspection process, if object is to be updated, then the oid has been set
            mustUpdate = nnoi.GetOid() != null;

            return mustUpdate
                       ? _objectWriter.UpdateNonNativeObjectInfo(nnoi, false)
                       : _objectWriter.InsertNonNativeObject(oid, nnoi, true);
        }

        /// <summary>
        ///   Returns a string of the meta-model
        /// </summary>
        /// <returns> The engine description </returns>
        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append(GetMetaModel());
            return buffer.ToString();
        }
    }
}