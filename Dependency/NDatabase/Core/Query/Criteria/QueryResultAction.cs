using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Core.Query.List;
using NDatabase.Meta;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria
{
    /// <summary>
    ///   Class that manage normal query.
    /// </summary>
    /// <remarks>
    ///   Class that manage normal query. Query that return a list of objects. For each object That matches the query criteria, the objectMatch method is called and it keeps the objects in the 'objects' instance.
    /// </remarks>
    internal sealed class QueryResultAction<T> : IMatchingObjectAction
    {
        private readonly bool _inMemory;
        private readonly IInternalQuery _query;

        private readonly bool _queryHasOrderBy;
        private readonly bool _returnObjects;
        private readonly IStorageEngine _storageEngine;

        /// <summary>
        ///   An object to build instances
        /// </summary>
        private readonly IInstanceBuilder _instanceBuilder;

        private IInternalObjectSet<T> _result;

        public QueryResultAction(IQuery query, bool inMemory, IStorageEngine storageEngine, bool returnObjects,
                                           IInstanceBuilder instanceBuilder)
        {
            _query = (IInternalQuery) query;
            _inMemory = inMemory;
            _storageEngine = storageEngine;
            _returnObjects = returnObjects;
            _queryHasOrderBy = _query.HasOrderBy();
            _instanceBuilder = instanceBuilder;
        }

        #region IMatchingObjectAction Members

        public void ObjectMatch(OID oid, IOdbComparable orderByKey)
        {
            if (_queryHasOrderBy)
                _result.AddWithKey(orderByKey, (T) oid);
            else
                _result.Add((T) oid);
        }

        public void ObjectMatch(OID oid, object o, IOdbComparable orderByKey)
        {
            var nnoi = (NonNativeObjectInfo) o;
            if (_inMemory)
            {
                if (_returnObjects)
                {
                    if (_queryHasOrderBy)
                        _result.AddWithKey(orderByKey, (T) GetCurrentInstance(nnoi));
                    else
                        _result.Add((T) GetCurrentInstance(nnoi));
                }
            }
            else
            {
                if (_queryHasOrderBy)
                    _result.AddWithKey(orderByKey, (T) oid);
                else
                    _result.AddOid(oid);
            }
        }

        public void Start()
        {
            if (_inMemory)
            {
                if (_query != null && _query.HasOrderBy())
                    _result = new InMemoryBTreeCollection<T>(_query.GetOrderByType());
                else
                    _result = new SimpleList<T>();
            }
            else
            {
                // result = new InMemoryBTreeCollection((int) nbObjects);
                if (_query != null && _query.HasOrderBy())
                    _result = new LazyBTreeCollection<T>(_storageEngine, _returnObjects);
                else
                    _result = new LazySimpleListFromOid<T>(_storageEngine, _returnObjects);
            }
        }

        public void End()
        {
        }

        public IInternalObjectSet<TItem> GetObjects<TItem>()
        {
            return (IInternalObjectSet<TItem>)_result;
        }

        #endregion

        private object GetCurrentInstance(NonNativeObjectInfo nnoi)
        {
            return nnoi.GetObject() ??
                   _instanceBuilder.BuildOneInstance(nnoi, _storageEngine.GetSession().GetCache());
        }
    }
}
