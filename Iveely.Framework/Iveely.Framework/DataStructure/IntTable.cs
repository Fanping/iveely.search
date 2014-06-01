/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Iveely.Framework.DataStructure
{
    /// <summary>
    /// Int类型哈希表
    /// 主要用于Key存储关键字，对值有累加操作的哈希表
    /// </summary>
    [Serializable]
    public class IntTable<TKey, TValue> : Hashtable
    {
        #region 公有方法

        protected IntTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public IntTable()
        {
        }

        /// <summary>
        /// 添加关键字
        /// </summary>
        /// <param name="key"> 关键字 </param>
        /// <param name="value"> 值（遇到重复值，则累加） </param>
        /// <param name="accumulate"></param>
        public void Add(TKey key, TValue value, bool accumulate = true)
        {
            if (ContainsKey(key) && accumulate)
            {
                this[key] = Add(ConvertType(this[key]), value);
            }
            else
            {
                this[key] = value;
            }
        }

        /// <summary>
        /// 批量插入值
        /// </summary>
        /// // <param name="keys">关键字数组</param>
        public void Add(TKey[] keys)
        {
            foreach (TKey key in keys)
            {
                Add(key, ConvertType(1));
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 泛型数据类型转换
        /// </summary>
        /// <param name="value"> 传入需要转换的值 </param>
        /// <returns> </returns>
        private TValue ConvertType(object value)
        {
            return (TValue)Convert.ChangeType(value, typeof(TValue));
        }

        /// <summary>
        /// 泛型相加
        /// </summary>
        /// <param name="num1"> </param>
        /// <param name="num2"> </param>
        /// <returns> </returns>
        private object Add(TValue num1, TValue num2)
        {
            //TODO:Fix ,should use lambda
            return double.Parse(num1.ToString()) + double.Parse(num2.ToString());
        }

        #endregion
    }
}
