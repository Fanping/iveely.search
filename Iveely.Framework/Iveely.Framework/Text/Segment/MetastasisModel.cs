using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.Text.Segment
{
    [Serializable]
    public enum WordState
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
    public class Word
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
    public class MetastasisModel
    {
        private Hashtable table = new Hashtable();

        public void Learn(string corpusFolder)
        {
            string[] dirs = Directory.GetDirectories(corpusFolder);
            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    string[] context = File.ReadAllText(file).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < context.Length; i++)
                    {
                        string[] text = context[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        if (text.Length == 2)
                            Analyze(text[0]);
                    }
                }
            }
        }

        public string Split(string sentence)
        {
            //获取概率矩阵
            List<Word> probabilityMatrix = new List<Word>();
            for (int i = 0; i < sentence.Length; i++)
            {
                if (table.ContainsKey(sentence[i].ToString()))
                {
                    Word word = (Word)table[sentence[i].ToString()];
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

            string strResult = "";
            for (int i = 0; i < path.Count; i++)
            {
                strResult += sentence[i];
                if (path[i] == WordState.END || path[i] == WordState.SINGLE)
                {
                    strResult += "/";
                }
            }

            return strResult;
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
            if (table.ContainsKey(keyword))
            {
                Word word = (Word)table[keyword];
                word.AddState(state);
            }
            else
            {
                Word word = new Word();
                word.AddState(state);
                table[keyword] = word;
            }
        }
    }
}
