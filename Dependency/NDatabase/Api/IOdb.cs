using System;
using NDatabase.Api.Query;
using NDatabase.Api.Triggers;

namespace NDatabase.Api
{
    /// <summary>
    /// Database engine interface. 
    /// </summary>
    /// <remarks>
    /// The interface provides all methods to store, retrieve and delete objects and to change object state.
    /// </remarks>
    public interface IOdb : IDisposable
    {
        /// <summary>
        /// Stores the specified plain object.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <param name="plainObject">The plain object.</param>
        /// <returns>Object ID of stored plain object.</returns>
        OID Store<T>(T plainObject) where T : class;

        /// <summary>
        /// Deletes the specified plain object.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <param name="plainObject">The plain object.</param>
        /// <returns>Object ID of deleted plain object.</returns>
        OID Delete<T>(T plainObject) where T : class;

        /// <summary>
        /// Deletes the object with Object ID.
        /// </summary>
        /// <param name="oid">The oid of the object to be deleted.</param>
        void DeleteObjectWithId(OID oid);

        /// <summary>
        /// Gets the object id of an database-aware object.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <param name="plainObject">The plain object.</param>
        /// <returns>The database internal Object ID.</returns>
        OID GetObjectId<T>(T plainObject) where T : class;

        /// <summary>
        /// Gets the object from Object ID.
        /// </summary>
        /// <param name="id">The Object ID.</param>
        /// <returns>The object with the specified Object ID.</returns>
        object GetObjectFromId(OID id);

        /// <summary>
        /// Get the extension of database interface to get the access to advanced functions
        /// </summary>
        /// <returns>Extended interface to database.</returns>
        IOdbExt Ext();

        /// <summary>
        /// Determines whether the database is closed.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the database is closed; otherwise, <c>false</c>.
        /// </returns>
        bool IsClosed();

        /// <summary>
        /// Queries the database for instances of specified type and execute the query.
        /// </summary>
        /// <remarks>
        /// Shortcut for <code>Query&lt;T&gt;().Execute&lt;T&gt;()</code>
        /// </remarks>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <returns>List of stored objects that matches the query.</returns>
        IObjectSet<T> QueryAndExecute<T>();

        /// <summary>
        /// Factory method to create a new instance of the query.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <returns>New instance of query for the specified object type.</returns>
        IQuery Query<T>();

        /// <summary>
        /// Factory method to create a new instance of the values query.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <returns>New instance of values query for the specified object type.</returns>
        IValuesQuery ValuesQuery<T>() where T : class;

        /// <summary>
        /// Factory method to create a new instance of the values query for specified oid.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <param name="oid">The oid of the stored plain object.</param>
        /// <returns>New instance of values query for the specified object with a given oid.</returns>
        IValuesQuery ValuesQuery<T>(OID oid) where T : class;

        /// <summary>
        /// As the queryable.
        /// </summary>
        /// <remarks>
        /// Interface for LINQ to NDatabase
        /// </remarks>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <returns>Queryable collection</returns>
        ILinqQueryable<T> AsQueryable<T>();

        /// <summary>
        /// Commits all changes of the database.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rollbacks all uncommitted changes of the database.
        /// </summary>
        void Rollback();

        /// <summary>
        ///   Closes the database.
        /// </summary>
        /// <remarks>
        ///   Automatically commit uncommitted changes of the database.
        /// </remarks>
        void Close();

        /// <summary>
        /// Defragments database to specified location.
        /// </summary>
        /// <param name="newFileName">New name of the file.</param>
        void DefragmentTo(string newFileName);

        /// <summary>
        /// Get the indexes manager for specified object type.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <returns>Index manager.</returns>
        IIndexManager IndexManagerFor<T>() where T : class;

        /// <summary>
        /// Get the triggers manager for specified object type.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <returns>Trigger manager.</returns>
        ITriggerManager TriggerManagerFor<T>() where T : class;

        /// <summary>
        /// Gets the refactor manager.
        /// </summary>
        /// <remarks>
        /// Refactor manager allows on updating database schema, when classes definition were changed.
        /// </remarks>
        /// <returns>Refactor manager.</returns>
        IRefactorManager GetRefactorManager();

        /// <summary>
        /// Disconnects the specified plain object from the current session.
        /// </summary>
        /// <remarks>
        /// The object is removed from the cache.
        /// </remarks>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <param name="plainObject">The plain object.</param>
        void Disconnect<T>(T plainObject) where T : class;
    }
}
