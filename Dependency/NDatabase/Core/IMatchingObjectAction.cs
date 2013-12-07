using NDatabase.Api;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core
{
    /// <summary>
    ///   The interface used to implement the classes that are called by the generic query executor when an object matches the query
    /// </summary>
    internal interface IMatchingObjectAction
    {
        /// <summary>
        ///   Called at the beginning of the query execution - used to prepare result object
        /// </summary>
        void Start();

        /// <summary>
        ///   Called (by the GenericQueryExecutor) when an object matches with lazy loading, only stores the OID
        /// </summary>
        void ObjectMatch(OID oid, IOdbComparable orderByKey);

        /// <summary>
        ///   Called (by the GenericQueryExecutor) when an object matches the query
        /// </summary>
        void ObjectMatch(OID oid, object @object, IOdbComparable orderByKey);

        /// <summary>
        ///   Called at the end of the query execution - used to clean or finish some task
        /// </summary>
        void End();

        /// <summary>
        ///   Returns the resulting objects
        /// </summary>
        IInternalObjectSet<T> GetObjects<T>();
    }
}
