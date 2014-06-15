using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Iveely.Data;
using Iveely.Database;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Text;

namespace Iveely.SearchEngine
{
    public class KnowlegeIndex
    {
        public class KnowledgeEntity
        {

            /// <summary>
            /// 编号
            /// </summary>
            public long Id { get; set; }

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
            /// 参考来自
            /// </summary>
            public string RefUrl { get; set; }

            /// <summary>
            /// 有效期
            /// </summary>
            public string EffectTime { get; set; }
        }

        /// <summary>
        /// 索引信息
        /// </summary>
        public class KeywordIndex
        {
            public string Keyword;
            public long Id;
            public double Weight;
        }

        /// <summary>
        /// 分词组件
        /// </summary>
        private static HMMSegment _segment;

        /// <summary>
        /// 问题提取
        /// </summary>
        private readonly QuestionGetter _questionGetter;

        /// <summary>
        /// 实体集
        /// </summary>
        private readonly List<KnowledgeEntity> entities;

        /// <summary>
        /// 最长长度
        /// </summary>
        private const int Maxcount = 100;

        /// <summary>
        /// 当前编号
        /// </summary>
        private long _currentId;

        /// <summary>
        /// 临时存放文本索引数据
        /// </summary>
        private static readonly List<KeywordIndex> Indexs = new List<KeywordIndex>();

        public KnowlegeIndex()
        {
            _questionGetter = new QuestionGetter();
            entities = new List<KnowledgeEntity>();
            _segment = HMMSegment.GetInstance();
        }

        public void Start()
        {
            string dataPath = "Baike\\Baike_data.db4";
            using (IStorageEngine engine = STSdb.FromFile(dataPath))
            {
                // 1.提取数据
                ITable<string, BaikeDataCrawler.Page> table = engine.OpenXTable<string, BaikeDataCrawler.Page>("WebPage");
                long totalCount = table.Count();
                foreach (var keyValuePair in table)
                {
                    Console.WriteLine(totalCount--);
                    BaikeDataCrawler.Page page = keyValuePair.Value;

                    // 2.提取问题
                    List<QuestionGetter.QuestionEntity> questionEntities = _questionGetter.GetKnowledge(page.Content);
                    if (questionEntities != null && questionEntities.Count > 0)
                    {
                        foreach (var questionEntity in questionEntities)
                        {
                            KnowledgeEntity entity = new KnowledgeEntity();
                            entity.EntityA = questionEntity.EntityA;
                            entity.EntityB = questionEntity.EntityB;
                            entity.QuestionDesc = questionEntity.QuestionDesc;
                            entity.Answer = questionEntity.Answer;
                            entity.Relation = questionEntity.Relation;
                            entity.EffectTime = DateTime.Now.ToShortDateString();
                            entity.RefUrl = page.Url;
                            entity.Id = _currentId++;
                            entities.Add(entity);
                        }
                    }

                    // 3.存储数据
                    if (entities.Count > Maxcount)
                    {
                        IEnumerable<KnowledgeEntity> ces = entities.Distinct();
                        if (ces.Count() > 0)
                            InsertEntity(ces);
                        entities.Clear();
                    }
                }
            }

            if (entities != null && entities.Count > 0)
            {
                IEnumerable<KnowledgeEntity> ces = entities.Distinct();
                if (ces.Count() > 0)
                    InsertEntity(ces);
            }
            Console.ReadLine();
        }

        private void InsertEntity(IEnumerable<KnowledgeEntity> ces)
        {
            string dataPath = "Baike\\Baike_question.db4";
            using (IStorageEngine engine = STSdb.FromFile(dataPath))
            {
                ITable<long, KnowledgeEntity> table = engine.OpenXTable<long, KnowledgeEntity>("WebPage");
                foreach (var knowledgeEntity in ces)
                {
                    table[knowledgeEntity.Id] = knowledgeEntity;
                    InsertIndex(knowledgeEntity.Id, knowledgeEntity.QuestionDesc);
                }
                engine.Commit();
            }
        }

        private void InsertIndex(long id, string text)
        {
            string dataPath = "Baike\\Baike_question_index.db4";
            var frequency = new IntTable<string, int>();
            string[] results = _segment.Split(text);
            if (results.Length < 1)
            {
                return;
            }
            frequency.Add(results);
            foreach (DictionaryEntry de in frequency)
            {
                KeywordIndex keywordIndex = new KeywordIndex();
                keywordIndex.Keyword = de.Key.ToString();
                keywordIndex.Weight = int.Parse(de.Value.ToString()) * 1.0 / results.Length;
                keywordIndex.Id = id;
                Indexs.Add(keywordIndex);
            }
            if (Indexs.Count > 0)
            {
                using (IStorageEngine engine = STSdb.FromFile(dataPath))
                {
                    ITable<string, List<Slots<long, double>>> table = engine.OpenXTable<string, List<Slots<long, double>>>("WebPage");
                    foreach (var keywordIndex in Indexs)
                    {
                        // 如果包含则追加
                        List<Slots<long, double>> list = table.Find(keywordIndex.Keyword);
                        if (list != null && list.Count > 0)
                        {
                            Slots<long, double> slot = new Slots<long, double>(keywordIndex.Id, keywordIndex.Weight);
                            list.Add(slot);
                        }
                        // 否则新增
                        else
                        {
                            list = new List<Slots<long, double>>();
                            Slots<long, double> slot = new Slots<long, double>(keywordIndex.Id, keywordIndex.Weight);
                            list.Add(slot);
                            table[keywordIndex.Keyword] = list;
                        }
                    }
                    engine.Commit();
                }
                Indexs.Clear();
            }
        }
    }
}
