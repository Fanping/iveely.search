using System;
using System.Collections;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Cache;
using NDatabase.Core.Session;
using NDatabase.Exceptions;
using NDatabase.Indexing;
using NDatabase.Meta;
using NDatabase.Meta.Introspector;
using NDatabase.Oid;
using NDatabase.Storage;
using NDatabase.Tool;
using NDatabase.Triggers;

namespace NDatabase.Core.Engine
{
    /// <summary>
    ///   Manage all IO writing
    /// </summary>
    internal sealed class ObjectWriter : IObjectWriter
    {
        private static readonly int NativeHeaderBlockSize = OdbType.Integer.Size + OdbType.Byte.Size +
                                                            OdbType.Integer.Size + OdbType.Boolean.Size;

        private static byte[] _nativeHeaderBlockSizeByte;

        private readonly ISession _session;

        private IIdManager _idManager;
        private IObjectReader _objectReader;
        private readonly INonNativeObjectWriter _nonNativeObjectWriter;

        /// <summary>
        ///   To manage triggers
        /// </summary>
        private IInternalTriggerManager _triggerManager;

        private IStorageEngine _storageEngine;

        public ObjectWriter(IStorageEngine engine)
        {
            _storageEngine = engine;

            _nativeHeaderBlockSizeByte = ByteArrayConverter.IntToByteArray(NativeHeaderBlockSize);
            _session = engine.GetSession();

            _nonNativeObjectWriter = new NonNativeObjectWriter(this, _storageEngine);

            FileSystemProcessor = new FileSystemWriter();
            FileSystemProcessor.BuildFileSystemInterface(engine, _session);
        }

        #region IObjectWriter Members

        public void AfterInit()
        {
            _objectReader = _storageEngine.GetObjectReader();
            _nonNativeObjectWriter.AfterInit();
            _idManager = _storageEngine.GetIdManager();
        }

        /// <summary>
        ///   Creates the header of the file
        /// </summary>
        /// <param name="creationDate"> The creation date </param>
        public void CreateEmptyDatabaseHeader(long creationDate)
        {
            FileSystemProcessor.CreateEmptyDatabaseHeader(_storageEngine, creationDate);
        }

        public IFileSystemWriter FileSystemProcessor { get; private set; }

        /// <summary>
        ///   PersistTo a single class info - This method is used by the XML Importer.
        /// </summary>
        /// <remarks>
        ///   PersistTo a single class info - This method is used by the XML Importer.
        /// </remarks>
        private ClassInfo PersistClass(ClassInfo newClassInfo, int lastClassInfoIndex, bool addClass,
                                              bool addDependentClasses)
        {
            var metaModel = _session.GetMetaModel();
            var classInfoId = newClassInfo.ClassInfoId;
            if (classInfoId == null)
            {
                classInfoId = GetIdManager().GetNextClassId(-1);
                newClassInfo.ClassInfoId = classInfoId;
            }
            var writePosition = FileSystemProcessor.FileSystemInterface.GetAvailablePosition();
            newClassInfo.Position = writePosition;
            GetIdManager().UpdateClassPositionForId(classInfoId, writePosition, true);

            #region Logging

            if (OdbConfiguration.IsLoggingEnabled())
            {
                var writePositionAsString = writePosition.ToString();
                DLogger.Debug(
                    string.Format("ObjectWriter: Persisting class into database : {0} with oid {1} at pos ",
                                  newClassInfo.FullClassName, classInfoId) + writePositionAsString);

                var numberOfAttributesAsString = newClassInfo.NumberOfAttributes.ToString();
                DLogger.Debug("ObjectWriter: class " + newClassInfo.FullClassName + " has " + numberOfAttributesAsString + " attributes");
            }

            #endregion

            // The class info oid is created in ObjectWriter.writeClassInfoHeader
            if (metaModel.GetNumberOfClasses() > 0 && lastClassInfoIndex != -2)
            {
                var lastClassinfo = lastClassInfoIndex == -1
                                        ? metaModel.GetLastClassInfo()
                                        : metaModel.GetClassInfo(lastClassInfoIndex);

                lastClassinfo.NextClassOID = newClassInfo.ClassInfoId;

                #region Logging

                if (OdbConfiguration.IsLoggingEnabled())
                {
                    var positionAsString = lastClassinfo.Position.ToString();
                    var classOffsetAsString = StorageEngineConstant.ClassOffsetNextClassPosition.ToString();
                    DLogger.Debug("ObjectWriter: changing next class oid. of class info " + lastClassinfo.FullClassName + "@ " +
                                  positionAsString + " + offset " + classOffsetAsString +
                                  string.Format(" to {0}({1})", newClassInfo.ClassInfoId, newClassInfo.FullClassName));
                }

                #endregion

                FileSystemProcessor.FileSystemInterface.SetWritePosition(lastClassinfo.Position + StorageEngineConstant.ClassOffsetNextClassPosition,
                                      true);

                FileSystemProcessor.FileSystemInterface.WriteLong(newClassInfo.ClassInfoId.ObjectId, true); // next class oid

                newClassInfo.PreviousClassOID = lastClassinfo.ClassInfoId;
            }

            if (addClass)
                metaModel.AddClass(newClassInfo);

            // updates number of classes
            FileSystemProcessor.WriteNumberOfClasses(metaModel.GetNumberOfClasses(), true);
            // If it is the first class , updates the first class OID
            if (newClassInfo.PreviousClassOID == null)
                FileSystemProcessor.WriteFirstClassInfoOID(newClassInfo.ClassInfoId, true);

            // Writes the header of the class - out of transaction (FIXME why out of
            // transaction)
            WriteClassInfoHeader(newClassInfo, writePosition, false);

            if (addDependentClasses)
            {
                var dependingAttributes = newClassInfo.GetAllNonNativeAttributes();

                UpdateClass(dependingAttributes, metaModel);
            }

            WriteClassInfoBody(newClassInfo, FileSystemProcessor.FileSystemInterface.GetAvailablePosition(), true);
            return newClassInfo;
        }

        private void UpdateClass(IEnumerable<ClassAttributeInfo> dependingAttributes, IMetaModel metaModel)
        {
            foreach (var classAttributeInfo in dependingAttributes)
            {
                try
                {
                    var existingClassInfo = metaModel.GetClassInfo(classAttributeInfo.GetFullClassname(), false);
                    if (existingClassInfo == null)
                    {
                        AddClasses(ClassIntrospector.Introspect(classAttributeInfo.GetFullClassname()));
                    }
                    else
                    {
                        // Even,if it exist,take the one from metamodel
                        classAttributeInfo.SetClassInfo(existingClassInfo);
                    }
                }
                catch (Exception e)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.ClassIntrospectionError.AddParameter(classAttributeInfo.GetFullClassname()), e);
                }
            }
        }

        public ClassInfo AddClass(ClassInfo newClassInfo, bool addDependentClasses)
        {
            var classInfo = _session.GetMetaModel().GetClassInfo(newClassInfo.FullClassName, false);
            if (classInfo != null && classInfo.Position != -1)
                return classInfo;

            return PersistClass(newClassInfo, -1, true, addDependentClasses);
        }

        public ClassInfoList AddClasses(ClassInfoList classInfoList)
        {
            IEnumerator iterator = classInfoList.GetClassInfos().GetEnumerator();
            while (iterator.MoveNext())
                AddClass((ClassInfo) iterator.Current, true);

            return classInfoList;
        }

        /// <summary>
        ///   Write the class info header to the database file
        /// </summary>
        /// <param name="classInfo"> The class info to be written </param>
        /// <param name="position"> The position at which it must be written </param>
        /// <param name="writeInTransaction"> true if the write must be done in transaction, false to write directly </param>
        private void WriteClassInfoHeader(ClassInfo classInfo, long position, bool writeInTransaction)
        {
            var classId = classInfo.ClassInfoId;
            if (classId == null)
            {
                classId = _idManager.GetNextClassId(position);
                classInfo.ClassInfoId = classId;
            }
            else
                _idManager.UpdateClassPositionForId(classId, position, true);

            FileSystemProcessor.FileSystemInterface.SetWritePosition(position, writeInTransaction);
            if (OdbConfiguration.IsLoggingEnabled())
            {
                var positionAsString = position.ToString();
                DLogger.Debug("ObjectWriter: Writing new Class info header at " + positionAsString + " : " + classInfo);
            }

            // Real value of block size is only known at the end of the writing
            FileSystemProcessor.FileSystemInterface.WriteInt(0, writeInTransaction);
            FileSystemProcessor.FileSystemInterface.WriteByte(BlockTypes.BlockTypeClassHeader, writeInTransaction);
            FileSystemProcessor.FileSystemInterface.WriteByte(2, writeInTransaction);
            FileSystemProcessor.FileSystemInterface.WriteLong(classId.ObjectId, writeInTransaction); //class id

            FileSystemProcessor.WriteOid(classInfo.PreviousClassOID, writeInTransaction);

            FileSystemProcessor.WriteOid(classInfo.NextClassOID, writeInTransaction);

            FileSystemProcessor.FileSystemInterface.WriteLong(classInfo.CommitedZoneInfo.GetNumberbOfObjects(), writeInTransaction); //class nb objects

            FileSystemProcessor.WriteOid(classInfo.CommitedZoneInfo.First, writeInTransaction);

            FileSystemProcessor.WriteOid(classInfo.CommitedZoneInfo.Last, writeInTransaction);

            // FIXME : append extra info if not empty (.net compatibility)
            FileSystemProcessor.FileSystemInterface.WriteString(classInfo.FullClassName, writeInTransaction);
            FileSystemProcessor.FileSystemInterface.WriteInt(classInfo.MaxAttributeId, writeInTransaction);

            if (classInfo.AttributesDefinitionPosition != -1)
            {
                FileSystemProcessor.FileSystemInterface.WriteLong(classInfo.AttributesDefinitionPosition, writeInTransaction); //class att def pos
            }
            else
            {
                // todo check this
                FileSystemProcessor.FileSystemInterface.WriteLong(-1, writeInTransaction); //class att def pos
            }

            var blockSize = (int) (FileSystemProcessor.FileSystemInterface.GetPosition() - position);
            FileSystemProcessor.WriteBlockSizeAt(position, blockSize, writeInTransaction, classInfo);
        }

        public void UpdateClassInfo(ClassInfo classInfo, bool writeInTransaction)
        {
            // first check dependent classes
            var dependingAttributes = classInfo.GetAllNonNativeAttributes();
            var metaModel = _session.GetMetaModel();

            UpdateClass(dependingAttributes, metaModel);

            // To force the rewrite of class info body
            classInfo.AttributesDefinitionPosition = -1;
            var newCiPosition = FileSystemProcessor.FileSystemInterface.GetAvailablePosition();
            classInfo.Position = newCiPosition;
            WriteClassInfoHeader(classInfo, newCiPosition, writeInTransaction);
            WriteClassInfoBody(classInfo, FileSystemProcessor.FileSystemInterface.GetAvailablePosition(), writeInTransaction);
        }

        /// <summary>
        ///   Insert the object in the index
        /// </summary>
        /// <param name="oid"> The object id </param>
        /// <param name="nnoi"> The object meta represenation </param>
        /// <returns> The number of indexes </returns>
        public void ManageIndexesForInsert(OID oid, NonNativeObjectInfo nnoi)
        {
            var indexes = nnoi.GetClassInfo().GetIndexes();

            foreach (var index in indexes)
            {
                try
                {
                    var odbComparable = IndexTool.BuildIndexKey(index.Name, nnoi, index.AttributeIds);
                    index.BTree.Insert(odbComparable, oid);
                }
                catch (DuplicatedKeyException)
                {
                    // rollback what has been done
                    // bug #2510966
                    _session.Rollback();
                    throw;
                }
                // Check consistency : index should have size equal to the class
                // info element number
                if (index.BTree.GetSize() != nnoi.GetClassInfo().NumberOfObjects)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.BtreeSizeDiffersFromClassElementNumber.AddParameter(index.BTree.GetSize()).
                            AddParameter(nnoi.GetClassInfo().NumberOfObjects));
                }
            }
        }

        /// <summary>
        ///   Insert the object in the index
        /// </summary>
        /// <param name="oid"> The object id </param>
        /// <param name="nnoi"> The object meta represenation </param>
        /// <returns> The number of indexes </returns>
        private static void ManageIndexesForDelete(OID oid, NonNativeObjectInfo nnoi)
        {
            var indexes = nnoi.GetClassInfo().GetIndexes();

            foreach (var index in indexes)
            {
                // TODO manage collision!
                var odbComparable = IndexTool.BuildIndexKey(index.Name, nnoi, index.AttributeIds);
                index.BTree.Delete(odbComparable, oid);

                // Check consistency : index should have size equal to the class
                // info element number
                if (index.BTree.GetSize() != nnoi.GetClassInfo().NumberOfObjects)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.BtreeSizeDiffersFromClassElementNumber.AddParameter(index.BTree.GetSize()).
                            AddParameter(nnoi.GetClassInfo().NumberOfObjects));
                }
            }
        }

        /// <param name="oid"> The Oid of the object to be inserted </param>
        /// <param name="nnoi"> The object meta representation The object to be inserted in the database </param>
        /// <param name="isNewObject"> To indicate if object is new </param>
        /// <returns> The position of the inserted object </returns>
        public OID InsertNonNativeObject(OID oid, NonNativeObjectInfo nnoi, bool isNewObject)
        {
            return _nonNativeObjectWriter.InsertNonNativeObject(oid, nnoi, isNewObject);
        }

        /// <summary>
        ///   Updates an object.
        /// </summary>
        /// <remarks>
        ///   Updates an object. <pre>Try to update in place. Only change what has changed. This is restricted to particular types (fixed size types). If in place update is
        ///                        not possible, then deletes the current object and creates a new at the end of the database file and updates
        ///                        OID object position.
        ///                        &#064;param object The object to be updated
        ///                        &#064;param forceUpdate when true, no verification is done to check if update must be done.
        ///                        &#064;return The oid of the object, as a negative number
        ///                        &#064;</pre>
        /// </remarks>
        public OID UpdateNonNativeObjectInfo(NonNativeObjectInfo nnoi, bool forceUpdate)
        {
            return _nonNativeObjectWriter.UpdateNonNativeObjectInfo(nnoi, forceUpdate);
        }

        private long WriteAtomicNativeObject(AtomicNativeObjectInfo anoi, bool writeInTransaction,
                                                    int totalSpaceIfString = -1)
        {
            var startPosition = FileSystemProcessor.FileSystemInterface.GetPosition();
            var odbTypeId = anoi.GetOdbTypeId();
            WriteNativeObjectHeader(odbTypeId, anoi.IsNull(), BlockTypes.BlockTypeNativeObject, writeInTransaction);
            if (anoi.IsNull())
            {
                // Even if object is null, reserve space for to simplify/enable in
                // place update
                FileSystemProcessor.FileSystemInterface.EnsureSpaceFor(anoi.GetOdbType());
                return startPosition;
            }
            var @object = anoi.GetObject();
            switch (odbTypeId)
            {
                case OdbType.ByteId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteByte(((byte) @object), writeInTransaction);
                        break;
                    }

                case OdbType.SByteId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteSByte(((sbyte)@object), writeInTransaction);
                        break;
                    }

                case OdbType.BooleanId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteBoolean(((bool) @object), writeInTransaction);
                        break;
                    }

                case OdbType.CharacterId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteChar(((char) @object), writeInTransaction);
                        break;
                    }

                case OdbType.FloatId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteFloat(((float) @object), writeInTransaction);
                        break;
                    }

                case OdbType.DoubleId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteDouble(((double) @object), writeInTransaction);
                        break;
                    }

                case OdbType.IntegerId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteInt(((int) @object), writeInTransaction);
                        break;
                    }

                case OdbType.UIntegerId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteUInt(((uint)@object), writeInTransaction); // native attr
                        break;
                    }

                case OdbType.LongId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteLong(((long)@object), writeInTransaction); // native attr
                        break;
                    }

                case OdbType.ULongId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteULong(((ulong)@object), writeInTransaction); // native attr
                        break;
                    }

                case OdbType.ShortId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteShort(((short) @object), writeInTransaction);
                        break;
                    }

                case OdbType.UShortId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteUShort(((ushort)@object), writeInTransaction);
                        break;
                    }

                case OdbType.DecimalId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteBigDecimal((Decimal) @object, writeInTransaction);
                        break;
                    }

                case OdbType.DateId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteDate((DateTime) @object, writeInTransaction);
                        break;
                    }

                case OdbType.StringId:
                    {
                        FileSystemProcessor.FileSystemInterface.WriteString((string) @object, writeInTransaction, totalSpaceIfString);
                        break;
                    }

                case OdbType.OidId:
                    {
                        var oid = ((ObjectOID) @object).ObjectId;
                        FileSystemProcessor.FileSystemInterface.WriteLong(oid, writeInTransaction);
                        break;
                    }

                case OdbType.ObjectOidId:
                    {
                        var ooid = ((ObjectOID) @object).ObjectId;
                        FileSystemProcessor.FileSystemInterface.WriteLong(ooid, writeInTransaction);
                        break;
                    }

                case OdbType.ClassOidId:
                    {
                        var coid = ((ClassOID) @object).ObjectId;
                        FileSystemProcessor.FileSystemInterface.WriteLong(coid, writeInTransaction);
                        break;
                    }

                default:
                {
                    var typeId = odbTypeId.ToString();
                    var message = 
                        "native type with odb type id " + typeId + " (" + OdbType.GetNameFromId(odbTypeId) + ") for attribute ? is not suported";

                    throw new OdbRuntimeException(NDatabaseError.InternalError.AddParameter(message));
                }
            }
            return startPosition;
        }

        /// <summary>
        ///   Updates the previous object position field of the object at objectPosition
        /// </summary>
        /// <param name="objectOID"> </param>
        /// <param name="previousObjectOID"> </param>
        /// <param name="writeInTransaction"> </param>
        public void UpdatePreviousObjectFieldOfObjectInfo(OID objectOID, OID previousObjectOID,
                                                                  bool writeInTransaction)
        {
            var objectPosition = _idManager.GetObjectPositionWithOid(objectOID, true);
            FileSystemProcessor.FileSystemInterface.SetWritePosition(objectPosition + StorageEngineConstant.ObjectOffsetPreviousObjectOid,
                                  writeInTransaction);
            FileSystemProcessor.WriteOid(previousObjectOID, writeInTransaction);
        }

        /// <summary>
        ///   Update next object oid field of the object at the specific position
        /// </summary>
        public void UpdateNextObjectFieldOfObjectInfo(OID objectOID, OID nextObjectOID, bool writeInTransaction)
        {
            var objectPosition = _idManager.GetObjectPositionWithOid(objectOID, true);
            FileSystemProcessor.FileSystemInterface.SetWritePosition(
                objectPosition + StorageEngineConstant.ObjectOffsetNextObjectOid, writeInTransaction);
            FileSystemProcessor.WriteOid(nextObjectOID, writeInTransaction); // next object oid of object info
        }

        /// <summary>
        ///   Mark a block as deleted
        /// </summary>
        /// <returns> The block size </returns>
        public void MarkAsDeleted(long currentPosition, bool writeInTransaction)
        {
            FileSystemProcessor.FileSystemInterface.SetReadPosition(currentPosition);
            var blockSize = FileSystemProcessor.FileSystemInterface.ReadInt();
            FileSystemProcessor.FileSystemInterface.SetWritePosition(currentPosition + StorageEngineConstant.NativeObjectOffsetBlockType,
                                  writeInTransaction);
            // Do not write block size, leave it as it is, to know the available
            // space for future use
            FileSystemProcessor.FileSystemInterface.WriteByte(BlockTypes.BlockTypeDeleted, writeInTransaction);
            StoreFreeSpace(currentPosition, blockSize);
        }

        public IIdManager GetIdManager()
        {
            return _idManager;
        }

        public void Close()
        {
            _objectReader = null;
            if (_idManager != null)
            {
                _idManager.Clear();
                _idManager = null;
            }
            _storageEngine = null;
            FileSystemProcessor.Close();
        }

        public OID Delete(ObjectInfoHeader header)
        {
            var lsession = _session;
            var cache = lsession.GetCache();
            var objectPosition = header.GetPosition();
            var classInfoId = header.GetClassInfoId();
            var oid = header.GetOid();
            // gets class info from in memory meta model
            var ci = _session.GetMetaModel().GetClassInfoFromId(classInfoId);
            var withIndex = !ci.GetIndexes().IsEmpty();
            NonNativeObjectInfo nnoi = null;
            // When there is index,we must *always* load the old meta representation
            // to compute index keys
            if (withIndex)
                nnoi = _objectReader.ReadNonNativeObjectInfoFromPosition(ci, header.GetOid(), objectPosition, true,
                                                                         false);
            // a boolean value to indicate if object is in connected zone or not
            // This will be used to know if work can be done out of transaction
            // for unconnected object,changes can be written directly, else we must
            // use Transaction (using WriteAction)
            var objectIsInConnectedZone = cache.IsInCommitedZone(header.GetOid());
            // triggers
            _triggerManager.ManageDeleteTriggerBefore(ci.UnderlyingType, null, header.GetOid());
            
            var previousObjectOID = header.GetPreviousObjectOID();
            var nextObjectOID = header.GetNextObjectOID();
            if (OdbConfiguration.IsLoggingEnabled())
            {
                var isInConnectedZone = objectIsInConnectedZone.ToString();
                var hasIndex = withIndex.ToString();
                DLogger.Debug("ObjectWriter: Deleting object with id " + header.GetOid() + " - In connected zone =" +
                              isInConnectedZone + " -  with index =" + hasIndex);
                
                var positionAsString = objectPosition.ToString();
                DLogger.Debug("ObjectWriter: position =  " + positionAsString + " | prev oid = " + previousObjectOID + " | next oid = " +
                              nextObjectOID);
            }
            var isFirstObject = previousObjectOID == null;
            var isLastObject = nextObjectOID == null;
            var mustUpdatePreviousObjectPointers = false;
            var mustUpdateNextObjectPointers = false;
            var mustUpdateLastObjectOfClassInfo = false;

            if (isFirstObject || isLastObject)
            {
                if (isFirstObject)
                {
                    // The deleted object is the first, must update first instance
                    // OID field of the class
                    if (objectIsInConnectedZone)
                    {
                        // update first object oid of the class info in memory
                        ci.CommitedZoneInfo.First = nextObjectOID;
                    }
                    else
                    {
                        // update first object oid of the class info in memory
                        ci.UncommittedZoneInfo.First = nextObjectOID;
                    }
                    if (nextObjectOID != null)
                    {
                        // Update next object 'previous object oid' to null
                        UpdatePreviousObjectFieldOfObjectInfo(nextObjectOID, null, objectIsInConnectedZone);
                        mustUpdateNextObjectPointers = true;
                    }
                }
                // It can be first and last
                if (isLastObject)
                {
                    // The deleted object is the last, must update last instance
                    // OID field of the class
                    // update last object position of the class info in memory
                    if (objectIsInConnectedZone)
                    {
                        // the object is a committed object
                        ci.CommitedZoneInfo.Last = previousObjectOID;
                    }
                    else
                    {
                        // The object is not committed and it is the last and is
                        // being deleted
                        ci.UncommittedZoneInfo.Last = previousObjectOID;
                    }
                    if (previousObjectOID != null)
                    {
                        // Update 'next object oid' of previous object to null
                        // if we are in unconnected zone, change can be done
                        // directly,else it must be done in transaction
                        UpdateNextObjectFieldOfObjectInfo(previousObjectOID, null, objectIsInConnectedZone);
                        // Now update data of the cache
                        mustUpdatePreviousObjectPointers = true;
                        mustUpdateLastObjectOfClassInfo = true;
                    }
                }
            }
            else
            {
                // Normal case, the deleted object has previous and next object
                // pull the deleted object
                // Mark the 'next object oid field' of the previous object
                // pointing the next object
                UpdateNextObjectFieldOfObjectInfo(previousObjectOID, nextObjectOID, objectIsInConnectedZone);
                // Mark the 'previous object position field' of the next object
                // pointing the previous object
                UpdatePreviousObjectFieldOfObjectInfo(nextObjectOID, previousObjectOID, objectIsInConnectedZone);
                mustUpdateNextObjectPointers = true;
                mustUpdatePreviousObjectPointers = true;
            }
            if (mustUpdateNextObjectPointers)
                UpdateNextObjectPreviousPointersInCache(nextObjectOID, previousObjectOID, cache);
            if (mustUpdatePreviousObjectPointers)
            {
                var oih = UpdatePreviousObjectNextPointersInCache(nextObjectOID, previousObjectOID, cache);
                if (mustUpdateLastObjectOfClassInfo)
                    ci.LastObjectInfoHeader = oih;
            }
            var metaModel = lsession.GetMetaModel();
            // Saves the fact that something has changed in the class (number of
            // objects and/or last object oid)
            metaModel.AddChangedClass(ci);
            if (objectIsInConnectedZone)
                ci.CommitedZoneInfo.DecreaseNbObjects();
            else
                ci.UncommittedZoneInfo.DecreaseNbObjects();
            // Manage deleting the last object of the committed zone
            CIZoneInfo commitedZoneInfo = ci.CommitedZoneInfo;

            var isLastObjectOfCommitedZone = oid.Equals(commitedZoneInfo.Last);
            if (isLastObjectOfCommitedZone)
            {
                // Load the object info header of the last committed object
                var oih = _objectReader.ReadObjectInfoHeaderFromOid(oid, true);
                // Updates last committed object id of the committed zone.
                // Here, it can be null, but there is no problem
                commitedZoneInfo.Last = oih.GetPreviousObjectOID();
                // A simple check, if commitedZI.last is null, nbObject must be 0
                if (commitedZoneInfo.Last == null && commitedZoneInfo.HasObjects())
                {
                    var numberbOfObjectsAsString = commitedZoneInfo.GetNumberbOfObjects().ToString();

                    throw new OdbRuntimeException(
                        NDatabaseError.InternalError.AddParameter(
                            "The last object of the commited zone has been deleted but the Zone still have objects : nbobjects=" +
                            numberbOfObjectsAsString));
                }
            }
            // Manage deleting the first object of the uncommitted zone
            var uncommittedZoneInfo = ci.UncommittedZoneInfo;

            var isFirstObjectOfUncommitedZone = oid.Equals(uncommittedZoneInfo.First);
            if (isFirstObjectOfUncommitedZone)
            {
                if (uncommittedZoneInfo.HasObjects())
                {
                    // Load the object info header of the first uncommitted object
                    var oih = _objectReader.ReadObjectInfoHeaderFromOid(oid, true);
                    // Updates first uncommitted oid with the second uncommitted oid
                    // Here, it can be null, but there is no problem
                    uncommittedZoneInfo.First = oih.GetNextObjectOID();
                }
                else
                    uncommittedZoneInfo.First = null;
            }
            if (isFirstObject && isLastObject)
            {
                // The object was the first and the last object => it was the only
                // object
                // There is no more objects of this type => must set to null the
                // ClassInfo LastObjectOID
                ci.LastObjectInfoHeader = null;
            }
            GetIdManager().UpdateIdStatus(header.GetOid(), IDStatus.Deleted);
            // The update of the place must be done in transaction if object is in
            // committed zone, else it can be done directly in the file
            MarkAsDeleted(objectPosition, objectIsInConnectedZone);
            cache.MarkIdAsDeleted(header.GetOid());
            if (withIndex)
                ManageIndexesForDelete(header.GetOid(), nnoi);
            // triggers
            _triggerManager.ManageDeleteTriggerAfter(ci.UnderlyingType, null, header.GetOid());
            return header.GetOid();
        }

        public void SetTriggerManager(IInternalTriggerManager triggerManager)
        {
            _triggerManager = triggerManager;
            _nonNativeObjectWriter.SetTriggerManager(triggerManager);
        }

        #endregion

        /// <summary>
        ///   Write the class info body to the database file.
        /// </summary>
        /// <remarks>
        ///   Write the class info body to the database file. TODO Check if we really must recall the writeClassInfoHeader
        /// </remarks>
        private void WriteClassInfoBody(ClassInfo classInfo, long position, bool writeInTransaction)
        {
            if (OdbConfiguration.IsLoggingEnabled())
            {
                var positionAsString = position.ToString();
                DLogger.Debug("ObjectWriter: Writing new Class info body at " + positionAsString + " : " + classInfo);
            }
            // updates class info
            classInfo.AttributesDefinitionPosition = position;
            // FIXME : change this to write only the position and not the whole
            // header
            WriteClassInfoHeader(classInfo, classInfo.Position, writeInTransaction);
            FileSystemProcessor.FileSystemInterface.SetWritePosition(position, writeInTransaction);
            // block definition
            FileSystemProcessor.FileSystemInterface.WriteInt(0, writeInTransaction);
            FileSystemProcessor.FileSystemInterface.WriteByte(BlockTypes.BlockTypeClassBody, writeInTransaction);
            // number of class attributes
            FileSystemProcessor.FileSystemInterface.WriteLong(classInfo.Attributes.Count, writeInTransaction);
            
            foreach (var classAttributeInfo in classInfo.Attributes)
                FileSystemProcessor.WriteClassAttributeInfo(_storageEngine, classAttributeInfo, writeInTransaction);

            var blockSize = (int) (FileSystemProcessor.FileSystemInterface.GetPosition() - position);
            FileSystemProcessor.WriteBlockSizeAt(position, blockSize, writeInTransaction, classInfo);
        }

        /// <summary>
        ///   Actually write the object data to the database file
        /// </summary>
        /// <param name="noi"> The object meta infor The object info to be written </param>
        /// <param name="position"> if -1, it is a new instance, if not, it is an update </param>
        /// <param name="writeInTransaction"> </param>
        /// <returns> The object posiiton or id(if &lt; 0) </returns>
        private long WriteNativeObjectInfo(NativeObjectInfo noi, long position, bool writeInTransaction)
        {
            if (OdbConfiguration.IsLoggingEnabled())
            {
                var positionAsString = position.ToString();
                DLogger.Debug(string.Concat("ObjectWriter: Writing native object at", positionAsString,
                                            string.Format(" : Type={0} | Value={1}",
                                                          OdbType.GetNameFromId(noi.GetOdbTypeId()), noi)));
            }

            if (noi.IsAtomicNativeObject())
                return WriteAtomicNativeObject((AtomicNativeObjectInfo) noi, writeInTransaction);
            if (noi.IsNull())
            {
                WriteNullNativeObjectHeader(noi.GetOdbTypeId(), writeInTransaction);
                return position;
            }
            if (noi.IsArrayObject())
                return WriteArray((ArrayObjectInfo) noi, writeInTransaction);
            if (noi.IsEnumObject())
                return WriteEnumNativeObject((EnumNativeObjectInfo) noi, writeInTransaction);
            throw new OdbRuntimeException(NDatabaseError.NativeTypeNotSupported.AddParameter(noi.GetOdbTypeId()));
        }

        /// <summary>
        ///   Updates pointers of objects, Only changes uncommitted info pointers
        /// </summary>
        /// <param name="objectInfo"> The meta representation of the object being inserted </param>
        /// <param name="classInfo"> The class of the object being inserted </param>
        public void ManageNewObjectPointers(NonNativeObjectInfo objectInfo, ClassInfo classInfo)
        {
            var cache = _storageEngine.GetSession().GetCache();
            var isFirstUncommitedObject = !classInfo.UncommittedZoneInfo.HasObjects();
            // if it is the first uncommitted object
            if (isFirstUncommitedObject)
            {
                classInfo.UncommittedZoneInfo.First = objectInfo.GetOid();
                var lastCommittedObjectOid = classInfo.CommitedZoneInfo.Last;
                if (lastCommittedObjectOid != null)
                {
                    // Also updates the last committed object next object oid in
                    // memory to connect the committed
                    // zone with unconnected for THIS transaction (only in memory)
                    var oih = cache.GetObjectInfoHeaderByOid(lastCommittedObjectOid, true);
                    oih.SetNextObjectOID(objectInfo.GetOid());
                    // And sets the previous oid of the current object with the last
                    // committed oid
                    objectInfo.SetPreviousInstanceOID(lastCommittedObjectOid);
                }
            }
            else
            {
                // Gets the last object, updates its (next object)
                // pointer to the new object and updates the class info 'last
                // uncommitted object
                // oid' field
                var oip = classInfo.LastObjectInfoHeader;
                if (oip == null)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.InternalError.AddParameter("last OIP is null in manageNewObjectPointers oid=" +
                                                                 objectInfo.GetOid()));
                }
                if (oip.GetNextObjectOID() != objectInfo.GetOid())
                {
                    oip.SetNextObjectOID(objectInfo.GetOid());
                    // Here we are working in unconnected zone, so this
                    // can be done without transaction: actually
                    // write in database file
                    UpdateNextObjectFieldOfObjectInfo(oip.GetOid(), oip.GetNextObjectOID(), false);
                    objectInfo.SetPreviousInstanceOID(oip.GetOid());
                    // Resets the class info oid: In some case,
                    // (client // server) it may be -1.
                    oip.SetClassInfoId(classInfo.ClassInfoId);
                    // object info oip has been changed, we must put it
                    // in the cache to turn this change available for current
                    // transaction until the commit
                    _storageEngine.GetSession().GetCache().AddObjectInfoOfNonCommitedObject(oip);
                }
            }
            // always set the new last object oid and the number of objects
            classInfo.UncommittedZoneInfo.Last = objectInfo.GetOid();
            classInfo.UncommittedZoneInfo.IncreaseNbObjects();
            // Then updates the last info pointers of the class info
            // with this new created object
            // At this moment, the objectInfo.getHeader() do not have the
            // attribute ids.
            // but later in this code, the attributes will be set, so the class
            // info also will have them
            classInfo.LastObjectInfoHeader = objectInfo.GetHeader();
            // // Saves the fact that something has changed in the class (number of
            // objects and/or last object oid)
            _storageEngine.GetSession().GetMetaModel().AddChangedClass(classInfo);
        }

        // This will be done by the mainStoreObject method
        // Context.getCache().endInsertingObject(object);
        /// <param name="noi"> The native object meta representation The object to be inserted in the database </param>
        /// <returns> The position of the inserted object </returns>
        private long InsertNativeObject(NativeObjectInfo noi)
        {
            var writePosition = FileSystemProcessor.FileSystemInterface.GetAvailablePosition();
            FileSystemProcessor.FileSystemInterface.SetWritePosition(writePosition, true);
            // true,false = update pointers,do not write in transaction, writes
            // directly to hard disk
            return WriteNativeObjectInfo(noi, writePosition, false);
        }

        /// <summary>
        ///   Store a meta representation of an object(already as meta representation)in ODBFactory database.
        /// </summary>
        /// <remarks>
        ///   Store a meta representation of an object(already as meta representation)in ODBFactory database. To detect if object must be updated or insert, we use the cache. To update an object, it must be first selected from the database. When an object is to be stored, if it exist in the cache, then it will be updated, else it will be inserted as a new object. If the object is null, the cache will be used to check if the meta representation is in the cache
        /// </remarks>
        /// <param name="oid"> The oid of the object to be inserted/updates </param>
        /// <param name="nnoi"> The meta representation of an object </param>
        /// <returns> The object position </returns>
        public OID StoreObject(OID oid, NonNativeObjectInfo nnoi)
        {
            // first detects if we must perform an insert or an update
            // If object is in the cache, we must perform an update, else an insert
            var @object = nnoi.GetObject();
            var mustUpdate = false;
            var cache = _session.GetCache();
            if (@object != null)
            {
                var cacheOid = cache.IdOfInsertingObject(@object);
                if (cacheOid != null)
                    return cacheOid;
                // throw new ODBRuntimeException("Inserting meta representation of
                // an object without the object itself is not yet supported");
                mustUpdate = cache.Contains(@object);
            }
            if (!mustUpdate)
                mustUpdate = !Equals(nnoi.GetOid(), StorageEngineConstant.NullObjectId);
            
            return mustUpdate
                       ? UpdateNonNativeObjectInfo(nnoi, false)
                       : InsertNonNativeObject(oid, nnoi, true);
        }

        /// <summary>
        ///   Store a meta representation of a native object(already as meta representation)in ODBFactory database.
        /// </summary>
        /// <remarks>
        ///   Store a meta representation of a native object(already as meta representation)in ODBFactory database. A Native object is an object that use native language type, String for example To detect if object must be updated or insert, we use the cache. To update an object, it must be first selected from the database. When an object is to be stored, if it exist in the cache, then it will be updated, else it will be inserted as a new object. If the object is null, the cache will be used to check if the meta representation is in the cache
        /// </remarks>
        /// <param name="noi"> The meta representation of an object </param>
        /// <returns> The object position @ </returns>
        public long InternalStoreObject(NativeObjectInfo noi)
        {
            return InsertNativeObject(noi);
        }

        private void UpdateNextObjectPreviousPointersInCache(OID nextObjectOID, OID previousObjectOID, IOdbCache cache)
        {
            var oip = cache.GetObjectInfoHeaderByOid(nextObjectOID, false);

            // If object is not in the cache, then read the header from the file
            if (oip == null)
            {
                oip = _objectReader.ReadObjectInfoHeaderFromOid(nextObjectOID, false);
                cache.AddObjectInfoOfNonCommitedObject(oip);
            }

            oip.SetPreviousObjectOID(previousObjectOID);
        }

        private ObjectInfoHeader UpdatePreviousObjectNextPointersInCache(OID nextObjectOID, OID previousObjectOID,
                                                                                IOdbCache cache)
        {
            var oip = cache.GetObjectInfoHeaderByOid(previousObjectOID, false);

            // If object is not in the cache, then read the header from the file
            if (oip == null)
            {
                oip = _objectReader.ReadObjectInfoHeaderFromOid(previousObjectOID, false);
                cache.AddObjectInfoOfNonCommitedObject(oip);
            }

            oip.SetNextObjectOID(nextObjectOID);
            return oip;
        }

        /// <summary>
        ///   <pre>Write an array to the database
        ///     This is done by writing :
        ///     - the array type : array
        ///     - the array element type (String if it os a String [])
        ///     - the position of the non native type, if element are non java / C# native
        ///     - the number of element s and then the position of all elements.
        ///     </pre>
        /// </summary>
        /// <remarks>
        ///   <pre>Write an array to the database
        ///     This is done by writing :
        ///     - the array type : array
        ///     - the array element type (String if it os a String [])
        ///     - the position of the non native type, if element are non java / C# native
        ///     - the number of element s and then the position of all elements.
        ///     Example : an array with two string element : 'ola' and 'chico'
        ///     write 22 : array
        ///     write  20 : array of STRING
        ///     write 0 : it is a java native object
        ///     write 2 (as an int) : the number of elements
        ///     write two times 0 (as long) to reserve the space for the elements positions
        ///     then write the string 'ola', and keeps its position in the 'positions' array of long
        ///     then write the string 'chico' and keeps its position in the 'positions' array of long
        ///     Then write back all the positions (in this case , 2 positions) after the size of the array
        ///     Example : an array with two User element : user1 and user2
        ///     write 22 : array
        ///     write  23 : array of NON NATIVE Objects
        ///     write 251 : if 250 is the position of the user class info in database
        ///     write 2 (as an int) : the number of elements
        ///     write two times 0 (as long) to reserve the space for the elements positions
        ///     then write the user user1, and keeps its position in the 'positions' array of long
        ///     then write the user user2 and keeps its position in the 'positions' array of long
        ///     &lt;pre&gt;
        ///     &#064;param object
        ///     &#064;param odbType
        ///     &#064;param position
        ///     &#064;param writeInTransaction
        ///     &#064;</pre>
        /// </remarks>
        private long WriteArray(ArrayObjectInfo aoi, bool writeInTransaction)
        {
            var startPosition = FileSystemProcessor.FileSystemInterface.GetPosition();
            WriteNativeObjectHeader(aoi.GetOdbTypeId(), aoi.IsNull(), BlockTypes.BlockTypeArrayObject,
                                    writeInTransaction);
            if (aoi.IsNull())
                return startPosition;
            var array = aoi.GetArray();
            var arraySize = array.Length;
            // Writes the fact that it is an array
            FileSystemProcessor.FileSystemInterface.WriteString(aoi.GetRealArrayComponentClassName(), writeInTransaction);
            // write the size of the array
            FileSystemProcessor.FileSystemInterface.WriteInt(arraySize, writeInTransaction);
            // build a n array to store all element positions
            var attributeIdentifications = new long[arraySize];
            // Gets the current position, to know later where to put the
            // references
            var firstObjectPosition = FileSystemProcessor.FileSystemInterface.GetPosition();
            // reserve space for object positions : write 'arraySize' long
            // with zero to store each object position
            for (var i = 0; i < arraySize; i++)
                FileSystemProcessor.FileSystemInterface.WriteLong(0, writeInTransaction); // array element pos
            for (var i = 0; i < arraySize; i++)
            {
                var element = (AbstractObjectInfo) array[i];
                if (element == null || element.IsNull())
                {
                    // TODO Check this
                    attributeIdentifications[i] = StorageEngineConstant.NullObjectIdId;
                    continue;
                }
                attributeIdentifications[i] = InternalStoreObjectWrapper(element);
            }
            var positionAfterWrite = FileSystemProcessor.FileSystemInterface.GetPosition();
            // now that all objects have been stored, sets their position in the
            // space that have been reserved
            FileSystemProcessor.FileSystemInterface.SetWritePosition(firstObjectPosition, writeInTransaction);
            for (var i = 0; i < arraySize; i++)
            {
                FileSystemProcessor.FileSystemInterface.WriteLong(attributeIdentifications[i], writeInTransaction); //array real element pos
            }
            // Gos back to the end of the array
            FileSystemProcessor.FileSystemInterface.SetWritePosition(positionAfterWrite, writeInTransaction);
            return startPosition;
        }

        /// <summary>
        ///   This method is used to store the object : natibe or non native and return a number : - The position of the object if it is a native object - The oid (as a negative number) if it is a non native object
        /// </summary>
        /// <param name="aoi"> </param>
        /// <returns> </returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        private long InternalStoreObjectWrapper(AbstractObjectInfo aoi)
        {
            if (aoi.IsNative())
                return InternalStoreObject((NativeObjectInfo) aoi);
            if (aoi.IsNonNativeObject())
            {
                var oid = StoreObject(null, (NonNativeObjectInfo) aoi);
                return -oid.ObjectId;
            }
            // Object references are references to object already stored.
            // But in the case of map, the reference can appear before the real
            // object (as order may change)
            // If objectReference.getOid() is null, it is the case. In this case,
            // We take the object being referenced and stores it directly.
            var objectReference = (ObjectReference) aoi;
            if (objectReference.GetOid() == null)
            {
                var oid = StoreObject(null, objectReference.GetNnoi());
                return -oid.ObjectId;
            }
            return -objectReference.GetOid().ObjectId;
        }

        private void WriteNullNativeObjectHeader(int odbTypeId, bool writeInTransaction)
        {
            WriteNativeObjectHeader(odbTypeId, true, BlockTypes.BlockTypeNativeNullObject, writeInTransaction);
        }

        /// <summary>
        ///   Write the header of a native attribute
        /// </summary>
        private void WriteNativeObjectHeader(int odbTypeId, bool isNull, byte blockType,
                                                       bool writeDataInTransaction)
        {
            var bytes = new byte[10];
            bytes[0] = _nativeHeaderBlockSizeByte[0];
            bytes[1] = _nativeHeaderBlockSizeByte[1];
            bytes[2] = _nativeHeaderBlockSizeByte[2];
            bytes[3] = _nativeHeaderBlockSizeByte[3];
            bytes[4] = blockType;

            var bytesTypeId = ByteArrayConverter.IntToByteArray(odbTypeId);
            bytes[5] = bytesTypeId[0];
            bytes[6] = bytesTypeId[1];
            bytes[7] = bytesTypeId[2];
            bytes[8] = bytesTypeId[3];
            bytes[9] = ByteArrayConverter.BooleanToByteArray(isNull)[0];

            FileSystemProcessor.FileSystemInterface.WriteBytes(bytes, writeDataInTransaction);
        }

        private long WriteEnumNativeObject(EnumNativeObjectInfo anoi, bool writeInTransaction)
        {
            var startPosition = FileSystemProcessor.FileSystemInterface.GetPosition();
            var odbTypeId = anoi.GetOdbTypeId();
            WriteNativeObjectHeader(odbTypeId, anoi.IsNull(), BlockTypes.BlockTypeNativeObject, writeInTransaction);
            // Writes the Enum ClassName
            FileSystemProcessor.FileSystemInterface.WriteLong(anoi.GetEnumClassInfo().ClassInfoId.ObjectId, writeInTransaction); //enum class info id
            // Write the Enum String value
            FileSystemProcessor.FileSystemInterface.WriteString(anoi.GetObject().ToString(), writeInTransaction);
            return startPosition;
        }

        private static void StoreFreeSpace(long currentPosition, int blockSize)
        {
            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug(string.Concat("ObjectWriter: Storing free space at position ", currentPosition.ToString(),
                                            " | block size = ", blockSize.ToString()));
        }

        public void Dispose()
        {
            Close();
        }
    }
}
