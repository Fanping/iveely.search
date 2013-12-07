using System.Collections.Generic;
using NDatabase.Api;

namespace NDatabase.Meta
{
    /// <summary>
    ///   To keep meta informations about an object
    /// </summary>
    internal abstract class AbstractObjectInfo
    {
        /// <summary>
        ///   The Type of the object
        /// </summary>
        private readonly OdbType _odbType;

        /// <summary>
        ///   The Type Id of the object
        /// </summary>
        protected readonly int OdbTypeId;

        protected AbstractObjectInfo(int typeId)
        {
            OdbTypeId = typeId;
            _odbType = OdbType.GetFromId(OdbTypeId);
        }

        protected AbstractObjectInfo(OdbType type)
        {
            if (type != null)
                OdbTypeId = type.Id;

            _odbType = type;
        }

        public virtual bool IsNative()
        {
            return IsAtomicNativeObject() || IsArrayObject();
        }

        public virtual bool IsNull()
        {
            return GetObject() == null;
        }

        public abstract object GetObject();

        public int GetOdbTypeId()
        {
            return OdbTypeId;
        }

        public OdbType GetOdbType()
        {
            return _odbType;
        }

        public virtual bool IsNonNativeObject()
        {
            return false;
        }

        public virtual bool IsAtomicNativeObject()
        {
            return false;
        }

        public virtual bool IsArrayObject()
        {
            return false;
        }

        public virtual bool IsDeletedObject()
        {
            return false;
        }

        public virtual bool IsObjectReference()
        {
            return false;
        }

        public virtual bool IsEnumObject()
        {
            return false;
        }

        public abstract AbstractObjectInfo CreateCopy(IDictionary<OID, AbstractObjectInfo> cache, bool onlyData);
    }
}
