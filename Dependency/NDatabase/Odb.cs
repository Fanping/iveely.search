using System.IO;
using System.Threading;
using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Api.Triggers;
using NDatabase.Core;
using NDatabase.Core.BTree;
using NDatabase.Core.Engine;
using NDatabase.Core.Query;
using NDatabase.Core.Query.Criteria;
using NDatabase.Core.Query.Linq;
using NDatabase.Core.Query.Values;
using NDatabase.IO;
using NDatabase.Meta;
using NDatabase.Meta.Introspector;
using NDatabase.Triggers;

namespace NDatabase
{
    /// <summary>
    ///   A basic adapter for ODB interface
    /// </summary>
    internal sealed class Odb : IOdb, IOdbForTrigger, IClassInfoProvider
    {
        private readonly IStorageEngine _storageEngine;
        private IOdbExt _ext;

        /// <summary>
        ///   protected Constructor
        /// </summary>
        private Odb(string fileName)
            : this(new StorageEngine(new FileIdentification(fileName)))
        {
        }

        private Odb()
            : this(new StorageEngine(new InMemoryIdentification()))
        {
        }

        internal static Odb GetInstance(string fileName)
        {
            var odb = new Odb(fileName);
            odb.TriggerManagerFor<object>().AddSelectTrigger(new EnrichWithOidTrigger());

            return odb;
        }

        internal static Odb GetInMemoryInstance()
        {
            var odb = new Odb();
            odb.TriggerManagerFor<object>().AddSelectTrigger(new EnrichWithOidTrigger());

            return odb;
        }

        internal Odb(IStorageEngine storageEngine)
        {
            _storageEngine = storageEngine;
        }

        #region IOdb Members

        public void Commit()
        {
            _storageEngine.Commit();
        }

        public void Rollback()
        {
            _storageEngine.Rollback();
        }

        public OID Store<T>(T plainObject) where T : class
        {
            return _storageEngine.Store(plainObject);
        }

        public IObjectSet<T> QueryAndExecute<T>()
        {
            return Query<T>().Execute<T>();
        }

        public IQuery Query<T>()
        {
            var criteriaQuery = new SodaQuery(typeof(T));
            ((IInternalQuery)criteriaQuery).SetQueryEngine(_storageEngine);
            return criteriaQuery;
        }

        public IValuesQuery ValuesQuery<T>() where T : class
        {
            var criteriaQuery = new ValuesCriteriaQuery(typeof(T));
            ((IInternalQuery)criteriaQuery).SetQueryEngine(_storageEngine);
            return criteriaQuery;
        }

        public IValuesQuery ValuesQuery<T>(OID oid) where T : class
        {
            var criteriaQuery = new ValuesCriteriaQuery(typeof(T), oid);
            ((IInternalQuery)criteriaQuery).SetQueryEngine(_storageEngine);
            return criteriaQuery;
        }

        public ILinqQueryable<T> AsQueryable<T>()
        {
            if (typeof(T) == typeof(object)) 
                return new PlaceHolderQuery<T>(this).AsQueryable();

            var linqQuery = new LinqQuery<T>(this);
            return linqQuery.AsQueryable();
        }

        public IValues GetValues(IValuesQuery query)
        {
            return _storageEngine.GetValues((IInternalValuesQuery) query, -1, -1);
        }

        public void Close()
        {
            _storageEngine.Close();
        }

        public OID Delete<T>(T plainObject) where T : class
        {
            return _storageEngine.Delete(plainObject);
        }

        /// <summary>
        ///   Delete an object from the database with the id
        /// </summary>
        /// <param name="oid"> The object id to be deleted </param>
        public void DeleteObjectWithId(OID oid)
        {
            _storageEngine.DeleteObjectWithOid(oid);
        }

        public OID GetObjectId<T>(T plainObject) where T : class
        {
            return _storageEngine.GetObjectId(plainObject, true);
        }

        public object GetObjectFromId(OID id)
        {
            return _storageEngine.GetObjectFromOid(id);
        }

        public void DefragmentTo(string newFileName)
        {
            _storageEngine.DefragmentTo(newFileName);
        }

        public IIndexManager IndexManagerFor<T>() where T : class
        {
            var clazz = typeof (T);
            var classInfo = _storageEngine.GetSession().GetMetaModel().GetClassInfo(clazz, false);

            if (classInfo == null)
            {
                var classInfoList = ClassIntrospector.Introspect(clazz, true);
                _storageEngine.GetObjectWriter().AddClasses(classInfoList);
                classInfo = classInfoList.GetMainClassInfo();
            }

            return new IndexManager(_storageEngine, classInfo);
        }

        public ITriggerManager TriggerManagerFor<T>() where T : class
        {
            return new TriggerManager<T>(_storageEngine);
        }

        public IRefactorManager GetRefactorManager()
        {
            return _storageEngine.GetRefactorManager();
        }

        public IOdbExt Ext()
        {
            return _ext ?? (_ext = new OdbExt(_storageEngine));
        }

        public void Disconnect<T>(T plainObject) where T : class
        {
            _storageEngine.Disconnect(plainObject);
        }

        public bool IsClosed()
        {
            return _storageEngine.IsClosed();
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            finally
            {
                if (_storageEngine.GetBaseIdentification() is FileIdentification)
                {
                    var fileName = _storageEngine.GetBaseIdentification().FileName;
                    Monitor.Exit(string.Intern(Path.GetFullPath(fileName)));    
                }
            }
        }

        #endregion

        internal IStorageEngine GetStorageEngine()
        {
            return _storageEngine;
        }

        public IObjectIntrospectionDataProvider GetClassInfoProvider()
        {
            return _storageEngine.GetClassInfoProvider();
        }
    }
}
