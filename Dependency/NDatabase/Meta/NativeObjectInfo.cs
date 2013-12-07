namespace NDatabase.Meta
{
    /// <summary>
    ///   To keep info about a native instance
    /// </summary>
    internal abstract class NativeObjectInfo : AbstractObjectInfo
    {
        public override int GetHashCode()
        {
            return (TheObject != null
                        ? TheObject.GetHashCode()
                        : 0);
        }

        /// <summary>
        ///   The object being represented
        /// </summary>
        protected readonly object TheObject;

        protected NativeObjectInfo(object @object, int odbTypeId) : base(odbTypeId)
        {
            TheObject = @object;
        }

        protected NativeObjectInfo(object @object, OdbType odbType) : base(odbType)
        {
            TheObject = @object;
        }

        public override string ToString()
        {
            return TheObject != null ? TheObject.ToString() : "null";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var noi = (NativeObjectInfo) obj;

            return TheObject == noi.GetObject() || TheObject.Equals(noi.GetObject());
        }

        public override object GetObject()
        {
            return TheObject;
        }
    }
}
