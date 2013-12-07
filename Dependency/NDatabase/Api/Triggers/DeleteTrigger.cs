namespace NDatabase.Api.Triggers
{
    /// <summary>
    /// Abstract class - derive from it if you want to create delete trigger
    /// </summary>
    public abstract class DeleteTrigger : Trigger
    {
        /// <summary>
        /// Action which will happen before delete
        /// </summary>
        /// <param name="object">Deleted object</param>
        /// <param name="oid">Oid of deleted object</param>
        /// <returns>True if object was deleted, false if not</returns>
        public abstract bool BeforeDelete(object @object, OID oid);

        /// <summary>
        /// Action which will happen after delete
        /// </summary>
        /// <param name="object">Deleted object</param>
        /// <param name="oid">Oid of deleted object</param>
        public abstract void AfterDelete(object @object, OID oid);
    }
}
