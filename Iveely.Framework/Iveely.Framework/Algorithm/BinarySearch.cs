using Iveely.Framework.DataStructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Algorithm
{
#if DEBUG
    [TestClass]
#endif
    public class BinarySearch
    {
        /// <summary>
        /// 二分查找算法
        /// </summary>
        /// <param name="min"> 起始位置 </param>
        /// <param name="max"> 结束位置 </param>
        /// <param name="num"> 被查找数 </param>
        /// <param name="list"> 查找集合 </param>
        /// <returns> 返回坐标位置 </returns>
        public static int Find(int min, int max, int num, SortedList<int> list)
        {
            if (min >= max)
            {
                return -1;
            }

            int mid = (min + max) / 2;
            if (list[mid] == num)
            {
                return mid;
            }

            if (list[mid] < num)
            {
                return Find(mid + 1, max, num, list);
            }

            return Find(min, mid - 1, num, list);
        }

#if DEBUG
        [TestMethod]
        public void TestFind()
        {
            SortedList<int> list = new SortedList<int>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }
            Assert.IsTrue(Find(0, 100, 50, list) == 50);
            Assert.IsTrue(Find(0, 100, 0, list) == 0);
            Assert.IsTrue(Find(100, 0, 0, list) == -1);
            Assert.IsTrue(Find(0, 100, -1, list) == -1);
        }

#endif
    }
}
