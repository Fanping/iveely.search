/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.CloudComputing.Client;
using Iveely.Framework.Algorithm;
using Iveely.Framework.Algorithm.AI;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Log;
using Iveely.Framework.Text;

namespace Iveely.SearchEngine
{
    /// <summary>
    /// 问题提取器
    /// </summary>
    public class QuestionGetter
    {
        public class QuestionEntity
        {
            /// <summary>
            /// 实体A
            /// </summary>
            public string EntityA { get; set; }

            /// <summary>
            /// 实体B
            /// </summary>
            public string EntityB { get; set; }

            /// <summary>
            /// 关系
            /// </summary>
            public string Relation { get; set; }

            /// <summary>
            /// 问题描述
            /// </summary>
            public string QuestionDesc { get; set; }

            /// <summary>
            /// 问题答案
            /// </summary>
            public string Answer { get; set; }

            /// <summary>
            /// 知识匹配模板
            /// </summary>
            public string KnowledgePattern { get; set; }

            public override string ToString()
            {
                return string.Format("EntityA:{0}  Relation:{1} EntityB:{2} \n Question={3}  Key={4} \n pattern:{5}", EntityA,
                     Relation, EntityB, QuestionDesc, Answer, KnowledgePattern);
            }
        }

        /// <summary>
        /// 分词组件
        /// </summary>
        private Iveely.Framework.Text.HMMSegment segment;

        /// <summary>
        /// 知识匹配模式集合
        /// </summary>
        private List<string> knowledgePatterns = new List<string>();

        /// <summary>
        /// 知识匹配模式集合
        /// </summary>
        private List<string> questionPatterns = new List<string>();

        public QuestionGetter()
        {
            segment = Iveely.Framework.Text.HMMSegment.GetInstance();
            string[] lines = File.ReadAllLines("Init\\pattern.txt");
            foreach (var line in lines)
            {
                string[] str = line.Split(new string[] { " -- " }, StringSplitOptions.RemoveEmptyEntries);
                if (str.Length == 2)
                {
                    knowledgePatterns.Add(str[0].Trim());
                    questionPatterns.Add(str[1].Trim());
                }
            }
        }

        /// <summary>
        /// 获取本例中包含的知识图
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<QuestionEntity> GetKnowledge(string text)
        {
            List<QuestionEntity> result = new List<QuestionEntity>();
            try
            {
                string[] lines = text.Split(new string[] { ".", "。", ",", "，", "\r\n", "\n", "!" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    Tuple<string[], string[]> tuple = segment.SplitToArray(line);
                    for (int i = 0; i < knowledgePatterns.Count; i++)
                    {
                        string entityA = "";
                        string relation = "";
                        string entityB = "";
                        string[] entities = knowledgePatterns[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int index = 0;
                        bool flag = false;
                        int current = 0;
                        List<string> resultList = new List<string>();
                        for (int j = 0; j < tuple.Item1.Length; j++)
                        {
                            if (entities.Length == index)
                            {
                                entityB = string.Join("", resultList.ToArray());
                                QuestionEntity entity = new QuestionEntity();
                                entity.EntityA = entityA;
                                entity.EntityB = entityB;
                                entity.Relation = relation;
                                entity.KnowledgePattern = knowledgePatterns[i];
                                string[] qus = questionPatterns[i].Replace("[1]", entityA)
                                    .Replace("[2]", relation)
                                    .Replace("[3]", entityB).Split(new[] { "key=" }, StringSplitOptions.RemoveEmptyEntries);
                                entity.QuestionDesc = qus[0];
                                entity.Answer = qus[1];
                                result.Add(entity);
                                break;
                            }
                            if (entities[index] == "*")
                            {
                                if (current == 1)
                                {
                                    entityA = string.Join("", resultList.ToArray());
                                }
                                if (current == 2)
                                {
                                    relation = string.Join("", resultList.ToArray());
                                }
                                resultList.Clear();
                                current++;
                                index++;
                            }
                            if (tuple.Item2[j] == entities[index])
                            {
                                resultList.Add(tuple.Item1[j]);
                                flag = true;
                                index++;
                            }
                            if (index < entities.Length && j < tuple.Item2.Length - 1 &&
                                tuple.Item2[j + 1] != entities[index] && entities[index] != "*" && flag && resultList.Count > 0)
                            {
                                resultList.RemoveAt(resultList.Count - 1);
                                index--;
                                flag = false;
                            }
                        }
                        if (entityB == string.Empty && entityA != string.Empty && relation != string.Empty &&
                            resultList.Count > 0)
                        {
                            entityB = string.Join("", resultList.ToArray());
                            QuestionEntity entity = new QuestionEntity();
                            entity.EntityA = entityA;
                            entity.EntityB = entityB;
                            entity.Relation = relation;
                            entity.KnowledgePattern = knowledgePatterns[i];
                            string[] qus = questionPatterns[i].Replace("[1]", entityA)
                                .Replace("[2]", relation)
                                .Replace("[3]", entityB).Split(new[] { "key=" }, StringSplitOptions.RemoveEmptyEntries);
                            entity.QuestionDesc = qus[0];
                            entity.Answer = qus[1];
                            result.Add(entity);
                        }
                    }
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            return result;
        }

        /// <summary>
        /// 获取可以解答的问题集合
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<Tuple<string, string>> GetQuestion(string text)
        {
            return null;
        }
    }
}
