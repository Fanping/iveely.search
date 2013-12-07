using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Exceptions;

namespace NDatabase.Meta
{
    /// <summary>
    ///   Meta representation of an object reference.
    /// </summary>
    internal sealed class ObjectReference : AbstractObjectInfo
    {
        private readonly OID _id;

        private readonly NonNativeObjectInfo _nnoi;

        public ObjectReference(NonNativeObjectInfo nnoi) : base(OdbType.NonNativeId)
        {
            _id = null;
            _nnoi = nnoi;
        }

        /// <returns> Returns the id. </returns>
        public OID GetOid()
        {
            return _nnoi != null ? _nnoi.GetOid() : _id;
        }

        public override bool IsObjectReference()
        {
            return true;
        }

        public override string ToString()
        {
            return string.Format("ObjectReference to oid {0}", GetOid());
        }

        public override bool IsNull()
        {
            return false;
        }

        public override object GetObject()
        {
            throw new OdbRuntimeException(
                NDatabaseError.MethodShouldNotBeCalled.AddParameter("getObject").AddParameter(GetType().FullName));
        }

        public NonNativeObjectInfo GetNnoi()
        {
            return _nnoi;
        }

        public override AbstractObjectInfo CreateCopy(IDictionary<OID, AbstractObjectInfo> cache, bool onlyData)
        {
            return new ObjectReference((NonNativeObjectInfo) _nnoi.CreateCopy(cache, onlyData));
        }
    }
}
