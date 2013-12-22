using System.Linq;
using System.Threading;
using Iveely.CloudComputing.Cacher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputing.IntegrationTesting
{
    /// <summary>
    /// 缓存的集成测试
    /// </summary>
    [TestClass]
    public class CacheTest
    {
        private Executor _executor;

        [TestMethod]
        public void IT_CacheEndToEnd()
        {
            //1. 启动缓存服务
            Thread thread = new Thread(StartCache);
            thread.Start();
            Thread.Sleep(5000);

            //2. 测试单一存和取
            GetAndSet();

            thread.Abort();
            CloseCache();
        }

        [TestMethod]
        public void IT_CacheSetList()
        {
            //1. 启动缓存服务
            Thread thread = new Thread(StartCache);
            thread.Start();
            Thread.Sleep(5000);

            //2. 测试获取集合
            object[] keys = { '1', '2' };
            CacheAPI.Memory.SetList(keys, 0);

            object[] myKeys = CacheAPI.Memory.GetKeysByValue(0, 10, 1);
            Assert.IsTrue(myKeys.Count() == 2);
            CloseCache();

        }

        private void StartCache()
        {
            if (_executor == null)
            {
                _executor = new Executor();
            }
            _executor.Start();
        }

        private void CloseCache()
        {
            if (_executor != null)
            {
                _executor.Stop();
            }
        }

        /// <summary>
        /// 测试单一存和取
        /// </summary>
        private void GetAndSet()
        {
            CacheAPI.Memory.Set(1, 1);
            Assert.AreEqual(CacheAPI.Memory.Get(1), 1);

            CacheAPI.Memory.Set(1, 2);
            Assert.AreEqual(CacheAPI.Memory.Get(1), 2);

            CacheAPI.Memory.Set(1, 3, false);
            Assert.AreEqual(CacheAPI.Memory.Get(1), 2);

            Assert.AreEqual(CacheAPI.Memory.Get(4), null);
        }
    }
}
