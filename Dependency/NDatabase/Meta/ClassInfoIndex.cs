using System.Linq;
using NDatabase.Api;
using NDatabase.Btree;

namespace NDatabase.Meta
{
    /// <summary>
    ///   An index of a class info
    /// </summary>
    internal sealed class ClassInfoIndex
    {
        public OID ClassInfoId { get; set; }

        public int[] AttributeIds { get; set; }

        public bool IsUnique { get; set; }

        public string Name { get; set; }

        public IBTree BTree { get; set; }

        /// <summary>
        ///   Check if a list of attribute can use the index
        /// </summary>
        /// <returns> true if the list of attribute can use this index </returns>
        public bool MatchAttributeIds(int[] attributeIdsToMatch)
        {
            //TODO an index with lesser attribute than the one to match can be used
            if (AttributeIds.Length != attributeIdsToMatch.Length)
                return false;

            foreach (var attributeIdToMatch in attributeIdsToMatch)
            {
                var found = AttributeIds.Any(t => t == attributeIdToMatch);
                if (!found)
                    return false;
            }

            return true;
        }
    }
}
