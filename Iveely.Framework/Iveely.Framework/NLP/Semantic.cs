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
using System.Text;
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
        internal class Explain
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
        /// 词汇
        /// </summary>
        public class Word
        {
            /// <summary>
            /// 单词
            /// </summary>
            public string Text
            {
                get;
                set;
            }

            /// <summary>
            /// 词汇类型
            /// </summary>
            public string Type
            {
                get;
                set;
            }

            /// <summary>
            /// 诠释
            /// </summary>
            public string Comment
            {
                get;
                set;
            }

            /// <summary>
            /// 第一基本义原
            /// </summary>
            public string FirstPrimitive;

            /// <summary>
            /// 其它基本义原
            /// </summary>
            public List<string> OtherPrimitives;

            /// <summary>
            /// 结构义原
            /// </summary>
            public List<string> StructruralWords;

            /// <summary>
            /// 该词的关系义原
            /// </summary>
            public Dictionary<string, List<string>> RelationalPrimitives;

            /// <summary>
            /// 词的关系符号义原
            /// </summary>
            public Dictionary<string, List<string>> RelationSimbolPrimitives;

            public Word()
            {
                OtherPrimitives = new List<string>();
                StructruralWords = new List<string>();
                RelationalPrimitives = new Dictionary<string, List<string>>();
                RelationSimbolPrimitives = new Dictionary<string, List<string>>();
            }

            /// <summary>
            /// 是否为虚词--如果是虚词，该structruralWords非空。
            /// </summary>
            /// <returns></returns>
            public bool IsStructruralWord()
            {
                return this.StructruralWords.Count != 0;
            }

            /// <summary>
            /// 添加结构义原
            /// </summary>
            /// <param name="structruralWord"></param>
            public void AddStructruralWord(string structruralWord)
            {
                this.StructruralWords.Add(structruralWord);
            }

            /// <summary>
            /// 添加关系义原
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void AddRelationalPrimitive(string key, string value)
            {
                List<string> list = null;
                if (RelationalPrimitives.ContainsKey(key))
                {
                    list = RelationalPrimitives[key];
                    list.Add(value);
                }
                else
                {
                    list = new List<string>();
                    list.Add(value);
                    RelationalPrimitives.Add(key, list);
                }
            }

            /// <summary>
            /// 添加结构符号义原
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void AddRelationSimbolPrimitive(string key, string value)
            {
                List<string> list = null;
                if (RelationSimbolPrimitives.ContainsKey(key))
                {
                    list = RelationSimbolPrimitives[key];
                    list.Add(value);
                }
                else
                {
                    list = new List<string> { value };
                    RelationSimbolPrimitives.Add(key, list);
                }
            }
        }

        /// <summary>
        /// 词汇类型
        /// </summary>
        internal enum WordType
        {
            PREFIX, PREP, ECHO, EXPR, SUFFIX, PUNC, N, ADV, CLAS, COOR, CONJ, V, STRU, PP, P, ADJ, PRON, AUX, NUM
        }

        /// <summary>
        /// 义原
        /// </summary>
        internal class Primitive
        {
            /// <summary>
            /// 义原关系组
            /// </summary>
            public Dictionary<int, int> AllRelationPrimitive;

            /// <summary>
            /// 义原信息
            /// </summary>
            public Dictionary<string, int> PrimitiveInfo;

            /// <summary>
            /// 单例模式实体
            /// </summary>
            private static Primitive _singlePrimitive;

            /// <summary>
            /// 获取义原实例
            /// </summary>
            /// <returns></returns>
            public static Primitive GetInstance()
            {
                return _singlePrimitive ?? (_singlePrimitive = new Primitive());
            }

            /// <summary>
            /// 构造方法
            /// </summary>
            private Primitive()
            {
                AllRelationPrimitive = new Dictionary<int, int>();
                PrimitiveInfo = new Dictionary<string, int>();

                string[] allLines = File.ReadAllLines("Init\\WHOLE.txt", Encoding.UTF8);
                foreach (string line in allLines)
                {
                    string[] context = line.Split(new string[] { "\t", "|", " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (context.Length == 4)
                    {
                        int id = int.Parse(context[0]);
                        int parentId = int.Parse(context[3]);
                        if (!PrimitiveInfo.ContainsKey(context[2]))
                        {
                            PrimitiveInfo.Add(context[2], id);
                            AllRelationPrimitive.Add(id, parentId);
                        }
                    }
                }
            }

            /// <summary>
            /// 获取义原的父义原
            /// </summary>
            /// <param name="primitive"></param>
            /// <returns></returns>
            public List<int> GetParents(string primitive)
            {
                List<int> list = new List<int>();
                if (PrimitiveInfo.ContainsKey(primitive))
                {
                    int id = PrimitiveInfo[primitive];
                    if (id >= 0)
                    {
                        list.Add(id);
                        while (AllRelationPrimitive.ContainsKey(id))
                        {
                            int parentId = AllRelationPrimitive[id];
                            list.Add(parentId);
                            if (parentId == id)
                            {
                                break;
                            }
                            id = parentId;
                        }
                    }
                }
                return list;
            }


            /// <summary>
            /// 判断是否是义原
            /// </summary>
            /// <param name="primitive"></param>
            /// <returns></returns>
            public bool IsPrimitive(string primitive)
            {
                return PrimitiveInfo.ContainsKey(primitive);
            }
        }

        /// <summary>
        /// 词汇相似度度量
        /// </summary>
        internal class WordSimilarity
        {
            /// <summary>
            /// 所有词汇
            /// </summary>
            public Dictionary<string, List<Word>> AllWords;

            private const double Alpha = 1.6;
            private const double Beta1 = 0.5;
            private const double Beta2 = 0.2;
            private const double Beta3 = 0.17;
            private const double Beta4 = 0.13;

            /// <summary>
            /// 具体词与义原的相似度一律处理为一个比较小的常数. 具体词和具体词的相似度，如果两个词相同，则为1，否则为0
            /// </summary>
            private const double Gamma = 0.2;

            /// <summary>
            /// 将任一非空值与空值的相似度定义为一个比较小的常数
            /// </summary>
            private const double Delta = 0.2;

            /// <summary>
            /// 两个无关义原之间的默认距离
            /// </summary>
            private const int DefaultPrimitiveDis = 20;

            /// <summary>
            /// 知网中的关系符号, 如果含有下面的符号，说明为义项的关系义原
            /// </summary>
            private const String RelationalSymbol = "#%$*+&@?!";

            /// <summary>
            /// 知网中的特殊符号，虚词，或具体词,知网中的虚词都是用{}括起来的
            /// </summary>
            private const string SpecialSymbol = "{";

            /// <summary>
            /// 单例模式
            /// </summary>
            private static WordSimilarity _wordSimilarity;

            public static WordSimilarity GetInstance()
            {
                if (_wordSimilarity == null)
                {
                    _wordSimilarity = new WordSimilarity();
                }
                return _wordSimilarity;
            }

            private WordSimilarity()
            {
                AllWords = new Dictionary<string, List<Word>>();
                LoadGlossary();
            }

            /// <summary>
            /// 加载义项
            /// </summary>
            private void LoadGlossary()
            {
                string[] allLines = File.ReadAllLines("Init\\glossary.txt", Encoding.UTF8);
                int i = allLines.Length;
                foreach (string line in allLines)
                {
                    i--;
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                    string[] content = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (content.Length == 3)
                    {
                        Word word = new Word();
                        word.Text = content[0].Trim();
                        word.Type = content[1].Trim();
                        word.Comment = content[2].Trim();
                        ParseDetail(content[2].Trim(), word);
                        AddWord(word);
                    }
                }
            }

            /// <summary>
            /// 解析具体概念部分
            /// </summary>
            /// <param name="related"></param>
            /// <param name="word"></param>
            public void ParseDetail(string related, Word word)
            {
                try
                {
                    string[] parts = related.Split(',');
                    bool isFirst = true;
                    bool isRelational = false;
                    bool isSimbol = false;
                    string chinese = null;
                    string relationalPrimitiveKey = null;
                    string simbolKey = null;
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i].StartsWith("("))
                        {
                            parts[i] = parts[i].Substring(1, parts[i].Length - 1);
                        }
                        if (parts[i].Contains("="))
                        {
                            isRelational = true;
                            string[] strs = parts[i].Split('=');
                            relationalPrimitiveKey = strs[0];
                            string value = strs[1].Split('|')[1];//[BUG]等号后面的|后面的是中文的值VALUE
                            word.AddRelationalPrimitive(relationalPrimitiveKey, value);
                            continue;
                        }
                        string[] strss = parts[i].Split('|');
                        if (strss.Length > 1)
                        {
                            chinese = strss[1];
                        }
                        if (chinese != null && (chinese.EndsWith(")") || chinese.EndsWith("}")))
                        {
                            chinese = chinese.Substring(0, chinese.Length - 1);
                        }
                        int type = GetPrimitiveType(strss[0]);
                        if (type == 0)
                        {
                            if (isRelational)
                            {
                                word.AddRelationalPrimitive(relationalPrimitiveKey, chinese);
                                continue;
                            }
                            if (isSimbol)
                            {
                                word.AddRelationSimbolPrimitive(simbolKey, chinese);
                                continue;
                            }
                            if (isFirst)
                            {
                                word.FirstPrimitive = chinese;
                                isFirst = false;
                                continue;
                            }
                            word.OtherPrimitives.Add(chinese);
                            continue;
                        }
                        if (type == 1)
                        {
                            isSimbol = true;
                            isRelational = false;
                            simbolKey = (strss[0].ToCharArray()[0]).ToString();
                            word.AddRelationSimbolPrimitive(simbolKey, chinese);
                            continue;
                        }

                        if (type == 2)
                        {
                            isSimbol = false;
                            isRelational = true;
                            if (strss[0].StartsWith("{"))
                            {
                                string englishi = strss[0].Substring(1);
                                if (chinese != null)
                                {
                                    word.AddStructruralWord(chinese);
                                    continue;
                                }
                                word.AddStructruralWord(englishi);
                            }

                        }

                    }
                }
                catch (Exception ex)
                {

                }

            }

            /// <summary>
            /// 获取义原类型
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public int GetPrimitiveType(string str)
            {
                string first = (str.ToCharArray()[0]).ToString();
                if (RelationalSymbol.Contains(first))
                {
                    //符号义原
                    return 1;
                }
                if (SpecialSymbol.Contains(first))
                {
                    //虚词
                    return 2;
                }
                //基本义原
                return 0;
            }


            /// <summary>
            /// 计算两个词语之间的相似度，取两个词语的所有义项之间的最大值作为两个词语的相似度
            /// </summary>
            /// <param name="word1"></param>
            /// <param name="word2"></param>
            /// <returns></returns>
            public double SimWord(string word1, string word2)
            {
                if (AllWords.ContainsKey(word1) && AllWords.ContainsKey(word2))
                {
                    List<Word> list1 = AllWords[word1];
                    List<Word> list2 = AllWords[word2];
                    double max = 0;
                    Word[] lst1 = new Word[list1.Count];
                    list1.CopyTo(lst1, 0);
                    Word[] lst2 = new Word[list2.Count];
                    list2.CopyTo(lst2, 0);
                    for (int i = 0; i < lst1.Length; i++)
                    {
                        Word w1 = lst1[i];
                        for (int j = 0; j < lst2.Length; j++)
                        {
                            Word w2 = lst2[j];
                            double sim = SimWord(w1, w2);
                            max = (sim > max) ? sim : max;
                        }
                    }
                    return max;
                }
                return 0;

            }

            /// <summary>
            /// 词汇相似度
            /// </summary>
            /// <param name="w1"></param>
            /// <param name="w2"></param>
            /// <returns></returns>
            private double SimWord(Word w1, Word w2)
            {
                // 虚词和实词的相似度为零
                if (w1.IsStructruralWord() != w2.IsStructruralWord())
                {
                    return 0;
                }
                //虚词
                //由于虚词概念总是用“{句法义原}”或“{关系义原}”这两种方式进行描述，所以，虚词概念的相似度计算非常简单，只需要计算其对应的句法义原或关系义原之间的相似度即可。
                if (w1.IsStructruralWord() && w2.IsStructruralWord())
                {
                    List<string> list1 = w1.StructruralWords;
                    List<string> list2 = w2.StructruralWords;
                    return SimList(list1, list2);
                }
                //实词
                if (!w1.IsStructruralWord() && !w2.IsStructruralWord())
                {
                    // 实词的相似度分为4个部分
                    // 基本义原相似度
                    string firstPrimitive1 = w1.FirstPrimitive;
                    string firstPrimitive2 = w2.FirstPrimitive;
                    double sim1 = SimPrimitive(firstPrimitive1, firstPrimitive2);
                    // 其他基本义原相似度
                    List<string> list1 = w1.OtherPrimitives;
                    List<string> list2 = w2.OtherPrimitives;
                    double sim2 = SimList(list1, list2);
                    // 关系义原相似度
                    Dictionary<string, List<string>> dic1 = w1.RelationalPrimitives;
                    Dictionary<string, List<string>> dic2 = w2.RelationalPrimitives;
                    double sim3 = SimDictionary(dic1, dic2);
                    // 关系符号相似度
                    dic1 = w1.RelationSimbolPrimitives;
                    dic2 = w2.RelationSimbolPrimitives;
                    double sim4 = SimDictionary(dic1, dic2);

                    double product = sim1;
                    double sum = Beta1 * product;
                    product *= sim2;
                    sum += Beta2 * product;
                    product *= sim3;
                    sum += Beta3 * product;
                    product *= sim4;
                    sum += Beta4 * product;
                    return sum;
                }
                return 0;
            }

            /// <summary>
            /// 比较两个集合的相似度
            /// </summary>
            /// <param name="list1"></param>
            /// <param name="list2"></param>
            /// <returns></returns>
            public double SimList(List<string> list1, List<string> list2)
            {
                if (list1.Count == 0 && list2.Count == 0)
                    return 1;
                if (list1.Count == 0 || list2.Count == 0)
                    return 0;
                int m = list1.Count;
                int n = list2.Count;
                int big = m > n ? m : n;
                int N = (m < n) ? m : n;
                int count = 0;
                int index1 = 0, index2 = 0;
                double sum = 0;
                double max = 0;
                while (count < N)
                {
                    max = 0;
                    for (int i = 0; i < list1.Count; i++)
                    {
                        for (int j = 0; j < list2.Count; j++)
                        {
                            double sim = InnerSimWord(list1[i], list2[j]);
                            if (sim > max)
                            {
                                index1 = i;
                                index2 = j;
                                max = sim;
                            }
                        }

                    }
                    sum += max;
                    if (list1.Count > index1)
                        list1.RemoveAt(index1);
                    if (list2.Count > index2)
                        list2.RemoveAt(index2);
                    count++;
                }
                return (sum + Delta * (big - N)) / big;
            }

            /// <summary>
            /// 内部比较两个词，可能是为具体词，也可能是义原
            /// </summary>
            /// <param name="word1"></param>
            /// <param name="word2"></param>
            /// <returns></returns>
            private double InnerSimWord(string word1, string word2)
            {
                Primitive p = Primitive.GetInstance();
                bool isPrimitive1 = p.IsPrimitive(word1);
                bool isPrimitive2 = p.IsPrimitive(word2);
                //两个义原
                if (isPrimitive1 && isPrimitive2)
                    return SimPrimitive(word1, word2);
                if (!isPrimitive1 && !isPrimitive2)
                {
                    if (word1.Equals(word2))
                        return 1;
                    return 0;
                }
                // 义原和具体词的相似度, 默认为gamma=0.2
                return Gamma;
            }

            /// <summary>
            /// 计算两个义原之间的相似度
            /// </summary>
            /// <param name="primitive1"></param>
            /// <param name="primitive2"></param>
            /// <returns></returns>
            public double SimPrimitive(string primitive1, string primitive2)
            {
                int dis = DisPrimitive(primitive1, primitive2);
                return Alpha / (dis + Alpha);

            }

            /// <summary>
            /// 计算两个义原之间的距离，如果两个义原层次没有共同节点，则设置他们的距离为20。
            /// </summary>
            /// <param name="primitive1"></param>
            /// <param name="primitive2"></param>
            /// <returns></returns>
            public int DisPrimitive(string primitive1, string primitive2)
            {
                Primitive p = Primitive.GetInstance();
                List<int> list1 = p.GetParents(primitive1);
                List<int> list2 = p.GetParents(primitive2);
                for (int i = 0; i < list1.Count; i++)
                {
                    int id1 = list1[i];
                    if (list2.Contains(id1))
                    {
                        int index = list2.IndexOf(id1);
                        return index + i;//两个义原在树上的节点的路径数
                    }
                }
                return DefaultPrimitiveDis;
            }


            /// <summary>
            /// 特征结构Dictionary的相似度
            /// </summary>
            /// <param name="dic1"></param>
            /// <param name="dic2"></param>
            /// <returns></returns>
            private double SimDictionary(Dictionary<string, List<string>> dic1, Dictionary<string, List<string>> dic2)
            {
                if (dic1.Count == 0 && dic2.Count == 0)//若两个结构都为空相似度为1；
                {
                    return 1;
                }
                int total = dic1.Count + dic2.Count;
                double sim = 0;
                int count = 0;

                foreach (string key in dic1.Keys)
                {
                    System.Console.WriteLine(key);

                    if (dic2.ContainsKey(key))
                    {
                        //如果两个Map中的key相同，就计算两个Map的相似度。
                        List<string> list1 = dic1[key];
                        List<string> list2 = dic2[key];
                        sim += SimList(list1, list2);
                        count++;
                    }
                }
                return (sim + Delta * (total - 2 * count)) / (total - count);

            }

            /// <summary>
            /// 加入一个词语
            /// </summary>
            /// <param name="word"></param>
            public void AddWord(Word word)
            {
                List<Word> list = null;
                string wordString = word.Text;
                if (AllWords.ContainsKey(wordString))
                {
                    list = AllWords[wordString];
                    list.Add(word);
                }
                else
                {
                    list = new List<Word>();
                    list.Add(word);
                    AllWords.Add(word.Text, list);
                }
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
                Explain explain = (Explain)_dictionary[word];
                return string.Join("\n", explain.Information);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取相似词汇
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public List<string> GetSimilarWords(string word)
        {
            return null;
        }

        /// <summary>
        /// 文本语义相似度
        /// 语义-分词-义原
        /// </summary>
        /// <param name="sentence1"></param>
        /// <param name="sentence2"></param>
        /// <returns></returns>
        public double TextSimilarity(string sentence1, string sentence2)
        {
            Iveely.Framework.Text.Segment.IctclasSegment segment = Text.Segment.IctclasSegment.GetInstance();
            List<string> list1 = new List<string>();
            List<string> speech1 = new List<string>();
            List<WordResult[]> results1 = segment.SplitToArray(sentence1);
            for (int i = 0; i < results1.Count; i++)
            {
                for (int j = 1; j < results1[i].Length - 1; j++)
                {
                    list1.Add(results1[i][j].sWord);
                    speech1.Add(Utility.GetPOSString(results1[i][j].nPOS));
                }
            }

            List<string> list2 = new List<string>();
            List<string> speech2 = new List<string>();
            List<WordResult[]> results2 = segment.SplitToArray(sentence2);
            for (int i = 0; i < results2.Count; i++)
            {
                for (int j = 1; j < results2[i].Length - 1; j++)
                {
                    list2.Add(results2[i][j].sWord);
                    speech2.Add(Utility.GetPOSString(results2[i][j].nPOS));
                }
            }

            return SpeechSimilarity(speech1, list1, speech2, list2);
        }

        /// <summary>
        /// 词性逻辑相似度
        /// </summary>
        /// <param name="speech1"></param>
        /// <param name="speech2"></param>
        /// <returns></returns>
        private double SpeechSimilarity(List<string> speech1, List<string> list1, List<string> speech2, List<string> list2)
        {

            //获取1 R-V-N[D]
            List<string> nr1 = new List<string>();
            List<string> v1 = new List<string>();
            List<string> n1 = new List<string>();
            for (int i = 0; i < speech1.Count; i++)
            {
                //人名
                if (speech1[i] == "r")
                {
                    nr1.Add(list1[i]);
                }

                //动词 
                if (speech1[i] == "v")
                {
                    v1.Add(list1[i]);
                }

                //名词
                if (speech1[i] == "n")
                {
                    n1.Add(list1[i]);
                }

            }


            //获取1 R-V-N[D]
            List<string> nr2 = new List<string>();
            List<string> v2 = new List<string>();
            List<string> n2 = new List<string>();
            for (int i = 0; i < speech2.Count; i++)
            {
                //人名
                if (speech2[i] == "r")
                {
                    nr2.Add(list2[i]);
                }

                //动词 
                if (speech2[i] == "v")
                {
                    v2.Add(list2[i]);
                }

                //名词
                if (speech2[i] == "n")
                {
                    n2.Add(list2[i]);
                }

            }

            WordSimilarity similarity = WordSimilarity.GetInstance();

            //人名主题相似度
            double nrSim = similarity.SimList(nr1, nr2);
            double vSim = similarity.SimList(v1, v2);
            double nSim = similarity.SimList(n1, n2);
            return nrSim*0.5 + vSim*0.3 + nSim*0.2;
        }
    }
}
