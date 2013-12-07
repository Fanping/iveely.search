using System;
using System.Collections;
using System.Collections.Generic;
using NDatabase.Exceptions;

namespace NDatabase.Btree
{
    internal abstract class BTreeNodeMultipleValuesPerKey : AbstractBTreeNode, IBTreeNodeMultipleValuesPerKey
    {
        protected BTreeNodeMultipleValuesPerKey(IBTree btree) : base(btree)
        {
        }

        #region IBTreeNodeMultipleValuesPerKey Members

        protected IList GetValueAt(int index)
        {
            return (IList) Values[index];
        }

        public override void InsertKeyAndValue(IComparable key, object value)
        {
            var position = GetPositionOfKey(key);
            var addToExistingCollection = false;
            int realPosition;

            if (position >= 0)
            {
                addToExistingCollection = true;
                realPosition = position - 1;
            }
            else
            {
                realPosition = -position - 1;
            }

            // If there is an element at this position and the key is different,
            // then right shift, size
            // safety is guaranteed by the rightShiftFrom method
            if (realPosition < NbKeys && key.CompareTo(Keys[realPosition]) != 0)
                RightShiftFrom(realPosition, true);

            Keys[realPosition] = key;
            
            // This is a non unique btree node, manage collection
            ManageCollectionValue(realPosition, value);
            
            if (!addToExistingCollection)
                NbKeys++;
        }

        public virtual IList Search(IComparable key)
        {
            var positionOfKey = GetPositionOfKey(key);
            var keyIsHere = positionOfKey > 0;
            int realPosition;
            
            if (keyIsHere)
            {
                realPosition = positionOfKey - 1;
                var valueAsList = GetValueAt(realPosition);

                return valueAsList;
            }
            
            if (IsLeaf())
            {
                // key is not here and node is leaf
                return null;
            }

            realPosition = -positionOfKey - 1;
            
            var node = (IBTreeNodeMultipleValuesPerKey) GetChildAt(realPosition, true);
            return node.Search(key);
        }

        public override object DeleteKeyForLeafNode(IKeyAndValue keyAndValue)
        {
            var objectHasBeenFound = false;
            var positionOfKey = GetPositionOfKey(keyAndValue.GetKey());
            
            if (positionOfKey < 0)
                return null;

            var realPosition = positionOfKey - 1;
            // In Multiple Values per key, the value is a list
            var value = (IList) Values[realPosition];
            // Here we must search for the right object. The list can contains more than 1 object
            
            var size = value.Count;
            for (var i = 0; i < size && !objectHasBeenFound; i++)
            {
                if (!value[i].Equals(keyAndValue.GetValue())) 
                    continue;

                value.Remove(i);
                objectHasBeenFound = true;
            }
            if (!objectHasBeenFound)
            {
                return null;
            }

            // If after removal, the list is empty, then remove the key from the node
            if (value.Count == 0)
            {
                // If we get there
                LeftShiftFrom(realPosition, false);
                NbKeys--;
            }

            BTreeValidator.ValidateNode(this);
            return keyAndValue.GetValue();
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

        private void ManageCollectionValue(int realPosition, object value)
        {
            var obj = Values[realPosition];

            if (obj == null)
            {
                obj = new List<object>();
                Values[realPosition] = obj;
            }
            else
            {
                if (!(obj is IList))
                {
                    var errorMessage =
                        string.Format("Value of Non Unique Value BTree should be collection and it is {0}",
                                      obj.GetType().FullName);

                    throw new BTreeException(errorMessage);
                }
            }

            var list = (IList) obj;
            list.Add(value);
        }
    }
}
