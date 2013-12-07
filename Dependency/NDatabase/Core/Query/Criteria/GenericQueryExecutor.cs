using System.Collections;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Btree;
using NDatabase.Container;
using NDatabase.Core.BTree;
using NDatabase.Core.Query.Execution;
using NDatabase.Core.Session;
using NDatabase.Exceptions;
using NDatabase.Indexing;
using NDatabase.Meta;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria
{
    /// <summary>
    ///   <p>Generic query executor</p> .
    /// </summary>
    /// <remarks>
    ///   <p>Generic query executor. This class does all the job of iterating in the
    ///     object list and call particular query matching to check if the object must be
    ///     included in the query result.</p> <p>If the query has index, An execution plan is calculated to optimize the
    ///                                         execution. The query execution plan is calculated by subclasses (using
    ///                                         abstract method getExecutionPlan).</p>
    /// </remarks>
    internal abstract class GenericQueryExecutor : IMultiClassQueryExecutor
    {
        /// <summary>
        ///   The object used to read object data from database
        /// </summary>
        protected readonly IObjectReader ObjectReader;

        /// <summary>
        ///   The query being executed
        /// </summary>
        protected readonly SodaQuery Query;

        /// <summary>
        ///   The current database session
        /// </summary>
        protected readonly ISession Session;

        /// <summary>
        ///   The storage engine
        /// </summary>
        protected readonly IStorageEngine StorageEngine;

        /// <summary>
        ///   The class of the object being fetched
        /// </summary>
        protected ClassInfo ClassInfo;

        protected NonNativeObjectInfo CurrentNnoi;
        protected OID CurrentOid;

        /// <summary>
        ///   The next object position
        /// </summary>
        protected OID NextOID;

        /// <summary>
        ///   Used for multi class executor to indicate not to execute start and end method of query result action
        /// </summary>
        private bool _executeStartAndEndOfQueryAction;

        /// <summary>
        ///   The key for ordering
        /// </summary>
        private IOdbComparable _orderByKey;

        /// <summary>
        ///   A boolean to indicate if query must be ordered
        /// </summary>
        private bool _queryHasOrderBy;

        protected GenericQueryExecutor(IQuery query, IStorageEngine engine)
        {
            Query = (SodaQuery) query;
            StorageEngine = engine;
            ObjectReader = StorageEngine.GetObjectReader();
            Session = StorageEngine.GetSession();
            _executeStartAndEndOfQueryAction = true;
        }

        #region IMultiClassQueryExecutor Members

        public virtual IInternalObjectSet<T> Execute<T>(bool inMemory, int startIndex, int endIndex, bool returnObjects,
                                                        IMatchingObjectAction queryResultAction)
        {
            if (StorageEngine.IsClosed())
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbIsClosed.AddParameter(StorageEngine.GetBaseIdentification().Id));
            }

            if (Session.IsRollbacked())
                throw new OdbRuntimeException(NDatabaseError.OdbHasBeenRollbacked);

            // Get the query execution plan
            var executionPlan = GetExecutionPlan();
            executionPlan.Start();

            try
            {
                if (executionPlan.UseIndex())
                    return ExecuteUsingIndex<T>(executionPlan.GetIndex(), inMemory, returnObjects,
                                                queryResultAction);

                // When query must be applied to a single object
                return Query.IsForSingleOid()
                           ? ExecuteForOneOid<T>(inMemory, returnObjects, queryResultAction)
                           : ExecuteFullScan<T>(inMemory, startIndex, endIndex, returnObjects, queryResultAction);
            }
            finally
            {
                executionPlan.End();
            }
        }

        public void SetExecuteStartAndEndOfQueryAction(bool yes)
        {
            _executeStartAndEndOfQueryAction = yes;
        }

        public IStorageEngine GetStorageEngine()
        {
            return StorageEngine;
        }

        public IInternalQuery GetQuery()
        {
            return Query;
        }

        public void SetClassInfo(ClassInfo classInfo)
        {
            ClassInfo = classInfo;
        }

        #endregion

        /// <summary>
        ///   Used to indicate if the execute method must call start and end method of the queryResultAction.
        /// </summary>
        /// <remarks>
        ///   Used to indicate if the execute method must call start and end method of the queryResultAction. The default is yes. For MultiClass Query executor, it is set to false to avoid to reset the result
        /// </remarks>
        /// <returns> true or false to indicate if start and end method of queryResultAction must be executed </returns>
        private bool ExecuteStartAndEndOfQueryAction()
        {
            return _executeStartAndEndOfQueryAction;
        }

        protected abstract IQueryExecutionPlan GetExecutionPlan();

        protected abstract void PrepareQuery();

        /// <summary>
        ///   This can be a NonNAtiveObjectInf or AttributeValuesMap
        /// </summary>
        protected abstract object GetCurrentObjectMetaRepresentation();

        /// <summary>
        ///   Check if the object with oid matches the query, returns true This method must compute the next object oid and the orderBy key if it exists!
        /// </summary>
        /// <param name="oid"> The object position </param>
        /// <param name="loadObjectInfo"> To indicate if object must loaded (when the query indicator 'in memory' is false, we do not need to load object, only ids) </param>
        /// <param name="inMemory"> To indicate if object must be actually loaded to memory </param>
        protected abstract bool MatchObjectWithOid(OID oid, bool loadObjectInfo, bool inMemory);

        /// <summary>
        ///   Take the fields of the index and take value from the query
        /// </summary>
        /// <param name="index"> The index </param>
        /// <returns> The key of the index </returns>
        protected virtual IOdbComparable ComputeIndexKey(ClassInfoIndex index)
        {
            var attributesNames = ClassInfo.GetAttributeNames(index.AttributeIds);
            var constraint = Query.GetCriteria();
            var values = ((IInternalConstraint)constraint).GetValues();
            return IndexTool.BuildIndexKey(index.Name, values, attributesNames);
        }

        /// <summary>
        ///   Query execution full scan <pre>startIndex &amp; endIndex
        ///                               A B C D E F G H I J K L
        ///                               [1,3] : nb &gt;=1 &amp;&amp; nb&lt;3
        ///                               1)
        ///                               analyze A
        ///                               nb = 0
        ///                               nb E [1,3] ? no
        ///                               r=[]
        ///                               2)
        ///                               analyze B
        ///                               nb = 1
        ///                               nb E [1,3] ? yes
        ///                               r=[B]
        ///                               3) analyze C
        ///                               nb = 2
        ///                               nb E [1,3] ? yes
        ///                               r=[B,C]
        ///                               4) analyze C
        ///                               nb = 3
        ///                               nb E [1,3] ? no and 3&gt; upperBound([1,3]) =&gt; exit</pre>
        /// </summary>
        /// <param name="inMemory"> </param>
        /// <param name="startIndex"> </param>
        /// <param name="endIndex"> </param>
        /// <param name="returnObjects"> </param>
        /// <param name="queryResultAction"> </param>
        private IInternalObjectSet<T> ExecuteFullScan<T>(bool inMemory, int startIndex, int endIndex, bool returnObjects,
                                                         IMatchingObjectAction queryResultAction)
        {
            if (StorageEngine.IsClosed())
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbIsClosed.AddParameter(StorageEngine.GetBaseIdentification().Id));
            }
            var nbObjects = ClassInfo.NumberOfObjects;

            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug(string.Format("GenericQueryExecutor: loading {0} instance(s) of {1}", nbObjects, ClassInfo.FullClassName));

            if (ExecuteStartAndEndOfQueryAction())
                queryResultAction.Start();

            OID currentOID = null;

            // TODO check if all instances are in the cache! and then load from the cache
            NextOID = ClassInfo.CommitedZoneInfo.First;

            if (nbObjects > 0 && NextOID == null)
            {
                // This means that some changes have not been commited!
                // Take next position from uncommited zone
                NextOID = ClassInfo.UncommittedZoneInfo.First;
            }

            PrepareQuery();
            if (Query != null)
                _queryHasOrderBy = Query.HasOrderBy();

            // used when startIndex and endIndex are not negative
            var nbObjectsInResult = 0;

            for (var i = 0; i < nbObjects; i++)
            {
                // Reset the order by key
                _orderByKey = null;
                var prevOID = currentOID;
                currentOID = NextOID;

                // This is an error
                if (currentOID == null)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.NullNextObjectOid.AddParameter(ClassInfo.FullClassName).AddParameter(i).
                            AddParameter(nbObjects).AddParameter(prevOID));
                }

                // If there is an endIndex condition
                if (endIndex != -1 && nbObjectsInResult >= endIndex)
                    break;

                // If there is a startIndex condition
                bool objectInRange;
                if (startIndex != -1 && nbObjectsInResult < startIndex)
                    objectInRange = false;
                else
                    objectInRange = true;

                // There is no query
                if (!inMemory && Query == null)
                {
                    nbObjectsInResult++;

                    // keep object position if we must
                    if (objectInRange)
                    {
                        _orderByKey = BuildOrderByKey(CurrentNnoi);
                        // TODO Where is the key for order by
                        queryResultAction.ObjectMatch(NextOID, _orderByKey);
                    }

                    NextOID = ObjectReader.GetNextObjectOID(currentOID);
                }
                else
                {
                    var objectMatches = MatchObjectWithOid(currentOID, returnObjects, inMemory);

                    if (objectMatches)
                    {
                        nbObjectsInResult++;

                        if (objectInRange)
                        {
                            if (_queryHasOrderBy)
                                _orderByKey = BuildOrderByKey(GetCurrentObjectMetaRepresentation());

                            queryResultAction.ObjectMatch(currentOID, GetCurrentObjectMetaRepresentation(), _orderByKey);
                        }
                    }
                }
            }

            if (ExecuteStartAndEndOfQueryAction())
                queryResultAction.End();

            return queryResultAction.GetObjects<T>();
        }

        /// <summary>
        ///   Execute query using index
        /// </summary>
        /// <param name="index"> </param>
        /// <param name="inMemory"> </param>
        /// <param name="returnObjects"> </param>
        /// <param name="queryResultAction"> </param>
        private IInternalObjectSet<T> ExecuteUsingIndex<T>(ClassInfoIndex index, bool inMemory,
                                                           bool returnObjects, IMatchingObjectAction queryResultAction)
        {
            // Index that have not been used yet do not have persister!
            if (index.BTree.GetPersister() == null)
                index.BTree.SetPersister(new LazyOdbBtreePersister(StorageEngine));

            var nbObjects = ClassInfo.NumberOfObjects;
            var btreeSize = index.BTree.GetSize();

            // the two values should be equal
            if (nbObjects != btreeSize)
            {
                var classInfo = StorageEngine.GetSession().GetMetaModel().GetClassInfoFromId(index.ClassInfoId);

                throw new OdbRuntimeException(
                    NDatabaseError.IndexIsCorrupted.AddParameter(index.Name).AddParameter(classInfo.FullClassName).
                        AddParameter(nbObjects).AddParameter(btreeSize));
            }

            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug(string.Format("GenericQueryExecutor: loading {0} instance(s) of {1}", nbObjects, ClassInfo.FullClassName));

            if (ExecuteStartAndEndOfQueryAction())
                queryResultAction.Start();

            PrepareQuery();
            if (Query != null)
                _queryHasOrderBy = Query.HasOrderBy();

            var tree = index.BTree;
            var isUnique = index.IsUnique;

            // Iterator iterator = new BTreeIterator(tree,
            // OrderByConstants.ORDER_BY_ASC);
            var key = ComputeIndexKey(index);
            IList list = null;

            // If index is unique, get the object
            if (isUnique)
            {
                var treeSingle = (IBTreeSingleValuePerKey) tree;
                var value = treeSingle.Search(key);
                if (value != null)
                    list = new List<object> {value};
            }
            else
            {
                var treeMultiple = (IBTreeMultipleValuesPerKey) tree;
                list = treeMultiple.Search(key);
            }

            if (list != null)
            {
                foreach (OID oid in list)
                {
                    // FIXME Why calling this method
                    ObjectReader.GetObjectPositionFromItsOid(oid, true, true);
                    _orderByKey = null;

                    var objectMatches = MatchObjectWithOid(oid, returnObjects, inMemory);
                    if (objectMatches)
                        queryResultAction.ObjectMatch(oid, GetCurrentObjectMetaRepresentation(), _orderByKey);
                }

                queryResultAction.End();
                return queryResultAction.GetObjects<T>();
            }

            if (ExecuteStartAndEndOfQueryAction())
                queryResultAction.End();

            return queryResultAction.GetObjects<T>();
        }

        /// <summary>
        ///   Execute query using index
        /// </summary>
        /// <param name="inMemory"> </param>
        /// <param name="returnObjects"> </param>
        /// <param name="queryResultAction"> </param>
        private IInternalObjectSet<T> ExecuteForOneOid<T>(bool inMemory, bool returnObjects,
                                                          IMatchingObjectAction queryResultAction)
        {
            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug(string.Format("GenericQueryExecutor: loading Object with oid {0} - class {1}", Query.GetOidOfObjectToQuery(),
                                            ClassInfo.FullClassName));

            if (ExecuteStartAndEndOfQueryAction())
                queryResultAction.Start();

            PrepareQuery();
            var oid = Query.GetOidOfObjectToQuery();

            MatchObjectWithOid(oid, returnObjects, inMemory);

            queryResultAction.ObjectMatch(oid, GetCurrentObjectMetaRepresentation(), _orderByKey);
            queryResultAction.End();

            return queryResultAction.GetObjects<T>();
        }

        private IOdbComparable BuildOrderByKey(object @object)
        {
            var attributeValuesMap = @object as AttributeValuesMap;

            return attributeValuesMap != null
                       ? BuildOrderByKey(attributeValuesMap)
                       : BuildOrderByKey((NonNativeObjectInfo) @object);
        }

        private IOdbComparable BuildOrderByKey(NonNativeObjectInfo nnoi)
        {
            // TODO cache the attributes ids to compute them only once
            var queryManager = DependencyContainer.Resolve<IQueryManager>();
            var orderByAttributeIds = queryManager.GetOrderByAttributeIds(ClassInfo, Query);

            return IndexTool.BuildIndexKey("OrderBy", nnoi, orderByAttributeIds);
        }

        private IOdbComparable BuildOrderByKey(AttributeValuesMap values)
        {
            return IndexTool.BuildIndexKey("OrderBy", values, Query.GetOrderByFieldNames());
        }
    }
}