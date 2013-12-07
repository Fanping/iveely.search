using System;
using System.Collections;
using System.Text;
using NDatabase.Api;
using NDatabase.Exceptions;

namespace NDatabase.Btree
{
    internal abstract class AbstractBTreeNode : IBTreeNode
    {
        /// <summary>
        ///   The BTree owner of this node
        /// </summary>
        [NonPersistent]
        protected IBTree Btree;

        protected int NbChildren;
        protected int NbKeys;
        protected object[] Values;
        protected IComparable[] Keys;
        protected int MaxNbChildren;

        private int _degree;
        private int _maxNbKeys;

        protected AbstractBTreeNode(IBTree btree)
        {
            BasicInit(btree);
        }

        #region IBTreeNode Members

        public abstract void InsertKeyAndValue(IComparable key, object value);

        public abstract IBTreeNode GetChildAt(int index, bool throwExceptionIfNotExist);

        public abstract IBTreeNode GetParent();

        public abstract object GetParentId();

        public abstract void SetParent(IBTreeNode node);

        public abstract bool HasParent();

        protected abstract void MoveChildFromTo(int sourceIndex, int destinationIndex, bool throwExceptionIfDoesNotExist);

        /// <summary>
        ///   Creates a new node with the right part of this node.
        /// </summary>
        /// <remarks>
        ///   Creates a new node with the right part of this node. This should only be called on a full node
        /// </remarks>
        public virtual IBTreeNode ExtractRightPart()
        {
            if (!IsFull())
                throw new BTreeException("extract right part called on non full node");

            // Creates an empty new node
            var rightPartNode = Btree.BuildNode();
            var j = 0;

            for (var i = _degree; i < _maxNbKeys; i++)
            {
                rightPartNode.SetKeyAndValueAt(Keys[i], Values[i], j, false, false);
                Keys[i] = null;
                Values[i] = null;
                rightPartNode.SetChildAt(this, i, j, false);

                // TODO must we load all nodes to set new parent
                var bTreeNode = rightPartNode.GetChildAt(j, false);
                if (bTreeNode != null)
                    bTreeNode.SetParent(rightPartNode);

                SetNullChildAt(i);
                j++;
            }

            rightPartNode.SetChildAt(this, MaxNbChildren - 1, j, false);
            // correct father id
            var c1TreeNode = rightPartNode.GetChildAt(j, false);
            if (c1TreeNode != null)
                c1TreeNode.SetParent(rightPartNode);

            SetNullChildAt(MaxNbChildren - 1);
            // resets last child
            Keys[_degree - 1] = null;
            // resets median element
            Values[_degree - 1] = null;
            // set numbers
            NbKeys = _degree - 1;
            var originalNbChildren = NbChildren;
            NbChildren = Math.Min(NbChildren, _degree);

            rightPartNode.SetNbKeys(_degree - 1);
            rightPartNode.SetNbChildren(originalNbChildren - NbChildren);

            BTreeValidator.ValidateNode(this);
            BTreeValidator.ValidateNode(rightPartNode);
            BTreeValidator.CheckDuplicateChildren(this, rightPartNode);

            return rightPartNode;
        }

        public virtual IKeyAndValue GetKeyAndValueAt(int index)
        {
            if (Keys[index] == null && Values[index] == null)
                return null;

            return new KeyAndValue(Keys[index], Values[index]);
        }

        public virtual IKeyAndValue GetLastKeyAndValue()
        {
            return GetKeyAndValueAt(NbKeys - 1);
        }

        public virtual IComparable GetKeyAt(int index)
        {
            return Keys[index];
        }

        public virtual IKeyAndValue GetMedian()
        {
            var medianPosition = _degree - 1;
            return GetKeyAndValueAt(medianPosition);
        }

        /// <summary>
        ///   Returns the position of the key.
        /// </summary>
        /// <remarks>
        ///   Returns the position of the key. If the key does not exist in node, returns the position where this key should be,multiplied by -1 <pre>for example for node of degree 3 : [1 89 452 789 - ],
        ///                                                                                                                                        calling getPositionOfKey(89) returns 2 (starts with 1)
        ///                                                                                                                                        calling getPositionOfKey(99) returns -2 (starts with 1),because the position should be done, but it does not exist so multiply by -1
        ///                                                                                                                                        this is used to know the child we should descend to!in this case the getChild(2).</pre>
        /// </remarks>
        /// <param name="key"> </param>
        /// <returns> The position of the key,as a negative number if key does not exist, warning, the position starts with 1and not 0! </returns>
        public virtual int GetPositionOfKey(IComparable key)
        {
            var i = 0;

            while (i < NbKeys)
            {
                var result = Keys[i].CompareTo(key);
                
                if (result == 0)
                    return i + 1;

                if (result > 0)
                    return -(i + 1);

                i++;
            }

            return -(i + 1);
        }

        public virtual void IncrementNbChildren()
        {
            NbChildren++;
        }

        public virtual void SetKeyAndValueAt(IComparable key, object value, int index)
        {
            Keys[index] = key;
            Values[index] = value;
        }

        public virtual void SetKeyAndValueAt(IKeyAndValue keyAndValue, int index)
        {
            SetKeyAndValueAt(keyAndValue.GetKey(), keyAndValue.GetValue(), index);
        }

        public virtual void SetKeyAndValueAt(IComparable key, object value, int index, bool shiftIfAlreadyExist, bool incrementNbKeys)
        {
            if (shiftIfAlreadyExist && index < NbKeys)
                RightShiftFrom(index, true);

            Keys[index] = key;
            Values[index] = value;

            if (incrementNbKeys)
                NbKeys++;
        }

        public virtual void SetKeyAndValueAt(IKeyAndValue keyAndValue, int index, bool shiftIfAlreadyExist, bool incrementNbKeys)
        {
            SetKeyAndValueAt(keyAndValue.GetKey(), keyAndValue.GetValue(), index, shiftIfAlreadyExist, incrementNbKeys);
        }

        public virtual bool IsFull()
        {
            return NbKeys == _maxNbKeys;
        }

        public virtual bool IsLeaf()
        {
            return NbChildren == 0;
        }

        /// <summary>
        ///   Can only merge node without intersection =&gt; the greater key of this must be smaller than the smallest key of the node
        /// </summary>
        public virtual void MergeWith(IBTreeNode node)
        {
            BTreeValidator.ValidateNode(this);
            BTreeValidator.ValidateNode(node);

            CheckIfCanMergeWith(node);

            var j = NbKeys;
            for (var i = 0; i < node.GetNbKeys(); i++)
            {
                SetKeyAndValueAt(node.GetKeyAt(i), node.GetValueAsObjectAt(i), j, false, false);
                SetChildAt(node, i, j, false);
                j++;
            }

            // in this, we have to take the last child
            if (node.GetNbChildren() > node.GetNbKeys())
                SetChildAt(node, node.GetNbChildren() - 1, j, true);

            NbKeys += node.GetNbKeys();
            NbChildren += node.GetNbChildren();
            BTreeValidator.ValidateNode(this);
        }

        public virtual int GetNbKeys()
        {
            return NbKeys;
        }

        public virtual void SetNbChildren(int nbChildren)
        {
            NbChildren = nbChildren;
        }

        public virtual void SetNbKeys(int nbKeys)
        {
            NbKeys = nbKeys;
        }

        public virtual int GetDegree()
        {
            return _degree;
        }

        public virtual int GetNbChildren()
        {
            return NbChildren;
        }

        public virtual object DeleteKeyForLeafNode(IKeyAndValue keyAndValue)
        {
            var position = GetPositionOfKey(keyAndValue.GetKey());
            if (position < 0)
                return null;

            var realPosition = position - 1;
            var value = Values[realPosition];
            LeftShiftFrom(realPosition, false);
            NbKeys--;
            BTreeValidator.ValidateNode(this);

            return value;
        }

        public void DeleteKeyAndValueAt(int keyIndex, bool shiftChildren)
        {
            LeftShiftFrom(keyIndex, shiftChildren);
            NbKeys--;

            // only decrease child number if child are involved in shifting
            if (shiftChildren && NbChildren > keyIndex)
                NbChildren--;
        }

        public virtual void SetBTree(IBTree btree)
        {
            Btree = btree;
        }

        public virtual IBTree GetBTree()
        {
            return Btree;
        }

        public virtual void Clear()
        {
            BasicInit(Btree);
        }

        public abstract void DeleteChildAt(int arg1);

        public abstract object GetChildIdAt(int arg1, bool arg2);

        public abstract object GetId();

        public abstract object GetValueAsObjectAt(int arg1);

        public abstract void SetChildAt(IBTreeNode arg1, int arg2, int arg3, bool arg4);

        public abstract void SetChildAt(IBTreeNode arg1, int arg2);

        public abstract void SetId(object arg1);

        protected abstract void SetNullChildAt(int arg1);

        #endregion

        private void BasicInit(IBTree btree)
        {
            Btree = btree;
            _degree = btree.GetDegree();
            _maxNbKeys = 2 * _degree - 1;
            MaxNbChildren = 2 * _degree;
            Keys = new IComparable[_maxNbKeys];
            Values = new object[_maxNbKeys];
            NbKeys = 0;
            NbChildren = 0;
            Init();
        }

        protected abstract void Init();

        protected void RightShiftFrom(int position, bool shiftChildren)
        {
            if (IsFull())
                throw new BTreeException("Node is full, can't right shift!");

            // Shift keys
            for (var i = NbKeys; i > position; i--)
            {
                Keys[i] = Keys[i - 1];
                Values[i] = Values[i - 1];
            }

            Keys[position] = null;
            Values[position] = null;
            // Shift children
            if (!shiftChildren)
                return;

            for (var i = NbChildren; i > position; i--)
                MoveChildFromTo(i - 1, i, true);

            // setChildAt(getChildAt(i-1,true),i);
            SetNullChildAt(position);
        }

        protected void LeftShiftFrom(int position, bool shiftChildren)
        {
            for (var i = position; i < NbKeys - 1; i++)
            {
                Keys[i] = Keys[i + 1];
                Values[i] = Values[i + 1];

                if (shiftChildren)
                    MoveChildFromTo(i + 1, i, false);
            }

            // setChildAt(getChildAt(i+1,false), i);
            Keys[NbKeys - 1] = null;
            Values[NbKeys - 1] = null;

            if (!shiftChildren)
                return;

            MoveChildFromTo(NbKeys, NbKeys - 1, false);

            // setChildAt(getChildAt(nbKeys,false), nbKeys-1);
            SetNullChildAt(NbKeys);
        }

        private void CheckIfCanMergeWith(IBTreeNode node)
        {
            if (NbKeys + node.GetNbKeys() > _maxNbKeys)
            {
                var errorMessage = string.Concat("Trying to merge two nodes with too many keys ", NbKeys.ToString(), " + ",
                                                 node.GetNbKeys().ToString(), " > ", _maxNbKeys.ToString());

                throw new BTreeException(errorMessage);
            }

            if (NbKeys > 0)
            {
                var greatestOfThis = Keys[NbKeys - 1];
                var smallestOfOther = node.GetKeyAt(0);

                if (greatestOfThis.CompareTo(smallestOfOther) >= 0)
                {
                    var errorMessage = string.Format("Trying to merge two nodes that have intersections :  {0} / {1}", ToString(), node);
                    throw new BTreeNodeValidationException(errorMessage);
                }
            }

            if (NbKeys < NbChildren)
                throw new BTreeNodeValidationException("Trying to merge two nodes where the first one has more children than keys");
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("id=").Append(GetId()).Append(" {keys(").Append(NbKeys).Append(")=(");

            for (var i = 0; i < NbKeys; i++)
            {
                if (i > 0)
                    buffer.Append(",");

                var value = BuildValueRepresentation(Values[i]);

                buffer.Append("[").Append(Keys[i]).Append("/").Append(value).Append("]");
            }

            buffer.Append("), child(").Append(NbChildren).Append(")}");

            return buffer.ToString();
        }

        private static string BuildValueRepresentation(object o)
        {
            var result = new StringBuilder();

            if (o is IList)
            {
                foreach (var item in (IList)o)
                    result.AppendFormat("{0},", item);
                result = result.Remove(result.Length - 1, 1);
            }
            else
            {
                result.Append(o);
            }

            return result.ToString();
        }
    }
}
