using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Text.Segment;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 问题类型检测
    /// </summary>
    public class QuestionTypeChecker
    {
        /// <summary>
        /// 问题类型
        /// </summary>
        public string[] types = new[]
        {
            "IsWhat",

            "IsHow",

            "IsWhy",

            "IsWhere",

            "IsWhen",

            "IsWho",

            "IsWhich"
        };

        private static QuestionTypeChecker _checker;

        private readonly ListTable<string> _questionRules;

        public static QuestionTypeChecker GetInstance()
        {
            if (_checker == null)
            {
                _checker = new QuestionTypeChecker();
            }
            return _checker;
        }

        private QuestionTypeChecker()
        {
            _questionRules = new ListTable<string>();
            Learn();
        }

        /// <summary>
        /// 学习规律
        /// </summary>
        private void Learn()
        {
            string[] allLines = File.ReadAllLines("Init\\Corpus_QuestionTypeChecker.txt", Encoding.UTF8);
            foreach (string line in allLines)
            {
                string[] context = line.Split(new[] { ',' }, StringSplitOptions.None);
                if (context.Length == 8)
                {
                    for (int i = 1; i < 8; i++)
                    {
                        if (context[i]=="True")
                        {
                            _questionRules.Add(context[0], types[i - 1]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取问题类型
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<string> GetQuestionTypes(string sentence)
        {
            List<string> qTypes = new List<string>();
            bool isOther = false;
            Framework.Text.Segment.IctclasSegment ictclasSegment = IctclasSegment.GetInstance();
            string[] sentenceSemantic = ictclasSegment.SplitToArray(sentence).Item2;
            if (sentenceSemantic != null && sentenceSemantic.Length > 0)
            {
                string text = string.Join(" ", sentenceSemantic);
               // Regex regex;
                foreach (DictionaryEntry dictionaryEntry in _questionRules)
                {
                    //regex = new Regex(dictionaryEntry.Key.ToString());
                    if (dictionaryEntry.Key.ToString().Contains(text))
                    {
                        qTypes.AddRange(((Iveely.Framework.DataStructure.SortedList<string>) dictionaryEntry.Value).ToArray());
                    }
                }
            }
         
            return qTypes.Distinct().ToList();
        }

#if DEBUG

        /// <summary>
        /// 疑问句提取
        /// </summary>
        public void Debug_GetQuestionFromQuery()
        {
            //Framework.Text.Segment.IctclasSegment ictclasSegment = Framework.Text.Segment.IctclasSegment.GetInstance();
            //Framework.NLP.QuestionTypeChecker checker = Framework.NLP.QuestionTypeChecker.GetInstance();
            //string[] allLines = File.ReadAllLines("Query.txt", Encoding.UTF8);
            //StringBuilder builder = new StringBuilder();
            //int known = 0;
            //int unknown = 0;
            //int index = 0;
            //foreach (string line in allLines)
            //{
            //    if (index++ % 10000 == 0)
            //    {
            //        Console.WriteLine(index);
            //    }
            //    List<Framework.NLP.QuestionTypeChecker.Type> types = checker.GetQuestionTypes(line);

            //    if (types[0] != Framework.NLP.QuestionTypeChecker.Type.Unknown)
            //    {
            //        known++;
            //        string result = string.Join(" ", types.ToArray());
            //        builder.AppendLine(ictclasSegment.SplitToString(line) + " " + result);
            //    }
            //    else
            //    {
            //        unknown++;
            //    }

            //}

            //File.WriteAllText("question_extract.txt", builder.ToString(), Encoding.UTF8);
            //Console.WriteLine("Known:" + known + "  Unknown:" + unknown);
            //Console.ReadLine();
        }

        /// <summary>
        /// 疑问句类型分析
        /// </summary>
        public void Debug_GetQuestionType()
        {
            //Framework.Text.Segment.IctclasSegment ictclasSegment = Framework.Text.Segment.IctclasSegment.GetInstance();
            //Framework.NLP.QuestionTypeChecker checker = Framework.NLP.QuestionTypeChecker.GetInstance();
            //string[] allLines = File.ReadAllLines("Init\\question.txt", Encoding.UTF8);
            //StringBuilder builder = new StringBuilder();
            //int known = 0;
            //int unknown = 0;
            //foreach (string line in allLines)
            //{
            //    List<Framework.NLP.QuestionTypeChecker.Type> types = checker.GetQuestionTypes(line);

            //    if (types[0] != Framework.NLP.QuestionTypeChecker.Type.Unknown)
            //    {
            //        known++;
            //    }
            //    else
            //    {
            //        unknown++;
            //    }

            //    string result = string.Join(" ", types.ToArray());
            //    builder.AppendLine(ictclasSegment.splitWithSemantic(line) + " " + result + "                                   " + ictclasSegment.SplitToGetSemantic(line));

            //}

            //File.WriteAllText("question_checker.txt", builder.ToString(), Encoding.UTF8);
            //Console.WriteLine("Known:" + known + "  Unknown:" + unknown);
        }

#endif
    }
}
