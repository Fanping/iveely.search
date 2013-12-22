using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.DataStructure
{

    /// <summary>
    /// (按照数的大小)排序泛型列表
    /// </summary>
    /// <typeparam name="T"> 类型 </typeparam>
    [Serializable]
#if DEBUG
    [TestClass]
#endif
    public class SortedList<T> : List<T>
    {
        public SortedList(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }

        public SortedList()
        {
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="item">元素</param>
        public new void Add(T item)
        {
            try
            {
                int position = BinarySearch(item);
                position = position > 0 ? position : ~position;
                Insert(position, item);
            }
            catch
            {
                base.Add(item);
            }
        }

        /// <summary>
        /// 修改指定索引位的元素值
        /// </summary>
        /// <param name="item">修改后的元素值</param>
        /// <param name="index">被修改的索引位</param>
        /// <returns>是否修改成功</returns>
        public bool Modify(T item, int index)
        {
            if (index < Count && index >= 0)
            {
                RemoveAt(index);
                Add(item);
                return true;
            }
            return false;
        }


#if DEBUG

        [TestMethod]
        public void Test_SortedList()
        {
            SortedList<int> sortedList = new SortedList<int> {2, 1, 3};
            for (int i = 1; i < 4; i++)
            {
                Assert.AreEqual(sortedList[i], i);
            }

            Assert.IsTrue(sortedList.Modify(1, 2));
            Assert.IsTrue(sortedList.Modify(2, 2));
            Assert.IsTrue(sortedList.Modify(3, 2));
            Assert.IsFalse(sortedList.Modify(-1, -1));
            Assert.IsFalse(sortedList.Modify(5, 5));
        }

#endif
    }
}
