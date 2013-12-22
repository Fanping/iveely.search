using System.Collections.Generic;
using System.Threading;
using Iveely.CloudComputing.CacheCommon;

namespace Iveely.CloudComputing.CacheAPI
{
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
            int maxRetryCount = 10;
            object obj = Selector.GetItem(key);
            while (obj == null && maxRetryCount > 0)
            {
                maxRetryCount--;
                obj = Selector.GetItem(key);
            }
            return obj;
        }

        /// <summary>
        /// 根据Value获取Key集合
        /// </summary>
        public static object[] GetKeysByValue(object expression, int topN, object changeValue = null)
        {
            int maxRetryCount = 3;
            object[] objects = Selector.GetKeyByValue(expression, topN, changeValue);
            while (objects == null && maxRetryCount > 0)
            {
                maxRetryCount--;
                Thread.Sleep(1000);
                objects = Selector.GetKeyByValue(expression, topN, changeValue);
            }
            return objects;

        }

        /// <summary>
        /// 设置Keys-Value集合（key不同，但是value相同）
        /// </summary>
        public static void SetList(IEnumerable<object> keys, object value, bool overrides = false)
        {
            Selector.SetItems(keys, value, overrides);
        }
    }
}
