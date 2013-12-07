using System;
using NDatabase.Api.Query;

namespace NDatabase.Api
{
    /// <summary>
    /// Database engine interface (simplified for triggers purpose). 
    /// </summary>
    /// <remarks>
    /// The interface provides all methods from IOdb which are allowed to access in triggers.
    /// </remarks>
    public interface IOdbForTrigger : IDisposable
    {
        /// <summary>
        /// Stores the specified plain object.
        /// </summary>
        /// <typeparam name="T">Plain object type.</typeparam>
        /// <param name="plainObject">The plain object.</param>
        /// <returns>Object ID of stored plain object.</returns>
        OID Store<T>(T plainObject) where T : class;

        /// <summary>
        /// Gets the values that matches the values query.
        /// </summary>
        /// <param name="query">The values query.</param>
        /// <returns>The list of values that matches the values query.</returns>
        IValues GetValues(IValuesQuery query);

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
    }
}