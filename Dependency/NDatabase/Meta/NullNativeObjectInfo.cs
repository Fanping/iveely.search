using System.Collections.Generic;
using NDatabase.Api;

namespace NDatabase.Meta
{
    /// <summary>
    ///   Meta representation of a null native object
    /// </summary>
    internal sealed class NullNativeObjectInfo : NativeObjectInfo
    {
        private static readonly NullNativeObjectInfo Instance = new NullNativeObjectInfo();

        private NullNativeObjectInfo() : base(null, OdbType.Null)
        {
        }

        public NullNativeObjectInfo(int odbTypeId) : base(null, odbTypeId)
        {
        }

        public override string ToString()
        {
            return "null";
        }

        public override bool IsNull()
        {
            return true;
        }

        public override bool IsNative()
        {
            return true;
        }

        public override AbstractObjectInfo CreateCopy(IDictionary<OID, AbstractObjectInfo> cache, bool onlyData)
        {
            return GetInstance();
        }

        public static NullNativeObjectInfo GetInstance()
        {
            return Instance;
        }
    }
}
