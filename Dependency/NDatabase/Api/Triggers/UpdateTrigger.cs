namespace NDatabase.Api.Triggers
{
    /// <summary>
    /// Abstract class - derive from it if you want to create update trigger
    /// </summary>
    public abstract class UpdateTrigger : Trigger
    {
        /// <summary>
        /// Action which will happen before update
        /// </summary>
        /// <param name="oldObjectRepresentation">Object representation</param>
        /// <param name="newObject">Updated object</param>
        /// <param name="oid">Oid of updated object</param>
        /// <returns>True if updated, in other case false</returns>
        public abstract bool BeforeUpdate(IObjectRepresentation oldObjectRepresentation, object newObject, OID oid);

        /// <summary>
        /// Action which will happen after update
        /// </summary>
        /// <param name="oldObjectRepresentation">Object representation</param>
        /// <param name="newObject">Updated object</param>
        /// <param name="oid">Oid of updated object</param>
        public abstract void AfterUpdate(IObjectRepresentation oldObjectRepresentation, object newObject, OID oid);
    }
}
