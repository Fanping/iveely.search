using System;
using NDatabase.Cache;
using NDatabase.Core;
using NDatabase.Core.Session;
using NDatabase.Exceptions;
using NDatabase.Meta;

namespace NDatabase.Transaction
{
    /// <summary>
    ///   An ODB Session.
    /// </summary>
    /// <remarks>
    ///   An ODB Session. Keeps track of all the session operations. 
    ///   Caches objects and manage the transaction. The meta model of the database is stored in the session.
    /// </remarks>
    internal abstract class Session : ISession
    {
        private readonly string _baseIdentification;
        private readonly IOdbCache _cache = new OdbCache();

        private readonly string _id;

        /// <summary>
        ///   A temporary cache used for object info read
        /// </summary>
        private readonly IReadObjectsCache _readObjectsCache  = new ReadObjectsCache();

        private IMetaModel _metaModel;
        private bool _rollbacked;

        protected Session(string id, string baseIdentification)
        {
            _id = id;
            _baseIdentification = baseIdentification;
        }

        #region ISession Members

        public int CompareTo(object o)
        {
            if (o == null || !(o is Session))
                return -100;

            var session = (ISession) o;
            return String.Compare(GetId(), session.GetId(), StringComparison.Ordinal);
        }

        public IOdbCache GetCache()
        {
            return _cache;
        }

        public IReadObjectsCache GetTmpCache()
        {
            return _readObjectsCache;
        }

        public virtual void Rollback()
        {
            ClearCache();
            _rollbacked = true;
        }

        public void Close()
        {
            Clear();
        }

        public bool IsRollbacked()
        {
            return _rollbacked;
        }

        public string GetId()
        {
            return _id;
        }

        public abstract IStorageEngine GetStorageEngine();

        public abstract bool TransactionIsPending();

        public abstract void Commit();

        public abstract ITransaction GetTransaction();

        public abstract void SetFileSystemInterfaceToApplyTransaction(IFileSystemInterface fsi);

        public IMetaModel GetMetaModel()
        {
            if (_metaModel == null)
            {
                // MetaModel can be null (this happens at the end of the
                // Transaction.commitMetaModel() method)when the user commited the
                // database
                // And continue using it. In this case, after the commit, the
                // metamodel is set to null
                // and lazy-reloaded when the user use the odb again.
                _metaModel = new MetaModel();
                try
                {
                    GetStorageEngine().GetObjectReader().LoadMetaModel(_metaModel, true);
                }
                catch (Exception e)
                {
                    throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter("Session.getMetaModel"), e);
                }
            }
            return _metaModel;
        }

        public void RemoveObjectFromCache(object @object)
        {
            _cache.RemoveObject(@object);
        }

        public abstract IObjectWriter GetObjectWriter();

        #endregion

        private void ClearCache()
        {
            _cache.Clear(false);
        }

        public override int GetHashCode()
        {
            return (_id != null
                        ? _id.GetHashCode()
                        : 0);
        }

        protected virtual void Clear()
        {
            _cache.Clear(true);
            if (_metaModel != null)
                _metaModel.Clear();
        }

        public override string ToString()
        {
            var transaction = GetTransaction();
            if (transaction == null)
                return string.Format("name={0} sid={1} - no transaction", _baseIdentification, _id);

            var n = transaction.GetNumberOfWriteActions().ToString();
            return string.Format("name={0} - sid={1} - Nb Actions = {2}", _baseIdentification, _id, n);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Session))
                return false;
            var session = (ISession) obj;
            return GetId().Equals(session.GetId());
        }
    }
}