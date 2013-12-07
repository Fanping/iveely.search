using NDatabase.Api;
using NDatabase.Exceptions;

namespace NDatabase.Btree
{
    /// <summary>
    ///   The NDatabase ODB BTree Node implementation.
    /// </summary>
    /// <remarks>
    ///   The NDatabase ODB BTree Node implementation. It extends the DefaultBTreeNode generic implementation to be able to be stored in the ODB database.
    /// </remarks>
    internal sealed class OdbBtreeNodeMultiple : BTreeNodeMultipleValuesPerKey
    {
        private OID[] _childrenOids;
        private OID _oid;

        /// <summary>
        ///   lazy loaded
        /// </summary>
        [NonPersistent]
        private IBTreeNode _parent;

        private OID _parentOid;

        public OdbBtreeNodeMultiple(IBTree btree) : base(btree)
        {
        }

        public override IBTreeNode GetChildAt(int index, bool throwExceptionIfNotExist)
        {
            var oid = _childrenOids[index];
            
            if (oid == null)
            {
                if (throwExceptionIfNotExist)
                {
                    var indexAsString = index.ToString();
                    throw new BTreeException("Trying to load null child node at index " + indexAsString);
                }
                return null;
            }

            return Btree.GetPersister().LoadNodeById(oid);
        }

        public override IBTreeNode GetParent()
        {
            if (_parent != null)
                return _parent;

            _parent = Btree.GetPersister().LoadNodeById(_parentOid);
            return _parent;
        }

        public override void SetChildAt(IBTreeNode child, int index)
        {
            if (child != null)
            {
                if (child.GetId() == null)
                    Btree.GetPersister().SaveNode(child);

                _childrenOids[index] = (OID) child.GetId();
                child.SetParent(this);
            }
            else
            {
                _childrenOids[index] = null;
            }
        }

        public override void SetParent(IBTreeNode node)
        {
            _parent = node;

            if (_parent != null)
            {
                if (_parent.GetId() == null)
                    Btree.GetPersister().SaveNode(_parent);

                _parentOid = (OID) _parent.GetId();
            }
            else
            {
                _parentOid = null;
            }
        }

        public override bool HasParent()
        {
            return _parentOid != null;
        }

        protected override void Init()
        {
            _childrenOids = new OID[MaxNbChildren];
            _parentOid = null;
            _parent = null;
        }

        public override object GetId()
        {
            return _oid;
        }

        public override void SetId(object id)
        {
            _oid = (OID) id;
        }

        public override void Clear()
        {
            base.Clear();
            _parent = null;
            _parentOid = null;
            _childrenOids = null;
            _oid = null;
        }

        public override void DeleteChildAt(int index)
        {
            _childrenOids[index] = null;
            NbChildren--;
        }

        protected override void MoveChildFromTo(int sourceIndex, int destinationIndex, bool throwExceptionIfDoesNotExist)
        {
            if (throwExceptionIfDoesNotExist && _childrenOids[sourceIndex] == null)
            {
                var index = sourceIndex.ToString();
                throw new BTreeException(string.Format("Trying to load null child node at index {0}", index));
            }

            _childrenOids[destinationIndex] = _childrenOids[sourceIndex];
        }

        public override void SetChildAt(IBTreeNode node, int childIndex, int indexDestination,
                                        bool throwExceptionIfDoesNotExist)
        {
            var childOid = (OID) node.GetChildIdAt(childIndex, throwExceptionIfDoesNotExist);
            _childrenOids[indexDestination] = childOid;

            if (childOid == null) 
                return;

            // The parent of the child has changed
            var child = Btree.GetPersister().LoadNodeById(childOid);
            child.SetParent(this);
            Btree.GetPersister().SaveNode(child);
        }

        protected override void SetNullChildAt(int childIndex)
        {
            _childrenOids[childIndex] = null;
        }

        public override object GetChildIdAt(int childIndex, bool throwExceptionIfDoesNotExist)
        {
            if (throwExceptionIfDoesNotExist && _childrenOids[childIndex] == null)
            {
                var index = childIndex.ToString();
                throw new BTreeException("Trying to load null child node at index " + index);
            }

            return _childrenOids[childIndex];
        }

        public override object GetParentId()
        {
            return _parentOid;
        }

        public override object GetValueAsObjectAt(int index)
        {
            return GetValueAt(index);
        }
    }
}
