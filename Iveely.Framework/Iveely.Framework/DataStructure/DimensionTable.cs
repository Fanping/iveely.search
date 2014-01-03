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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.DataStructure
{
    /// <summary>
    /// 二维表
    /// </summary>
    /// <typeparam name="TValue"> </typeparam>
    /// <typeparam name="TRow"> </typeparam>
    /// <typeparam name="TColumn"> </typeparam>


#if DEBUG
    [TestClass]
#endif
    [Serializable]
    public class DimensionTable<TColumn, TRow, TValue>
    {
        #region 属性或字段

        /// <summary>
        /// 列集合
        /// </summary>
        private readonly Hashtable _cloumns = new Hashtable();

        #endregion

        #region 内部类

        /// <summary>
        /// 列
        /// </summary>
        [Serializable]
        public class Cloumn<T2>
        {
            /// <summary>
            /// 列存储器
            /// </summary>
            private readonly ListTable<T2> _table = new ListTable<T2>();

            /// <summary>
            /// 索引器
            /// </summary>
            /// <param name="index"> </param>
            /// <returns> </returns>
            public TValue this[T2 index]
            {
                get
                {
                    if (_table[index] == null)
                    {
                        return default(TValue);
                    }
                    return (TValue)Convert.ChangeType(_table[index], typeof(TValue));
                }
                set { _table[index] = value; }
            }

            /// <summary>
            /// 获取所有的列集合
            /// (将一列的所有提取出来)
            /// </summary>
            /// <returns> </returns>
            public SortedList<TValue> GetAllValue()
            {
                var result = new SortedList<TValue>();
                foreach (DictionaryEntry row in _table)
                {
                    result.Add((TValue)row.Value);
                }
                return result;
            }

            public List<TRow> GetAllKeys()
            {
                var result = new List<TRow>();
                foreach (DictionaryEntry row in _table)
                {
                    result.Add((TRow)row.Key);
                }
                return result;
            }

            /// <summary>
            /// 获取所有的列集合
            /// (将一列的所有提取出来以及文档编号)
            /// 小数部分是文档编号
            /// 整数部分是次数
            /// </summary>
            /// <returns> </returns>
            public List<Tuple<TRow, TValue>> GetAllKeyValue(int maxCount)
            {
                var result = new List<Tuple<TRow, TValue>>();
                foreach (DictionaryEntry row in _table)
                {
                    Tuple<TRow, TValue> tuple = new Tuple<TRow, TValue>((TRow)row.Key, (TValue)row.Value);
                    result.Add(tuple);
                    maxCount--;
                    if (maxCount == 0)
                    {
                        break;
                    }
                }
                return result;
            }

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

            #endregion
        }

        #endregion

        #region 公有方法

        /// <summary>
        /// 列访问索引器
        /// </summary>
        /// <param name="index"> </param>
        /// <returns> </returns>
        public Cloumn<TRow> this[TColumn index]
        {
            get
            {
                if (!_cloumns.ContainsKey(index))
                {
                    _cloumns.Add(index, new Cloumn<TRow>());
                }
                return (Cloumn<TRow>)_cloumns[index];
            }
            set { _cloumns[index] = value; }
        }

        /// <summary>
        /// 根据行名字，获取整行数据
        /// 类似于我们选中某行，我们将选中某行的数据
        /// 以列表的形式取出
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> </returns>
        public SortedList<TValue> GetValueByName(TRow name)
        {
            if (!_cloumns.ContainsKey(name))
            {
                return null;
            }
            return ((Cloumn<TRow>)_cloumns[name]).GetAllValue();
        }

        /// <summary>
        /// 根据行名字，获取整行数据
        /// 类似于我们选中某行，我们将选中某行的数据
        /// 以列表的形式取出
        /// </summary>
        /// <returns> </returns>
        public SortedList<TValue> GetKeyValueByName(TRow name)
        {
            if (!_cloumns.ContainsKey(name))
            {
                return null;
            }
            return ((Cloumn<TRow>)_cloumns[name]).GetAllValue();
        }

        /// <summary>
        /// 获取第一列集合
        /// </summary>
        /// <returns> </returns>
        public List<TRow> GetFirstCloumns()
        {
            //读取每一行
            return (from DictionaryEntry cloumn in _cloumns select (TRow)cloumn.Key).ToList();
        }

        /// <summary>
        /// 根据列的名字清楚列数据(不建议常用，效率较低，会去读取每一行然后去删除该列)
        /// </summary>
        /// <param name="name"> </param>
        public void CleanByColumnName(TRow name)
        {
            //读取每一行
            foreach (DictionaryEntry cloumn in _cloumns)
            {
                //删除改行中所有相关数据
                ((Hashtable)cloumn.Value).Remove(name);
            }
        }

        #endregion

        #region 测试

#if DEBUG

        [TestMethod]
        public void TestDimensionTable()
        {
            var strIntTable = new DimensionTable<string, int, double>();
            strIntTable["a"][1] = 0.01;
            strIntTable["b"][2] = 0.02;
            Assert.AreEqual(strIntTable["a"][1], 0.01);
            Assert.AreEqual(strIntTable["b"][2], 0.02);

            var strStrTable = new DimensionTable<string, string, double>();
            strStrTable["a"]["_1"] = 0.031;
            strStrTable["a"]["_3"] = 0.033;
            strStrTable["a"]["_2"] = 0.032;
            SortedList<double> values = strStrTable["a"].GetAllValue();
            Assert.IsTrue(values[2] == 0.033);
            Assert.IsTrue(values[1] == 0.032);
            Assert.IsTrue(values[0] == 0.031);

            List<string> keys = strStrTable["a"].GetAllKeys();
            Assert.IsTrue(keys[2] == "_3");
            Assert.IsTrue(keys[1] == "_2");
            Assert.IsTrue(keys[0] == "_1");

            List<Tuple<string, double>> keyValues = strStrTable["a"].GetAllKeyValue();
            Assert.IsTrue(keyValues.Count == 3);
        }

#endif
        #endregion
    }
}
