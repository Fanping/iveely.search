/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Iveely.CloudComputing.Configuration;
using Iveely.CloudComputing.MergerCommon;
using Iveely.Framework.Algorithm;
using Iveely.Framework.Log;
using Iveely.Framework.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputing.Client
{
    /// <summary>
    /// 数学方法（全局）
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public class Mathematics
    {
        private static Framework.Network.Synchronous.Client _client;

        /// <summary>
        /// 全局求和
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="val">本地和</param>
        /// <returns>全局和</returns>
        public static T Sum<T>(double val)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(val), MergePacket.MergeType.Sum, Application.Parameters[4].ToString(), Application.Parameters[5].ToString())
            {
                WaiteCallBack = true
            };
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send sum commond,value is " + val);
            return (T)Convert.ChangeType(_client.Send<object>(packet), typeof(T));
        }

        /// <summary>
        /// 全局求平均
        /// </summary>
        /// <param name="val">本地均值</param>
        /// <returns>全局均值</returns>
        public static double Average(double val)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(val), MergePacket.MergeType.Average,
                Application.Parameters[4].ToString(), Application.Parameters[5].ToString()) {WaiteCallBack = true};
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send average commond,value is " + val);
            return _client.Send<double>(packet);
        }

        /// <summary>
        /// 合并列表
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="objects">列表集合（本地）</param>
        /// <returns>列表集合（全局）</returns>
        public static List<T> CombineList<T>(List<T> objects)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(objects), MergePacket.MergeType.CombineList,
                Application.Parameters[4].ToString(), Application.Parameters[5].ToString()) {WaiteCallBack = true};
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send combine list commond.");
            return _client.Send<List<T>>(packet);
        }

        /// <summary>
        /// 合并哈希表
        /// </summary>
        /// <param name="table">哈希表（本地）</param>
        /// <returns>哈希表（全局）</returns>
        public static Hashtable CombineTable(Hashtable table)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(table), MergePacket.MergeType.CombineTable,
                Application.Parameters[4].ToString(), Application.Parameters[5].ToString()) {WaiteCallBack = true};
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send combine table commond.");
            return _client.Send<Hashtable>(packet);
        }

        /// <summary>
        /// 归并排序
        /// </summary>
        /// <typeparam name="T">排序数据类型（目前只支持int和double）</typeparam>
        /// <param name="objects">排序列表（本地）</param>
        /// <returns>排序列表（全局）</returns>
        public static T[] CombineSort<T>(T[] objects) where T : IComparable
        {
            Init();
            Logger.Info("Start local sort.");
            QuickSort<T> quickSort = new QuickSort<T>();
            objects = quickSort.GetResult(objects);
            Logger.Info("Local sort has finied,now send to Merger to combine.");
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(objects), MergePacket.MergeType.CombineSort,
            Application.Parameters[4].ToString(), Application.Parameters[5].ToString()) {WaiteCallBack = true};
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send combine sort commond.");
            object[] results = _client.Send<object[]>(packet);
            return Array.ConvertAll(results, n => (T) Convert.ChangeType(n, typeof (T)));
        }

        /// <summary>
        /// 列表去重
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="objects">列表（本地）</param>
        /// <returns>列表（全局）</returns>
        public static List<T> Distinct<T>(List<T> objects)
        {
            Init();
            objects = (List<T>)objects.Distinct();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(objects), MergePacket.MergeType.Distinct,
                 Application.Parameters[4].ToString(), Application.Parameters[5].ToString()) {WaiteCallBack = true};
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send distinct commond. ");
            List<object> results = _client.Send<List<object>>(packet);
            return results.Select(result => (T) result).ToList();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private static void Init()
        {
            if (_client == null)
            {
                string remoteServer = SettingItem.GetInstance().MergeServerIP;
                const int remotePort = 8801;
                _client = new Framework.Network.Synchronous.Client(remoteServer, remotePort);
            }
        }

#if DEBUG

        [TestMethod]
        public void Test_QuickSort()
        {
            int[] array = new int[10000];
            for (int i = 0; i < 10000; i++)
            {
                Random random = new Random(i);
                array[i] = random.Next(0, 10000);
            }
            QuickSort<int> quickSort = new QuickSort<int>();
            array = quickSort.GetResult(array);
            Assert.IsTrue(array[10] <= array[100]);

            array = quickSort.GetResult(null);
            Assert.IsNull(array);
        }


        [TestMethod]
        public void Test_CombineSort()
        {
            int[] arrayA = { 1, 2 };
            int[] arrayB = { 3, 4 };
            CombineSort<int> combineSort = new CombineSort<int>();
            int[] array = combineSort.GetResult(arrayA, arrayB);
            for (int i = 1; i < 5; i++)
            {
                Assert.AreEqual(array[i - 1], i);
            }

            arrayA = new[] { 1, 3 };
            arrayB = new[] { 2, 4 };
            combineSort = new CombineSort<int>();
            array = combineSort.GetResult(arrayA, arrayB);
            for (int i = 1; i < 5; i++)
            {
                Assert.AreEqual(array[i - 1], i);
            }

            arrayB = new[] { 2, 4 };
            combineSort = new CombineSort<int>();
            array = combineSort.GetResult(null, arrayB);
            Assert.AreEqual(arrayB, array);

            arrayA = new[] { 2, 4 };
            combineSort = new CombineSort<int>();
            array = combineSort.GetResult(arrayA, null);
            Assert.AreEqual(arrayA, array);

            combineSort = new CombineSort<int>();
            array = combineSort.GetResult(null, null);
            Assert.AreEqual(null, array);


        }

#endif
    }
}
