using Iveely.Data;
using Iveely.Database;
using Iveely.General.Communication;
using Iveely.Remote.Commands;
using Iveely.WaterfallTree;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Iveely.Remote
{
    public class StorageEngineClient : IStorageEngine
    {
        private int cacheSize;
        private ConcurrentDictionary<string, XTableRemote> indexes = new ConcurrentDictionary<string, XTableRemote>();

        public static readonly Descriptor StorageEngineDescriptor = new Descriptor(-1, "", DataType.Boolean, DataType.Boolean);
        public readonly ClientConnection ClientConnection;

        public StorageEngineClient(string machineName = "localhost", int port = 7182)
        {
            ClientConnection = new ClientConnection(machineName, port);
            ClientConnection.Start();

            Heap = new RemoteHeap(this);
        }

        #region IStorageEngine

        public ITable<TKey, TRecord> OpenXTablePortable<TKey, TRecord>(string name, DataType keyDataType, DataType recordDataType, ITransformer<TKey, IData> keyTransformer, ITransformer<TRecord, IData> recordTransformer)
        {
            var index = OpenXTablePortable(name, keyDataType, recordDataType);

            return new XTablePortable<TKey, TRecord>(index, keyTransformer, recordTransformer);
        }

        public ITable<IData, IData> OpenXTablePortable(string name, DataType keyType, DataType recordType)
        {
            var cmd = new StorageEngineOpenXIndexCommand(name, keyType, recordType);
            InternalExecute(cmd);

            var descriptor = new Descriptor(cmd.ID, name, keyType, recordType);

            var index = new XTableRemote(this, descriptor);
            indexes.TryAdd(name, index);

            return index;
        }

        public ITable<TKey, TRecord> OpenXTablePortable<TKey, TRecord>(string name)
        {
            var keyDataType = DataTypeUtils.BuildDataType(typeof(TKey));
            var recordDataType = DataTypeUtils.BuildDataType(typeof(TRecord));

            var keyTransformer = new DataTransformer<TKey>(typeof(TKey));
            var recordTransformer = new DataTransformer<TRecord>(typeof(TRecord));

            return OpenXTablePortable<TKey, TRecord>(name, keyDataType, recordDataType, null, null);
        }

        public ITable<TKey, TRecord> OpenXTable<TKey, TRecord>(string name)
        {
            return OpenXTablePortable<TKey, TRecord>(name);
        }

        public XFile OpenXFile(string name)
        {
            throw new NotSupportedException();
        }

        public void Rename(string name, string newName)
        {
            InternalExecute(new StorageEngineRenameCommand(name, newName));
        }

        public IDescriptor this[string name]
        {
            get
            {
                return indexes[name].Descriptor;
            }
        }

        public void Delete(string name)
        {
            var cmd = new StorageEngineDeleteCommand(name);
            InternalExecute(cmd);
        }

        public bool Exists(string name)
        {
            var cmd = new StorageEngineExistsCommand(name);
            InternalExecute(cmd);

            return cmd.Exist;
        }

        public int Count
        {
            get
            {
                var cmd = new StorageEngineCountCommand();
                InternalExecute(cmd);

                return cmd.Count;
            }
        }

        public IDescriptor Find(long id)
        {
            var cmd = new StorageEngineFindByIDCommand(null, id);
            InternalExecute(cmd);

            return cmd.Descriptor;
        }

        public IEnumerator<IDescriptor> GetEnumerator()
        {
            var cmd = new StorageEngineGetEnumeratorCommand();
            InternalExecute(cmd);

            return cmd.Descriptions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Commit()
        {
            foreach (var index in indexes.Values)
                index.Flush();

            InternalExecute(new StorageEngineCommitCommand());
        }

        public IHeap Heap { get; private set; }

        #endregion

        #region Server

        public CommandCollection Execute(IDescriptor descriptor, CommandCollection commands)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            Message message = new Message(descriptor, commands);
            message.Serialize(writer);

            Packet packet = new Packet(ms);
            ClientConnection.Send(packet);

            packet.Wait();

            BinaryReader reader = new BinaryReader(packet.Response);
            message = Message.Deserialize(reader, (id) => { return descriptor; });

            return message.Commands;
        }

        private void InternalExecute(ICommand command)
        {
            CommandCollection cmds = new CommandCollection(1);
            cmds.Add(command);

            var resultCommand = Execute(StorageEngineDescriptor, cmds)[0];
            SetResult(command, resultCommand);
        }

        private void SetResult(ICommand command, ICommand resultCommand)
        {
            switch (resultCommand.Code)
            {
                case CommandCode.STORAGE_ENGINE_COMMIT:
                    break;

                case CommandCode.STORAGE_ENGINE_OPEN_XTABLE:
                    {
                        ((StorageEngineOpenXIndexCommand)command).ID = ((StorageEngineOpenXIndexCommand)resultCommand).ID;
                        ((StorageEngineOpenXIndexCommand)command).CreateTime = ((StorageEngineOpenXIndexCommand)resultCommand).CreateTime;
                    }
                    break;

                case CommandCode.STORAGE_ENGINE_OPEN_XFILE:
                    ((StorageEngineOpenXFileCommand)command).ID = ((StorageEngineOpenXFileCommand)resultCommand).ID;
                    break;

                case CommandCode.STORAGE_ENGINE_EXISTS:
                    ((StorageEngineExistsCommand)command).Exist = ((StorageEngineExistsCommand)resultCommand).Exist;
                    break;

                case CommandCode.STORAGE_ENGINE_FIND_BY_ID:
                    ((StorageEngineFindByIDCommand)command).Descriptor = ((StorageEngineFindByIDCommand)resultCommand).Descriptor;
                    break;

                case CommandCode.STORAGE_ENGINE_FIND_BY_NAME:
                    ((StorageEngineFindByNameCommand)command).Descriptor = ((StorageEngineFindByNameCommand)resultCommand).Descriptor;
                    break;

                case CommandCode.STORAGE_ENGINE_DELETE:
                    break;

                case CommandCode.STORAGE_ENGINE_COUNT:
                    ((StorageEngineCountCommand)command).Count = ((StorageEngineCountCommand)resultCommand).Count;
                    break;

                case CommandCode.STORAGE_ENGINE_GET_ENUMERATOR:
                    ((StorageEngineGetEnumeratorCommand)command).Descriptions = ((StorageEngineGetEnumeratorCommand)resultCommand).Descriptions;
                    break;

                case CommandCode.STORAGE_ENGINE_GET_CACHE_SIZE:
                    ((StorageEngineGetCacheSizeCommand)command).CacheSize = ((StorageEngineGetCacheSizeCommand)resultCommand).CacheSize;
                    break;

                case CommandCode.EXCEPTION:
                    throw new Exception(((ExceptionCommand)resultCommand).Exception);

                default:
                    break;
            }
        }

        #endregion

        #region IDisposable Members

        private volatile bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    ClientConnection.Stop();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~StorageEngineClient()
        {
            Dispose(false);
        }

        public void Close()
        {
            Dispose();
        }

        #endregion

        public int CacheSize
        {
            get
            {
                StorageEngineGetCacheSizeCommand command = new StorageEngineGetCacheSizeCommand(0);

                CommandCollection collection = new CommandCollection(1);
                collection.Add(command);

                StorageEngineGetCacheSizeCommand resultComamnd = (StorageEngineGetCacheSizeCommand)Execute(StorageEngineDescriptor, collection)[0];

                return resultComamnd.CacheSize;
            }
            set
            {
                cacheSize = value;
                StorageEngineSetCacheSizeCommand command = new StorageEngineSetCacheSizeCommand(cacheSize);

                CommandCollection collection = new CommandCollection(1);
                collection.Add(command);

                Execute(StorageEngineDescriptor, collection);
            }
        }

        private class RemoteHeap : IHeap
        {
            public StorageEngineClient Engine { get; private set; }

            public RemoteHeap(StorageEngineClient engine)
            {
                if (engine == null)
                    throw new ArgumentNullException("engine");

                Engine = engine;
            }

            private void InternalExecute(ICommand command)
            {
                CommandCollection cmds = new CommandCollection(1);
                cmds.Add(command);

                var resultCommand = Engine.Execute(StorageEngineClient.StorageEngineDescriptor, cmds)[0];
                SetResult(command, resultCommand);
            }

            #region IHeap

            public long ObtainNewHandle()
            {
                var cmd = new HeapObtainNewHandleCommand();
                InternalExecute(cmd);

                return cmd.Handle;
            }

            public void Release(long handle)
            {
                InternalExecute(new HeapReleaseHandleCommand(handle));
            }

            public bool Exists(long handle)
            {
                var cmd = new HeapExistsHandleCommand(handle, false);
                InternalExecute(cmd);

                return cmd.Exist;
            }

            public void Write(long handle, byte[] buffer, int index, int count)
            {
                InternalExecute(new HeapWriteCommand(handle, buffer, index, count));
            }

            public byte[] Read(long handle)
            {
                var cmd = new HeapReadCommand(handle, null);
                InternalExecute(cmd);

                return cmd.Buffer;
            }

            public void Commit()
            {
                InternalExecute(new HeapCommitCommand());
            }

            public void Close()
            {
                InternalExecute(new HeapCloseCommand());
            }

            public byte[] Tag
            {
                get
                {
                    var cmd = new HeapGetTagCommand();
                    InternalExecute(cmd);

                    return cmd.Tag;
                }
                set
                {
                    InternalExecute(new HeapSetTagCommand(value));
                }
            }

            public long DataSize
            {
                get
                {
                    var cmd = new HeapDataSizeCommand();
                    InternalExecute(cmd);

                    return cmd.DataSize;
                }
            }

            public long Size
            {
                get
                {
                    var cmd = new HeapSizeCommand();
                    InternalExecute(cmd);

                    return cmd.Size;
                }
            }

            #endregion

            private void SetResult(ICommand command, ICommand resultCommand)
            {
                switch (resultCommand.Code)
                {
                    case CommandCode.HEAP_OBTAIN_NEW_HANDLE:
                        ((HeapObtainNewHandleCommand)command).Handle = ((HeapObtainNewHandleCommand)resultCommand).Handle;
                        break;

                    case CommandCode.HEAP_RELEASE_HANDLE:
                        break;

                    case CommandCode.HEAP_EXISTS_HANDLE:
                        ((HeapExistsHandleCommand)command).Exist = ((HeapExistsHandleCommand)resultCommand).Exist;
                        break;

                    case CommandCode.HEAP_WRITE:
                        break;

                    case CommandCode.HEAP_READ:
                        ((HeapReadCommand)command).Buffer = ((HeapReadCommand)resultCommand).Buffer;
                        break;

                    case CommandCode.HEAP_COMMIT:
                        break;

                    case CommandCode.HEAP_CLOSE:
                        break;

                    case CommandCode.HEAP_SET_TAG:
                        break;

                    case CommandCode.HEAP_GET_TAG:
                        ((HeapGetTagCommand)command).Tag = ((HeapGetTagCommand)resultCommand).Tag;
                        break;

                    case CommandCode.HEAP_DATA_SIZE:
                        ((HeapDataSizeCommand)command).DataSize = ((HeapDataSizeCommand)resultCommand).DataSize;
                        break;

                    case CommandCode.HEAP_SIZE:
                        ((HeapSizeCommand)command).Size = ((HeapSizeCommand)resultCommand).Size;
                        break;

                    case CommandCode.EXCEPTION:
                        throw new Exception(((ExceptionCommand)resultCommand).Exception);

                    default:
                        break;
                }
            }
        }
    }

}
