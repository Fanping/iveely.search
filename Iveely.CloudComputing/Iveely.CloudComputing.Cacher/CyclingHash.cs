/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputing.Cacher
{
    /// <summary>
    /// 环形哈希空间
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public class CyclingHash
    {
        public int Count { get; private set; }

        /// <summary>
        /// 环形哈希空间容量(默认1亿)
        /// </summary>
        private const int Capacity = 100 * 100;

        /// <summary>
        /// 最后更新的索引位置
        /// </summary>
        private int _latestIndex;

        /// <summary>
        /// Key集合
        /// </summary>
        private readonly object[] _keys;

        /// <summary>
        /// Hash Set
        /// </summary>
        private readonly Hashtable _hashSet;

        /// <summary>
        /// 构造方法
        /// </summary>
        public CyclingHash()
        {
            _keys = new object[Capacity];
            _hashSet = new Hashtable();
        }

        /// <summary>
        /// 添加Key-Value的缓存记录
        /// </summary>
        public void Add(object key, object value)
        {
            object tempKey = _keys[_latestIndex % Capacity];
            _keys[_latestIndex % Capacity] = key;
            if (ContainsKey(key))
            {
                ((CyclingBuffer)_hashSet[key]).Update(value);
            }
            else
            {
                Count++;
                if (Count > Capacity)
                {
                    _hashSet.Remove(tempKey);
                    Count--;
                }
                CyclingBuffer buffer = new CyclingBuffer();
                buffer.Update(value);
                _hashSet.Add(key, buffer);
            }
            _latestIndex++;
            _latestIndex %= Capacity;
        }

        /// <summary>
        /// 获取最近更新的数据
        /// </summary>
        /// <returns></returns>
        public object GetCurrentData()
        {
            object value;
            if (_latestIndex > 0)
            {
                value = _keys[(_latestIndex - 1) % Capacity];
            }
            else
            {
                value = _keys[Capacity - 1];
            }
            return value;
        }

        /// <summary>
        /// 根据缓存的Key获取对应的Value
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>关键字对应的值</returns>
        public object GetValue(object key)
        {
            object obj = _hashSet[key];
            if (obj == null)
            {
                return null;
            }
            return ((CyclingBuffer)obj).GetCurrentData();
        }

        /// <summary>
        /// 获取所有Key的集合
        /// </summary>
        public object[] Read()
        {
            List<object> avaiableData = new List<object>();
            for (int i = _latestIndex % Capacity; i > -1; i--)
            {
                avaiableData.Add(_keys[i]);
            }
            for (int i = Capacity - 1; i > _latestIndex % Capacity; i--)
            {
                avaiableData.Add(_keys[i]);
            }
            return avaiableData.ToArray();
        }

        /// <summary>
        /// 根据value获取key的集合
        /// </summary>
        public object[] ReadByValue(object value, object changeValue, int topN)
        {
            List<object> avaiableKeys = new List<object>();
            for (int i = _latestIndex % Capacity; i >= 1 && topN > avaiableKeys.Count && _keys[i - 1] != null; i--)
            {
                object val = ((CyclingBuffer)_hashSet[_keys[i - 1]]).GetCurrentData();
                if (Equals(value, val))
                {
                    if (changeValue != null)
                    {
                        ((CyclingBuffer)_hashSet[_keys[i - 1]]).Update(changeValue);
                    }
                    avaiableKeys.Add(_keys[i - 1]);
                }
            }
            for (int i = Capacity - 1; i > _latestIndex % Capacity && topN > avaiableKeys.Count && _keys[i] != null; i--)
            {
                //TODO: abstract to a method
                object key = _hashSet[_keys[i]];
                if (key == null)
                {
                    break;
                }
                string val = ((CyclingBuffer)key).GetCurrentData().ToString();
                if ((string)value == val)
                {
                    if (changeValue != null)
                    {
                        ((CyclingBuffer)_hashSet[_keys[i]]).Update(changeValue);
                    }
                    avaiableKeys.Add(_keys[i]);
                }
            }
            return avaiableKeys.ToArray();
        }

        /// <summary>
        /// 判断一个key是否在集合中存在
        /// </summary>
        /// <returns></returns>
        public bool ContainsKey(object key)
        {
            bool isExist = _hashSet.Contains(key);
            return isExist;
        }

#if DEBUG

        [TestMethod]
        public void TestGetAndSet()
        {
            CyclingHash hash = new CyclingHash();
            hash.Add("A", "a");
            hash.Add("B", "b");
            hash.Add("C", "c");
            Assert.AreEqual(hash.ContainsKey("B"), true);
            Assert.AreEqual(hash.GetCurrentData(), "C");
            Assert.AreEqual(hash.GetValue("A"), "a");
        }

        [TestMethod]
        public void TestGetKeyByValue()
        {
            CyclingHash hash = new CyclingHash();
            for (int i = 0; i < 1000; i++)
            {
                hash.Add(i, i % 10);
            }
            List<int> objs = new List<int>(hash.ReadByValue("2", null, 101).Cast<int>());
            for (int i = 0; i < objs.Count; i++)
            {
                Assert.IsTrue(objs[i] == 992 - i * 10);
            }
        }

        [TestMethod]
        public void TestGetKeyByValueChangeValue()
        {
            CyclingHash hash = new CyclingHash();
            for (int i = 0; i < 1000; i++)
            {
                hash.Add(i, i % 10);
            }
            List<int> objsA = new List<int>(hash.ReadByValue(9, -1, 10).Cast<int>());
            for (int i = 0; i < objsA.Count; i++)
            {
                Assert.IsTrue(objsA[i] == 999 - i * 10);
            }
        }

#endif
    }
}
