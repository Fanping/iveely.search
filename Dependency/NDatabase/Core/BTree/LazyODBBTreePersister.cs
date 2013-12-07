using System;
using System.Collections;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Btree;
using NDatabase.Exceptions;
using NDatabase.Meta;
using NDatabase.Services;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.BTree
{
    /// <summary>
    ///   Class that persists the BTree and its node into the NDatabase ODB Database.
    /// </summary>
    internal sealed class LazyOdbBtreePersister : IBTreePersister, ICommitListener
    {
        /// <summary>
        ///   The odb interface
        /// </summary>
        private readonly IStorageEngine _engine;

        /// <summary>
        ///   The list is used to keep the order.
        /// </summary>
        /// <remarks>
        ///   The list is used to keep the order. Deleted object will be replaced by null value, to keep the positions
        /// </remarks>
        private readonly IOdbList<OID> _modifiedObjectOidList;

        /// <summary>
        ///   All modified nodes : the map is used to avoid duplication The key is the oid, the value is the position is the list
        /// </summary>
        private readonly OdbHashMap<object, int> _modifiedObjectOids;

        /// <summary>
        ///   All loaded nodes
        /// </summary>
        private readonly IDictionary<OID, object> _oids;

        private int _nbPersist;

        /// <summary>
        ///   The tree we are persisting
        /// </summary>
        private IBTree _tree;

        public LazyOdbBtreePersister(IStorageEngine engine)
        {
            _oids = new OdbHashMap<OID, object>();
            _modifiedObjectOids = new OdbHashMap<object, int>();
            _modifiedObjectOidList = new OdbList<OID>(500);
            _engine = engine;
            _engine.AddCommitListener(this);
        }

        #region IBTreePersister Members

        /// <summary>
        ///   Loads a node from its id.
        /// </summary>
        /// <remarks>
        ///   Loads a node from its id. Tries to get if from memory, if not present then loads it from odb storage
        /// </remarks>
        /// <param name="id"> The id of the nod </param>
        /// <returns> The node with the specific id </returns>
        public IBTreeNode LoadNodeById(object id)
        {
            var oid = (OID) id;
            // Check if node is in memory
            var node = (IBTreeNode) _oids[oid];

            if (node != null)
            {
                return node;
            }

            // else load from odb
            try
            {
                if (OdbConfiguration.IsLoggingEnabled())
                    DLogger.Debug(string.Format("LazyOdbBtreePersister: Loading node with id {0}", oid));

                if (oid == null)
                    throw new OdbRuntimeException(BTreeError.InvalidIdForBtree.AddParameter("oid"));

                var pn = (IBTreeNode) _engine.GetObjectFromOid(oid);
                pn.SetId(oid);

                if (_tree != null)
                    pn.SetBTree(_tree);

                // Keep the node in memory
                _oids.Add(oid, pn);
                return pn;
            }
            catch (Exception e)
            {
                throw new OdbRuntimeException(BTreeError.InternalError, e);
            }
        }

        /// <summary>
        ///   saves the bree node Only puts the current node in an 'modified Node' map to be saved on commit
        /// </summary>
        public void SaveNode(IBTreeNode node)
        {
            OID oid;
            // Here we only save the node if it does not have id,
            // else we just save into the hashmap
            if (node.GetId() == StorageEngineConstant.NullObjectId)
            {
                try
                {
                    // first get the oid. : -2:it could be any value
                    oid = _engine.GetObjectWriter().GetIdManager().GetNextObjectId(-2);
                    node.SetId(oid);

                    oid = _engine.Store(oid, node);

                    if (OdbConfiguration.IsLoggingEnabled())
                        DLogger.Debug(string.Format("LazyOdbBtreePersister: Saved node id {0}", oid));

                    // + " : " +
                    // node.toString());
                    if (_tree != null && node.GetBTree() == null)
                        node.SetBTree(_tree);

                    _oids.Add(oid, node);
                    return;
                }
                catch (Exception e)
                {
                    throw new OdbRuntimeException(BTreeError.InternalError.AddParameter("While saving node"), e);
                }
            }

            oid = (OID) node.GetId();

            _oids.Add(oid, node);
            AddModifiedOid(oid);
        }

        public void Close()
        {
            Persist();
            _engine.Close();
        }

        public IBTree LoadBTree(object id)
        {
            var oid = (OID) id;

            try
            {
                if (OdbConfiguration.IsLoggingEnabled())
                    DLogger.Debug(string.Format("LazyOdbBtreePersister: Loading btree with id {0}", oid));

                if (oid == StorageEngineConstant.NullObjectId)
                    throw new OdbRuntimeException(
                        BTreeError.InvalidIdForBtree.AddParameter(StorageEngineConstant.NullObjectId));

                _tree = (IBTree) _engine.GetObjectFromOid(oid);
                _tree.SetId(oid);
                _tree.SetPersister(this);
                var root = _tree.GetRoot();

                root.SetBTree(_tree);

                return _tree;
            }
            catch (Exception e)
            {
                throw new OdbRuntimeException(BTreeError.InternalError, e);
            }
        }

        public void SaveBTree(IBTree treeToSave)
        {
            try
            {
                var oid = (OID) treeToSave.GetId();

                if (oid == null)
                {
                    // first get the oid. -2 : it could be any value
                    oid = _engine.GetObjectWriter().GetIdManager().GetNextObjectId(-2);
                    treeToSave.SetId(oid);
                    oid = _engine.Store(oid, treeToSave);

                    if (OdbConfiguration.IsLoggingEnabled())
                        DLogger.Debug(string.Format("LazyOdbBtreePersister: Saved btree {0} with id {1} and  root {2}", treeToSave.GetId(), oid,
                                                    treeToSave.GetRoot()));

                    if (_tree == null)
                        _tree = treeToSave;

                    _oids.Add(oid, treeToSave);
                }
                else
                {
                    _oids.Add(oid, treeToSave);
                    AddModifiedOid(oid);
                }
            }
            catch (Exception e)
            {
                throw new OdbRuntimeException(BTreeError.InternalError, e);
            }
        }

        public void DeleteNode(IBTreeNode o)
        {
            var oid = _engine.Delete(o);
            _oids.Remove(oid);

            var position = _modifiedObjectOids.Remove2(oid);

            // Just replace the element by null, to not modify all the other positions
            _modifiedObjectOidList[position] = null;
        }

        public void SetBTree(IBTree tree)
        {
            _tree = tree;
        }

        public void Flush()
        {
            Persist();
            ClearModified();
        }

        #endregion

        #region ICommitListener Members

        public void AfterCommit()
        {
        }

        // nothing to do
        public void BeforeCommit()
        {
            Persist();
            Clear();
        }

        #endregion

        private void Clear()
        {
            _oids.Clear();
            _modifiedObjectOids.Clear();
            _modifiedObjectOidList.Clear();
        }

        private void Persist()
        {
            _nbPersist++;

            if (OdbConfiguration.IsLoggingEnabled())
            {
                var count = _modifiedObjectOids.Count.ToString();
                DLogger.Debug(string.Concat("LazyOdbBtreePersister: ", "persist ", _nbPersist.ToString(), "  : Saving " + count + " objects - ",
                                            GetHashCode().ToString()));
            }

            var nbCommited = 0;
            var i = 0;
            var size = _modifiedObjectOids.Count;
            IEnumerator iterator = _modifiedObjectOidList.GetEnumerator();

            while (iterator.MoveNext())
            {
                var oid = (OID) iterator.Current;

                if (oid == null)
                    continue;

                nbCommited++;
                long t0;
                long t1;

                try
                {
                    t0 = OdbTime.GetCurrentTimeInMs();
                    var @object = _oids[oid];
                    _engine.Store(@object);
                    t1 = OdbTime.GetCurrentTimeInMs();
                }
                catch (Exception e)
                {
                    throw new OdbRuntimeException(
                        BTreeError.InternalError.AddParameter("Error while storing object with oid " + oid), e);
                }

                if (OdbConfiguration.IsLoggingEnabled())
                    DLogger.Debug(string.Concat("LazyOdbBtreePersister: ", "Committing oid " + oid, " | ", i.ToString(), "/", size.ToString(),
                                                " | ", (t1 - t0).ToString(), " ms"));

                i++;
            }

            if (OdbConfiguration.IsLoggingEnabled())
                DLogger.Debug(string.Concat("LazyOdbBtreePersister: ", nbCommited.ToString(), " commits / ", size.ToString()));
        }

        private void ClearModified()
        {
            _modifiedObjectOids.Clear();
            _modifiedObjectOidList.Clear();
        }

        private void AddModifiedOid(OID oid)
        {
            if (_modifiedObjectOids.ContainsKey(oid))
            {
                // Object is already in the list
                return;
            }

            _modifiedObjectOidList.Add(oid);
            // Keep the position of the oid in the list as the value of the map.
            // Used for the delete.
            _modifiedObjectOids.Add(oid, _modifiedObjectOidList.Count - 1);
        }
    }
}