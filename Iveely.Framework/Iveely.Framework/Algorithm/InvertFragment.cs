/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.DataStructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Algorithm
{
#if DEBUG
    [TestClass]
#endif
    [Serializable]
    public class InvertFragment : Invert<string>
    {
        public InvertFragment(string folder="")
        {
          
        }

        public override void ProcessWords(string[] words, object docId)
        {
            for (int i = 0; i < words.Length; i++)
            {
                string temp = string.Empty;
                for (int j = i - 3; j < i + 4; j++)
                {
                    if (j > -1 && j < words.Length)
                    {
                        if (j == i)
                        {
                            temp += "<strong>" + words[j] + "</strong>";
                        }
                        else
                        {
                            temp += words[j];
                        }
                    }
                }
                this.table[words[i]][docId.ToString()] = temp;
            }
        }

#if DEBUG
        [TestMethod]
        public void TestInvertFragment()
        {
            InvertFragment fragment = new InvertFragment();
            fragment.AddDocument("http://www.iveely.com/1", "iveely，你最想知道什么？");
            fragment.AddDocument("http://www.iveely.com/2", "我可以告诉你互联网上的一切");
            fragment.AddDocument("http://www.iveely.com/3", "真的那么准么？");
            fragment.AddDocument("http://www.iveely.com/4", "那是当然的！");
            List<string> commonDocs = new List<string>(fragment.FindCommonDocumentByKeys(new[] { "的" }));
            Assert.IsTrue(commonDocs.Any());
        }
#endif
    }
}
