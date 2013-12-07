namespace NDatabase.Api
{
    /// <summary>
    ///   used to give the user an instance of an object representation, level2.
    /// </summary>
    /// <remarks>
    ///   used to give the user an instance of an object representation, level2. 
    ///   The Object Representation encapsulates the NonNativeObjectInfo which is the internal object representation.
    /// </remarks>
    public interface IObjectRepresentation
    {
        /// <summary>
        ///   Retrieves the oid of the object
        /// </summary>
        OID GetOid();

        /// <summary>
        ///   Retrieves the full object class name
        /// </summary>
        string GetObjectClassName();

        /// <summary>
        ///   Return the value of a specific attribute
        /// </summary>
        object GetValueOf(string attributeName);

        /// <summary>
        ///   Sets the value of a specific attribute
        /// </summary>
        void SetValueOf(string attributeName, object value);
    }
}
