namespace NDatabase.Api
{
    /// <summary>
    ///   The main interface of all Object Values query results of NDatabase ODB
    /// </summary>
    public interface IValues : IObjectSet<IObjectValues>
    {
        /// <summary>
        /// Get next values set
        /// </summary>
        /// <returns>Next values</returns>
        IObjectValues NextValues();
    }
}
