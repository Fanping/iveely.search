using System;

namespace NDatabase.Btree
{
    /// <summary>
    ///   The interface for btree node.
    /// </summary>
    internal interface IBTreeNode
    {
        int GetNbKeys();

        bool IsFull();

        bool IsLeaf();

        IKeyAndValue GetKeyAndValueAt(int index);

        IComparable GetKeyAt(int index);

        object GetValueAsObjectAt(int index);

        IKeyAndValue GetLastKeyAndValue();

        IBTreeNode GetChildAt(int index, bool throwExceptionIfNotExist);

        IBTreeNode GetParent();

        object GetParentId();

        void SetKeyAndValueAt(IComparable key, object value, int index);

        void SetKeyAndValueAt(IKeyAndValue keyAndValue, int index);

        void SetKeyAndValueAt(IComparable key, object value, int index, bool shiftIfAlreadyExist, bool incrementNbKeys);

        void SetKeyAndValueAt(IKeyAndValue keyAndValue, int index, bool shiftIfAlreadyExist, bool incrementNbKeys);

        IBTreeNode ExtractRightPart();

        IKeyAndValue GetMedian();

        void SetChildAt(IBTreeNode node, int childIndex, int indexDestination, bool throwExceptionIfDoesNotExist);

        void SetChildAt(IBTreeNode child, int index);

        void IncrementNbChildren();

        /// <summary>
        ///   Returns the position of the key.
        /// </summary>
        /// <remarks>
        ///   Returns the position of the key. If the key does not exist in node, returns the position where this key should be,multiplied by -1 
        ///                                                                                                                                      
        ///  <pre>or example for node of degree 3 : [1 89 452 789 - ],    
        ///    calling getPositionOfKey(89) returns 2 (starts with 1)    
        ///    calling getPositionOfKey(99) returns -2 (starts with 1),because the position should be done, but it does not exist so multiply by -1
        ///    his is used to know the child we should descend to!in this case the getChild(2).</pre>
        /// 
        /// </remarks>
        /// <param name="key"> </param>
        /// <returns> The position of the key,as a negative number if key does not exist, warning, the position starts with 1and not 0! </returns>
        int GetPositionOfKey(IComparable key);

        void InsertKeyAndValue(IComparable key, object value);

        void MergeWith(IBTreeNode node);

        void SetNbKeys(int nbKeys);

        void SetNbChildren(int nbChildren);

        int GetDegree();

        int GetNbChildren();

        void SetParent(IBTreeNode node);

        object DeleteKeyForLeafNode(IKeyAndValue keyAndValue);

        void DeleteKeyAndValueAt(int index, bool shiftChildren);

        bool HasParent();

        object GetId();

        void SetId(object id);

        void SetBTree(IBTree btree);

        IBTree GetBTree();

        void Clear();

        void DeleteChildAt(int index);

        object GetChildIdAt(int childIndex, bool throwExceptionIfDoesNotExist);
    }
}
