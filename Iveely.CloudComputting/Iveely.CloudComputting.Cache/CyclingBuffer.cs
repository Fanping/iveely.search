/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputting.Cache
{
    /// <summary>
    /// 环形缓冲区
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public class CyclingBuffer
    {
        /// <summary>
        /// 缓冲区容量
        /// </summary>
        private const int Capacity = 7;

        /// <summary>
        /// 最后更新索引位
        /// </summary>
        private int _latestIndex;

        /// <summary>
        /// 缓冲池
        /// </summary>
        private readonly object[] _buffer;

        /// <summary>
        /// 构造方法
        /// </summary>
        public CyclingBuffer()
        {
            _buffer = new object[Capacity];
        }

        /// <summary>
        /// 更新环形缓存区中的值
        /// </summary>
        public void Update(object value)
        {
            _buffer[_latestIndex++ % Capacity] = value;
            _latestIndex %= Capacity;
        }

        /// <summary>
        /// 获取环形缓冲区中当前数据(最后更新)
        /// </summary>
        /// <returns>当前数据</returns>
        public object GetCurrentData()
        {
            return _latestIndex > 0 ? _buffer[(_latestIndex - 1) % Capacity] : _buffer[Capacity - 1];
        }

        /// <summary>
        /// 获取环形缓冲区的所有数据(先进后出)
        /// </summary>
        /// <returns>数据集合</returns>
        public object[] Read()
        {
            List<object> avaiableData = new List<object>();
            for (int i = _latestIndex % Capacity; i > -1; i--)
            {
                avaiableData.Add(_buffer[i]);
            }
            for (int i = Capacity - 1; i > _latestIndex % Capacity; i--)
            {
                avaiableData.Add(_buffer[i]);
            }
            return avaiableData.ToArray();
        }

        /// <summary>
        /// 判断一个值是否存在于环形缓冲区中
        /// </summary>
        /// <param name="value">被检查的值</param>
        /// <returns>是否存在</returns>
        public bool ContainsValue(object value)
        {
            return _buffer.Contains(value);
        }

#if DEBUG
        [TestMethod]
        public void TestCyclingBuffer_Update()
        {
            for (int i = 0; i < 10; i++)
            {
                Update(i.ToString(CultureInfo.InvariantCulture));
            }
            Assert.AreEqual(GetCurrentData(), "9");
            Assert.IsTrue(ContainsValue("6"));
            Assert.IsFalse(ContainsValue("1"));
        }

        [TestMethod]
        public void TestCyclingBuffer_Read()
        {
            for (int i = 0; i < 7; i++)
            {
                Update(i);
            }
            object[] objects = Read();
            Assert.IsTrue(objects.Count() == 7);
            Assert.AreEqual(objects[0], 0);
            Assert.AreEqual(objects[6], 1);

        }
#endif
    }
}
