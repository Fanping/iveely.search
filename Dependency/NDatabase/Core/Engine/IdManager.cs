using NDatabase.Api;
using NDatabase.Meta;
using NDatabase.Oid;
using NDatabase.Storage;

namespace NDatabase.Core.Engine
{
    /// <summary>
    ///   Class to manage the ids of all the objects of the database.
    /// </summary>
    internal sealed class IdManager : IIdManager
    {
        private const int IdBufferSize = 10;
        
        private int _currentBlockIdNumber;
        private long _currentBlockIdPosition;
        private int _lastIdIndex;
        private long[] _lastIdPositions;

        /// <summary>
        ///   Contains the last ids: id value,id position, id value, id position=&gt; the array is created with twice the size
        /// </summary>
        private OID[] _lastIds;

        private OID _maxId;
        private OID _nextId;

        private IObjectReader _objectReader;
        private IObjectWriter _objectWriter;

        private readonly object _syncRoot = new object();

        /// <param name="objectWriter"> The object writer </param>
        /// <param name="objectReader"> The object reader </param>
        /// <param name="currentIdBlock">Current Id block data </param>
        public IdManager(IObjectWriter objectWriter, IObjectReader objectReader, CurrentIdBlockInfo currentIdBlock)
        {
            _objectWriter = objectWriter;
            _objectReader = objectReader;
            _currentBlockIdPosition = currentIdBlock.CurrentIdBlockPosition;
            _currentBlockIdNumber = currentIdBlock.CurrentIdBlockNumber;
            _maxId = new ObjectOID((long)currentIdBlock.CurrentIdBlockNumber * StorageEngineConstant.NbIdsPerBlock);
            _nextId = new ObjectOID(currentIdBlock.CurrentIdBlockMaxOid.ObjectId + 1);

            _lastIds = new OID[IdBufferSize];
            for (var i = 0; i < IdBufferSize; i++)
                _lastIds[i] = StorageEngineConstant.NullObjectId;

            _lastIdPositions = new long[IdBufferSize];
            _lastIdIndex = 0;
        }

        #region IIdManager Members

        /// <summary>
        ///   To check if the id block must shift: that a new id block must be created
        /// </summary>
        /// <returns> a boolean value to check if block of id is full </returns>
        public bool MustShift()
        {
            lock (_syncRoot)
            {
                return _nextId.CompareTo(_maxId) > 0;
            }
        }

        public OID GetNextObjectId(long objectPosition)
        {
            lock (_syncRoot)
            {
                return GetNextId(objectPosition, IdTypes.Object, IDStatus.Active);
            }
        }

        public OID GetNextClassId(long objectPosition)
        {
            lock (_syncRoot)
            {
                return GetNextId(objectPosition, IdTypes.Class, IDStatus.Active);
            }
        }

        public void UpdateObjectPositionForOid(OID oid, long objectPosition, bool writeInTransaction)
        {
            var idPosition = GetIdPosition(oid);
            _objectWriter.FileSystemProcessor.UpdateObjectPositionForObjectOIDWithPosition(idPosition, objectPosition, writeInTransaction);
        }

        public void UpdateClassPositionForId(OID classId, long objectPosition, bool writeInTransaction)
        {
            var idPosition = GetIdPosition(classId);
            _objectWriter.FileSystemProcessor.UpdateClassPositionForClassOIDWithPosition(idPosition, objectPosition,
                                                                                         writeInTransaction);
        }

        public void UpdateIdStatus(OID id, byte newStatus)
        {
            var idPosition = GetIdPosition(id);
            _objectWriter.FileSystemProcessor.UpdateStatusForIdWithPosition(idPosition, newStatus, true);
        }

        public long GetObjectPositionWithOid(OID oid, bool useCache)
        {
            return _objectReader.GetObjectPositionFromItsOid(oid, useCache, true);
        }

        public void Clear()
        {
            _objectReader = null;
            _objectWriter = null;
            _lastIdPositions = null;
            _lastIds = null;
        }

        #endregion

        /// <summary>
        ///   Gets an id for an object (instance)
        /// </summary>
        /// <param name="objectPosition"> the object position (instance) </param>
        /// <param name="idType"> The type id : object,class, unknown </param>
        /// <param name="idStatus"> </param>
        /// <returns> The id </returns>
        private OID GetNextId(long objectPosition, byte idType, byte idStatus)
        {
            lock (_syncRoot)
            {
                if (MustShift())
                    ShiftBlock();

                // Keep the current id
                var currentNextId = _nextId;
                if (idType == IdTypes.Class)
                {
                    // If its a class, build a class OID instead.
                    currentNextId = new ClassOID(currentNextId.ObjectId);
                }

                // Compute the new index to be used to store id and its position in the lastIds and lastIdPositions array
                var currentIndex = (_lastIdIndex + 1) % IdBufferSize;

                // Stores the id
                _lastIds[currentIndex] = currentNextId;

                // really associate id to the object position
                var idPosition = AssociateIdToObject(idType, idStatus, objectPosition);

                // Store the id position
                _lastIdPositions[currentIndex] = idPosition;

                // Update the id buffer index
                _lastIdIndex = currentIndex;

                return currentNextId;
            }
        }

        private long GetIdPosition(OID oid)
        {
            // first check if it is the last
            if (_lastIds[_lastIdIndex] != null && _lastIds[_lastIdIndex].Equals(oid))
                return _lastIdPositions[(_lastIdIndex)];

            for (var i = 0; i < IdBufferSize; i++)
            {
                if (_lastIds[i] != null && _lastIds[i].Equals(oid))
                    return _lastIdPositions[i];
            }

            // object id is not is cache
            return _objectReader.ReadOidPosition(oid);
        }

        private long AssociateIdToObject(byte idType, byte idStatus, long objectPosition)
        {
            var idPosition = _objectWriter.FileSystemProcessor.AssociateIdToObject(idType, idStatus, _currentBlockIdPosition, _nextId,
                                                               objectPosition, false);

            _nextId = new ObjectOID(_nextId.ObjectId + 1);

            return idPosition;
        }

        private void ShiftBlock()
        {
            var currentBlockPosition = _currentBlockIdPosition;

            // the block has reached the end, , must create a new id block
            var newBlockPosition = CreateNewBlock();

            // Mark the current block as full
            MarkBlockAsFull(currentBlockPosition, newBlockPosition);

            _currentBlockIdNumber++;
            _currentBlockIdPosition = newBlockPosition;
            _maxId = new ObjectOID((long) _currentBlockIdNumber * StorageEngineConstant.NbIdsPerBlock);
        }

        private void MarkBlockAsFull(long currentBlockIdPosition, long nextBlockPosition)
        {
            _objectWriter.FileSystemProcessor.MarkIdBlockAsFull(currentBlockIdPosition, nextBlockPosition, false);
        }

        private long CreateNewBlock()
        {
            var position = _objectWriter.FileSystemProcessor.WriteIdBlock(-1, StorageEngineConstant.IdBlockSize,
                                                                          BlockStatus.BlockNotFull,
                                                                          _currentBlockIdNumber + 1,
                                                                          _currentBlockIdPosition, false);
            return position;
        }
    }
}
