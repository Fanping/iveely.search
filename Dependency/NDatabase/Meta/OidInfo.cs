using NDatabase.Api;

namespace NDatabase.Meta
{
    internal sealed class OidInfo
    {
        public OidInfo()
        {
            PreviousClassOID = null;
            NextClassOID = null;
        }

        public OID ID { get; set; }

        /// <summary>
        ///   Where is the next class, -1, if it does not exist
        /// </summary>
        public OID NextClassOID { get; set; }

        /// <summary>
        ///   Where is the previous class.
        /// </summary>
        /// <remarks>
        ///   Where is the previous class. -1, if it does not exist
        /// </remarks>
        public OID PreviousClassOID { get; set; }
    }
}