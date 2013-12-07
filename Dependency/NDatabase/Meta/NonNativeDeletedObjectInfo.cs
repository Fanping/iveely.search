namespace NDatabase.Meta
{
    /// <summary>
    ///   To specify that an object has been mark as deleted
    /// </summary>
    internal sealed class NonNativeDeletedObjectInfo : NonNativeObjectInfo
    {
        public NonNativeDeletedObjectInfo() : base(null, null)
        {
        }

        public override string ToString()
        {
            return "deleted";
        }

        public override object GetObject()
        {
            return null;
        }

        public override bool IsDeletedObject()
        {
            return true;
        }

        /// <summary>
        ///   A deleted non native object is considered to be null!
        /// </summary>
        public override bool IsNull()
        {
            return true;
        }
    }
}
