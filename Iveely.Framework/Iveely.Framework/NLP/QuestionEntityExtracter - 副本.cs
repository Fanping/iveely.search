/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 问题中的实体抽取
    /// </summary>
    public class QuestionEntityExtracter
    {
        ///<summary>
        /// 问题类型
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// 问什么类型的问题
            /// </summary>
            What,

            /// <summary>
            /// 问地点类型的问题
            /// </summary>
            Where,

            /// <summary>
            /// 问世间类型的问题
            /// </summary>
            When,

            /// <summary>
            /// 问为什么类型的问题
            /// </summary>
            Why,

            /// <summary>
            /// 问人物类型的问题
            /// </summary>
            Who,

            /// <summary>
            /// 问怎么类型的问题
            /// </summary>
            How,

            /// <summary>
            /// 其它复合疑问句
            /// </summary>
            Other,

            //不是疑问句
            Unknown
        }

        /// <summary>
        /// 提取匹配模式
        /// </summary>
        private static List<InformationExtracter.ExtractPattern> _extractPatterns;

        /// <summary>
        /// 单例对象
        /// </summary>
        private static QuestionEntityExtracter _questionExtrator;

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static QuestionEntityExtracter GetInstance()
        {
            if (_questionExtrator == null)
            {
                _questionExtrator = new QuestionEntityExtracter();
            }
            return _questionExtrator;
        }

        private QuestionEntityExtracter()
        {
            _extractPatterns = new List<InformationExtracter.ExtractPattern>();

            //分析所有问题的风格
            LoadCorpusPattern("Init\\Corpus_Question_Style.txt");
        }

        /// <summary>
        /// 从语料库分析问题的风格
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadCorpusPattern(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName, Encoding.UTF8);
            foreach (string line in lines)
            {
                string[] text = line.Split(new[] { "  ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (text.Length == 3)
                {
                    string[] vals =
                    text[0].Split(new[] { "/", " " }, StringSplitOptions.RemoveEmptyEntries);
                    string patternStr = string.Empty;
                    for (int i = 1; i < vals.Length; i = i + 2)
                    {
                        patternStr += vals[i] + " ";
                    }
                    InformationExtracter.ExtractPattern pattern =
                      new InformationExtracter.ExtractPattern(patternStr, text[1]);
                    pattern.Type = text[2];
                    _extractPatterns.Add(pattern);
                }
            }
        }

        /// <summary>
        /// 对问句提取信息
        /// </summary>
        /// <param name="questionStr">疑问句</param>
        /// <returns></returns>
        public List<string[]> GetInforByPattern(string questionStr)
        {
            List<string[]> allResults = new List<string[]>();
            var ictclasSegment = Text.Segment.IctclasSegment.GetInstance();
            Tuple<string[], string[]> tuple = ictclasSegment.SplitToArray(questionStr);
            for (int i = 0; i < tuple.Item2.Length; i++)
            {
                string nextSemantic;
                if (i < tuple.Item2.Length - 1)
                {
                    nextSemantic = tuple.Item2[i + 1];
                }
                else
                {
                    nextSemantic = string.Empty;
                }
                foreach (var extractPattern in _extractPatterns)
                {
                    extractPattern.Check(tuple.Item2[i], tuple.Item1[i]);
                    if (nextSemantic != tuple.Item2[i] && extractPattern.IsMatch())
                    {
                        string[] results = extractPattern.GetResult();
                        extractPattern.Recover();
                        allResults.Add(new[] { results[0], results[1], results[2], extractPattern.Type });

                    }
                }
            }

            foreach (var extractPattern in _extractPatterns)
            {
                string[] results = extractPattern.GetResult(true);
                if (results != null)
                {
                    allResults.Add(new[] { results[0], results[1], results[2], extractPattern.Type });
                }
                extractPattern.Recover();
            }
            return allResults;
        }

#if DEBUG
        public void Debug_GetQuestionSemantic()
        {
            if (File.Exists("question_semantic_result.txt"))
            {
                File.Delete("question_semantic_result.txt");
            }
            string[] lines = File.ReadAllLines("question_extract.txt", Encoding.UTF8);
            Hashtable table = new Hashtable();
            foreach (var line in lines)
            {
                if (!table.ContainsKey(line))
                {
                    table.Add(line, "");
                }
            }

            StringBuilder builder = new StringBuilder();
            foreach (DictionaryEntry dictionaryEntry in table)
            {
                builder.AppendLine(
                    Text.Segment.IctclasSegment.GetInstance().splitWithSemantic(dictionaryEntry.Key.ToString()));
            }
            File.WriteAllText("question_semantic_result.txt", builder.ToString(), Encoding.UTF8);
        }

        public void Debug_GetQuestionSemantic_SingleSemantic()
        {
            string[] lines = File.ReadAllLines("question_extract.txt", Encoding.UTF8);
            Hashtable table = new Hashtable();
            foreach (var line in lines)
            {
                if (!table.ContainsKey(line))
                {
                    table.Add(line, "");
                }
            }

            StringBuilder builder = new StringBuilder();
            HashSet<string> semantics = new HashSet<string>();
            foreach (DictionaryEntry dictionaryEntry in table)
            {
                Tuple<string[], string[]> tuple = Text.Segment.IctclasSegment.GetInstance().SplitToArray(dictionaryEntry.Key.ToString());
                string semanticstr = string.Join(" ", tuple.Item2);
                if (!semantics.Contains(semanticstr))
                {
                    semantics.Add(semanticstr);
                    builder.AppendLine(
                        Text.Segment.IctclasSegment.GetInstance().splitWithSemantic(dictionaryEntry.Key.ToString()));
                }
            }
            File.WriteAllText("question_single_semantic_result.txt", builder.ToString(), Encoding.UTF8);
        }
#endif
    }
}
