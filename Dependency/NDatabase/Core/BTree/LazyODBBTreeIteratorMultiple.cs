using System;
using NDatabase.Api;
using NDatabase.Btree;
using NDatabase.Exceptions;

namespace NDatabase.Core.BTree
{
    /// <summary>
    ///   A Lazy BTree Iterator : It iterate on the object OIDs and lazy load objects from them (OIDs) Used by the LazyBTreeCollection
    /// </summary>
    internal sealed class LazyOdbBtreeIteratorMultiple<T> : BTreeIteratorMultipleValuesPerKey<T>
    {
        private readonly bool _returnObjects;
        private readonly IStorageEngine _storageEngine;

        /// <param name="tree"> </param>
        /// <param name="orderByType"> </param>
        /// <param name="storageEngine"> </param>
        /// <param name="returnObjects"> </param>
        public LazyOdbBtreeIteratorMultiple(IBTree tree, OrderByConstants orderByType, IStorageEngine storageEngine,
                                            bool returnObjects) : base(tree, orderByType)
        {
            _storageEngine = storageEngine;
            _returnObjects = returnObjects;
        }

        public override T Current
        {
            get
            {
                var oid = (OID) base.Current;
                try
                {
                    return (T) LoadObject(oid);
                }
                catch (Exception e)
                {
                    throw new OdbRuntimeException(BTreeError.LazyLoadingNode.AddParameter(oid), e);
                }
            }
        }

        private object LoadObject(OID oid)
        {
            // true = to use cache
            var nnoi = _storageEngine.GetObjectReader().ReadNonNativeObjectInfoFromOid(null, oid, true, _returnObjects);
            
            if (_returnObjects)
            {
                var loadedObject = nnoi.GetObject();
                return loadedObject ?? _storageEngine.GetObjectReader().BuildOneInstance(nnoi);
            }

            return nnoi;
        }
    }
}
