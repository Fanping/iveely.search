using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.Data;
using Iveely.Database;
using Iveely.Framework.DataStructure;

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
        private static Iveely.Framework.Text.HMMSegment segment;

        /// <summary>
        /// 问题提取
        /// </summary>
        private QuestionGetter questionGetter;

        /// <summary>
        /// 实体集
        /// </summary>
        private List<KnowledgeEntity> entities;

        /// <summary>
        /// 最长长度
        /// </summary>
        private const int MAXCOUNT = 100;

        /// <summary>
        /// 当前编号
        /// </summary>
        private long currentId = 0;

        /// <summary>
        /// 临时存放文本索引数据
        /// </summary>
        private static List<KeywordIndex> indexs = new List<KeywordIndex>();

        public KnowlegeIndex()
        {
            questionGetter = new QuestionGetter();
            entities = new List<KnowledgeEntity>();
            segment = Iveely.Framework.Text.HMMSegment.GetInstance();
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
                    BaikeDataCrawler.Page page = (BaikeDataCrawler.Page)keyValuePair.Value;

                    // 2.提取问题
                    List<QuestionGetter.QuestionEntity> questionEntities = questionGetter.GetKnowledge(page.Content);
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
                            entity.EffectTime = System.DateTime.Now.ToShortDateString();
                            entity.RefUrl = page.Url;
                            entity.Id = currentId++;
                            entities.Add(entity);
                        }
                    }

                    // 3.存储数据
                    if (entities.Count > MAXCOUNT)
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
            string[] results = segment.Split(text);
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
                indexs.Add(keywordIndex);
            }
            if (indexs.Count > 0)
            {
                using (IStorageEngine engine = STSdb.FromFile(dataPath))
                {
                    ITable<string, List<Iveely.Data.Slots<long, double>>> table = engine.OpenXTable<string, List<Iveely.Data.Slots<long, double>>>("WebPage");
                    foreach (var keywordIndex in indexs)
                    {
                        // 如果包含则追加
                        List<Iveely.Data.Slots<long, double>> list = table.Find(keywordIndex.Keyword);
                        if (list != null && list.Count > 0)
                        {
                            Iveely.Data.Slots<long, double> slot = new Slots<long, double>(keywordIndex.Id, keywordIndex.Weight);
                            list.Add(slot);
                        }
                        // 否则新增
                        else
                        {
                            list = new List<Slots<long, double>>();
                            Iveely.Data.Slots<long, double> slot = new Slots<long, double>(keywordIndex.Id, keywordIndex.Weight);
                            list.Add(slot);
                            table[keywordIndex.Keyword] = list;
                        }
                    }
                    engine.Commit();
                }
                indexs.Clear();
            }
        }
    }
}
