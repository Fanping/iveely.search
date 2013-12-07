using System;
using NDatabase.Exceptions;

namespace NDatabase.Btree
{
    internal abstract class BTreeNodeSingleValuePerKey : AbstractBTreeNode, IBTreeNodeOneValuePerKey
    {
        protected BTreeNodeSingleValuePerKey(IBTree btree) : base(btree)
        {
        }

        #region IBTreeNodeOneValuePerKey Members

        public virtual object GetValueAt(int index)
        {
            return Values[index];
        }

        public override void InsertKeyAndValue(IComparable key, object value)
        {
            var position = GetPositionOfKey(key);
            
            if (position >= 0)
                throw new DuplicatedKeyException("Duplicated Key: " + key);

            var realPosition = -position - 1;
            
            // If there is an element at this position, then right shift, size
            // safety is guaranteed by the rightShiftFrom method
            if (realPosition < NbKeys)
                RightShiftFrom(realPosition, true);

            Keys[realPosition] = key;
            Values[realPosition] = value;
            NbKeys++;
        }

        public object Search(IComparable key)
        {
            var positionOfKey = GetPositionOfKey(key);
            var keyIsHere = positionOfKey > 0;
            int realPosition;

            if (keyIsHere)
            {
                realPosition = positionOfKey - 1;
                var value = GetValueAt(realPosition);
                return value;
            }
            
            if (IsLeaf())
            {
                // key is not here and node is leaf
                return null;
            }

            realPosition = -positionOfKey - 1;

            var node = (IBTreeNodeOneValuePerKey) GetChildAt(realPosition, true);
            return node.Search(key);
        }

        public abstract override void DeleteChildAt(int arg1);

        public abstract override object GetChildIdAt(int arg1, bool arg2);

        public abstract override object GetId();

        public abstract override object GetValueAsObjectAt(int arg1);

        public abstract override void SetChildAt(IBTreeNode arg1, int arg2, int arg3, bool arg4);

        public abstract override void SetChildAt(IBTreeNode arg1, int arg2);

        public abstract override void SetId(object arg1);

        protected abstract override void SetNullChildAt(int arg1);

        #endregion
    }
}