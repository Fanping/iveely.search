/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if DEBUG
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
namespace Iveely.Framework.Text.Segment
{
    /// <summary>
    /// N-Gram 分词模型
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public class NGram
    {
        public enum Type
        {
            /// <summary>
            /// 一元组
            /// </summary>
            UnitGram,

            /// <summary>
            /// 二元组
            /// </summary>
            BiGram,

            /// <summary>
            /// 三元组
            /// </summary>
            TriGram,

            /// <summary>
            /// 所有一元组、二元组、三元组
            /// </summary>
            AllGram
        }

        /// <summary>
        /// 获取元组
        /// </summary>
        /// <param name="text">切分文本</param>
        /// <param name="getGramType">期待的元组类型(默认所有元组)</param>
        /// <returns>返回期待的元组</returns>
        public static string[] GetGram(string text, Type getGramType = Type.AllGram)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new string[0];
            }

            List<string> arrayString = new List<string>();
            string temp = string.Empty;
            text = text.ToLower();
            foreach (char c in text)
            {
                //连续的字母和数字应该在一起
                if (!(char.IsWhiteSpace(c)|| char.IsSeparator(c) || char.IsPunctuation(c)))
                {
                    if (c < 128)
                    {
                        temp += c;
                    }
                    else
                    {
                        arrayString.Add(c.ToString(CultureInfo.InvariantCulture));
                        if (temp != string.Empty)
                        {
                            arrayString.Add(temp);
                            temp = string.Empty;
                        }
                    }
                }
                else
                {
                    if (temp != string.Empty)
                    {
                        arrayString.Add(temp);
                        temp = string.Empty;
                    }
                }
            }
            string[] uniGrams = arrayString.ToArray();//(string[])Convert.ChangeType(text.ToCharArray(),typeof(string[]));//text.ToLower().ToCharArray().Cast<string>().ToArray();//.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries);

            //如果需要的是一元组
            if (getGramType == Type.UnitGram)
            {
                return uniGrams;
            }

            //如果需要的是二元组
            if (getGramType == Type.BiGram)
            {
                return GetBiGram(uniGrams);
            }

                //如果需要的是三元组
            if (getGramType == Type.TriGram)
            {
                return GetTriGram(uniGrams);
            }

                //如果需要的是所有元组
            List<string> allGram = new List<string>();
            allGram.AddRange(uniGrams);
            allGram.AddRange(GetBiGram(uniGrams));
            allGram.AddRange(GetTriGram(uniGrams));
            return allGram.ToArray();
        }

        /// <summary>
        /// 获取二元组
        /// </summary>
        /// <param name="uniGram">一元组</param>
        /// <returns>返回二元组</returns>
        private static string[] GetBiGram(string[] uniGram)
        {
            List<string> bigrams = new List<string>();
            string lastChar = uniGram[0];
            for (int i = 1; i < uniGram.Length; i++)
            {
                //BUG:中文和英文在这里表达的方式不一样
                bigrams.Add(lastChar + uniGram[i]);
                lastChar = uniGram[i];
            }
            return bigrams.ToArray();
        }

        /// <summary>
        /// 获取三元组
        /// </summary>
        /// <param name="uniGram">一元组</param>
        /// <returns>返回三元组</returns>
        private static string[] GetTriGram(string[] uniGram)
        {
            if (uniGram.Length < 3)
            {
                return new string[0];
            }
            List<string> triGram = new List<string>();
            string head = uniGram[0];
            string middle = uniGram[1];
            for (int i = 2; i < uniGram.Length; i++)
            {
                //BUG:中文和英文在这里表达的方式不一样
                triGram.Add(head + middle + uniGram[i]);
                head = middle;
                middle = uniGram[i];
            }
            return triGram.ToArray();
        }

        #region Test

#if DEBUG

        [TestMethod]
        public void Test_GetUniGram()
        {
            const string content = "Iveely Computing.";
            string[] uniGrams = GetGram(content, Type.UnitGram);
            Assert.IsTrue(uniGrams.Contains("iveely"));
            Assert.IsTrue(uniGrams.Contains("computing"));
            Assert.IsTrue(uniGrams.Count() == 2);

            string emptyContent = string.Empty;
            string[] emptyUniGrams = GetGram(emptyContent, Type.UnitGram);
            Assert.IsTrue(emptyUniGrams.Length == 0);

            const string chineseContent = "我们是Iveely的忠实开发者，你呢?";
            string[] chineseUniGrams = GetGram(chineseContent, Type.UnitGram);
            Assert.IsTrue(chineseUniGrams.Length == 12);
        }

        [TestMethod]
        public void Test_GetBiGram()
        {
            const string content = "Iveely Computing platform";
            string[] biGrams = GetGram(content, Type.BiGram);
            Assert.IsTrue(biGrams.Contains("iveelycomputing"));
            Assert.IsTrue(biGrams.Contains("computingplatform"));
            Assert.IsTrue(biGrams.Count() == 2);

            const string emptyContent = "Iveely";
            string[] emptyBiGrams = GetGram(emptyContent, Type.BiGram);
            Assert.IsTrue(emptyBiGrams.Length == 0);
        }

        [TestMethod]
        public void Test_GetTriGram()
        {
            const string content = "Iveely Computing platform";
            string[] triGrams = GetGram(content, Type.TriGram);
            Assert.IsTrue(triGrams.Contains("iveelycomputingplatform"));
            Assert.IsTrue(triGrams.Count() == 1);

            const string emptyContent = "Iveely Computing";
            string[] emptyTriGrams = GetGram(emptyContent, Type.TriGram);
            Assert.IsTrue(emptyTriGrams.Length == 0);
        }

        [TestMethod]
        public void Test_GetAllGram()
        {
            const string content = "Iveely Computing platform";
            string[] grams = GetGram(content);
            Assert.IsTrue(grams.Length == 6);
        }
#endif
        #endregion
    }
}
