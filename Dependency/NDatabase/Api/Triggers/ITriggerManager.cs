namespace NDatabase.Api.Triggers
{
    /// <summary>
    /// Triggers manager
    /// </summary>
    public interface ITriggerManager
    {
        /// <summary>
        ///   Used to add an update trigger callback for the specific class
        /// </summary>
        void AddUpdateTrigger(UpdateTrigger trigger);

        /// <summary>
        ///   Used to add an insert trigger callback for the specific class
        /// </summary>
        void AddInsertTrigger(InsertTrigger trigger);

        /// <summary>
        ///   USed to add a delete trigger callback for the specific class
        /// </summary>
        void AddDeleteTrigger(DeleteTrigger trigger);

        /// <summary>
        ///   Used to add a select trigger callback for the specific class
        /// </summary>
        void AddSelectTrigger(SelectTrigger trigger);
    }
}
