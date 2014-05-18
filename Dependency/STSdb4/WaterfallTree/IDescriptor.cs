using Iveely.General.Persist;
using Iveely.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.WaterfallTree
{
    //[Flags]
    //public enum CustomData : byte
    //{
    //    None = 0,
    //    KeyType = 1,
    //    RecordType = 2,
    //    KeyComparer = 4,
    //    KeyEqualityComparer = 8,
    //    KeyPersist = 16,
    //    RecordPersist = 32,
    //    KeyIndexerPersist = 64,
    //    RecordIndexerPersist = 128,

    //    All = KeyType | RecordType | KeyComparer | KeyEqualityComparer | KeyPersist | RecordPersist | KeyIndexerPersist | RecordIndexerPersist
    //}

    public interface IDescriptor
    {
        long ID { get; }
        string Name { get; }
        int StructureType { get; }

        /// <summary>
        /// Describes the KeyType
        /// </summary>
        DataType KeyDataType { get; }

        /// <summary>
        /// Describes the RecordType
        /// </summary>
        DataType RecordDataType { get; }

        /// <summary>
        /// Can be anonymous or user type
        /// </summary>
        Type KeyType { get; set; }

        /// <summary>
        /// Can be anonymous or user type
        /// </summary>
        Type RecordType { get; set; }

        IComparer<IData> KeyComparer { get; set; }
        IEqualityComparer<IData> KeyEqualityComparer { get; set; }
        IPersist<IData> KeyPersist { get; set; }
        IPersist<IData> RecordPersist { get; set; }
        IIndexerPersist<IData> KeyIndexerPersist { get; set; }
        IIndexerPersist<IData> RecordIndexerPersist { get; set; }

        DateTime CreateTime { get; }
        DateTime ModifiedTime { get; }
        DateTime AccessTime { get; }

        byte[] Tag { get; set; }
    }
}