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
using Iveely.Framework.DataStructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Algorithm
{
    /// <summary>
    /// 倒排索引建立
    /// </summary>

#if DEBUG
    [TestClass]
#endif

    [Serializable]
    public class InvertFrequency : Invert<double>
    {
        public override void ProcessWords(string[] words, object docId)
        {
            var frequency = new IntTable<string, int>();
            frequency.Add(words);
            foreach (DictionaryEntry de in frequency)
            {
                if (de.Value != null)
                {
                    Table[de.Key.ToString()][docId.ToString()] = double.Parse(de.Value.ToString());
                }
            }
        }

#if DEBUG
        [TestMethod]
        public void Test_FindDocumentByKey()
        {
            var invert = new InvertFragment();
            invert.AddDocument(1, "今天天气真好");
            invert.AddDocument(2, "今天天气虽然很好，但是风大");
            invert.AddDocument(3, "天天就知道吃");
            invert.AddDocument(4, "爱是粉红的羽毛");
            invert.AddDocument(5, "雪白的羽毛");
            invert.AddDocument(6, "生活天很好");
            IList<string> result = invert.FindCommonDocumentByKeys(new[] { "爱", "风", "好" },10);
            Assert.IsTrue(result.Any());
        }

#endif

    }
}
