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
using Iveely.Framework.DataStructure;
using Iveely.Framework.Text.Segment;

namespace Iveely.Framework.Algorithm
{
    [Serializable]
    public abstract class Invert<TKey, TValue>
    {
        #region 属性或字段

        /// <summary>
        /// 分词组件
        /// </summary>
        protected readonly MetastasisSegment Participle;

        /// <summary>
        /// 倒排表
        /// </summary>
        //private ListTable<double> table;
        public readonly DimensionTable<TKey, TKey, TValue> Table;

        #endregion

        #region 公有方法

        /// <summary>
        /// 构造方法
        /// </summary>
        protected Invert()
        {
            Table = new DimensionTable<TKey, TKey, TValue>();
            Participle = new MetastasisSegment();
        }

        /// <summary>
        /// 添加文档
        /// <example>
        ///  如果该文档编号已经存在则，覆盖以前的索引
        /// </example>
        /// </summary>
        /// <param name="id"> 文档编号 </param>
        /// <param name="doc"> 文档内容 </param>
        /// <param name="split"></param>
        public void AddDocument(object id, string doc, bool split = false)
        {
            // 获取此文档的词频集合
            string[] words = split ? doc.Split(' ') : Participle.Split(doc).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            ProcessWords(words, id);
        }

        public void AddDocument(object id, string[] arrayInDoc)
        {
            ProcessWords(arrayInDoc, id);
        }


        /// <summary>
        /// 根据关键字获取它所在地文档以及在文档中的频率
        /// <example>
        ///  例如传入关键字：“北京”
        ///  传出结果：3.231 6.2145 9.542 ...
        ///  分别表示在文档231中，出现次数3
        ///  在文档2145中出现次数6
        ///  依次类推。
        /// </example>
        /// </summary>
        /// <param name="key"> 关键字 </param>
        /// <param name="asc"> 是否为升序 </param>
        /// <returns> 返回按照频率的集合 </returns>
        public List<TKey> FindDocumentByKey(TKey key, bool asc)
        {
            return Table[key].GetAllKeys();
        }

        //public List<T> FindDocIdByKey(string key, bool asc)
        //{
        //    return this.table.GetValueByName(key);
        //}

        public List<TValue> FindValueByKey(TKey[] keys)
        {
            List<TValue> result = new List<TValue>();
            foreach (TKey key in keys)
            {
                List<TValue> temp = Table.GetValueByName(key);
                result.AddRange(temp);
            }
            return result;
        }

        /// <summary>
        /// 根据关键字集获取它所在地文档以及在文档中的频率
        /// <example>
        ///  例如传入关键字：“北京 地铁”
        ///  会将二者对应的文档按照同时出现的情况进行合并
        /// </example>
        /// </summary>
        /// <returns> 返回按照频率的集合 </returns>
        public List<TValue> FindCommonDocumentByKeys(TKey[] keys, int maxCount)
        {
            IntTable<string, int> table = new IntTable<string, int>();
            foreach (TKey key in keys)
            {
                List<TKey> temp = FindDocumentByKey(key, false);
                if (temp != null && temp.Count > 0)
                {
                    foreach (var t in temp)
                    {
                        table.Add(t.ToString(), 1, true);
                    }
                }
            }
            if (table.Count < 1)
                return null;

            List<TValue> result = new List<TValue>();
            ArrayList list = new ArrayList(table.Values);
            list.Sort();
            list.Reverse();
            for (int i = 0; i < maxCount && i < list.Count; i++)
            {
                IDictionaryEnumerator ide = table.GetEnumerator();
                while (ide.MoveNext())
                {
                    // TValue k= (TValue)ide.Key;
                    if (ide.Value == list[i]) // && int.Parse(list[i].ToString()) == keys.Length)
                    {
                        result.Add((TValue)ide.Key);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// 获取文档中关键字的频率
        /// </summary>
        /// <param name="words"> </param>
        /// <param name="docId"></param>
        /// <returns> </returns>
        public abstract void ProcessWords(string[] words, object docId);


        #endregion
    }
}
