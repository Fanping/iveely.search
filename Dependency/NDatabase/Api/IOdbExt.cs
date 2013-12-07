namespace NDatabase.Api
{
    /// <summary>
    ///   An interface to provider extended access to ODB.
    /// </summary>
    public interface IOdbExt
    {
        /// <summary>
        ///   Gets the external OID of an Object.
        /// </summary>
        /// <remarks>
        ///   Gets the external OID of an Object. 
        ///   The external OID contains the ID of the database + the oid of the object. 
        ///   The External OID can be used to identify objects outside the ODB database 
        ///   as it should be unique across databases. It can be used for example 
        ///   to implement a replication process.
        /// </remarks>
        IExternalOID GetObjectExternalOID<T>(T plainObject) where T : class;

        /// <summary>
        ///   Get the Database ID
        /// </summary>
        IDatabaseId GetDatabaseId();

        /// <summary>
        ///   Convert an OID to External OID
        /// </summary>
        /// <returns> The external OID </returns>
        IExternalOID ConvertToExternalOID(OID oid);

        /// <summary>
        ///   Returns the object version of the object that has the specified OID
        /// </summary>
        int GetObjectVersion(OID oid);

        /// <summary>
        ///   Returns the object creation date in ms since 1/1/1970
        /// </summary>
        /// <returns> The creation date </returns>
        long GetObjectCreationDate(OID oid);

        /// <summary>
        ///   Returns the object last update date in ms since 1/1/1970
        /// </summary>
        /// <returns> The last update date </returns>
        long GetObjectUpdateDate(OID oid);

        /// <summary>
        ///   Return the name of the database
        /// </summary>
        /// <returns> the file name </returns>
        string GetDbId();
    }
}
