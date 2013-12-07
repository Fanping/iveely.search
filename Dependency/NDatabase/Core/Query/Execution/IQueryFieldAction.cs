namespace NDatabase.Core.Query.Execution
{
    /// <summary>
    ///   Used to implement generic action on matching object.
    /// </summary>
    /// <remarks>
    ///   Used to implement generic action on matching object.
    ///   The Generic query executor is responsible for checking if 
    ///   an object meets the criteria conditions. Then an(some) object 
    ///   actions are called to execute what must be done with matching 
    ///   objects. A ValuesQuery can contain more than one QueryFieldAction.
    /// </remarks>
    internal interface IQueryFieldAction
    {
        void Start();

        void End();

        object GetValue();

        string GetAttributeName();

        string GetAlias();

        /// <summary>
        ///   To indicate if a query will return one row (for example, sum, average, max and min, or will return more than one row
        /// </summary>
        bool IsMultiRow();

        void SetMultiRow(bool isMultiRow);

        IQueryFieldAction Copy();

        void SetReturnInstance(bool returnInstance);
    }
}
