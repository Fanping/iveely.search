using STSdb4.General.Persist;
using STSdb4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STSdb4.WaterfallTree
{
    public class SchemeRecord
    {
        public long ID { get; protected set; }
        public int StructureType { get; protected set; }
        public string Name { get; protected set; }
        public KeyDescriptor KeyDescriptor { get; protected set; }
        public RecordDescriptor RecordDescriptor { get; protected set; }

        public DateTime CreateTime { get; protected set; }
        public byte[] Tag { get; protected set; }
    }

    //public interface ILocator : ISchemeRecord, IComparable<ILocator>, IEquatable<ILocator>
    //{
    //    IOperationCollection CreateOperationCollection(int capacity);
    //    IDataContainer CreateDataContainer();

    //    bool IsReady { get; }
    //    void Prepare();

    //    IApply Apply { get; }
    //    IPersistDataContainer PersistDataContainer { get; }
    //    IPersistOperationCollection PersistOperations { get; }
    //    IPersist<IData> PersistKey { get; }
    //    IComparer<IData> KeyComparer { get; }
    //    IEqualityComparer<IData> KeyEqualityComparer { get; }
    //}
}
