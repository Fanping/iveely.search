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
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Text.Segment;

namespace Iveely.Framework.Algorithm
{
    [Serializable]
    public abstract class Invert<T>
    {
        #region 属性或字段

        /// <summary>
        /// 分词组件
        /// </summary>
        protected readonly Participle participle;

        /// <summary>
        /// 倒排表
        /// </summary>
        //private ListTable<double> table;
        public readonly DimensionTable<string, string, T> table;

        #endregion

        #region 公有方法

        /// <summary>
        /// 构造方法
        /// </summary>
        public Invert()
        {
            this.table = new DimensionTable<string, string, T>();
            participle = Participle.GetInstance();
        }

        /// <summary>
        /// 添加文档
        /// <example>
        ///  如果该文档编号已经存在则，覆盖以前的索引
        /// </example>
        /// </summary>
        /// <param name="id"> 文档编号 </param>
        /// <param name="doc"> 文档内容 </param>
        public void AddDocument(object id, string doc, bool split = false)
        {
            /// / 获取此文档的词频集合
            string[] words;
            if (split)
            {
                words = doc.Split(' ');
            }
            else
            {
                words = participle.Split(doc).Split('/');
            }
            ProcessWords(words, id);
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
        public List<T> FindDocumentByKey(string key, bool asc)
        {
            return this.table.GetKeyValueByName(key);
        }

        public List<T> FindValueByKey(string[] keys)
        {
            List<T> result = new List<T>();
            foreach (string key in keys)
            {
                List<T> temp = this.table.GetValueByName(key);
                if (result != null)
                {
                    result.AddRange(temp);
                }
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
        public List<T> FindCommonDocumentByKeys(string[] keys)
        {
            List<T> result = new List<T>();
            foreach (string key in keys)
            {
                List<T> temp = this.FindDocumentByKey(key, false);
                if (temp != null)
                {
                    result.AddRange(temp);
                }
            }
            result.Sort();
            return result;
        }



        /// <summary>
        /// 获取文档中关键字的频率
        /// </summary>
        /// <param name="words"> </param>
        /// <returns> </returns>
        public abstract void ProcessWords(string[] words, object docId);


        #endregion
    }
}
