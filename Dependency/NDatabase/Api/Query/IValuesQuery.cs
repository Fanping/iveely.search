namespace NDatabase.Api.Query
{
    /// <summary>
    /// Extending query with additional query metrics.
    /// </summary>
    public interface IValuesQuery : IQuery
    {
        /// <summary>
        /// Counts the objects that matches the specified values query.
        /// </summary>
        /// <param name="alias">The alias for query value.</param>
        /// <returns>Values query with alias item set to count value.</returns>
        IValuesQuery Count(string alias);

        /// <summary>
        /// Sums the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Values query with sum value.</returns>
        IValuesQuery Sum(string fieldName);

        /// <summary>
        /// Sums the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="alias">The alias for query value.</param>
        /// <returns>Values query with alias item set to sum value.</returns>
        IValuesQuery Sum(string fieldName, string alias);

        /// <summary>
        /// Averages the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="alias">The alias for query value.</param>
        /// <returns>Values query with alias item set to avg value.</returns>
        IValuesQuery Avg(string fieldName, string alias);

        /// <summary>
        /// Avgs the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Values query with avg value.</returns>
        IValuesQuery Avg(string fieldName);

        /// <summary>
        /// Max for the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="alias">The alias for query value.</param>
        /// <returns>Values query with alias item set to max value.</returns>
        IValuesQuery Max(string fieldName, string alias);

        /// <summary>
        /// Max for the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Values query max value.</returns>
        IValuesQuery Max(string fieldName);

        /// <summary>
        /// Field value for the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Values query with field value.</returns>
        IValuesQuery Field(string fieldName);

        /// <summary>
        /// Field value for the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="alias">The alias for query value.</param>
        /// <returns>Values query with alias item set to field value.</returns>
        IValuesQuery Field(string fieldName, string alias);

        /// <summary>
        /// Sublists the specified attribute name (collection or string).
        /// </summary>
        /// <param name="attributeName">Name of the attribute (collection or string).</param>
        /// <param name="alias">The alias for query value.</param>
        /// <param name="fromIndex">Start index.</param>
        /// <param name="size">The size.</param>
        /// <param name="throwException">if set to <c>true</c> [throws exception].</param>
        /// <returns>Values query with alias item set to sublist value.</returns>
        IValuesQuery Sublist(string attributeName, string alias, int fromIndex, int size, bool throwException);

        /// <summary>
        /// Sublists the specified attribute name (collection or string).
        /// </summary>
        /// <param name="attributeName">Name of the attribute (collection or string).</param>
        /// <param name="fromIndex">Start index.</param>
        /// <param name="size">The size.</param>
        /// <param name="throwException">if set to <c>true</c> [throws exception].</param>
        /// <returns>Values query with sublist value.</returns>
        IValuesQuery Sublist(string attributeName, int fromIndex, int size, bool throwException);

        /// <summary>
        /// Sublists the specified attribute name (collection or string).
        /// </summary>
        /// <param name="attributeName">Name of the attribute (collection or string).</param>
        /// <param name="alias">The alias for query value.</param>
        /// <param name="fromIndex">Start index.</param>
        /// <param name="toIndex">End index.</param>
        /// <returns>Values query with alias item set to sublist value.</returns>
        IValuesQuery Sublist(string attributeName, string alias, int fromIndex, int toIndex);

        /// <summary>
        /// Sublists the specified attribute name (collection or string).
        /// </summary>
        /// <param name="attributeName">Name of the attribute (collection or string).</param>
        /// <param name="fromIndex">Start index.</param>
        /// <param name="toIndex">End index.</param>
        /// <returns>Values query with sublist value.</returns>
        IValuesQuery Sublist(string attributeName, int fromIndex, int toIndex);

        /// <summary>
        /// Size of the specified attribute name (collection or string).
        /// </summary>
        /// <param name="attributeName">Name of the attribute (collection).</param>
        /// <returns>Values query with size value.</returns>
        IValuesQuery Size(string attributeName);

        /// <summary>
        /// Size of the specified attribute name (collection or string).
        /// </summary>
        /// <param name="attributeName">Name of the attribute (collection).</param>
        /// <param name="alias">The alias for query value.</param>
        /// <returns>Values query with alias item set to size value.</returns>
        IValuesQuery Size(string attributeName, string alias);

        /// <summary>
        /// Groups by the specified field list.
        /// </summary>
        /// <param name="fieldList">The fields list.</param>
        /// <returns>Values query with specified group by.</returns>
        IValuesQuery GroupBy(string fieldList);

        /// <summary>
        /// Enables or disables the return instance option.
        /// </summary>
        /// <remarks>
        /// To indicate if query execution must build instances or return object representation, Default value is true(return instance)
        /// </remarks>
        /// <param name="returnInstance">if set to <c>true</c> [return instance].</param>
        void SetReturnInstance(bool returnInstance);

        /// <summary>
        /// Min for the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Values query with min value.</returns>
        IValuesQuery Min(string fieldName);

        /// <summary>
        /// Min for the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="alias">The alias for query value.</param>
        /// <returns>Values query with alias item set to min value.</returns>
        IValuesQuery Min(string fieldName, string alias);

        /// <summary>
        /// Gets the values that matches the values query.
        /// </summary>
        /// <returns>The list of values that matches the values query.</returns>
        IValues Execute();
    }
}
