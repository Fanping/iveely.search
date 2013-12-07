namespace NDatabase.Api
{
    /// <summary>
    ///   Interface that will be implemented to hold a row of a result of an Object Values Query
    /// </summary>
    public interface IObjectValues
    {
        /// <summary>
        /// Get result by alias
        /// </summary>
        /// <param name="alias">Alias for result</param>
        /// <returns>Object result</returns>
        object GetByAlias(string alias);

        /// <summary>
        /// Get result by index
        /// </summary>
        /// <param name="index">Index for result</param>
        /// <returns>Object result</returns>
        object GetByIndex(int index);

        /// <summary>
        /// Get values result
        /// </summary>
        /// <returns>Array of objects as values</returns>
        object[] GetValues();
    }
}
