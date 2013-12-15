namespace Polenter.Serialization
{
    ///<summary>
    ///  What format has the serialized binary file. It could be SizeOptimized or Burst.
    ///</summary>
    public enum BinarySerializationMode
    {
        /// <summary>
        ///   All types are serialized to string lists, which are stored in the file header. Duplicates are removed. Serialized objects only reference these types. It reduces size especially if serializing collections. Refer to SizeOptimizedBinaryWriter for more details.
        /// </summary>
        SizeOptimized = 0,
        /// <summary>
        ///   There are as many type definitions as many objects stored, not regarding if there are duplicate types defined. It reduces the overhead if storing single items, but increases the file size if storing collections. See BurstBinaryWriter for details.
        /// </summary>
        Burst
    }
}