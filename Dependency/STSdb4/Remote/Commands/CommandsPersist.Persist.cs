using Iveely.Data;
using Iveely.General.Compression;
using Iveely.WaterfallTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iveely.Remote.Commands
{
    public partial class CommandPersist
    {
        #region XIndex Commands

        private void WriteReplaceCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (ReplaceCommand)command;

            KeyPersist.Write(writer, cmd.Key);
            RecordPersist.Write(writer, cmd.Record);
        }

        private ReplaceCommand ReadReplaceCommand(BinaryReader reader)
        {
            return new ReplaceCommand(KeyPersist.Read(reader), RecordPersist.Read(reader));
        }

        private void WriteDeleteCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (DeleteCommand)command;

            KeyPersist.Write(writer, cmd.Key);
        }

        private DeleteCommand ReadDeleteCommand(BinaryReader reader)
        {
            return new DeleteCommand(KeyPersist.Read(reader));
        }

        private void WriteDeleteRangeCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (DeleteRangeCommand)command;

            KeyPersist.Write(writer, cmd.FromKey);
            KeyPersist.Write(writer, cmd.ToKey);
        }

        private DeleteRangeCommand ReadDeleteRangeCommand(BinaryReader reader)
        {
            return new DeleteRangeCommand(KeyPersist.Read(reader), KeyPersist.Read(reader));
        }

        private void WriteClearCommand(BinaryWriter writer, ICommand command)
        {
        }

        private ClearCommand ReadClearCommand(BinaryReader reader)
        {
            return new ClearCommand();
        }

        private void WriteInsertOrIgnoreCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (InsertOrIgnoreCommand)command;

            KeyPersist.Write(writer, cmd.Key);
            RecordPersist.Write(writer, cmd.Record);
        }

        private InsertOrIgnoreCommand ReadInsertOrIgnoreCommand(BinaryReader reader)
        {
            return new InsertOrIgnoreCommand(KeyPersist.Read(reader), RecordPersist.Read(reader));
        }

        private void WriteTryGetCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (TryGetCommand)command;

            KeyPersist.Write(writer, cmd.Key);

            writer.Write(cmd.Record != null);
            if (cmd.Record != null)
                RecordPersist.Write(writer, cmd.Record);
        }

        private TryGetCommand ReadTryGetCommand(BinaryReader reader)
        {
            return new TryGetCommand(KeyPersist.Read(reader), reader.ReadBoolean() ? RecordPersist.Read(reader) : null);
        }

        private void WriteForwardCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (ForwardCommand)command;

            writer.Write(cmd.PageCount);

            writer.Write(cmd.FromKey != null);
            if (cmd.FromKey != null)
                KeyPersist.Write(writer, cmd.FromKey);

            writer.Write(cmd.ToKey != null);
            if (cmd.ToKey != null)
                KeyPersist.Write(writer, cmd.ToKey);

            writer.Write(cmd.List != null);
            if (cmd.List != null)
                SerializeList(writer, cmd.List, cmd.List.Count);
        }

        private ForwardCommand ReadForwardCommand(BinaryReader reader)
        {
            int pageCount = reader.ReadInt32();
            IData from = reader.ReadBoolean() ? KeyPersist.Read(reader) : null;
            IData to = reader.ReadBoolean() ? KeyPersist.Read(reader) : null;
            List<KeyValuePair<IData, IData>> list = reader.ReadBoolean() ? DeserializeList(reader) : null;

            return new ForwardCommand(pageCount, from, to, list);
        }

        private void WriteBackwardCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (BackwardCommand)command;

            writer.Write(cmd.PageCount);

            writer.Write(cmd.FromKey != null);
            if (cmd.FromKey != null)
                KeyPersist.Write(writer, cmd.FromKey);

            writer.Write(cmd.ToKey != null);
            if (cmd.ToKey != null)
                KeyPersist.Write(writer, cmd.ToKey);

            writer.Write(cmd.List != null);
            if (cmd.List != null)
                SerializeList(writer, cmd.List, cmd.List.Count);
        }

        private BackwardCommand ReadBackwardCommand(BinaryReader reader)
        {
            int pageCount = reader.ReadInt32();
            IData from = reader.ReadBoolean() ? KeyPersist.Read(reader) : null;
            IData to = reader.ReadBoolean() ? KeyPersist.Read(reader) : null;
            List<KeyValuePair<IData, IData>> list = reader.ReadBoolean() ? DeserializeList(reader) : null;

            return new BackwardCommand(pageCount, from, to, list);
        }

        private void WriteFindNextCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (FindNextCommand)command;

            KeyPersist.Write(writer, cmd.Key);

            writer.Write(cmd.KeyValue.HasValue);
            if (cmd.KeyValue.HasValue)
            {
                KeyPersist.Write(writer, cmd.KeyValue.Value.Key);
                RecordPersist.Write(writer, cmd.KeyValue.Value.Value);
            }
        }

        private FindNextCommand ReadFindNextCommand(BinaryReader reader)
        {
            IData Key = KeyPersist.Read(reader);

            bool hasValue = reader.ReadBoolean();
            IData key = hasValue ? KeyPersist.Read(reader) : null;
            IData rec = hasValue ? RecordPersist.Read(reader) : null;

            return new FindNextCommand(Key, hasValue ? (KeyValuePair<IData, IData>?)new KeyValuePair<IData, IData>(key, rec) : null);
        }

        private void WriteFindAfterCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (FindAfterCommand)command;

            KeyPersist.Write(writer, cmd.Key);

            writer.Write(cmd.KeyValue.HasValue);
            if (cmd.KeyValue.HasValue)
            {
                KeyPersist.Write(writer, cmd.KeyValue.Value.Key);
                RecordPersist.Write(writer, cmd.KeyValue.Value.Value);
            }
        }

        private FindAfterCommand ReadFindAfterCommand(BinaryReader reader)
        {
            IData Key = KeyPersist.Read(reader);

            bool hasValue = (reader.ReadBoolean());
            IData key = hasValue ? KeyPersist.Read(reader) : null;
            IData rec = hasValue ? RecordPersist.Read(reader) : null;

            return new FindAfterCommand(Key, hasValue ? (KeyValuePair<IData, IData>?)new KeyValuePair<IData, IData>(key, rec) : null);
        }

        private void WriteFindPrevCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (FindPrevCommand)command;

            KeyPersist.Write(writer, cmd.Key);

            writer.Write(cmd.KeyValue.HasValue);
            if (cmd.KeyValue.HasValue)
            {
                KeyPersist.Write(writer, cmd.KeyValue.Value.Key);
                RecordPersist.Write(writer, cmd.KeyValue.Value.Value);
            }
        }

        private FindPrevCommand ReadFindPrevCommand(BinaryReader reader)
        {
            IData Key = KeyPersist.Read(reader);

            bool hasValue = (reader.ReadBoolean());
            IData key = hasValue ? KeyPersist.Read(reader) : null;
            IData rec = hasValue ? RecordPersist.Read(reader) : null;

            return new FindPrevCommand(Key, hasValue ? (KeyValuePair<IData, IData>?)new KeyValuePair<IData, IData>(key, rec) : null);
        }

        private void WriteFindBeforeCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (FindBeforeCommand)command;

            KeyPersist.Write(writer, cmd.Key);

            writer.Write(cmd.KeyValue.HasValue);
            if (cmd.KeyValue.HasValue)
            {
                KeyPersist.Write(writer, cmd.KeyValue.Value.Key);
                RecordPersist.Write(writer, cmd.KeyValue.Value.Value);
            }
        }

        private FindBeforeCommand ReadFindBeforeCommand(BinaryReader reader)
        {
            IData Key = KeyPersist.Read(reader);

            bool hasValue = (reader.ReadBoolean());
            IData key = hasValue ? KeyPersist.Read(reader) : null;
            IData rec = hasValue ? RecordPersist.Read(reader) : null;

            return new FindBeforeCommand(Key, hasValue ? (KeyValuePair<IData, IData>?)new KeyValuePair<IData, IData>(key, rec) : null);
        }

        private void WriteFirstRowCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (FirstRowCommand)command;

            writer.Write(cmd.Row.HasValue);
            if (cmd.Row.HasValue)
            {
                KeyPersist.Write(writer, cmd.Row.Value.Key);
                RecordPersist.Write(writer, cmd.Row.Value.Value);
            }
        }

        private FirstRowCommand ReadFirstRowCommand(BinaryReader reader)
        {
            bool hasValue = (reader.ReadBoolean());
            IData key = hasValue ? KeyPersist.Read(reader) : null;
            IData rec = hasValue ? RecordPersist.Read(reader) : null;

            return new FirstRowCommand(hasValue ? (KeyValuePair<IData, IData>?)new KeyValuePair<IData, IData>(key, rec) : null);
        }

        private void WriteLastRowCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (LastRowCommand)command;

            writer.Write(cmd.Row.HasValue);
            if (cmd.Row.HasValue)
            {
                KeyPersist.Write(writer, cmd.Row.Value.Key);
                RecordPersist.Write(writer, cmd.Row.Value.Value);
            }
        }

        private LastRowCommand ReadLastRowCommand(BinaryReader reader)
        {
            bool hasValue = (reader.ReadBoolean());
            IData key = hasValue ? KeyPersist.Read(reader) : null;
            IData rec = hasValue ? RecordPersist.Read(reader) : null;

            return new LastRowCommand(hasValue ? (KeyValuePair<IData, IData>?)new KeyValuePair<IData, IData>(key, rec) : null);
        }

        private void WriteCountCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (CountCommand)command;
            writer.Write(cmd.Count);
        }

        private CountCommand ReadCountCommand(BinaryReader reader)
        {
            return new CountCommand(reader.ReadInt64());
        }

        private void WriteXIndexDescriptorGetCommand(BinaryWriter writer, ICommand command)
        {
            XTableDescriptorGetCommand cmd = (XTableDescriptorGetCommand)command;
            IDescriptor descriptor = cmd.Descriptor;

            writer.Write(descriptor != null);

            if (descriptor != null)
                SerializeDescriptor(writer, descriptor);
        }

        private XTableDescriptorGetCommand ReadXIndexDescriptorGetCommand(BinaryReader reader)
        {
            IDescriptor description = null;

            if (reader.ReadBoolean()) // Description != null
                description = Descriptor.Deserialize(reader);

            return new XTableDescriptorGetCommand(description);
        }

        private void WriteXIndexDescriptorSetCommand(BinaryWriter writer, ICommand command)
        {
            XTableDescriptorSetCommand cmd = (XTableDescriptorSetCommand)command;
            Descriptor descriptor = (Descriptor)cmd.Descriptor;

            writer.Write(descriptor != null);

            if (descriptor != null)
                descriptor.Serialize(writer);
        }

        private XTableDescriptorSetCommand ReadXIndexDescriptorSetCommand(BinaryReader reader)
        {
            IDescriptor descriptor = null;

            if (reader.ReadBoolean()) // Descriptor != null
                descriptor = Descriptor.Deserialize(reader);

            return new XTableDescriptorSetCommand(descriptor);
        }

        #endregion

        #region Storage EngineCommands

        private void WriteStorageEngineCommitCommand(BinaryWriter writer, ICommand command)
        {
        }

        private StorageEngineCommitCommand ReadStorageEngineCommitCommand(BinaryReader reader)
        {
            return new StorageEngineCommitCommand();
        }

        private void WriteStorageEngineGetEnumeratorCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineGetEnumeratorCommand)command;

            if (cmd.Descriptions == null)
                writer.Write(true);
            else
            {
                writer.Write(false);

                int listCount = cmd.Descriptions.Count;
                CountCompression.Serialize(writer, (ulong)listCount);

                for (int i = 0; i < listCount; i++)
                    SerializeDescriptor(writer, cmd.Descriptions[i]);
            }
        }

        private StorageEngineGetEnumeratorCommand ReadStorageEngineGetEnumeratorCommand(BinaryReader reader)
        {
            bool isListNull = reader.ReadBoolean();
            List<IDescriptor> descriptions = new List<IDescriptor>();

            if (!isListNull)
            {
                int listCount = (int)CountCompression.Deserialize(reader);

                for (int i = 0; i < listCount; i++)
                    descriptions.Add((Descriptor)DeserializeDescriptor(reader));
            }

            return new StorageEngineGetEnumeratorCommand(descriptions);
        }

        private void WriteStorageEngineRenameCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineRenameCommand)command;

            writer.Write(cmd.Name);
            writer.Write(cmd.NewName);
        }

        private StorageEngineRenameCommand ReadStorageEngineRenameCommand(BinaryReader reader)
        {
            string name = reader.ReadString();
            string newName = reader.ReadString();

            return new StorageEngineRenameCommand(name, newName);
        }

        private void WriteStorageEngineExistCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineExistsCommand)command;

            writer.Write(cmd.Name);
            writer.Write(cmd.Exist);
        }

        private StorageEngineExistsCommand ReadStorageEngineExistCommand(BinaryReader reader)
        {
            string name = reader.ReadString();
            bool exist = reader.ReadBoolean();

            return new StorageEngineExistsCommand(exist, name);
        }

        private void WriteStorageEngineFindByIDCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineFindByIDCommand)command;

            writer.Write(cmd.ID);

            writer.Write(cmd.Descriptor != null);
            if (cmd.Descriptor != null)
                SerializeDescriptor(writer, cmd.Descriptor);
        }

        private StorageEngineFindByIDCommand ReadStorageEngineFindByIDCommand(BinaryReader reader)
        {
            long id = reader.ReadInt64();
            var schemeRecord = reader.ReadBoolean() ? DeserializeDescriptor(reader) : null;

            return new StorageEngineFindByIDCommand(schemeRecord, id);
        }

        private void WriteStorageEngineOpenXIndexCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineOpenXIndexCommand)command;

            writer.Write(cmd.ID);
            if (cmd.ID < 0)
            {
                cmd.KeyType.Serialize(writer);
                cmd.RecordType.Serialize(writer);

                writer.Write(cmd.Name);
            }
        }

        private StorageEngineOpenXIndexCommand ReadStorageEngineOpenXIndexCommand(BinaryReader reader)
        {
            long id = reader.ReadInt64();

            if (id < 0)
            {
                var keyType = DataType.Deserialize(reader);
                var recordType = DataType.Deserialize(reader);

                string name = reader.ReadString();

                return new StorageEngineOpenXIndexCommand(name, keyType, recordType);
            }

            return new StorageEngineOpenXIndexCommand(id);
        }

        private void WriteStorageEngineOpenXFileCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineOpenXFileCommand)command;

            writer.Write(cmd.Name == null);
            if (cmd.Name == null)
                writer.Write(cmd.ID);
            else
                writer.Write(cmd.Name);
        }

        private StorageEngineOpenXFileCommand ReadStorageEngineOpenXFileCommand(BinaryReader reader)
        {
            if (reader.ReadBoolean())
                return new StorageEngineOpenXFileCommand(reader.ReadInt64());
            else
                return new StorageEngineOpenXFileCommand(reader.ReadString());
        }

        private void WriteStorageEngineDeleteCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineDeleteCommand)command;
            writer.Write(cmd.Name);
        }

        private StorageEngineDeleteCommand ReadStorageEngineDeleteCommand(BinaryReader reader)
        {
            return new StorageEngineDeleteCommand(reader.ReadString());
        }

        private void WriteStorageEngineCountCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineCountCommand)command;
            CountCompression.Serialize(writer, (ulong)cmd.Count);
        }

        private StorageEngineCountCommand ReadStorageEngineCountCommand(BinaryReader reader)
        {
            return new StorageEngineCountCommand((int)CountCompression.Deserialize(reader));
        }

        private void WriteStorageEngineFindByNameCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (StorageEngineFindByNameCommand)command;

            writer.Write(cmd.Name);
            writer.Write(cmd.Descriptor != null);

            if (cmd.Descriptor != null)
                SerializeDescriptor(writer, cmd.Descriptor);
        }

        private StorageEngineFindByNameCommand ReadStorageEngineFindByNameCommand(BinaryReader reader)
        {
            string name = reader.ReadString();
            var description = reader.ReadBoolean() ? DeserializeDescriptor(reader) : null;

            return new StorageEngineFindByNameCommand(name, description);
        }

        private void WriteStorageEngineDescriptionCommand(BinaryWriter writer, ICommand command)
        {
            StorageEngineDescriptionCommand cmd = (StorageEngineDescriptionCommand)command;
            IDescriptor description = cmd.Descriptor;

            writer.Write(description != null);

            if (description != null)
                SerializeDescriptor(writer, description);
        }

        private StorageEngineDescriptionCommand ReadStorageEngineDescriptionCommand(BinaryReader reader)
        {
            IDescriptor description = null;

            if (reader.ReadBoolean()) // Description != null
                description = DeserializeDescriptor(reader);

            return new StorageEngineDescriptionCommand(description);
        }

        private void WriteStorageEngineGetCacheCommand(BinaryWriter writer, ICommand command)
        {
            StorageEngineGetCacheSizeCommand cmd = (StorageEngineGetCacheSizeCommand)command;

            writer.Write(cmd.CacheSize);
        }

        private StorageEngineGetCacheSizeCommand ReadStorageEngineGetCacheSizeCommand(BinaryReader reader)
        {
            int cacheSize = reader.ReadInt32();

            return new StorageEngineGetCacheSizeCommand(cacheSize);
        }

        private void WriteStorageEngineSetCacheCommand(BinaryWriter writer, ICommand command)
        {
            StorageEngineSetCacheSizeCommand cmd = (StorageEngineSetCacheSizeCommand)command;

            writer.Write(cmd.CacheSize);
        }

        private StorageEngineSetCacheSizeCommand ReadStorageEngineSetCacheCommand(BinaryReader reader)
        {
            int cacheSize = reader.ReadInt32();

            return new StorageEngineSetCacheSizeCommand(cacheSize);
        }

        #endregion

        #region HeapCommands

        private void WriteHeapObtainNewHandleCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapObtainNewHandleCommand)command;
            writer.Write(cmd.Handle);
        }

        private HeapObtainNewHandleCommand ReadHeapObtainNewHandleCommand(BinaryReader reader)
        {
            return new HeapObtainNewHandleCommand(reader.ReadInt64());
        }

        private void WriteHeapReleaseHandleCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapReleaseHandleCommand)command;
            writer.Write(cmd.Handle);
        }

        private HeapReleaseHandleCommand ReadHeapReleaseHandleCommand(BinaryReader reader)
        {
            return new HeapReleaseHandleCommand(reader.ReadInt64());
        }

        private void WriteHeapExistsHandleCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapExistsHandleCommand)command;
            writer.Write(cmd.Handle);
            writer.Write(cmd.Exist);
        }

        private HeapExistsHandleCommand ReadHeapExistsHandleCommand(BinaryReader reader)
        {
            return new HeapExistsHandleCommand(reader.ReadInt64(), reader.ReadBoolean());
        }

        private void WriteHeapWriteCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapWriteCommand)command;

            writer.Write(cmd.Handle);

            writer.Write(cmd.Count);
            writer.Write(cmd.Index);

            if (cmd.Buffer == null)
                writer.Write(false);
            else
            {
                writer.Write(true);
                writer.Write(cmd.Buffer.Length);
                writer.Write(cmd.Buffer, 0, cmd.Buffer.Length);
            }
        }

        private HeapWriteCommand ReadHeapWriteCommand(BinaryReader reader)
        {
            var handle = reader.ReadInt64();

            var count = reader.ReadInt32();
            var index = reader.ReadInt32();

            byte[] buffer = null; ;
            if (reader.ReadBoolean())
            {
                buffer = new byte[reader.ReadInt32()];
                reader.Read(buffer, 0, buffer.Length);
            }

            return new HeapWriteCommand(handle, buffer, index, count);
        }

        private void WriteHeapReadCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapReadCommand)command;

            writer.Write(cmd.Handle);

            if (cmd.Buffer == null)
                writer.Write(false);
            else
            {
                writer.Write(true);
                writer.Write(cmd.Buffer.Length);
                writer.Write(cmd.Buffer);
            }
        }

        private HeapReadCommand ReadHeapReadCommand(BinaryReader reader)
        {
            var handle = reader.ReadInt64();

            byte[] buffer = null;
            if (reader.ReadBoolean())
            {
                int count = reader.ReadInt32();
                buffer = reader.ReadBytes(count);
            }

            return new HeapReadCommand(handle, buffer);
        }

        private void WriteHeapCommitCommand(BinaryWriter writer, ICommand command)
        {
        }

        private HeapCommitCommand ReadHeapCommitCommand(BinaryReader reader)
        {
            return new HeapCommitCommand();
        }

        private void WriteHeapCloseCommand(BinaryWriter writer, ICommand command)
        {
        }

        private HeapCloseCommand ReadHeapCloseCommand(BinaryReader reader)
        {
            return new HeapCloseCommand();
        }

        public void WriteHeapSetTagCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapSetTagCommand)command;

            if (cmd.Buffer == null)
                writer.Write(false);
            else
            {
                writer.Write(true);
                writer.Write(cmd.Buffer.Length);
                writer.Write(cmd.Buffer);
            }
        }

        public HeapSetTagCommand ReadHeapSetTagCommand(BinaryReader reader)
        {
            byte[] buffer = null;
            if (reader.ReadBoolean())
            {
                int count = reader.ReadInt32();
                buffer = new byte[count];

                reader.Read(buffer, 0, count);
            }

            return new HeapSetTagCommand(buffer);
        }

        public void WriteHeapGetTagCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapGetTagCommand)command;

            if (cmd.Tag == null)
                writer.Write(false);
            else
            {
                writer.Write(true);
                writer.Write(cmd.Tag.Length);
                writer.Write(cmd.Tag);
            }
        }

        public HeapGetTagCommand ReadHeapGetTagCommand(BinaryReader reader)
        {
            byte[] tag = null;
            if (reader.ReadBoolean())
            {
                int count = reader.ReadInt32();
                tag = new byte[count];

                reader.Read(tag, 0, count);
            }

            return new HeapGetTagCommand(tag);
        }

        public void WriteHeapDataSizeCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapDataSizeCommand)command;

            writer.Write(cmd.DataSize);
        }

        public HeapDataSizeCommand ReadHeapDataSizeCommand(BinaryReader reader)
        {
            return new HeapDataSizeCommand(reader.ReadInt64());
        }

        public void WriteHeapSizeCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (HeapSizeCommand)command;

            writer.Write(cmd.Size);
        }

        public HeapSizeCommand ReadHeapSizeCommand(BinaryReader reader)
        {
            return new HeapSizeCommand(reader.ReadInt64());
        }

        #endregion

        #region Other Commands

        private void WriteExceptionCommand(BinaryWriter writer, ICommand command)
        {
            var cmd = (ExceptionCommand)command;
            writer.Write(cmd.Exception);
        }

        private ExceptionCommand ReadExceptionCommand(BinaryReader reader)
        {
            return new ExceptionCommand(reader.ReadString());
        }

        #endregion

        #region Helper Methods

        private void SerializeList(BinaryWriter writer, List<KeyValuePair<IData, IData>> list, int count)
        {
            writer.Write(count);

            foreach (var kv in list)
            {
                KeyPersist.Write(writer, kv.Key);
                RecordPersist.Write(writer, kv.Value);
            }
        }

        private List<KeyValuePair<IData, IData>> DeserializeList(BinaryReader reader)
        {
            int count = reader.ReadInt32();

            List<KeyValuePair<IData, IData>> list = new List<KeyValuePair<IData, IData>>(count);
            for (int i = 0; i < count; i++)
            {
                IData key = KeyPersist.Read(reader);
                IData rec = RecordPersist.Read(reader);

                list.Add(new KeyValuePair<IData, IData>(key, rec));
            }

            return list;
        }

        private void SerializeDescriptor(BinaryWriter writer, IDescriptor description)
        {
            CountCompression.Serialize(writer, (ulong)description.ID);
            writer.Write(description.Name);

            CountCompression.Serialize(writer, (ulong)description.StructureType);

            description.KeyDataType.Serialize(writer);
            description.RecordDataType.Serialize(writer);

            CountCompression.Serialize(writer, (ulong)description.CreateTime.Ticks);
            CountCompression.Serialize(writer, (ulong)description.ModifiedTime.Ticks);
            CountCompression.Serialize(writer, (ulong)description.AccessTime.Ticks);

            if (description.Tag == null)
                CountCompression.Serialize(writer, 0);
            else
            {
                CountCompression.Serialize(writer, (ulong)description.Tag.Length + 1);
                writer.Write(description.Tag);
            }
        }

        private IDescriptor DeserializeDescriptor(BinaryReader reader)
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

            return new Descriptor(id, name, structureType, keyDataType, recordDataType, keyType, recordType, createTime, modifiedTime, accessTime, tag);
        }

        #endregion
    }
}

