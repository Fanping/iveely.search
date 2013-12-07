using System.Collections.Generic;
using NDatabase.Api;

namespace NDatabase.Meta
{
    /// <summary>
    ///   Meta representation of an enum.
    /// </summary>
    /// <remarks>
    ///   Meta representation of an enum. Which is internally represented by a string : Its name
    /// </remarks>
    internal sealed class EnumNativeObjectInfo : NativeObjectInfo
    {
        private readonly ClassInfo _enumClassInfo;

        public EnumNativeObjectInfo(ClassInfo classInfo, string enumName) : base(enumName, OdbType.EnumId)
        {
            _enumClassInfo = classInfo;
        }

        public override string ToString()
        {
            return GetObject().ToString();
        }

        public override bool IsNull()
        {
            return GetObject() == null;
        }

        public override bool IsNative()
        {
            return true;
        }

        public override bool IsEnumObject()
        {
            return true;
        }

        public override AbstractObjectInfo CreateCopy(IDictionary<OID, AbstractObjectInfo> cache, bool onlyData)
        {
            return new EnumNativeObjectInfo(_enumClassInfo, GetObject() == null
                                                               ? null
                                                               : GetObject().ToString());
        }

        public string GetEnumValue()
        {
            return GetObject().ToString();
        }

        public ClassInfo GetEnumClassInfo()
        {
            return _enumClassInfo;
        }
    }
}
