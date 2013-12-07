using System;
using NDatabase.Api;
using NDatabase.Exceptions;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.List
{
    /// <summary>
    ///   A simple list to hold query result.
    /// </summary>
    /// <remarks>
    ///   A simple list to hold query result. It is used when no index and no order by is used and inMemory = false This collection does not store the objects, it only holds the OIDs of the objects. When user ask an object the object is lazy loaded by the getObjectFromId method
    /// </remarks>
    internal sealed class LazySimpleListFromOid<T> : OdbList<T>, IInternalObjectSet<T>
    {
        /// <summary>
        ///   The odb engine to lazily get objects
        /// </summary>
        private readonly IStorageEngine _engine;

        private readonly OdbList<OID> _oids;

        /// <summary>
        ///   indicate if objects must be returned as instance (true) or as non native objects (false)
        /// </summary>
        private readonly bool _returnInstance;

        /// <summary>
        ///   a cursor when getting objects
        /// </summary>
        private int _currentPosition;

        public LazySimpleListFromOid(IStorageEngine engine, bool returnObjects)
        {
            _engine = engine;
            _returnInstance = returnObjects;
            _oids = new OdbList<OID>();
        }

        #region IObjects<T> Members

        public void AddWithKey(IOdbComparable key, T @object)
        {
            throw new OdbRuntimeException(NDatabaseError.OperationNotImplemented);
        }

        public T GetFirst()
        {
            try
            {
                return Get(0);
            }
            catch (Exception e)
            {
                throw new OdbRuntimeException(NDatabaseError.ErrorWhileGettingObjectFromListAtIndex.AddParameter(0), e);
            }
        }

        public bool HasNext()
        {
            return _currentPosition < _oids.Count;
        }

        public new int Count
        {
            get { return _oids.Count; }
        }

        public T Next()
        {
            try
            {
                return Get(_currentPosition++);
            }
            catch (Exception e)
            {
                throw new OdbRuntimeException(NDatabaseError.ErrorWhileGettingObjectFromListAtIndex.AddParameter(0), e);
            }
        }

        public void Reset()
        {
            _currentPosition = 0;
        }

        public void AddOid(OID oid)
        {
            _oids.Add(oid);
        }

        #endregion

        private T Get(int index)
        {
            var oid = _oids[index];
            try
            {
                if (_returnInstance)
                    return (T) _engine.GetObjectFromOid(oid);
                return (T) _engine.GetObjectReader().GetObjectFromOid(oid, false, false);
            }
            catch (Exception)
            {
                throw new OdbRuntimeException(NDatabaseError.ErrorWhileGettingObjectFromListAtIndex.AddParameter(index));
            }
        }
    }
}
