using NDatabase.Meta;

namespace NDatabase.Core.Query.Execution
{
    internal interface IMultiClassQueryExecutor : IQueryExecutor
    {
        void SetExecuteStartAndEndOfQueryAction(bool yes);

        IStorageEngine GetStorageEngine();

        IInternalQuery GetQuery();

        /// <summary>
        ///   The class on which to execute the query
        /// </summary>
        void SetClassInfo(ClassInfo ci);
    }
}
