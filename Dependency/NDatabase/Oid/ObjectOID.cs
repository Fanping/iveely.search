using NDatabase.Api;

namespace NDatabase.Oid
{
    internal class ObjectOID : BaseOID
    {
        public ObjectOID(long oid) : base(oid)
        {
        }

        public override int CompareTo(OID oid)
        {
            if (oid == null || !(oid is ObjectOID))
                return -1000;

            var otherOid = oid;
            return (int) (ObjectId - otherOid.ObjectId);
        }

        public override bool Equals(object @object)
        {
            var oid = @object as OID;

            return this == @object || CompareTo(oid) == 0;
        }

        public override int GetHashCode()
        {
            // Copy of the Long hashcode algorithm
            return (int) (ObjectId ^ (UrShift(ObjectId, 32)));
        }
    }
}
