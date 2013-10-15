/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Diagnostics;

namespace Iveely.CloudComputting.Cache
{
    /// <summary>
    /// 分布式内缓存
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public class Memory
    {
        /// <summary>
        /// 结点选择器
        /// </summary>
        private static readonly Selector Selector = Selector.GetInstance();

        /// <summary>
        /// 设置Key-Value项
        /// </summary>
        public static void Set(object key, object value, bool overrides = true)
        {
            Selector.SetItem(key, value, overrides);
        }

        /// <summary>
        /// 根据Key，获取Value项
        /// </summary>
        public static object Get(object key)
        {
            return Selector.GetItem(key);
        }

        /// <summary>
        /// 根据Value获取Key集合
        /// </summary>
        public static object[] GetKeysByValue(object expression, int topN, object changeValue = null)
        {
            return Selector.GetKeyByValue(expression, topN, changeValue);
        }

        /// <summary>
        /// 设置Keys-Value集合（key不同，但是value相同）
        /// </summary>
        public static void SetList(IEnumerable<object> keys, object value, bool overrides = false)
        {
            Selector.SetItems(keys, value, overrides);
        }

#if DEBUG
        Executor executor = new Executor();
        public void DoTestGetKeysByValue()
        {
            executor.Start();
        }

        [TestMethod]
        public void TestGetKeysByValue()
        {
            Thread thread = new Thread(DoTestGetKeysByValue);
            thread.Start();
            Thread.Sleep(2000);
            //for (int i = 0; i < 1000; i++)
            //{
            //    Set(i, i % 10);
            //}

            //List<int> objs = new List<int>(GetKeysByValue(2, 101).Cast<int>());
            //for (int i = 0; i < objs.Count; i++)
            //{
            //    Assert.AreEqual(objs[i], 992 - i * 10);
            //}
            executor.Stop();
        }

        [TestMethod]
        public void TestPerfOnSet()
        {
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //for (int i = 0; i < 100; i++)
            //{
            //    Set(i, i);
            //}
            //double time = watch.Elapsed.TotalMilliseconds;
            
        }

        [TestMethod]
        public void TestPerfOnGet()
        {

        }
    }
#endif
}
