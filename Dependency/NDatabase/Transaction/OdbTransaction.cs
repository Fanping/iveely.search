using System.Collections.Generic;
using System.Text;
using NDatabase.Api;
using NDatabase.Core;
using NDatabase.Core.Engine;
using NDatabase.Core.Session;
using NDatabase.Exceptions;
using NDatabase.IO;
using NDatabase.Meta;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Transaction
{
    /// <summary>
    /// The transaction class is used to guarantee ACID behavior.
    /// </summary>
    /// <remarks>
    /// The transaction class is used to guarantee ACID behavior. It keep tracks of all session
    /// operations. It uses the WriteAction class to store all changes that can not be written to the file
    /// before the commit.
    /// The transaction is held by The Session class and manage commits and rollbacks.
    /// All WriteActions are written in a transaction file to be sure to be able to commit and in case
    /// of very big transaction where all WriteActions can not be stored in memory.
    /// </remarks>
    internal sealed class OdbTransaction : ITransaction
    {
        /// <summary>
        ///   The transaction creation time
        /// </summary>
        private long _creationDateTime;

        /// <summary>
        ///   The same write action is reused for successive writes
        /// </summary>
        private WriteAction _currentWriteAction;

        /// <summary>
        ///   The position of the next write for WriteAction
        /// </summary>
        private long _currentWritePositionInWa;

        /// <summary>
        ///   A file interface to the transaction file - used to read/write the file
        /// </summary>
        private IFileSystemInterface _fsi;

        /// <summary>
        ///   A file interface to the engine main file
        /// </summary>
        private IFileSystemInterface _fsiToApplyWriteActions;

        /// <summary>
        ///   To indicate if all write actions are in memory - if not, transaction must read them from transaction file o commit the transaction
        /// </summary>
        private bool _hasAllWriteActionsInMemory;

        /// <summary>
        ///   To indicate if transaction has already been persisted in file
        /// </summary>
        private bool _hasBeenPersisted;

        /// <summary>
        ///   To indicate if transaction was confirmed = committed
        /// </summary>
        private bool _isCommited;

        /// <summary>
        ///   The number of write actions
        /// </summary>
        private int _numberOfWriteActions;

        /// <summary>
        ///   The transaction session
        /// </summary>
        private ISession _session;

        /// <summary>
        ///   To indicate if transaction was rollbacked
        /// </summary>
        private bool _wasRollbacked;

        /// <summary>
        ///   All the pending writing that must be applied to actually commit the transaction
        /// </summary>
        private IList<WriteAction> _writeActions;

        public OdbTransaction(ISession session, IFileSystemInterface fsiToApplyTransaction)
        {
            _fsiToApplyWriteActions = fsiToApplyTransaction;
            Init(session);
        }

        #region ITransaction Members

        public void Clear()
        {
            if (_writeActions == null)
                return;

            _writeActions.Clear();
            _writeActions = null;
        }

        /// <summary>
        ///   Reset the transaction
        /// </summary>
        public void Reset()
        {
            Clear();
            Init(_session);
            _fsi = null;
        }

        public void Rollback()
        {
            _wasRollbacked = true;
            if (_fsi != null)
                _fsi.Close();
        }

        public void Commit()
        {
            if (OdbConfiguration.IsLoggingEnabled())
            {
                var numberOfWriteActionsAsString = _numberOfWriteActions.ToString();
                var hasAllWriteActionsInMemoryAsString = _hasAllWriteActionsInMemory.ToString();

                DLogger.Info("OdbTransaction: Commiting " + numberOfWriteActionsAsString + " write actions - In Memory : " +
                             hasAllWriteActionsInMemoryAsString + string.Format(" - sid={0}", _session.GetId()));
            }

            // Check if database has been rollbacked
            CheckRollback();

            // call the commit listeners
            ManageCommitListenersBefore();

            if (_currentWriteAction != null && !_currentWriteAction.IsEmpty())
            {
                AddWriteAction(_currentWriteAction, true);
                _currentWriteAction = null;
            }

            if (_fsi == null && _numberOfWriteActions != 0)
                throw new OdbRuntimeException(NDatabaseError.TransactionAlreadyCommitedOrRollbacked);

            if (_numberOfWriteActions == 0)
            {
                // FIXME call commitMetaModel in realOnlyMode?
                CommitMetaModel();
                // Nothing to do
                if (_fsi != null)
                {
                    _fsi.Close();
                    _fsi = null;
                }
                if (_session != null)
                    _session.GetCache().ClearOnCommit();
                return;
            }

            // Marks the transaction as committed
            SetCommited();

            // Apply the write actions the main database file
            ApplyTo();

            // Commit Meta Model changes
            CommitMetaModel();

            if (_fsi != null)
            {
                _fsi.Close();
                _fsi = null;
            }

            if (_session != null)
                _session.GetCache().ClearOnCommit();

            ManageCommitListenersAfter();
        }

        public void SetFsiToApplyWriteActions(IFileSystemInterface fsi)
        {
            _fsiToApplyWriteActions = fsi;
        }

        /// <returns> Returns the numberOfWriteActions. </returns>
        public int GetNumberOfWriteActions()
        {
            if (_currentWriteAction != null && !_currentWriteAction.IsEmpty())
                return _numberOfWriteActions + 1;
            return _numberOfWriteActions;
        }

        /// <summary>
        ///   Set the write position (position in main database file).
        /// </summary>
        /// <remarks>
        ///   Set the write position (position in main database file). This is used to know if the next write can be appended to the previous one (in the same current Write Action) or not.
        /// </remarks>
        /// <param name="position"> </param>
        public void SetWritePosition(long position)
        {
            if (position != _currentWritePositionInWa)
            {
                _currentWritePositionInWa = position;
                if (_currentWriteAction != null)
                {
                    AddWriteAction(_currentWriteAction, true);
                }
                _currentWriteAction = new WriteAction(position);
            }
            else
            {
                if (_currentWriteAction == null)
                {
                    _currentWriteAction = new WriteAction(position);
                    _currentWritePositionInWa = position;
                }
            }
        }

        public void ManageWriteAction(long position, byte[] bytes)
        {
            if (_currentWritePositionInWa == position)
            {
                if (_currentWriteAction == null)
                {
                    _currentWriteAction = new WriteAction(position);
                }
                _currentWriteAction.AddBytes(bytes);
                _currentWritePositionInWa += bytes.Length;
            }
            else
            {
                if (_currentWriteAction != null)
                {
                    AddWriteAction(_currentWriteAction, true);
                }
                _currentWriteAction = new WriteAction(position, bytes);
                _currentWritePositionInWa = position + bytes.Length;
            }
        }

        #endregion

        private string GetName()
        {
            var parameters = _fsiToApplyWriteActions.GetFileIdentification();

            return string.Format("{0}-{1}-{2}.transaction", parameters.Id, _creationDateTime, _session.GetId());
        }

        private void Init(ISession session)
        {
            _session = session;
            _isCommited = false;
            _creationDateTime = OdbTime.GetCurrentTimeInTicks();
            _writeActions = new List<WriteAction>(1000);
            _hasAllWriteActionsInMemory = true;
            _numberOfWriteActions = 0;
            _hasBeenPersisted = false;
            _wasRollbacked = false;
            _currentWritePositionInWa = -1;
        }

        /// <summary>
        ///   Adds a write action to the transaction
        /// </summary>
        /// <param name="writeAction"> The write action to be added </param>
        /// <param name="persistWriteAction"> To indicate if write action must be persisted </param>
        private void AddWriteAction(WriteAction writeAction, bool persistWriteAction)
        {
            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Info(string.Format("OdbTransaction: Adding WriteAction in Transaction of session {0}", _session.GetId()));

            if (writeAction.IsEmpty())
                return;

            CheckRollback();
            if (!_hasBeenPersisted && persistWriteAction)
                Persist();

            if (persistWriteAction)
                writeAction.PersistMeTo(_fsi);

            // Only adds the write action to the list if the transaction keeps all in
            // memory
            if (_hasAllWriteActionsInMemory)
                _writeActions.Add(writeAction);

            _numberOfWriteActions++;

            if (_hasAllWriteActionsInMemory &&
                _numberOfWriteActions > StorageEngineConstant.MaxNumberOfWriteObjectPerTransaction)
            {
                _hasAllWriteActionsInMemory = false;

                foreach (var defaultWriteAction in _writeActions)
                    defaultWriteAction.Clear();

                _writeActions.Clear();

                if (OdbConfiguration.IsLoggingEnabled())
                {
                    var numberOfWriteActions = _numberOfWriteActions.ToString();
                    var maxNumberOfWriteObjectPerTransactionAsString =
                        StorageEngineConstant.MaxNumberOfWriteObjectPerTransaction.ToString();

                    DLogger.Info("OdbTransaction: Number of objects has exceeded the max number " + numberOfWriteActions + "/" +
                                 maxNumberOfWriteObjectPerTransactionAsString +
                                 ": switching to persistent transaction managment");
                }
            }
        }

        private IDbIdentification GetParameters()
        {
            var parameters = _fsiToApplyWriteActions.GetFileIdentification();

            return parameters.GetTransactionIdentification(_creationDateTime, _session.GetId());
        }

        private void CheckFileAccess(string fileName)
        {
            lock (this)
            {
                if (_fsi != null)
                    return;

                IDbIdentification parameters;
                if (_fsiToApplyWriteActions.GetFileIdentification() is InMemoryIdentification)
                {
                    parameters = GetParameters();
                }
                else
                {
                    parameters = _fsiToApplyWriteActions == null
                                     ? new FileIdentification(fileName)
                                     : GetParameters();
                }

                _fsi = new FileSystemInterface(parameters, MultiBuffer.DefaultBufferSizeForTransaction, _session);
            }
        }

        private void Persist()
        {
            CheckFileAccess(null);

            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug(string.Format("OdbTransaction: # Persisting transaction {0}", GetName()));

            _fsi.SetWritePosition(0, false);
            _fsi.WriteBoolean(_isCommited, false);
            _fsi.WriteLong(_creationDateTime, false);

            // Size
            _fsi.WriteLong(0, false);
            _hasBeenPersisted = true;
        }

        /// <summary>
        ///   Mark te transaction file as committed
        /// </summary>
        private void SetCommited()
        {
            _isCommited = true;
            CheckFileAccess(null);

            // TODO Check atomicity
            // Writes the number of write actions after the byte and date
            _fsi.SetWritePositionNoVerification(OdbType.Byte.Size + OdbType.Long.Size, false);
            _fsi.WriteLong(_numberOfWriteActions, false);
            // FIXME The fsi.flush should not be called after the last write?
            _fsi.Flush();
            // Only set useBuffer = false when it is a local database to avoid
            // net io overhead

            _fsi.SetWritePositionNoVerification(0, false);
            _fsi.WriteByte(1, false);
        }

        private void CheckRollback()
        {
            if (_wasRollbacked)
                throw new OdbRuntimeException(NDatabaseError.OdbHasBeenRollbacked);
        }

        private void ManageCommitListenersAfter()
        {
            var listeners = _session.GetStorageEngine().GetCommitListeners();
            if (listeners == null || listeners.IsEmpty())
                return;

            foreach (var commitListener in listeners)
                commitListener.AfterCommit();
        }

        private void ManageCommitListenersBefore()
        {
            var listeners = _session.GetStorageEngine().GetCommitListeners();
            if (listeners == null || listeners.IsEmpty())
                return;

            foreach (var commitListener in listeners)
                commitListener.BeforeCommit();
        }

        /// <summary>
        ///   Used to commit meta model : classes This is useful when running in client server mode TODO Check this
        /// </summary>
        private void CommitMetaModel()
        {
            var sessionMetaModel = _session.GetMetaModel();
            // If meta model has not been modified, there is nothing to do
            if (!sessionMetaModel.HasChanged())
                return;

            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug("OdbTransaction: Start commitMetaModel");

            // In local mode, we must not reload the meta model as there is no
            // concurrent access
            var lastCommitedMetaModel = sessionMetaModel;

            var writer = _session.GetStorageEngine().GetObjectWriter();

            // Gets the classes that have changed (that have modified ,deleted or inserted objects)
            var enumerator = sessionMetaModel.GetChangedClassInfo().GetEnumerator();

            // for all changes between old and new meta model
            while (enumerator.MoveNext())
            {
                var newClassInfo = enumerator.Current;

                if (newClassInfo == null)
                    continue;

                ClassInfo lastCommittedClassInfo;

                if (lastCommitedMetaModel.ExistClass(newClassInfo.UnderlyingType))
                {
                    // The last CI represents the last committed meta model of the
                    // database
                    lastCommittedClassInfo = lastCommitedMetaModel.GetClassInfoFromId(newClassInfo.ClassInfoId);
                    // Just be careful to keep track of current CI committed zone
                    // deleted objects
                    lastCommittedClassInfo.CommitedZoneInfo.SetNbDeletedObjects(
                        newClassInfo.CommitedZoneInfo.GetNbDeletedObjects());
                }
                else
                    lastCommittedClassInfo = newClassInfo;

                var lastCommittedObjectOidOfThisTransaction = newClassInfo.CommitedZoneInfo.Last;
                var lastCommittedObjectOidOfPrevTransaction = lastCommittedClassInfo.CommitedZoneInfo.Last;
                var lastCommittedObjectOid = lastCommittedObjectOidOfPrevTransaction;

                // If some object have been created then
                if (lastCommittedObjectOidOfPrevTransaction != null)
                {
                    // Checks if last object of committed meta model has not been
                    // deleted
                    if (_session.GetCache().IsDeleted(lastCommittedObjectOidOfPrevTransaction))
                    {
                        // TODO This is wrong: if a committed transaction deleted a
                        // committed object and creates x new
                        // objects, then all these new objects will be lost:
                        // if it has been deleted then use the last object of the
                        // session class info
                        lastCommittedObjectOid = lastCommittedObjectOidOfThisTransaction;
                        newClassInfo.CommitedZoneInfo.Last = lastCommittedObjectOid;
                    }
                }

                // Connect Unconnected zone to connected zone
                // make next oid of last committed object point to first
                // uncommitted object
                // make previous oid of first uncommitted object point to
                // last committed object
                if (lastCommittedObjectOid != null && newClassInfo.UncommittedZoneInfo.HasObjects())
                {
                    if (newClassInfo.CommitedZoneInfo.HasObjects())
                    {
                        // these 2 updates are executed directly without
                        // transaction, because
                        // We are in the commit process.
                        writer.UpdateNextObjectFieldOfObjectInfo(lastCommittedObjectOid,
                                                                 newClassInfo.UncommittedZoneInfo.First, false);
                        writer.UpdatePreviousObjectFieldOfObjectInfo(newClassInfo.UncommittedZoneInfo.First,
                                                                     lastCommittedObjectOid, false);
                    }
                    else
                    {
                        // Committed zone has 0 object
                        writer.UpdatePreviousObjectFieldOfObjectInfo(newClassInfo.UncommittedZoneInfo.First, null,
                                                                     false);
                    }
                }

                // The number of committed objects must be updated with the number
                // of the last committed CI because a transaction may have been
                // committed changing this number.
                // Notice that the setNbObjects receive the full CommittedCIZoneInfo
                // object
                // because it will set the number of objects and the number of
                // deleted objects
                newClassInfo.CommitedZoneInfo.SetNbObjects(lastCommittedClassInfo.CommitedZoneInfo);

                // and don't forget to set the deleted objects
                // This sets the number of objects, the first object OID and the
                // last object OID
                newClassInfo = BuildClassInfoForCommit(newClassInfo);

                writer.FileSystemProcessor.UpdateInstanceFieldsOfClassInfo(newClassInfo, false);

                if (OdbConfiguration.IsLoggingEnabled())
                {
                    DLogger.Debug(string.Format("OdbTransaction: Analysing class {0}", newClassInfo.FullClassName));
                    DLogger.Debug(string.Format("\t-Commited CI   = {0}", newClassInfo));
                    DLogger.Debug(
                        string.Format("\t-connect last commited object with oid {0} to first uncommited object {1}",
                                      lastCommittedObjectOid, newClassInfo.UncommittedZoneInfo.First));
                    DLogger.Debug(string.Concat("\t-Commiting new Number of objects = ",
                                                newClassInfo.NumberOfObjects.ToString()));
                }
            }

            sessionMetaModel.ResetChangedClasses();
        }

        /// <summary>
        ///   Shift all unconnected infos to connected (committed) infos
        /// </summary>
        /// <param name="classInfo"> </param>
        /// <returns> The updated class info </returns>
        private static ClassInfo BuildClassInfoForCommit(ClassInfo classInfo)
        {
            var nbObjects = classInfo.NumberOfObjects;
            classInfo.CommitedZoneInfo.SetNbObjects(nbObjects);

            if (classInfo.CommitedZoneInfo.First == null)
            {
                // nothing to change
                classInfo.CommitedZoneInfo.First = classInfo.UncommittedZoneInfo.First;
            }

            if (classInfo.UncommittedZoneInfo.Last != null)
                classInfo.CommitedZoneInfo.Last = classInfo.UncommittedZoneInfo.Last;

            classInfo.UncommittedZoneInfo.Reset();

            return classInfo;
        }

        private void LoadWriteActions(string filename, bool apply)
        {
            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug(string.Format("OdbTransaction: Load write actions of {0}", filename));

            CheckFileAccess(filename);
            _fsi.SetReadPosition(0);
            _isCommited = _fsi.ReadByte() == 1;
            _creationDateTime = _fsi.ReadLong();
            var totalNumberOfWriteActions = _fsi.ReadLong();

            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Info(string.Concat("OdbTransaction: ", _writeActions.Count.ToString(), " write actions in file"));

            for (var i = 0; i < totalNumberOfWriteActions; i++)
            {
                var defaultWriteAction = WriteAction.Read(_fsi);

                if (apply)
                {
                    defaultWriteAction.ApplyTo(_fsiToApplyWriteActions);
                    defaultWriteAction.Clear();
                }
                else
                    AddWriteAction(defaultWriteAction, false);
            }

            if (apply)
                _fsiToApplyWriteActions.Flush();
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append("state=").Append(_isCommited).Append(" | creation=").Append(_creationDateTime).Append(
                " | write actions numbers=").Append(_numberOfWriteActions);

            return buffer.ToString();
        }

        private void ApplyTo()
        {
            if (!_isCommited)
            {
                DLogger.Info("OdbTransaction: can not execute a transaction that is not confirmed");
                return;
            }

            if (_hasAllWriteActionsInMemory)
            {
                foreach (var wa in _writeActions)
                {
                    wa.ApplyTo(_fsiToApplyWriteActions);
                    wa.Clear();
                }

                _fsiToApplyWriteActions.Flush();
            }
            else
            {
                LoadWriteActions(GetName(), true);
                _fsiToApplyWriteActions.Flush();
            }
        }
    }
}