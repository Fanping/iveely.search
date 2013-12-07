using NDatabase.Exceptions;

namespace NDatabase.Core.Query.Execution
{
    /// <summary>
    ///   <p>A class to execute a query on more than one class and then merges the result.</p>
    /// </summary>
    /// <remarks>
    ///   <p>A class to execute a query on more than one class and then merges the result. It is used when polymophic is set to true because
    ///     in this case, we must execute query on the main class and all its persistent subclasses</p>
    /// </remarks>
    internal sealed class MultiClassGenericQueryExecutor : IQueryExecutor
    {
        private readonly IMultiClassQueryExecutor _executor;

        public MultiClassGenericQueryExecutor(IMultiClassQueryExecutor executor)
        {
            _executor = executor;

            // To avoid reseting the result for each query
            _executor.SetExecuteStartAndEndOfQueryAction(false);
        }

        #region IQueryExecutor Members

        /// <summary>
        ///   The main query execution method
        /// </summary>
        /// <param name="inMemory"> </param>
        /// <param name="startIndex"> </param>
        /// <param name="endIndex"> </param>
        /// <param name="returnObjects"> </param>
        /// <param name="queryResultAction"> </param>
        public IInternalObjectSet<T> Execute<T>(bool inMemory, int startIndex, int endIndex, bool returnObjects,
                                              IMatchingObjectAction queryResultAction)
        {
            if (_executor.GetStorageEngine().IsClosed())
            {
                throw new OdbRuntimeException(
                    NDatabaseError.OdbIsClosed.AddParameter(
                        _executor.GetStorageEngine().GetBaseIdentification().Id));
            }

            if (_executor.GetStorageEngine().GetSession().IsRollbacked())
                throw new OdbRuntimeException(NDatabaseError.OdbHasBeenRollbacked);

            // Get the main class
            var underlyingType = _executor.GetQuery().UnderlyingType;

            // this is done once.
            queryResultAction.Start();
            var allClassInfos =
                _executor.GetStorageEngine().GetSession().GetMetaModel().GetPersistentSubclassesOf(underlyingType);

            var nbClasses = allClassInfos.Count;
            for (var i = 0; i < nbClasses; i++)
            {
                var classInfo = allClassInfos[i];
                // Sets the class info to the current
                _executor.SetClassInfo(classInfo);
                // Then execute query
                _executor.Execute<T>(inMemory, startIndex, endIndex, returnObjects, queryResultAction);
            }

            queryResultAction.End();
            return queryResultAction.GetObjects<T>();
        }

        #endregion
    }
}
