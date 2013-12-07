using System;
using NDatabase.Cache;
using NDatabase.Meta;

namespace NDatabase.Core.Session
{
    internal interface ISession : IComparable
    {
        IOdbCache GetCache();

        IReadObjectsCache GetTmpCache();

        void Rollback();

        void Close();

        bool IsRollbacked();

        IStorageEngine GetStorageEngine();

        bool TransactionIsPending();

        void Commit();

        ITransaction GetTransaction();

        void SetFileSystemInterfaceToApplyTransaction(IFileSystemInterface fsi);

        IMetaModel GetMetaModel();

        string GetId();

        void RemoveObjectFromCache(object @object);

        IObjectWriter GetObjectWriter();
    }
}
