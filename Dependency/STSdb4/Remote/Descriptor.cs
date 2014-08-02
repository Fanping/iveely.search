using Iveely.STSdb4.Data;
using Iveely.STSdb4.Database;
using Iveely.STSdb4.General.Compression;
using Iveely.STSdb4.General.Persist;
using Iveely.STSdb4.WaterfallTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iveely.STSdb4.Remote.Commands;

namespace Iveely.STSdb4.Remote
{
    public class Descriptor : IDescriptor
    {
        private DescriptorStructure InternalDescriptor { get; set; }

        public Descriptor(long id, string name, int structureType, DataType keyDataType, DataType recordDataType, Type keyType, Type recordType, DateTime createTime, DateTime modifiedTime, DateTime accessTime, byte[] tag)
        {
            InternalDescriptor = new DescriptorStructure(id, name, structureType, keyDataType, recordDataType, keyType, recordType, createTime, modifiedTime, accessTime, tag);
        }

        public Descriptor(long id, string name, DataType keyDataType, DataType recordDataType)
            : this(id, name, Iveely.STSdb4.Database.StructureType.XTABLE, keyDataType, recordDataType, DataTypeUtils.BuildType(keyDataType), DataTypeUtils.BuildType(recordDataType), DateTime.Now, DateTime.Now, DateTime.Now, null)
        {
        }

        public Descriptor(long id, string name, int structureType, DataType keyDataType, DataType recordDataType, Type keyType, Type recordType)
            : this(id, name, structureType, keyDataType, recordDataType, keyType, recordType, DateTime.Now, DateTime.Now, DateTime.Now, null)
        {
        }

        private Descriptor(DescriptorStructure descriptor)
        {
            InternalDescriptor = descriptor;
        }

        #region IDescriptor

        public long ID
        {
            get { return InternalDescriptor.ID; }
            set { InternalDescriptor.ID = value; }
        }

        public string Name
        {
            get { return InternalDescriptor.Name; }
            set { InternalDescriptor.Name = value; }
        }

        public int StructureType
        {
            get { return InternalDescriptor.StructureType; }
            set { InternalDescriptor.StructureType = value; }
        }

        public DataType KeyDataType
        {
            get { return InternalDescriptor.KeyDataType; }
            set { InternalDescriptor.KeyDataType = value; }
        }

        public DataType RecordDataType
        {
            get { return InternalDescriptor.RecordDataType; }
            set { InternalDescriptor.RecordDataType = value; }
        }

        public Type KeyType
        {
            get { return InternalDescriptor.KeyType; }
            set { InternalDescriptor.KeyType = value; }
        }

        public Type RecordType
        {
            get { return InternalDescriptor.RecordType; }
            set { InternalDescriptor.RecordType = value; }
        }

        public IComparer<IData> KeyComparer
        {
            get { return InternalDescriptor.KeyComparer; }
            set { InternalDescriptor.KeyComparer = value; }
        }

        public IEqualityComparer<IData> KeyEqualityComparer
        {
            get { return InternalDescriptor.KeyEqualityComparer; }
            set { InternalDescriptor.KeyEqualityComparer = value; }
        }

        public IPersist<IData> KeyPersist
        {
            get { return InternalDescriptor.KeyPersist; }
            set { InternalDescriptor.KeyPersist = value; }
        }

        public IPersist<IData> RecordPersist
        {
            get { return InternalDescriptor.RecordPersist; }
            set { InternalDescriptor.RecordPersist = value; }
        }

        public IIndexerPersist<IData> KeyIndexerPersist
        {
            get { return InternalDescriptor.KeyIndexerPersist; }
            set { InternalDescriptor.KeyIndexerPersist = value; }
        }

        public IIndexerPersist<IData> RecordIndexerPersist
        {
            get { return InternalDescriptor.RecordIndexerPersist; }
            set { InternalDescriptor.RecordIndexerPersist = value; }
        }

        public DateTime CreateTime
        {
            get { return InternalDescriptor.CreateTime; }
            private set { InternalDescriptor.CreateTime = value; }
        }

        public DateTime ModifiedTime
        {
            get { return InternalDescriptor.ModifiedTime; }
            private set { InternalDescriptor.ModifiedTime = value; }
        }

        public DateTime AccessTime
        {
            get { return InternalDescriptor.AccessTime; }
            private set { InternalDescriptor.AccessTime = value; }
        }

        public byte[] Tag
        {
            get { return InternalDescriptor.Tag; }
            set { InternalDescriptor.Tag = value; }
        }

        #endregion

        public void Serialize(BinaryWriter writer)
        {
            InternalDescriptor.Serialize(writer);
        }

        public static IDescriptor Deserialize(BinaryReader reader)
        {
            DescriptorStructure descriptorStructure = DescriptorStructure.Deserialize(reader);

            return new Descriptor(descriptorStructure);
        }
    }

    public class DescriptorStructure : IDescriptor
    {
        public DescriptorStructure(long id, string name, int structureType, DataType keyDataType, DataType recordDataType, Type keyType, Type recordType, DateTime createTime, DateTime modifiedTime, DateTime accessTime, byte[] tag)
        {
            ID = id;
            Name = name;
            StructureType = structureType;

            KeyDataType = keyDataType;
            RecordDataType = recordDataType;

            KeyType = keyType;
            RecordType = recordType;

            CreateTime = createTime;
            ModifiedTime = modifiedTime;
            AccessTime = accessTime;

            Tag = tag;
        }

        #region IDescriptor

        public long ID { get; set; }
        public string Name { get; set; }
        public int StructureType { get; set; }

        public DataType KeyDataType { get; set; }
        public DataType RecordDataType { get; set; }

        public Type KeyType { get; set; }
        public Type RecordType { get; set; }

        public IComparer<IData> KeyComparer { get; set; }
        public IEqualityComparer<IData> KeyEqualityComparer { get; set; }

        public IPersist<IData> KeyPersist { get; set; }
        public IPersist<IData> RecordPersist { get; set; }

        public IIndexerPersist<IData> KeyIndexerPersist { get; set; }
        public IIndexerPersist<IData> RecordIndexerPersist { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime AccessTime { get; set; }

        public byte[] Tag { get; set; }

        //public byte[] Tag
        //{
        //    get
        //    {
        //        //if (ForSerialize)
        //        //    return tag;

        //        IDescriptor descriptor = GetDescriptor(this);
        //        tag = descriptor.Tag;

        //        return tag;
        //    }
        //    set
        //    {
        //        //this.ForSerialize = true;
        //        //this.tag = value;

        //        //IDescriptor descriptor = SetDescriptor(this);

        //        //tag = descriptor.Tag;
        //        //this.ForSerialize = false;
        //    }
        //}

        #endregion

        public void Serialize(BinaryWriter writer)
        {
            CountCompression.Serialize(writer, (ulong)ID);
            writer.Write(Name);

            CountCompression.Serialize(writer, (ulong)StructureType);

            KeyDataType.Serialize(writer);
            RecordDataType.Serialize(writer);

            CountCompression.Serialize(writer, (ulong)CreateTime.Ticks);
            CountCompression.Serialize(writer, (ulong)ModifiedTime.Ticks);
            CountCompression.Serialize(writer, (ulong)AccessTime.Ticks);

            if (Tag == null)
                CountCompression.Serialize(writer, 0);
            else
            {
                CountCompression.Serialize(writer, (ulong)Tag.Length + 1);
                writer.Write(Tag);
            }
        }

        public static DescriptorStructure Deserialize(BinaryReader reader)
        {
            long id = (long)CountCompression.Deserialize(reader);
            string name = reader.ReadString();

            int structureType = (int)CountCompression.Deserialize(reader);

            var keyDataType = DataType.Deserialize(reader);
            var recordDataType = DataType.Deserialize(reader);

            var keyType = DataTypeUtils.BuildType(keyDataType);
            var recordType = DataTypeUtils.BuildType(recordDataType);

            var createTime = new DateTime((long)CountCompression.Deserialize(reader));
            var modifiedTime = new DateTime((long)CountCompression.Deserialize(reader));
            var accessTime = new DateTime((long)CountCompression.Deserialize(reader));

            var tagLength = (int)CountCompression.Deserialize(reader) - 1;
            byte[] tag = tagLength >= 0 ? reader.ReadBytes(tagLength) : null;

            return new DescriptorStructure(id, name, structureType, keyDataType, recordDataType, keyType, recordType, createTime, modifiedTime, accessTime, tag);
        }
    }
}