using NDatabase.Core;
using NDatabase.Core.Session;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Transaction
{
    /// <summary>
    ///   The session object used when ODB is used in local/embedded mode
    /// </summary>
    internal sealed class LocalSession : Session
    {
        private IFileSystemInterface _fsiToApplyTransaction;
        private IStorageEngine _storageEngine;
        private ITransaction _transaction;

        public LocalSession(IStorageEngine engine)
            : base(GetSessionId(), engine.GetBaseIdentification().Id)
        {
            _storageEngine = engine;
        }

        private static string GetSessionId()
        {
            return
                string.Concat("local ", OdbTime.GetCurrentTimeInTicks().ToString(),
                              OdbRandom.GetRandomInteger().ToString());
        }

        public override void SetFileSystemInterfaceToApplyTransaction(IFileSystemInterface fsi)
        {
            _fsiToApplyTransaction = fsi;
            if (_transaction != null)
                _transaction.SetFsiToApplyWriteActions(_fsiToApplyTransaction);
        }

        public override ITransaction GetTransaction()
        {
            return _transaction ?? (_transaction = new OdbTransaction(this, _fsiToApplyTransaction));
        }

        public override bool TransactionIsPending()
        {
            if (_transaction == null)
                return false;
            return _transaction.GetNumberOfWriteActions() != 0;
        }

        private void ResetTranstion()
        {
            if (_transaction == null) 
                return;

            _transaction.Clear();
            _transaction = null;
        }

        public override void Commit()
        {
            if (_transaction == null) 
                return;

            _transaction.Commit();
            _transaction.Reset();
        }

        public override void Rollback()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                ResetTranstion();
            }
            base.Rollback();
        }

        public override IStorageEngine GetStorageEngine()
        {
            return _storageEngine;
        }

        protected override void Clear()
        {
            base.Clear();
            if (_transaction != null)
                _transaction.Clear();
            _storageEngine = null;
        }

        public override IObjectWriter GetObjectWriter()
        {
            return _storageEngine.GetObjectWriter();
        }
    }
}