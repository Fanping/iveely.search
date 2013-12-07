namespace NDatabase.Api
{
    /// <summary>
    /// Database identification
    /// </summary>
    public interface IDatabaseId
    {
        /// <summary>
        /// Long numbers identifing database
        /// </summary>
        /// <returns>Array of long numbers which identifies the database</returns>
        long[] GetIds();
    }
}
