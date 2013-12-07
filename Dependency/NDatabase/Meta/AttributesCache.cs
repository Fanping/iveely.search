using System.Collections.Generic;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Meta
{
    internal sealed class AttributesCache
    {
        internal AttributesCache()
        {
            AttributesByName = new OdbHashMap<string, ClassAttributeInfo>();
            AttributesById = new OdbHashMap<int, ClassAttributeInfo>();
        }

        /// <summary>
        ///   This map is redundant with the field 'attributes', 
        ///   but it is to enable fast access to attributes 
        ///   by id key=attribute Id(Integer), key =ClassAttributeInfo
        /// </summary>
        internal IDictionary<int, ClassAttributeInfo> AttributesById { get; set; }

        /// <summary>
        ///   This map is redundant with the field 'attributes', 
        ///   but it is to enable fast access to attributes by name
        /// </summary>
        internal IDictionary<string, ClassAttributeInfo> AttributesByName { get; set; }
    }
}