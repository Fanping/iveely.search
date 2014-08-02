using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iveely.STSdb4.General.Persist;
using Iveely.STSdb4.Data;

namespace Iveely.STSdb4.Remote.Commands
{
    public class CommandCollectionPersist : ICommandCollectionPersist
    {
        public IPersist<ICommand> Persist { get; private set; }

        public CommandCollectionPersist(IPersist<ICommand> persist)
        {
            Persist = persist;
        }

        public void Write(BinaryWriter writer, CommandCollection collection)
        {
            int collectionCount = collection.Count;
            int commonAction = collection.CommonAction;

            writer.Write(collectionCount);
            writer.Write(commonAction);

            if (collectionCount > 1 && commonAction > 0)
            {
                switch (commonAction)
                {
                    case CommandCode.REPLACE:
                    case CommandCode.INSERT_OR_IGNORE:
                    case CommandCode.DELETE:
                    case CommandCode.DELETE_RANGE:
                    case CommandCode.CLEAR:
                        {
                            for (int i = 0; i < collectionCount; i++)
                                Persist.Write(writer, collection[i]);
                        }
                        break;

                    default:
                        throw new NotImplementedException("Command is not implemented");
                }
            }
            else
            {
                foreach (var command in collection)
                    Persist.Write(writer, command);
            }
        }

        public CommandCollection Read(BinaryReader reader)
        {
            int collectionCount = reader.ReadInt32();
            int commonAction = reader.ReadInt32();

            CommandCollection collection = new CommandCollection(collectionCount);

            if (collectionCount > 1 && commonAction > 0)
            {
                switch (commonAction)
                {
                    case CommandCode.REPLACE:
                    case CommandCode.INSERT_OR_IGNORE:
                    case CommandCode.DELETE:
                    case CommandCode.DELETE_RANGE:
                    case CommandCode.CLEAR:
                        {
                            for (int i = 0; i < collectionCount; i++)
                                collection.Add(Persist.Read(reader));
                        }
                        break;

                    default:
                        throw new NotImplementedException("Command is not implemented");
                }
            }
            else
            {
                for (int i = 0; i < collectionCount; i++)
                    collection.Add(Persist.Read(reader));
            }

            return collection;
        }
    }

    public partial class CommandPersist : IPersist<ICommand>
    {
        private Action<BinaryWriter, ICommand>[] writes;
        private Func<BinaryReader, ICommand>[] reads;

        public IPersist<IData> KeyPersist { get; private set; }
        public IPersist<IData> RecordPersist { get; private set; }

        public CommandPersist(IPersist<IData> keyPersist, IPersist<IData> recordPersist)
        {
            KeyPersist = keyPersist;
            RecordPersist = recordPersist;

            // XTable writers
            writes = new Action<BinaryWriter, ICommand>[CommandCode.MAX];
            writes[CommandCode.REPLACE] = WriteReplaceCommand;
            writes[CommandCode.DELETE] = WriteDeleteCommand;
            writes[CommandCode.DELETE_RANGE] = WriteDeleteRangeCommand;
            writes[CommandCode.INSERT_OR_IGNORE] = WriteInsertOrIgnoreCommand;
            writes[CommandCode.CLEAR] = WriteClearCommand;
            writes[CommandCode.TRY_GET] = WriteTryGetCommand;
            writes[CommandCode.FORWARD] = WriteForwardCommand;
            writes[CommandCode.BACKWARD] = WriteBackwardCommand;
            writes[CommandCode.FIND_NEXT] = WriteFindNextCommand;
            writes[CommandCode.FIND_AFTER] = WriteFindAfterCommand;
            writes[CommandCode.FIND_PREV] = WriteFindPrevCommand;
            writes[CommandCode.FIND_BEFORE] = WriteFindBeforeCommand;
            writes[CommandCode.FIRST_ROW] = WriteFirstRowCommand;
            writes[CommandCode.LAST_ROW] = WriteLastRowCommand;
            writes[CommandCode.COUNT] = WriteCountCommand;
            writes[CommandCode.XTABLE_DESCRIPTOR_GET] = WriteXIndexDescriptorGetCommand;
            writes[CommandCode.XTABLE_DESCRIPTOR_SET] = WriteXIndexDescriptorSetCommand;

            // XTable reads
            reads = new Func<BinaryReader, ICommand>[CommandCode.MAX];
            reads[CommandCode.REPLACE] = ReadReplaceCommand;
            reads[CommandCode.DELETE] = ReadDeleteCommand;
            reads[CommandCode.DELETE_RANGE] = ReadDeleteRangeCommand;
            reads[CommandCode.INSERT_OR_IGNORE] = ReadInsertOrIgnoreCommand;
            reads[CommandCode.CLEAR] = ReadClearCommand;
            reads[CommandCode.TRY_GET] = ReadTryGetCommand;
            reads[CommandCode.FORWARD] = ReadForwardCommand;
            reads[CommandCode.BACKWARD] = ReadBackwardCommand;
            reads[CommandCode.FIND_NEXT] = ReadFindNextCommand;
            reads[CommandCode.FIND_AFTER] = ReadFindAfterCommand;
            reads[CommandCode.FIND_PREV] = ReadFindPrevCommand;
            reads[CommandCode.FIND_BEFORE] = ReadFindBeforeCommand;
            reads[CommandCode.FIRST_ROW] = ReadFirstRowCommand;
            reads[CommandCode.LAST_ROW] = ReadLastRowCommand;
            reads[CommandCode.COUNT] = ReadCountCommand;
            reads[CommandCode.XTABLE_DESCRIPTOR_GET] = ReadXIndexDescriptorGetCommand;
            reads[CommandCode.XTABLE_DESCRIPTOR_SET] = ReadXIndexDescriptorSetCommand;

            // Storage engine writes
            writes[CommandCode.STORAGE_ENGINE_COMMIT] = WriteStorageEngineCommitCommand;
            writes[CommandCode.STORAGE_ENGINE_GET_ENUMERATOR] = WriteStorageEngineGetEnumeratorCommand;
            writes[CommandCode.STORAGE_ENGINE_RENAME] = WriteStorageEngineRenameCommand;
            writes[CommandCode.STORAGE_ENGINE_EXISTS] = WriteStorageEngineExistCommand;
            writes[CommandCode.STORAGE_ENGINE_FIND_BY_ID] = WriteStorageEngineFindByIDCommand;
            writes[CommandCode.STORAGE_ENGINE_FIND_BY_NAME] = WriteStorageEngineFindByNameCommand;
            writes[CommandCode.STORAGE_ENGINE_OPEN_XTABLE] = WriteStorageEngineOpenXIndexCommand;
            writes[CommandCode.STORAGE_ENGINE_OPEN_XFILE] = WriteStorageEngineOpenXFileCommand;
            writes[CommandCode.STORAGE_ENGINE_DELETE] = WriteStorageEngineDeleteCommand;
            writes[CommandCode.STORAGE_ENGINE_COUNT] = WriteStorageEngineCountCommand;
            writes[CommandCode.STORAGE_ENGINE_DESCRIPTOR] = WriteStorageEngineDescriptionCommand;
            writes[CommandCode.STORAGE_ENGINE_GET_CACHE_SIZE] = WriteStorageEngineGetCacheCommand;
            writes[CommandCode.STORAGE_ENGINE_SET_CACHE_SIZE] = WriteStorageEngineSetCacheCommand;

            // Storage engine reads
            reads[CommandCode.STORAGE_ENGINE_COMMIT] = ReadStorageEngineCommitCommand;
            reads[CommandCode.STORAGE_ENGINE_GET_ENUMERATOR] = ReadStorageEngineGetEnumeratorCommand;
            reads[CommandCode.STORAGE_ENGINE_RENAME] = ReadStorageEngineRenameCommand;
            reads[CommandCode.STORAGE_ENGINE_EXISTS] = ReadStorageEngineExistCommand;
            reads[CommandCode.STORAGE_ENGINE_FIND_BY_ID] = ReadStorageEngineFindByIDCommand;
            reads[CommandCode.STORAGE_ENGINE_FIND_BY_NAME] = ReadStorageEngineFindByNameCommand;
            reads[CommandCode.STORAGE_ENGINE_OPEN_XTABLE] = ReadStorageEngineOpenXIndexCommand;
            reads[CommandCode.STORAGE_ENGINE_OPEN_XFILE] = ReadStorageEngineOpenXFileCommand;
            reads[CommandCode.STORAGE_ENGINE_DELETE] = ReadStorageEngineDeleteCommand;
            reads[CommandCode.STORAGE_ENGINE_COUNT] = ReadStorageEngineCountCommand;
            reads[CommandCode.STORAGE_ENGINE_DESCRIPTOR] = ReadStorageEngineDescriptionCommand;
            reads[CommandCode.STORAGE_ENGINE_GET_CACHE_SIZE] = ReadStorageEngineGetCacheSizeCommand;
            reads[CommandCode.STORAGE_ENGINE_SET_CACHE_SIZE] = ReadStorageEngineSetCacheCommand;

            //Heap writes
            writes[CommandCode.HEAP_OBTAIN_NEW_HANDLE] = WriteHeapObtainNewHandleCommand;
            writes[CommandCode.HEAP_RELEASE_HANDLE] = WriteHeapReleaseHandleCommand;
            writes[CommandCode.HEAP_EXISTS_HANDLE] = WriteHeapExistsHandleCommand;
            writes[CommandCode.HEAP_WRITE] = WriteHeapWriteCommand;
            writes[CommandCode.HEAP_READ] = WriteHeapReadCommand;
            writes[CommandCode.HEAP_COMMIT] = WriteHeapCommitCommand;
            writes[CommandCode.HEAP_CLOSE] = WriteHeapCloseCommand;
            writes[CommandCode.HEAP_SET_TAG] = WriteHeapSetTagCommand;
            writes[CommandCode.HEAP_GET_TAG] = WriteHeapGetTagCommand;
            writes[CommandCode.HEAP_DATA_SIZE] = WriteHeapDataSizeCommand;
            writes[CommandCode.HEAP_SIZE] = WriteHeapSizeCommand;

            //Heap reads
            reads[CommandCode.HEAP_OBTAIN_NEW_HANDLE] = ReadHeapObtainNewHandleCommand;
            reads[CommandCode.HEAP_RELEASE_HANDLE] = ReadHeapReleaseHandleCommand;
            reads[CommandCode.HEAP_EXISTS_HANDLE] = ReadHeapExistsHandleCommand;
            reads[CommandCode.HEAP_WRITE] = ReadHeapWriteCommand;
            reads[CommandCode.HEAP_READ] = ReadHeapReadCommand;
            reads[CommandCode.HEAP_COMMIT] = ReadHeapCommitCommand;
            reads[CommandCode.HEAP_CLOSE] = ReadHeapCloseCommand;
            reads[CommandCode.HEAP_SET_TAG] = ReadHeapSetTagCommand;
            reads[CommandCode.HEAP_GET_TAG] = ReadHeapGetTagCommand;
            reads[CommandCode.HEAP_DATA_SIZE] = ReadHeapDataSizeCommand;
            reads[CommandCode.HEAP_SIZE] = ReadHeapSizeCommand;

            writes[CommandCode.EXCEPTION] = WriteExceptionCommand;
            reads[CommandCode.EXCEPTION] = ReadExceptionCommand;
        }

        public void Write(BinaryWriter writer, ICommand item)
        {
            writer.Write(item.Code);
            writes[item.Code](writer, item);
        }

        public ICommand Read(BinaryReader reader)
        {
            int code = reader.ReadInt32();

            return reads[code](reader);
        }
    }
}