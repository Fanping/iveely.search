using NDatabase.Api;

namespace NDatabase.Storage
{
    internal sealed class CurrentIdBlockInfo
    {
        /// <summary>
        ///   The max id already allocated in the current id block
        /// </summary>
        internal OID CurrentIdBlockMaxOid { get; set; }

        /// <summary>
        ///   The current id block number
        /// </summary>
        internal int CurrentIdBlockNumber { get; set; }

        /// <summary>
        ///   The position of the current block where IDs are stored
        /// </summary>
        internal long CurrentIdBlockPosition { get; set; }
    }
}