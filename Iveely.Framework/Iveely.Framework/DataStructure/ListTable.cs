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
using System.Runtime.Serialization;

namespace Iveely.Framework.DataStructure
{
    /// <summary>
    /// 值为List的哈希表
    /// 主要用于一个关键字对应一个集合
    /// </summary>
    [Serializable]
    public class ListTable<T> : Hashtable
    {
        #region 公有方法

        protected ListTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ListTable()
        {
        }

        /// <summary>
        /// 添加关键字
        /// </summary>
        /// <param name="key"> 添加的关键字 </param>
        /// <param name="value"> 值（出现重复值，会累加） </param>
        public void Add(object key, T value)
        {
            if (ContainsKey(key))
            {
                var list = (SortedList<T>)this[key];
                list.Add(value);
                this[key] = list;
            }
            else
            {
                var list = new SortedList<T> {value};
                Add(key, list);
            }
        }

        /// <summary>
        /// 以升序的方式获取关键字对应的值
        /// </summary>
        /// <param name="key"> 关键字 </param>
        /// <returns> 升序值序列 </returns>
        public List<T> GetValuesByAsc(object key)
        {
            return (List<T>)this[key];
        }

        /// <summary>
        /// 以降序的方式获取关键字对应的值
        /// </summary>
        /// <param name="key"> 关键字 </param>
        /// <returns> 降序值序列 </returns>
        public List<T> GetValuesByDesc(object key)
        {
            List<T> list = GetValuesByAsc(key);
            list.Reverse();
            return list;
        }

        #endregion

        #region 测试

        public static void Test()
        {
            var table = new ListTable<double> {{"a", 2.0}, {"b", 6.0}, {"a", 3.6}, {"b", 2.2}, {"a", 6.3}, {"b", 3.8}};

            List<double> list = table.GetValuesByAsc("a");
            foreach (double va in list)
            {
                Console.WriteLine(va);
            }
        }

        #endregion
    }
}
