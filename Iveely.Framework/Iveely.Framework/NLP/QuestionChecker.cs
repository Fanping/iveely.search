using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Iveely.Framework.Text.Segment;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 问题检测
    /// </summary>
    public class QuestionChecker
    {
        /// <summary>
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

        private static QuestionChecker _checker;

        private Hashtable _questionRules;

        public static QuestionChecker GetInstance()
        {
            if (_checker == null)
            {
                _checker = new QuestionChecker();
            }
            return _checker;
        }

        private QuestionChecker()
        {
            _questionRules = new Hashtable();
            Learn();
        }

        /// <summary>
        /// 学习规律
        /// </summary>
        private void Learn()
        {
            string[] allLines = File.ReadAllLines("Init\\QuestionRule.txt", Encoding.UTF8);
            foreach (string line in allLines)
            {
                string[] context = line.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (context.Length == 3)
                {
                    _questionRules.Add(context[0],(Type)int.Parse(context[1]));
                }
            }
        }

        /// <summary>
        /// 获取问题类型
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<Type> GetQuestionTypes(string sentence)
        {
            List<Type> qTypes = new List<Type>();
            bool isOther = false;
            Framework.Text.Segment.IctclasSegment ictclasSegment = IctclasSegment.GetInstance();
            string[] sentenceSemantic= ictclasSegment.SplitToArray(sentence).Item2;
            if (sentenceSemantic != null && sentenceSemantic.Length > 0)
            {
                string text = string.Join(" ", sentenceSemantic);
                Regex regex;
                foreach (DictionaryEntry dictionaryEntry in _questionRules)
                {
                    regex = new Regex(dictionaryEntry.Key.ToString());
                    if (regex.IsMatch(text))
                    {
                        Type type = (Type)dictionaryEntry.Value;
                        if (type != Type.Other)
                        {
                            qTypes.Add(type);
                        }
                        else
                        {
                            isOther = true;
                        }
                    }
                }
            }
            if (qTypes.Count == 0 && isOther)
            {
                qTypes.Add(Type.Other);
            }
            if (qTypes.Count == 0)
            {
                qTypes.Add(Type.Unknown);
            }
            return qTypes;
        }

#if DEBUG

        /// <summary>
        /// 疑问句提取
        /// </summary>
        public void Debug_GetQuestionFromQuery()
        {
            Framework.Text.Segment.IctclasSegment ictclasSegment = Framework.Text.Segment.IctclasSegment.GetInstance();
            Framework.NLP.QuestionChecker checker = Framework.NLP.QuestionChecker.GetInstance();
            string[] allLines = File.ReadAllLines("Query.txt", Encoding.UTF8);
            StringBuilder builder = new StringBuilder();
            int known = 0;
            int unknown = 0;
            int index = 0;
            foreach (string line in allLines)
            {
                if (index++ % 10000 == 0)
                {
                    Console.WriteLine(index);
                }
                List<Framework.NLP.QuestionChecker.Type> types = checker.GetQuestionTypes(line);

                if (types[0] != Framework.NLP.QuestionChecker.Type.Unknown)
                {
                    known++;
                    string result = string.Join(" ", types.ToArray());
                    builder.AppendLine(ictclasSegment.SplitToString(line) + " " + result);
                }
                else
                {
                    unknown++;
                }

            }

            File.WriteAllText("question_extract.txt", builder.ToString(), Encoding.UTF8);
            Console.WriteLine("Known:" + known + "  Unknown:" + unknown);
            Console.ReadLine();
        }

        /// <summary>
        /// 疑问句类型分析
        /// </summary>
        public void Debug_GetQuestionType()
        {
            Framework.Text.Segment.IctclasSegment ictclasSegment = Framework.Text.Segment.IctclasSegment.GetInstance();
            Framework.NLP.QuestionChecker checker = Framework.NLP.QuestionChecker.GetInstance();
            string[] allLines = File.ReadAllLines("Init\\question.txt", Encoding.UTF8);
            StringBuilder builder = new StringBuilder();
            int known = 0;
            int unknown = 0;
            foreach (string line in allLines)
            {
                List<Framework.NLP.QuestionChecker.Type> types = checker.GetQuestionTypes(line);

                if (types[0] != Framework.NLP.QuestionChecker.Type.Unknown)
                {
                    known++;
                }
                else
                {
                    unknown++;
                }

                string result = string.Join(" ", types.ToArray());
                builder.AppendLine(ictclasSegment.splitWithSemantic(line) + " " + result + "                                   " + ictclasSegment.SplitToGetSemantic(line));

            }

            File.WriteAllText("question_checker.txt", builder.ToString(), Encoding.UTF8);
            Console.WriteLine("Known:" + known + "  Unknown:" + unknown);
        }

#endif
    }
}
