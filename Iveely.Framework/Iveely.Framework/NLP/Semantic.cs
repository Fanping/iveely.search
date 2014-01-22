/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpICTCLAS;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 语义分析
    /// </summary>
    public class Semantic
    {
        /// <summary>
        /// 词语解释
        /// </summary>
        public class Explain
        {
            /// <summary>
            /// 普通解释
            /// </summary>
            public List<string> Information
            {
                get;
                private set;
            }

            /// <summary>
            /// 反义词
            /// </summary>
            public List<string> Antonym
            {
                get;
                private set;
            }

            /// <summary>
            /// 近义词
            /// </summary>
            public List<string> Thesaurus
            {
                get;
                private set;
            }

            public Explain()
            {
                Information = new List<string>();
                Antonym = new List<string>();
                Thesaurus = new List<string>();
            }
        }

        /// <summary>
        /// 词典
        /// </summary>
        private Hashtable _dictionary;

        /// <summary>
        /// 语义组件
        /// </summary>
        private static Semantic _semantic;

        private static Text.Segment.IctclasSegment segment;

        public static Semantic GetInstance()
        {
            return _semantic ?? (_semantic = new Semantic());
        }

        private Semantic()
        {
            _dictionary = new Hashtable();
            LoadDictionary("Chinese Dictionary.txt");
            segment = Text.Segment.IctclasSegment.GetInstance();
        }

        private void LoadDictionary(string dicPath)
        {
            string[] lines = File.ReadAllLines(dicPath);
            foreach (string line in lines)
            {
                string[] text = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (!_dictionary.ContainsKey(text[0]) && text.Length > 0)
                {
                    string[] explanations = text[1].Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                    Explain explain = new Explain();
                    foreach (string explanation in explanations)
                    {
                        if (explanation.StartsWith("[似]"))
                        {
                            explain.Thesaurus.Add(explanation.Replace("[似]", ""));
                        }
                        else if (explanation.StartsWith("[反]"))
                        {
                            explain.Antonym.Add(explanation.Replace("[反]", ""));
                        }
                        else
                        {
                            explain.Information.Add(explanation);
                        }
                    }
                    _dictionary.Add(text[0], explain);
                }
            }
        }

        /// <summary>
        /// 获取一句话的相似表达
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetSimilarContext(string context)
        {
            List<WordResult[]> results = segment.SplitToArray(context);
            for (int i = 0; i < results.Count; i++)
            {
                for (int j = 1; j < results[i].Length - 1; j++)
                {
                    Console.Write(results[i][j].sWord + " " + Utility.GetPOSString(results[i][j].nPOS));
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取一个词的解释
        /// </summary>
        /// <param name="word">词语</param>
        /// <returns></returns>
        public string GetWordExplain(string word)
        {
            if (_dictionary.ContainsKey(word))
            {
                Explain explain = (Explain) _dictionary[word];
                return string.Join("\n", explain.Information);
            }
            return string.Empty;
        }
    }
}
