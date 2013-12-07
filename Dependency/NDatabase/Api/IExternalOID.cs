namespace NDatabase.Api
{
    /// <summary>
    /// External OID, which contains database id
    /// </summary>
    public interface IExternalOID : OID
    {
        /// <summary>
        /// Get database id
        /// </summary>
        /// <returns>Database Id</returns>
        IDatabaseId GetDatabaseId();
    }
}
