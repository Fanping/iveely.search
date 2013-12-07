namespace NDatabase.Api.Triggers
{
    /// <summary>
    /// Abstract class - derive from it if you want to create insert trigger
    /// </summary>
    public abstract class InsertTrigger : Trigger
    {
        /// <summary>
        /// Action which will happen before insert
        /// </summary>
        /// <param name="object">Inserted object</param>
        /// <returns>True if object inserted, false in other case</returns>
        public abstract bool BeforeInsert(object @object);

        /// <summary>
        /// Action which will happen after insert
        /// </summary>
        /// <param name="object">Inserted object</param>
        /// <param name="oid">Oid of inserted object</param>
        public abstract void AfterInsert(object @object, OID oid);
    }
}
