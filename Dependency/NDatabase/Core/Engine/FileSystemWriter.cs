using NDatabase.Api;
using NDatabase.Core.Session;
using NDatabase.Exceptions;
using NDatabase.IO;
using NDatabase.Meta;
using NDatabase.Oid;
using NDatabase.Storage;
using NDatabase.Tool;

namespace NDatabase.Core.Engine
{
    internal sealed class FileSystemWriter : IFileSystemWriter
    {
        public IFileSystemInterface FileSystemInterface { get; private set; }

        public void BuildFileSystemInterface(IStorageEngine storageEngine, ISession session)
        {
            FileSystemInterface =  new FileSystemInterface(storageEngine.GetBaseIdentification(),
                                                MultiBuffer.DefaultBufferSizeForData, session);
        }

        /// <summary>
        ///   Write the status of the last odb close
        /// </summary>
        public void WriteLastOdbCloseStatus(bool ok, bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderLastCloseStatusPosition, writeInTransaction);
            //odb last close status
            FileSystemInterface.WriteBoolean(ok, writeInTransaction);
        }

        /// <summary>
        ///   Write the version in the database file
        /// </summary>
        private void WriteVersion()
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderVersionPosition, false);
            FileSystemInterface.WriteInt(StorageEngineConstant.CurrentFileFormatVersion, false);
        }

        private void WriteDatabaseId(IStorageEngine storageEngine, long creationDate)
        {
            var databaseId = GetDatabaseId(creationDate);

            FileSystemInterface.WriteLong(databaseId.GetIds()[0], false); //database id 1/4
            FileSystemInterface.WriteLong(databaseId.GetIds()[1], false); //database id 2/4
            FileSystemInterface.WriteLong(databaseId.GetIds()[2], false); //database id 3/4
            FileSystemInterface.WriteLong(databaseId.GetIds()[3], false); //database id 4/4

            storageEngine.SetDatabaseId(databaseId);
        }

        /// <summary>
        ///   Returns a database id : 4 longs
        /// </summary>
        /// <param name="creationDate"> </param>
        /// <returns> a 4 long array </returns>
        private static IDatabaseId GetDatabaseId(long creationDate)
        {
            var id = new[]
                         {
                             creationDate, UniqueIdGenerator.GetRandomLongId(), UniqueIdGenerator.GetRandomLongId(),
                             UniqueIdGenerator.GetRandomLongId()
                         };

            return new DatabaseId(id);
        }

        /// <summary>
        ///   Write the number of classes in meta-model
        /// </summary>
        public void WriteNumberOfClasses(long number, bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderNumberOfClassesPosition, writeInTransaction);
            FileSystemInterface.WriteLong(number, writeInTransaction); //nb classes
        }

        /// <summary>
        ///   Write the database characterEncoding
        /// </summary>
        private void WriteDatabaseCharacterEncoding()
        {
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderDatabaseCharacterEncodingPosition,
                                  false);

            FileSystemInterface.WriteString("UTF-8", false, 50);
        }

        public void WriteOid(OID oid, bool writeInTransaction)
        {
            if (oid == null)
                FileSystemInterface.WriteLong(-1, writeInTransaction);
            else
                FileSystemInterface.WriteLong(oid.ObjectId, writeInTransaction);
        }

        /// <summary>
        ///   Resets the position of the first class of the metamodel.
        /// </summary>
        /// <remarks>
        ///   Resets the position of the first class of the metamodel. It Happens when database is being refactored
        /// </remarks>
        public void WriteFirstClassInfoOID(OID classInfoId, bool inTransaction)
        {
            long positionToWrite = StorageEngineConstant.DatabaseHeaderFirstClassOid;
            FileSystemInterface.SetWritePosition(positionToWrite, inTransaction);
            WriteOid(classInfoId, inTransaction);
        }

        /// <summary>
        ///   Writes the header of a block of type ID - a block that contains ids of objects and classes
        /// </summary>
        /// <param name="position"> Position at which the block must be written, if -1, take the next available position </param>
        /// <param name="idBlockSize"> The block size in byte </param>
        /// <param name="blockStatus"> The block status </param>
        /// <param name="blockNumber"> The number of the block </param>
        /// <param name="previousBlockPosition"> The position of the previous block of the same type </param>
        /// <param name="writeInTransaction"> To indicate if write must be done in transaction </param>
        /// <returns> The position of the id @ </returns>
        public long WriteIdBlock(long position, int idBlockSize, byte blockStatus, int blockNumber,
                                         long previousBlockPosition, bool writeInTransaction)
        {
            if (position == -1)
                position = FileSystemInterface.GetAvailablePosition();

            // Updates the database header with the current id block position
            FileSystemInterface.SetWritePosition(StorageEngineConstant.DatabaseHeaderCurrentIdBlockPosition, writeInTransaction);
            FileSystemInterface.WriteLong(position, false); //current id block position
            FileSystemInterface.SetWritePosition(position, writeInTransaction);
            FileSystemInterface.WriteInt(idBlockSize, writeInTransaction);

            FileSystemInterface.WriteByte(BlockTypes.BlockTypeIds, writeInTransaction);
            FileSystemInterface.WriteByte(blockStatus, writeInTransaction);

            // prev block position
            FileSystemInterface.WriteLong(previousBlockPosition, writeInTransaction);

            // next block position
            FileSystemInterface.WriteLong(-1, writeInTransaction);
            FileSystemInterface.WriteInt(blockNumber, writeInTransaction);
            FileSystemInterface.WriteLong(0, writeInTransaction); //id block max id
            FileSystemInterface.SetWritePosition(position + StorageEngineConstant.IdBlockSize - 1, writeInTransaction);
            FileSystemInterface.WriteByte(0, writeInTransaction);

            return position;
        }

        /// <summary>
        ///   Marks a block of type id as full, changes the status and the next block position
        /// </summary>
        /// <param name="blockPosition"> </param>
        /// <param name="nextBlockPosition"> </param>
        /// <param name="writeInTransaction"> </param>
        /// <returns> The block position @ </returns>
        public void MarkIdBlockAsFull(long blockPosition, long nextBlockPosition, bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(blockPosition + StorageEngineConstant.BlockIdOffsetForBlockStatus, writeInTransaction);
            FileSystemInterface.WriteByte(BlockStatus.BlockFull, writeInTransaction);
            FileSystemInterface.SetWritePosition(blockPosition + StorageEngineConstant.BlockIdOffsetForNextBlock, writeInTransaction);

            FileSystemInterface.WriteLong(nextBlockPosition, writeInTransaction); //next id block pos
        }

        /// <summary>
        ///   Creates the header of the file
        /// </summary>
        /// <param name="storageEngine">Storage engine </param>
        /// <param name="creationDate"> The creation date </param>
        public void CreateEmptyDatabaseHeader(IStorageEngine storageEngine, long creationDate)
        {
            WriteVersion();
            WriteDatabaseId(storageEngine, creationDate);

            WriteNumberOfClasses(0, false);
            WriteFirstClassInfoOID(StorageEngineConstant.NullObjectId, false);
            WriteLastOdbCloseStatus(false, false);
            WriteDatabaseCharacterEncoding();

            // This is the position of the first block id. But it will always contain the position of the current id block
            FileSystemInterface.WriteLong(StorageEngineConstant.DatabaseHeaderFirstIdBlockPosition, false); // current id block position

            // Write an empty id block
            WriteIdBlock(-1, StorageEngineConstant.IdBlockSize, BlockStatus.BlockNotFull, 1, -1, false);
            Flush();

            var currentBlockInfo = new CurrentIdBlockInfo
            {
                CurrentIdBlockPosition = StorageEngineConstant.DatabaseHeaderFirstIdBlockPosition,
                CurrentIdBlockNumber = 1,
                CurrentIdBlockMaxOid = OIDFactory.BuildObjectOID(0)
            };

            storageEngine.SetCurrentIdBlockInfos(currentBlockInfo);
        }

        /// <summary>
        ///   Associate an object OID to its position
        /// </summary>
        /// <param name="idType"> The type : can be object or class </param>
        /// <param name="idStatus"> The status of the OID </param>
        /// <param name="currentBlockIdPosition"> The current OID block position </param>
        /// <param name="oid"> The OID </param>
        /// <param name="objectPosition"> The position </param>
        /// <param name="writeInTransaction"> To indicate if write must be executed in transaction </param>
        /// <returns> @ </returns>
        public long AssociateIdToObject(byte idType, byte idStatus, long currentBlockIdPosition, OID oid,
                                                long objectPosition, bool writeInTransaction)
        {
            // Update the max id of the current block
            FileSystemInterface.SetWritePosition(currentBlockIdPosition + StorageEngineConstant.BlockIdOffsetForMaxId,
                                  writeInTransaction);

            FileSystemInterface.WriteLong(oid.ObjectId, writeInTransaction); // id block max id update

            var l1 = (oid.ObjectId - 1) % StorageEngineConstant.NbIdsPerBlock;

            var idPosition = currentBlockIdPosition + StorageEngineConstant.BlockIdOffsetForStartOfRepetition +
                             (l1) * StorageEngineConstant.IdBlockRepetitionSize;

            // go to the next id position
            FileSystemInterface.SetWritePosition(idPosition, writeInTransaction);

            // id type
            FileSystemInterface.WriteByte(idType, writeInTransaction);

            // id
            FileSystemInterface.WriteLong(oid.ObjectId, writeInTransaction);

            // id status
            FileSystemInterface.WriteByte(idStatus, writeInTransaction);

            // object position
            FileSystemInterface.WriteLong(objectPosition, writeInTransaction);

            return idPosition;
        }

        /// <summary>
        ///   Updates the real object position of the object OID
        /// </summary>
        /// <param name="idPosition"> The OID position </param>
        /// <param name="objectPosition"> The real object position </param>
        /// <param name="writeInTransaction"> indicate if write must be done in transaction @ </param>
        public void UpdateObjectPositionForObjectOIDWithPosition(long idPosition, long objectPosition,
                                                                         bool writeInTransaction)
        {
           FileSystemInterface.SetWritePosition(idPosition, writeInTransaction);
           FileSystemInterface.WriteByte(IdTypes.Object, writeInTransaction);
           FileSystemInterface.SetWritePosition(idPosition + StorageEngineConstant.BlockIdRepetitionIdStatus, writeInTransaction);
           FileSystemInterface.WriteByte(IDStatus.Active, writeInTransaction);
           FileSystemInterface.WriteLong(objectPosition, writeInTransaction); //Updating object position of id
        }

        /// <summary>
        ///   Udates the real class positon of the class OID
        /// </summary>
        public void UpdateClassPositionForClassOIDWithPosition(long idPosition, long objectPosition,
                                                                       bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(idPosition, writeInTransaction);
            FileSystemInterface.WriteByte(IdTypes.Class, writeInTransaction);
            FileSystemInterface.SetWritePosition(idPosition + StorageEngineConstant.BlockIdRepetitionIdStatus, writeInTransaction);
            FileSystemInterface.WriteByte(IDStatus.Active, writeInTransaction);
            FileSystemInterface.WriteLong(objectPosition, writeInTransaction); // Updating class position of id
        }

        public void UpdateStatusForIdWithPosition(long idPosition, byte newStatus, bool writeInTransaction)
        {
            FileSystemInterface.SetWritePosition(idPosition + StorageEngineConstant.BlockIdRepetitionIdStatus, writeInTransaction);
            FileSystemInterface.WriteByte(newStatus, writeInTransaction);
        }

        /// <summary>
        ///   Updates the instance related field of the class info into the database file Updates the number of objects, the first object oid and the next class oid
        /// </summary>
        /// <param name="classInfo"> The class info to be updated </param>
        /// <param name="writeInTransaction"> To specify if it must be part of a transaction @ </param>
        public void UpdateInstanceFieldsOfClassInfo(ClassInfo classInfo, bool writeInTransaction)
        {
            var currentPosition = FileSystemInterface.GetPosition();

            var position = classInfo.Position + StorageEngineConstant.ClassOffsetClassNbObjects;
            FileSystemInterface.SetWritePosition(position, writeInTransaction);
            var nbObjects = classInfo.NumberOfObjects;
            FileSystemInterface.WriteLong(nbObjects, writeInTransaction); //class info update nb objects
            WriteOid(classInfo.CommitedZoneInfo.First, writeInTransaction); // class info update first obj oid
            WriteOid(classInfo.CommitedZoneInfo.Last, writeInTransaction); // class info update last obj oid
            
            FileSystemInterface.SetWritePosition(currentPosition, writeInTransaction);
        }

        public void WriteBlockSizeAt(long writePosition, int blockSize, bool writeInTransaction, object @object)
        {
            if (blockSize < 0)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.NegativeBlockSize.AddParameter(writePosition).AddParameter(blockSize).AddParameter(
                        @object.ToString()));
            }

            var currentPosition = FileSystemInterface.GetPosition();
            FileSystemInterface.SetWritePosition(writePosition, writeInTransaction);
            FileSystemInterface.WriteInt(blockSize, writeInTransaction);
            // goes back where we were
            FileSystemInterface.SetWritePosition(currentPosition, writeInTransaction);
        }

        /// <summary>
        ///   Writes a class attribute info, an attribute of a class
        /// </summary>
        public void WriteClassAttributeInfo(IStorageEngine storageEngine, ClassAttributeInfo cai, bool writeInTransaction)
        {
            FileSystemInterface.WriteInt(cai.GetId(), writeInTransaction);
            FileSystemInterface.WriteBoolean(cai.IsNative(), writeInTransaction);

            if (cai.IsNative())
            {
                FileSystemInterface.WriteInt(cai.GetAttributeType().Id, writeInTransaction);
                if (cai.GetAttributeType().IsArray())
                {
                    FileSystemInterface.WriteInt(cai.GetAttributeType().SubType.Id, writeInTransaction);
                    // when the attribute is not native, then write its class info position
                    if (cai.GetAttributeType().SubType.IsNonNative())
                    {
                        FileSystemInterface.WriteLong(
                            storageEngine.GetSession().GetMetaModel().GetClassInfo(
                                cai.GetAttributeType().SubType.Name, true).ClassInfoId.ObjectId,
                            writeInTransaction); // class info id of array subtype
                    }
                }
                // For enum, we write the class info id of the enum class
                if (cai.GetAttributeType().IsEnum())
                {
                    FileSystemInterface.WriteLong(
                        storageEngine.GetSession().GetMetaModel().GetClassInfo(cai.GetFullClassname(), true).ClassInfoId
                            .ObjectId, writeInTransaction); // class info id
                }
            }
            else
            {
                FileSystemInterface.WriteLong(
                    storageEngine.GetSession().GetMetaModel().GetClassInfo(cai.GetFullClassname(), true).ClassInfoId.
                        ObjectId, writeInTransaction); // class info id
            }

            FileSystemInterface.WriteString(cai.GetName(), writeInTransaction);
            FileSystemInterface.WriteBoolean(cai.IsIndex(), writeInTransaction);
        }

        public void Close()
        {
            FileSystemInterface.Close();
            FileSystemInterface = null;
        }

        public void Flush()
        {
            FileSystemInterface.Flush();
        }
    }
}
