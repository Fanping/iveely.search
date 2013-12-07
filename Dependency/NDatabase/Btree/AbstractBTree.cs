using System;
using System.Collections;
using NDatabase.Api;
using NDatabase.Exceptions;

namespace NDatabase.Btree
{
    internal abstract class AbstractBTree : IBTree
    {
        private readonly int _degree;

        private int _height;

        [NonPersistent]
        private IBTreePersister _persister;

        private IBTreeNode _root;
        private long _size;

        protected AbstractBTree(int degree, IBTreePersister persister)
        {
            _degree = degree;
            _size = 0;
            _height = 1;
            _persister = persister;
            _root = BuildNode();

            // TODO check if it is needed to store the root before the btree ->
            // saving btree will try to update root!
            persister.SaveNode(_root);
            persister.SaveBTree(this);
            persister.Flush();
        }

        #region IBTree Members

        public abstract IBTreeNode BuildNode();

        /// <summary>
        ///   TODO Manage collision
        /// </summary>
        public virtual object Delete(IComparable key, object value)
        {
            object obj;
            try
            {
                obj = InternalDelete(_root, new KeyAndValue(key, value));
            }
            catch (Exception e)
            {
                var errorMessage = string.Format("Error while deleting key='{0}' value='{1}'", key, value);
                throw new BTreeNodeValidationException(errorMessage, e);
            }

            if (obj != null)
                _size--;

            GetPersister().SaveBTree(this);
            // TODO flush or not?
            // persister.flush();
            return obj;
        }

        public virtual int GetDegree()
        {
            return _degree;
        }

        public virtual void Insert(IComparable key, object value)
        {
            if (_root.IsFull())
            {
                var newRoot = BuildNode();
                var oldRoot = _root;

                newRoot.SetChildAt(_root, 0);
                newRoot.SetNbChildren(1);

                _root = newRoot;

                Split(newRoot, oldRoot, 0);

                _height++;
                _persister.SaveNode(oldRoot);

                // TODO Remove the save of the new root : the save on the btree
                // should do the save on the new root(after introspector
                // refactoring)
                _persister.SaveNode(newRoot);
                _persister.SaveBTree(this);

                BTreeValidator.ValidateNode(newRoot, true);
            }
            InsertNonFull(_root, key, value);
            _size++;
            _persister.SaveBTree(this);
        }

        // Commented by Olivier 05/11/2007
        // persister.flush();

        /// <summary>
        ///   <pre>1 take median element
        ///     2 insert the median in the parent  (shifting necessary elements)
        ///     3 create a new node with right part elements (moving keys and values and children)
        ///     4 set this new node as a child of parent</pre>
        /// </summary>
        public virtual void Split(IBTreeNode parent, IBTreeNode node2Split, int childIndex)
        {
            // BTreeValidator.validateNode(parent, parent == root);
            // BTreeValidator.validateNode(node2Split, false);
            // 1
            var medianValue = node2Split.GetMedian();
            // 2
            parent.SetKeyAndValueAt(medianValue, childIndex, true, true);
            // 3
            var rightPartTreeNode = node2Split.ExtractRightPart();
            // 4
            parent.SetChildAt(rightPartTreeNode, childIndex + 1);
            parent.SetChildAt(node2Split, childIndex);
            parent.IncrementNbChildren();
            _persister.SaveNode(parent);
            _persister.SaveNode(rightPartTreeNode);
            _persister.SaveNode(node2Split);

            if (!OdbConfiguration.IsBTreeValidationEnabled())
                return;

            BTreeValidator.ValidateNode(parent, parent == _root);
            BTreeValidator.ValidateNode(rightPartTreeNode, false);
            BTreeValidator.ValidateNode(node2Split, false);
        }

        public virtual long GetSize()
        {
            return _size;
        }

        public virtual IBTreeNode GetRoot()
        {
            return _root;
        }

        public virtual int GetHeight()
        {
            return _height;
        }

        public virtual IBTreePersister GetPersister()
        {
            return _persister;
        }

        public virtual void SetPersister(IBTreePersister persister)
        {
            _persister = persister;
            _persister.SetBTree(this);

            if (_root.GetBTree() == null)
                _root.SetBTree(this);
        }

        public virtual void Clear()
        {
            _root.Clear();
        }

        public virtual IKeyAndValue GetBiggest(IBTreeNode node, bool delete)
        {
            var lastKeyIndex = node.GetNbKeys() - 1;
            var lastChildIndex = node.GetNbChildren() - 1;

            if (lastChildIndex > lastKeyIndex)
            {
                var child = node.GetChildAt(lastChildIndex, true);

                if (child.GetNbKeys() == _degree - 1)
                    node = PrepareForDelete(node, child, lastChildIndex);

                lastChildIndex = node.GetNbChildren() - 1;
                child = node.GetChildAt(lastChildIndex, true);
                return GetBiggest(child, delete);
            }

            var kav = node.GetKeyAndValueAt(lastKeyIndex);

            if (delete)
            {
                node.DeleteKeyAndValueAt(lastKeyIndex, false);
                _persister.SaveNode(node);
            }

            return kav;
        }

        public virtual IKeyAndValue GetSmallest(IBTreeNode node, bool delete)
        {
            if (!node.IsLeaf())
            {
                var childTreeNode = node.GetChildAt(0, true);

                if (childTreeNode.GetNbKeys() == _degree - 1)
                    node = PrepareForDelete(node, childTreeNode, 0);

                childTreeNode = node.GetChildAt(0, true);
                return GetSmallest(childTreeNode, delete);
            }

            var keyAndValue = node.GetKeyAndValueAt(0);

            if (delete)
            {
                node.DeleteKeyAndValueAt(0, true);
                _persister.SaveNode(node);
            }

            return keyAndValue;
        }

        public abstract object GetId();

        public abstract IEnumerator Iterator<T>(OrderByConstants arg1);

        public abstract void SetId(object arg1);

        #endregion

        /// <summary>
        ///   Returns the value of the deleted key
        /// </summary>
        /// <param name="node"> </param>
        /// <param name="keyAndValue"> </param>
        /// <returns> </returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        private object InternalDelete(IBTreeNode node, IKeyAndValue keyAndValue)
        {
            var positionOfKey = node.GetPositionOfKey(keyAndValue.GetKey());
            var keyIsHere = positionOfKey > 0;

            if (node.IsLeaf())
            {
                if (keyIsHere)
                {
                    var deletedValue = node.DeleteKeyForLeafNode(keyAndValue);
                    GetPersister().SaveNode(node);
                    return deletedValue;
                }
                // key does not exist
                return null;
            }
                
            int realPosition;
            
            if (!keyIsHere)
            {
                // descend
                realPosition = -positionOfKey - 1;
                var childTreeNode = node.GetChildAt(realPosition, true);

                if (childTreeNode.GetNbKeys() == _degree - 1)
                {
                    node = PrepareForDelete(node, childTreeNode, realPosition);
                    return InternalDelete(node, keyAndValue);
                }

                return InternalDelete(childTreeNode, keyAndValue);
            }

            // Here,the node is not a leaf and contains the key
            realPosition = positionOfKey - 1;
            var currentKey = node.GetKeyAt(realPosition);
            var currentValue = node.GetValueAsObjectAt(realPosition);

            // case 2a
            var leftNode = node.GetChildAt(realPosition, true);
            if (leftNode.GetNbKeys() >= _degree)
            {
                var prevKeyAndValue = GetBiggest(leftNode, true);
                node.SetKeyAndValueAt(prevKeyAndValue, realPosition);
                BTreeValidator.ValidateNode(node, node == _root);
                GetPersister().SaveNode(node);
                return currentValue;
            }

            // case 2b
            var rightNode = node.GetChildAt(realPosition + 1, true);
            if (rightNode.GetNbKeys() >= _degree)
            {
                var nextKeyAndValue = GetSmallest(rightNode, true);
                node.SetKeyAndValueAt(nextKeyAndValue, realPosition);
                BTreeValidator.ValidateNode(node, node == _root);
                GetPersister().SaveNode(node);
                return currentValue;
            }

            // case 2c
            // Here, both left and right part have degree-1 keys
            // remove the element to be deleted from node (shifting left all
            // right
            // elements, link to right link does not exist anymore)
            // insert the key to be deleted in left child and merge the 2 nodes.
            // rightNode should be deleted
            // if node is root, then leftNode becomes the new root and node
            // should be deleted
            // 
            node.DeleteKeyAndValueAt(realPosition, true);
            leftNode.InsertKeyAndValue(currentKey, currentValue);
            leftNode.MergeWith(rightNode);
            // If node is the root and is empty
            if (!node.HasParent() && node.GetNbKeys() == 0)
            {
                _persister.DeleteNode(node);
                _root = leftNode;
                leftNode.SetParent(null);
                // The height has been decreased. No need to save btree here.
                // The calling delete method will save it.
                _height--;
            }
            else
            {
                node.SetChildAt(leftNode, realPosition);
                // Node must only be validated if it is not the root
                BTreeValidator.ValidateNode(node, node == _root);
            }

            _persister.DeleteNode(rightNode);
            BTreeValidator.ValidateNode(leftNode, leftNode == _root);
            GetPersister().SaveNode(node);
            GetPersister().SaveNode(leftNode);
            return InternalDelete(leftNode, keyAndValue);
        }

        private IBTreeNode PrepareForDelete(IBTreeNode parent, IBTreeNode child, int childIndex)
        {
            BTreeValidator.ValidateNode(parent);
            BTreeValidator.ValidateNode(child);

            // case 3a
            IBTreeNode leftSibling = null;
            IBTreeNode rightSibling = null;

            if (childIndex > 0 && parent.GetNbChildren() > 0)
                leftSibling = parent.GetChildAt(childIndex - 1, false);

            if (childIndex < parent.GetNbChildren() - 1)
                rightSibling = parent.GetChildAt(childIndex + 1, false);

            // case 3a left
            if (leftSibling != null && leftSibling.GetNbKeys() >= _degree)
            {
                var elementToMoveDown = parent.GetKeyAndValueAt(childIndex - 1);
                var elementToMoveUp = leftSibling.GetLastKeyAndValue();

                parent.SetKeyAndValueAt(elementToMoveUp, childIndex - 1);
                child.InsertKeyAndValue(elementToMoveDown.GetKey(), elementToMoveDown.GetValue());

                if (leftSibling.GetNbChildren() > leftSibling.GetNbKeys())
                {
                    // Take the last child of the left sibling and set it the
                    // first child of the 'child' (incoming parameter)
                    // child.setChildAt(leftSibling.getChildAt(leftSibling.getNbChildren()
                    // - 1, true), 0);
                    child.SetChildAt(leftSibling, leftSibling.GetNbChildren() - 1, 0, true);
                    child.IncrementNbChildren();
                }

                leftSibling.DeleteKeyAndValueAt(leftSibling.GetNbKeys() - 1, false);

                if (!leftSibling.IsLeaf())
                    leftSibling.DeleteChildAt(leftSibling.GetNbChildren() - 1);

                _persister.SaveNode(parent);
                _persister.SaveNode(child);
                _persister.SaveNode(leftSibling);

                if (OdbConfiguration.IsBTreeValidationEnabled())
                {
                    BTreeValidator.ValidateNode(parent, parent == _root);
                    BTreeValidator.ValidateNode(child, false);
                    BTreeValidator.ValidateNode(leftSibling, false);
                    BTreeValidator.CheckDuplicateChildren(leftSibling, child);
                }

                return parent;
            }

            // case 3a right
            if (rightSibling != null && rightSibling.GetNbKeys() >= _degree)
            {
                var elementToMoveDown = parent.GetKeyAndValueAt(childIndex);
                var elementToMoveUp = rightSibling.GetKeyAndValueAt(0);

                parent.SetKeyAndValueAt(elementToMoveUp, childIndex);
                child.InsertKeyAndValue(elementToMoveDown.GetKey(), elementToMoveDown.GetValue());

                if (rightSibling.GetNbChildren() > 0)
                {
                    // Take the first child of the right sibling and set it the
                    // last child of the 'child' (incoming parameter)
                    child.SetChildAt(rightSibling, 0, child.GetNbChildren(), true);
                    child.IncrementNbChildren();
                }

                rightSibling.DeleteKeyAndValueAt(0, true);
                _persister.SaveNode(parent);
                _persister.SaveNode(child);
                _persister.SaveNode(rightSibling);

                if (OdbConfiguration.IsBTreeValidationEnabled())
                {
                    BTreeValidator.ValidateNode(parent, parent == _root);
                    BTreeValidator.ValidateNode(child, false);
                    BTreeValidator.ValidateNode(rightSibling, false);
                    BTreeValidator.CheckDuplicateChildren(rightSibling, child);
                }

                return parent;
            }

            // case 3b
            var isCase3B = (leftSibling != null && leftSibling.GetNbKeys() == _degree - 1) ||
                           (rightSibling != null && rightSibling.GetNbKeys() >= _degree - 1);

            var parentWasSetToNull = false;

            if (isCase3B)
            {
                // choose left sibling to execute merge
                if (leftSibling != null)
                {
                    var elementToMoveDown = parent.GetKeyAndValueAt(childIndex - 1);
                    leftSibling.InsertKeyAndValue(elementToMoveDown.GetKey(), elementToMoveDown.GetValue());
                    leftSibling.MergeWith(child);
                    parent.DeleteKeyAndValueAt(childIndex - 1, true);

                    if (parent.GetNbKeys() == 0)
                    {
                        // this is the root
                        if (!parent.HasParent())
                        {
                            _root = leftSibling;
                            _root.SetParent(null);
                            _height--;
                            parentWasSetToNull = true;
                        }
                        else
                        {
                            const string errorMessage = "Unexpected empty node that is node the root!";
                            throw new BTreeNodeValidationException(errorMessage);
                        }
                    }
                    else
                    {
                        parent.SetChildAt(leftSibling, childIndex - 1);
                    }

                    if (parentWasSetToNull)
                    {
                        _persister.DeleteNode(parent);
                    }
                    else
                    {
                        _persister.SaveNode(parent);
                        BTreeValidator.ValidateNode(parent, parent == _root);
                    }

                    // child was merged with another node it must be deleted
                    _persister.DeleteNode(child);
                    _persister.SaveNode(leftSibling);

                    // Validator.validateNode(child, child == root);
                    BTreeValidator.ValidateNode(leftSibling, leftSibling == _root);

                    // Validator.checkDuplicateChildren(leftSibling, child);
                    return parentWasSetToNull
                               ? _root
                               : parent;
                }

                // choose right sibling to execute merge
                {
                    var elementToMoveDown = parent.GetKeyAndValueAt(childIndex);
                    child.InsertKeyAndValue(elementToMoveDown.GetKey(), elementToMoveDown.GetValue());
                    child.MergeWith(rightSibling);
                    parent.DeleteKeyAndValueAt(childIndex, true);
                    if (parent.GetNbKeys() == 0)
                    {
                        // this is the root
                        if (!parent.HasParent())
                        {
                            _root = child;
                            _root.SetParent(null);
                            _height--;
                            parentWasSetToNull = true;
                        }
                        else
                        {
                            throw new BTreeNodeValidationException("Unexpected empty root node!");
                        }
                    }
                    else
                    {
                        parent.SetChildAt(child, childIndex);
                    }

                    if (parentWasSetToNull)
                    {
                        _persister.DeleteNode(parent);
                    }
                    else
                    {
                        _persister.SaveNode(parent);
                        BTreeValidator.ValidateNode(parent, parent == _root);
                    }

                    _persister.DeleteNode(rightSibling);
                    _persister.SaveNode(child);
                    BTreeValidator.ValidateNode(child, child == _root);
                    // Validator.validateNode(rightSibling, rightSibling ==
                    // root);
                    // Validator.checkDuplicateChildren(rightSibling, child);
                    return parentWasSetToNull
                               ? _root
                               : parent;
                }
            }

            throw new BTreeNodeValidationException("Unexpected case in executing prepare for delete");
        }

        private void InsertNonFull(IBTreeNode node, IComparable key, object value)
        {
            if (node.IsLeaf())
            {
                node.InsertKeyAndValue(key, value);
                _persister.SaveNode(node);
                return;
            }

            var position = node.GetPositionOfKey(key);

            // return an index starting
            // from 1 instead of 0
            var realPosition = -position - 1;

            // If position is positive, the key must be inserted in this node
            if (position >= 0)
            {
                node.InsertKeyAndValue(key, value);
                _persister.SaveNode(node);
                return;
            }

            // descend
            var nodeToDescend = node.GetChildAt(realPosition, true);
            if (nodeToDescend.IsFull())
            {
                Split(node, nodeToDescend, realPosition);
                if (node.GetKeyAt(realPosition).CompareTo(key) < 0)
                    nodeToDescend = node.GetChildAt(realPosition + 1, true);
            }

            InsertNonFull(nodeToDescend, key, value);
        }
    }
}
