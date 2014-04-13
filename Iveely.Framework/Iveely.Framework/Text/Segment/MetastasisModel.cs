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

        [Serializable]
        private class SemManager
        {
            public IntTable<string, int> table
            {
                get;
                private set;
            }

            public void Add(string obj)
            {
                table.Add(obj, 1, true);
            }

            public SemManager()
            {
                table = new IntTable<string, int>();
            }
        }

        /// <summary>
        /// 每个词拥有的词性集合
        /// </summary>
        private Hashtable wordSemantic = new Hashtable();

        /// <summary>
        /// 词性关联关系集合
        /// </summary>
        private Hashtable semanticRelation = new Hashtable();

        public void AddNextSemantic(string fromSem, string toSem)
        {
            if (semanticRelation.ContainsKey(fromSem))
            {
                SemManager semManager = (SemManager)semanticRelation[fromSem];
                semManager.Add(toSem);
            }
            else
            {
                SemManager semManager = new SemManager();
                semManager.Add(toSem);
                semanticRelation[fromSem] = semManager;
            }
        }

        public void AddWordSemantic(string word, string semantic)
        {
            if (wordSemantic.ContainsKey(word))
            {
                SemManager semManager = (SemManager)wordSemantic[word];
                semManager.Add(semantic);
            }
            else
            {
                SemManager semManager = new SemManager();
                semManager.Add(semantic);
                wordSemantic[word] = semManager;
            }
        }

        /// <summary>
        /// 获取词性序列
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public string[] GetSemanticSeq(string[] words)
        {
            List<string> semResult = new List<string>();
            string lastSem = string.Empty;
            for (int i = 0; i < words.Length; i++)
            {
                string[] sems = SortSem(words[i]);
                if (sems == null)
                {
                    semResult.Add("Unknow");
                }
                else
                {
                    if (i == 0)
                    {
                        lastSem = sems[0];
                        semResult.Add(lastSem);
                    }
                    else
                    {
                        if (semanticRelation.ContainsKey(lastSem))
                        {
                            SemManager semManager = (SemManager)semanticRelation[lastSem];
                            bool isHere = false;
                            foreach (var sem in sems)
                            {
                                if (semManager.table.ContainsKey(sem))
                                {
                                    lastSem = sem;
                                    semResult.Add(lastSem);
                                    isHere = true;
                                    break;
                                }
                            }
                            if (!isHere)
                            {
                                lastSem = sems[0];
                                semResult.Add(lastSem);
                            }
                        }
                        else
                        {
                            lastSem = sems[0];
                            semResult.Add(lastSem);
                        }
                    }
                }
            }
            return semResult.ToArray();
        }

        private string[] SortSem(string word)
        {
            if (!wordSemantic.ContainsKey(word))
            {
                return null;
            }
            SemManager semManager = (SemManager)wordSemantic[word];
            List<string> result = new List<string>();
            ArrayList list = new ArrayList(semManager.table.Values);
            list.Sort();
            list.Reverse();
            foreach (int svalue in list)
            {
                IDictionaryEnumerator ide = semManager.table.GetEnumerator();
                while (ide.MoveNext())
                {
                    if ((int)ide.Value == svalue)
                    {
                        result.Add(ide.Key.ToString());
                    }
                }
            }
            return result.ToArray();
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
                            _semantic.AddWordSemantic(text[0], text[1]);

                            //词性与词性
                            if (lastSem != string.Empty)
                            {
                                _semantic.AddNextSemantic(lastSem, text[1]);
                            }
                            lastSem = text[1];
                        }
                    }
                }
            }
        }

        public string[] SplitToStrings(string sentence)
        {
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
            string[] results = this.SplitToStrings(sentence);
            return string.Join(delimeter, results);
        }

        public Tuple<string[], string[]> SplitToArray(string sentence)
        {
            string[] words = this.SplitToStrings(sentence);
            string[] sems = _semantic.GetSemanticSeq(words);
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
    }
}
