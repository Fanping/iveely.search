using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Exceptions;
using NDatabase.Meta;
using NDatabase.Oid;
using NDatabase.Storage;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Engine
{
    internal sealed class FileSystemReader : IFileSystemReader
    {
        /// <summary>
        ///     The fsi is the object that knows how to write and read native types
        /// </summary>
        private readonly IFileSystemInterface _fsi;

        private IStorageEngine _storageEngine;

        /// <summary>
        ///   To hold block number.
        /// </summary>
        /// <remarks>
        ///   To hold block number. ODB compute the block number from the oid (as one block has 1000 oids), then it has to search the position of the block number! This cache is used to keep track of the positions of the block positions The key is the block number(Long) and the value the position (Long)
        /// </remarks>
        private IDictionary<long, long> _blockPositions = new OdbHashMap<long, long>();

        public FileSystemReader(IStorageEngine engine)
        {
            _storageEngine = engine;
            _fsi = engine.GetObjectWriter().FileSystemProcessor.FileSystemInterface;
        }

        public void ReadDatabaseHeader()
        {
            var version = ReadDatabaseVersion();
            StorageEngineConstant.CheckDbVersionCompatibility(version);

            var databaseIdsArray = new long[4];
            databaseIdsArray[0] = _fsi.ReadLong();
            databaseIdsArray[1] = _fsi.ReadLong();
            databaseIdsArray[2] = _fsi.ReadLong();
            databaseIdsArray[3] = _fsi.ReadLong();
            IDatabaseId databaseId = new DatabaseId(databaseIdsArray);

            var nbClasses = ReadNumberOfClasses();
            var firstClassPosition = ReadFirstClassOid();
            if (nbClasses < 0)
            {
                throw new CorruptedDatabaseException(
                    NDatabaseError.NegativeClassNumberInHeader.AddParameter(nbClasses).AddParameter(firstClassPosition));
            }
            ReadLastOdbCloseStatus();
            ReadDatabaseCharacterEncoding();

            var currentBlockPosition = _fsi.ReadLong();
            // Gets the current id block number
            _fsi.SetReadPosition(currentBlockPosition + StorageEngineConstant.BlockIdOffsetForBlockNumber);
            var currentBlockIdNumber = _fsi.ReadInt();
            var blockMaxId = OIDFactory.BuildObjectOID(_fsi.ReadLong());
            _storageEngine.SetDatabaseId(databaseId);

            var currentBlockInfo = new CurrentIdBlockInfo
                                       {
                                           CurrentIdBlockPosition = currentBlockPosition,
                                           CurrentIdBlockNumber = currentBlockIdNumber,
                                           CurrentIdBlockMaxOid = blockMaxId
                                       };

            _storageEngine.SetCurrentIdBlockInfos(currentBlockInfo);
        }

        /// <summary>
        ///     Reads the number of classes in database file
        /// </summary>
        public long ReadNumberOfClasses()
        {
            _fsi.SetReadPosition(StorageEngineConstant.DatabaseHeaderNumberOfClassesPosition);
            return _fsi.ReadLong();
        }

        /// <summary>
        ///     Reads the first class OID
        /// </summary>
        public long ReadFirstClassOid()
        {
            _fsi.SetReadPosition(StorageEngineConstant.DatabaseHeaderFirstClassOid);
            return _fsi.ReadLong();
        }

        /// <summary>
        ///     Read the version of the database file
        /// </summary>
        private int ReadDatabaseVersion()
        {
            _fsi.SetReadPosition(StorageEngineConstant.DatabaseHeaderVersionPosition);
            return _fsi.ReadInt();
        }

        /// <summary>
        ///     Reads the status of the last odb close
        /// </summary>
        private void ReadLastOdbCloseStatus()
        {
            //TODO:  we are reading lastOdbClose, but not using them, is that needed?
            _fsi.SetReadPosition(StorageEngineConstant.DatabaseHeaderLastCloseStatusPosition);
            _fsi.ReadBoolean(); // last odb status
        }

        /// <summary>
        ///     Reads the database character encoding
        /// </summary>
        private void ReadDatabaseCharacterEncoding()
        {
            //TODO:  we are reading databaseCharacterEncoding, but not using them, is that needed?
            _fsi.SetReadPosition(StorageEngineConstant.DatabaseHeaderDatabaseCharacterEncodingPosition);
            _fsi.ReadString();
        }

        /// <summary>
        ///   Returns information about all OIDs of the database
        /// </summary>
        /// <param name="idType"> </param>
        /// <returns> @ </returns>
        public IList<long> GetAllIds(byte idType)
        {
            IList<long> ids = new List<long>(5000);
            long currentBlockPosition = StorageEngineConstant.DatabaseHeaderFirstIdBlockPosition;
            while (currentBlockPosition != -1)
            {
                // Gets the next block position
                _fsi.SetReadPosition(currentBlockPosition + StorageEngineConstant.BlockIdOffsetForNextBlock);
                var nextBlockPosition = _fsi.ReadLong();
                // Gets the block max id
                _fsi.SetReadPosition(currentBlockPosition + StorageEngineConstant.BlockIdOffsetForMaxId);
                var blockMaxId = _fsi.ReadLong();
                long currentId;
                do
                {
                    var nextRepetitionPosition = _fsi.GetPosition() + StorageEngineConstant.IdBlockRepetitionSize;
                    var idTypeRead = _fsi.ReadByte();
                    currentId = _fsi.ReadLong();
                    var idStatus = _fsi.ReadByte();
                    if (idType == idTypeRead && IDStatus.IsActive(idStatus))
                        ids.Add(currentId);
                    _fsi.SetReadPosition(nextRepetitionPosition);
                } while (currentId != blockMaxId);
                currentBlockPosition = nextBlockPosition;
            }
            return ids;
        }

        /// <summary>
        ///   Gets the real object position from its OID
        /// </summary>
        /// <param name="oid"> The oid of the object to get the position </param>
        /// <param name="useCache"> </param>
        /// <param name="throwException"> To indicate if an exception must be thrown if object is not found </param>
        /// <returns> The object position, if object has been marked as deleted then return StorageEngineConstant.DELETED_OBJECT_POSITION @ </returns>
        public long GetObjectPositionFromItsOid(OID oid, bool useCache, bool throwException)
        {
            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug("ObjectReader: getObjectPositionFromItsId for oid " + oid);
            // Check if oid is in cache
            var position = StorageEngineConstant.ObjectIsNotInCache;
            if (useCache)
            {
                // This return -1 if not in the cache
                position = _storageEngine.GetSession().GetCache().GetObjectPositionByOid(oid);
            }
            // FIXME Check if we need this. Removing it causes the TestDelete.test6 to fail 
            if (position == StorageEngineConstant.DeletedObjectPosition)
            {
                if (throwException)
                    throw new CorruptedDatabaseException(NDatabaseError.ObjectIsMarkedAsDeletedForOid.AddParameter(oid));
                return StorageEngineConstant.DeletedObjectPosition;
            }
            if (position != StorageEngineConstant.ObjectIsNotInCache &&
                position != StorageEngineConstant.DeletedObjectPosition)
                return position;
            // The position was not found is the cache
            position = ReadOidPosition(oid);
            position += StorageEngineConstant.BlockIdRepetitionIdStatus;
            _fsi.SetReadPosition(position);
            var idStatus = _fsi.ReadByte();
            var objectPosition = _fsi.ReadLong();
            if (!IDStatus.IsActive(idStatus))
            {
                // if object position == 0, The object dos not exist
                if (throwException)
                {
                    if (objectPosition == 0)
                        throw new CorruptedDatabaseException(NDatabaseError.ObjectWithOidDoesNotExist.AddParameter(oid));
                    throw new CorruptedDatabaseException(NDatabaseError.ObjectIsMarkedAsDeletedForOid.AddParameter(oid));
                }

                return objectPosition == 0
                           ? StorageEngineConstant.ObjectDoesNotExist
                           : StorageEngineConstant.DeletedObjectPosition;
            }
            if (OdbConfiguration.IsLoggingEnabled())
            {
                var positionAsString = objectPosition.ToString();
                DLogger.Debug("ObjectReader: object position of object with oid " + oid + " is " + positionAsString);
            }
            return objectPosition;
        }

        public long ReadOidPosition(OID oid)
        {
            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug("ObjectReader: Start of readOidPosition for oid " + oid);

            var blockNumber = StorageEngineConstant.GetIdBlockNumberOfOid(oid);
            var blockPosition = GetIdBlockPositionFromNumber(blockNumber);

            if (OdbConfiguration.IsLoggingEnabled())
            {
                var blockNumberAsString = blockNumber.ToString();
                var blockPositionAsString = blockPosition.ToString();
                DLogger.Debug(string.Format("ObjectReader: Block number of oid {0} is ", oid) + blockNumberAsString +
                              " / block position = " + blockPositionAsString);
            }

            var position = blockPosition + StorageEngineConstant.BlockIdOffsetForStartOfRepetition +
                           ((oid.ObjectId - 1) % StorageEngineConstant.NbIdsPerBlock) *
                           StorageEngineConstant.IdBlockRepetitionSize;

            if (OdbConfiguration.IsLoggingEnabled())
            {
                var positionAsString = position.ToString();
                DLogger.Debug(string.Format("ObjectReader: End of readOidPosition for oid {0} returning position ", oid) + positionAsString);
            }

            return position;
        }

        public void Close()
        {
            _storageEngine = null;
            _blockPositions.Clear();
            _blockPositions = null;
        }

        /// <param name="blockNumberToFind"> </param>
        /// <returns> The block position @ </returns>
        private long GetIdBlockPositionFromNumber(long blockNumberToFind)
        {
            // first check if it exist in cache
            long lposition;

            _blockPositions.TryGetValue(blockNumberToFind, out lposition);
            if (lposition != 0)
                return lposition;
            long currentBlockPosition = StorageEngineConstant.DatabaseHeaderFirstIdBlockPosition;
            while (currentBlockPosition != -1)
            {
                // Gets the next block position
                _fsi.SetReadPosition(currentBlockPosition + StorageEngineConstant.BlockIdOffsetForNextBlock);
                var nextBlockPosition = _fsi.ReadLong();
                // Reads the block number
                var blockNumber = _fsi.ReadInt();
                if (blockNumber == blockNumberToFind)
                {
                    // Put result in map
                    _blockPositions.Add(blockNumberToFind, currentBlockPosition);
                    return currentBlockPosition;
                }
                currentBlockPosition = nextBlockPosition;
            }
            throw new CorruptedDatabaseException(NDatabaseError.BlockNumberDoesExist.AddParameter(blockNumberToFind));
        }

        /// <summary>
        ///   Read the class info header with the specific oid
        /// </summary>
        /// <returns> The read class info object @ </returns>
        public ClassInfo ReadClassInfoHeader(OID classInfoOid)
        {
            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug("FileSystemReader: Reading new Class info Header with oid " + classInfoOid);
            var classInfoPosition = GetObjectPositionFromItsOid(classInfoOid, true, true);
            _fsi.SetReadPosition(classInfoPosition);
            var blockSize = _fsi.ReadInt();
            var blockType = _fsi.ReadByte();
            if (!BlockTypes.IsClassHeader(blockType))
            {
                throw new OdbRuntimeException(
                    NDatabaseError.WrongTypeForBlockType.AddParameter("Class Header").AddParameter(blockType).
                        AddParameter(classInfoPosition));
            }
            //class info category, to remove
            _fsi.ReadByte();

            var classInfoId = OIDFactory.BuildClassOID(_fsi.ReadLong());
            var previousClassOID = ReadOid();
            var nextClassOID = ReadOid();
            var nbObjects = _fsi.ReadLong();
            var originalZoneInfoFirst = ReadOid();
            var originalZoneInfoLast = ReadOid();
            var fullClassName = _fsi.ReadString();
            var maxAttributeId = _fsi.ReadInt();
            var attributesDefinitionPosition = _fsi.ReadLong();

            var classInfo = new ClassInfo(fullClassName)
            {
                Position = classInfoPosition,
                ClassInfoId = classInfoId,
                PreviousClassOID = previousClassOID,
                NextClassOID = nextClassOID,
                MaxAttributeId = maxAttributeId,
                AttributesDefinitionPosition = attributesDefinitionPosition
            };

            classInfo.OriginalZoneInfo.SetNbObjects(nbObjects);
            classInfo.OriginalZoneInfo.First = originalZoneInfoFirst;
            classInfo.OriginalZoneInfo.Last = originalZoneInfoLast;
            classInfo.CommitedZoneInfo.SetBasedOn(classInfo.OriginalZoneInfo);

            // FIXME Convert block size to long ??
            var realBlockSize = (int)(_fsi.GetPosition() - classInfoPosition);
            if (blockSize != realBlockSize)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.WrongBlockSize.AddParameter(blockSize).AddParameter(realBlockSize).AddParameter(
                        classInfoPosition));
            }
            return classInfo;
        }

        private OID ReadOid()
        {
            var oid = _fsi.ReadLong();

            return oid == -1 ? null : OIDFactory.BuildObjectOID(oid);
        }

        public object ReadAtomicNativeObjectInfoAsObject(int odbTypeId)
        {
            object o = null;
            switch (odbTypeId)
            {
                case OdbType.ByteId:
                    {
                        o = _fsi.ReadByte();
                        break;
                    }

                case OdbType.SByteId:
                    {
                        o = _fsi.ReadSByte();
                        break;
                    }

                case OdbType.BooleanId:
                    {
                        o = _fsi.ReadBoolean();
                        break;
                    }

                case OdbType.CharacterId:
                    {
                        o = _fsi.ReadChar();
                        break;
                    }

                case OdbType.FloatId:
                    {
                        o = _fsi.ReadFloat();
                        break;
                    }

                case OdbType.DoubleId:
                    {
                        o = _fsi.ReadDouble();
                        break;
                    }

                case OdbType.IntegerId:
                    {
                        o = _fsi.ReadInt();
                        break;
                    }

                case OdbType.UIntegerId:
                    {
                        o = _fsi.ReadUInt();
                        break;
                    }

                case OdbType.LongId:
                    {
                        o = _fsi.ReadLong();
                        break;
                    }

                case OdbType.ULongId:
                    {
                        o = _fsi.ReadULong();
                        break;
                    }

                case OdbType.ShortId:
                    {
                        o = _fsi.ReadShort();
                        break;
                    }

                case OdbType.UShortId:
                    {
                        o = _fsi.ReadUShort();
                        break;
                    }

                case OdbType.DecimalId:
                    {
                        o = _fsi.ReadBigDecimal();
                        break;
                    }

                case OdbType.DateId:
                    {
                        o = _fsi.ReadDate();
                        break;
                    }

                case OdbType.ObjectOidId:
                    {
                        var oid = _fsi.ReadLong();
                        o = OIDFactory.BuildObjectOID(oid);
                        break;
                    }

                case OdbType.ClassOidId:
                    {
                        var cid = _fsi.ReadLong();
                        o = OIDFactory.BuildClassOID(cid);
                        break;
                    }

                case OdbType.StringId:
                    {
                        o = _fsi.ReadString();
                        break;
                    }

                case OdbType.EnumId:
                    {
                        o = _fsi.ReadString();
                        break;
                    }
            }
            if (o == null)
            {
                throw new OdbRuntimeException(
                    NDatabaseError.NativeTypeNotSupported.AddParameter(odbTypeId).AddParameter(
                        OdbType.GetNameFromId(odbTypeId)));
            }
            return o;
        }
    }
}