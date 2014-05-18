using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.DataStructure;

namespace Iveely.Framework.Text.Segment
{
    [Serializable]
    internal enum WordState
    {
        //词头
        HEAD,

        //词中
        MIDDLE,

        //词尾
        END,

        //单字成词
        SINGLE
    }

    [Serializable]
    internal class Semantic
    {

        /// <summary>
        /// 点边集合
        /// </summary>
        protected Hashtable points;

        private WordSemantic wordSema;

        /// <summary>
        /// 边定义
        /// </summary>
        [Serializable]
        internal class Edge : IComparable
        {
            public string From { get; private set; }

            public string To { get; private set; }

            public int Weight { get; set; }


            public Edge(string from, string to)
            {
                this.From = from;
                this.To = to;
                this.Weight = 1;
            }

            public override string ToString()
            {
                return From + "|" + To;
            }

            public int CompareTo(object obj)
            {
                if (((Edge)obj).Weight > this.Weight)
                {
                    return 1;
                }
                return 0;
            }
        }

        public Semantic()
        {
            points = new Hashtable();
        }

        public virtual void Add(string from, string to)
        {
            // 边
            if (points.ContainsKey(from))
            {
                List<Edge> cedges = (List<Edge>)points[from];
                bool hasFind = false;
                foreach (var cedge in cedges)
                {
                    if (cedge.To == to)
                    {
                        cedge.Weight++;
                        hasFind = true;
                        cedges.Sort();
                        break;
                    }
                }
                if (!hasFind)
                {
                    Edge edge = new Edge(from, to);
                    cedges.Add(edge);
                }
            }
            else
            {
                List<Edge> cedges = new List<Edge>();
                Edge edge = new Edge(from, to);
                cedges.Add(edge);
                points.Add(from, cedges);
            }
        }

        public List<string> GetFollows(string from)
        {
            List<string> result = new List<string>();
            if (points.ContainsKey(from))
            {
                List<Edge> edges = (List<Edge>)points[from];
                foreach (var edge in edges)
                {
                    result.Add(edge.To);
                }
                return result;
            }
            return result;
        }
    }

    [Serializable]
    internal class WordSemantic : Semantic
    {
        public override void Add(string @from, string to)
        {
            if (points.ContainsKey(from))
            {
                List<Edge> cedges = (List<Edge>)points[from];
                bool hasFind = false;
                foreach (var cedge in cedges)
                {
                    if (cedge.To == to)
                    {
                        cedge.Weight++;
                        hasFind = true;
                        cedges.Sort();
                        break;
                    }
                }
                if (!hasFind)
                {
                    Edge edge = new Edge(from, to);
                    cedges.Add(edge);
                }
            }
            else
            {
                List<Edge> cedges = new List<Edge>();
                Edge edge = new Edge(from, to);
                cedges.Add(edge);
                points.Add(from, cedges);
            }
        }
    }

    [Serializable]
    internal class Word
    {
        private Hashtable table = new Hashtable();

        public int this[WordState state]
        {
            get
            {
                if (table.ContainsKey(state))
                {
                    return (int)table[state];
                }
                return 0;
            }
            private set { }
        }

        public void AddState(WordState state)
        {
            if (table.Contains(state))
            {
                table[state] = (int)table[state] + 1;
            }
            else
            {
                table[state] = 1;
            }
        }

        public bool IsContains(string word)
        {
            return table.ContainsKey(word);
        }

        public List<WordState> SortStateCount()
        {
            List<WordState> result = new List<WordState>();
            ArrayList list = new ArrayList(table.Values);
            list.Sort();
            list.Reverse();
            foreach (int svalue in list)
            {
                IDictionaryEnumerator ide = table.GetEnumerator();
                while (ide.MoveNext())
                {
                    if ((int)ide.Value == svalue)
                    {
                        result.Add((WordState)ide.Key);
                    }
                }
            }
            return result;
        }

    }

    [Serializable]
    internal class MetastasisModel
    {
        /// <summary>
        /// 词
        /// </summary>
        private readonly Hashtable _table = new Hashtable();

        /// <summary>
        /// 词性
        /// </summary>
        private readonly Semantic _semantic = new Semantic();

        private readonly WordSemantic _wordSemantic = new WordSemantic();

        public void Learn(string corpusFolder)
        {
            string[] dirs = Directory.GetDirectories(corpusFolder);
            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    string[] context = File.ReadAllText(file).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string lastSem = string.Empty;
                    for (int i = 0; i < context.Length; i++)
                    {
                        string[] text = context[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                        if (text.Length == 2)
                        {
                            //分词训练
                            Analyze(text[0]);

                            //词与词性
                            _wordSemantic.Add(text[0], text[1]);

                            //词性与词性
                            if (lastSem != string.Empty)
                            {
                                _semantic.Add(lastSem, text[1]);
                            }
                            lastSem = text[1];
                        }
                    }
                }
            }
        }

        public string[] SplitToStrings(string sentence)
        {
            return DicSplit.GetInstance().Do(sentence);
            //获取概率矩阵
            List<Word> probabilityMatrix = new List<Word>();
            for (int i = 0; i < sentence.Length; i++)
            {
                if (_table.ContainsKey(sentence[i].ToString()))
                {
                    Word word = (Word)_table[sentence[i].ToString()];
                    probabilityMatrix.Add(word);
                }
                else
                {
                    probabilityMatrix.Add(null);
                }
            }

            //最佳路径计算(计算矩阵的最佳路线,暴力法)
            List<WordState> path = new List<WordState>();
            WordState[] states = new WordState[4];
            states[0] = WordState.SINGLE;
            states[1] = WordState.MIDDLE;
            states[2] = WordState.HEAD;
            states[3] = WordState.END;
            WordState lastState = WordState.END;
            for (int j = 0; j < probabilityMatrix.Count; j++)
            {
                if (probabilityMatrix[j] == null)
                {
                    path.Add(WordState.END);
                    continue;
                }
                List<WordState> currentStates = probabilityMatrix[j].SortStateCount();
                for (int i = 0; i < currentStates.Count; i++)
                {
                    if (currentStates[i] == WordState.SINGLE || currentStates[i] == WordState.HEAD)
                    {
                        if (lastState == WordState.END || lastState == WordState.SINGLE)
                        {
                            path.Add(currentStates[i]);
                            lastState = currentStates[i];
                            break;
                        }
                    }
                    else //if (currentStates[i] == State.MIDDLE || currentStates[i] == State.END)
                    {
                        if (lastState == WordState.HEAD || lastState == WordState.MIDDLE)
                        {
                            path.Add(currentStates[i]);
                            lastState = currentStates[i];
                            break;
                        }
                    }
                }
            }

            List<string> resultList = new List<string>();
            string strResult = "";
            for (int i = 0; i < path.Count; i++)
            {
                strResult += sentence[i];
                if (path[i] == WordState.END || path[i] == WordState.SINGLE)
                {
                    resultList.Add(strResult);
                    strResult = "";
                }
            }

            return resultList.ToArray();
        }

        public string Split(string sentence, string delimeter)
        {
            string[] results = DicSplit.GetInstance().Do(sentence); ;//this.SplitToStrings(sentence);
            return string.Join(delimeter, results);
        }

        public Tuple<string[], string[]> SplitToArray(string sentence)
        {
            string[] words = DicSplit.GetInstance().Do(sentence);//this.SplitToStrings(sentence);
            string[] sems = GetSemanticSeq(words);
            Tuple<string[], string[]> tuple = new Tuple<string[], string[]>(words, sems);
            return tuple;
        }

        private void Analyze(string words)
        {
            if (words.Length == 1)
            {
                BuildWords(words, WordState.SINGLE);
            }
            else if (words.Length == 2)
            {
                BuildWords(words[0].ToString(), WordState.HEAD);
                BuildWords(words[1].ToString(), WordState.END);
            }
            else
            {
                BuildWords(words[0].ToString(), WordState.HEAD);
                for (int i = 0; i < words.Length - 1; i++)
                {
                    BuildWords(words[i].ToString(), WordState.MIDDLE);
                }
                BuildWords(words[words.Length - 1].ToString(), WordState.END);
            }
        }

        private void BuildWords(string keyword, WordState state)
        {
            if (_table.ContainsKey(keyword))
            {
                Word word = (Word)_table[keyword];
                word.AddState(state);
            }
            else
            {
                Word word = new Word();
                word.AddState(state);
                _table[keyword] = word;
            }
        }

        private string[] GetSemanticSeq(string[] words)
        {
            List<string> result = new List<string>();
            foreach (var word in words)
            {
                List<string> list = _wordSemantic.GetFollows(word);
                if (list.Count > 0)
                {
                    result.Add(list[0]);
                }
                else
                {
                    result.Add("Unkown");
                }
            }
            return result.ToArray();
        }
    }
}
