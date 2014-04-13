using Iveely.Framework.DataStructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 实体关系倒排索引表
    /// </summary>
    [Serializable]
    public class InvertEntity
    {
        private  DimensionTable<Entity, Entity, List<Relation>> table = new DimensionTable<Entity, Entity, List<Relation>>();

        public InvertEntity()
        {

        }



        /// <summary>
        /// 添加关系索引
        /// </summary>
        /// <param name="times">时间</param>
        /// <param name="places">地点</param>
        /// <param name="whoms">人物</param>
        /// <param name="events">事件</param>
        public void Add(string[] times, string[] places, string[] whoms, string[] events)
        {
            //构建时间
            Entity[] entityTimes = BuildEntity(Common.Sentence.Time, times);

            //构建地点
            Entity[] entityPlaces = BuildEntity(Common.Sentence.Place, places);

            //构建人物
            Entity[] entityWhoms = BuildEntity(Common.Sentence.Whom, whoms);

            //构建事件
            Entity[] entityEvent = BuildEntity(Common.Sentence.Event, events);

            //时间-地点索引
            Relation timeToPlace = BuildRelation(entityWhoms, entityEvent);
            for (int i = 0; i < entityTimes.Length; i++)
            {
                for (int j = 0; j < entityPlaces.Length; j++)
                {
                    if (table[entityTimes[i]][entityPlaces[j]] == null)
                    {
                        List<Relation> relation = new List<Relation>();
                        table[entityTimes[i]][entityPlaces[j]] = relation;
                    }
                    table[entityTimes[i]][entityPlaces[j]].Add(timeToPlace);
                }
            }

            //时间-人物索引
            Relation timeToWhom = BuildRelation(entityPlaces, entityEvent);
            for (int i = 0; i < entityTimes.Length; i++)
            {
                for (int j = 0; j < entityWhoms.Length; j++)
                {
                    if (table[entityTimes[i]][entityWhoms[j]] == null)
                    {
                        List<Relation> relation = new List<Relation>();
                        table[entityTimes[i]][entityWhoms[j]] = relation;
                    }
                    table[entityTimes[i]][entityWhoms[j]].Add(timeToWhom);
                }
            }

            //时间-事件索引
            Relation timeToEvent = BuildRelation(entityPlaces, entityWhoms);
            for (int i = 0; i < entityTimes.Length; i++)
            {
                for (int j = 0; j < entityEvent.Length; j++)
                {
                    if (table[entityTimes[i]][entityEvent[j]] == null)
                    {
                        List<Relation> relation = new List<Relation>();
                        table[entityTimes[i]][entityEvent[j]] = relation;
                    }
                    table[entityTimes[i]][entityEvent[j]].Add(timeToEvent);
                }
            }

            //地点-人物索引
            Relation placeToWhom = BuildRelation(entityTimes, entityEvent);
            for (int i = 0; i < entityPlaces.Length; i++)
            {
                for (int j = 0; j < entityWhoms.Length; j++)
                {
                    if (table[entityPlaces[i]][entityWhoms[j]] == null)
                    {
                        List<Relation> relation = new List<Relation>();
                        table[entityPlaces[i]][entityWhoms[j]] = relation;
                    }
                    table[entityPlaces[i]][entityWhoms[j]].Add(placeToWhom);
                }
            }

            //地点-事件索引
            Relation placeToEvent = BuildRelation(entityTimes, entityWhoms);
            for (int i = 0; i < entityPlaces.Length; i++)
            {
                for (int j = 0; j < entityEvent.Length; j++)
                {
                    if (table[entityPlaces[i]][entityEvent[j]] == null)
                    {
                        List<Relation> relation = new List<Relation>();
                        table[entityPlaces[i]][entityEvent[j]] = relation;
                    }
                    table[entityPlaces[i]][entityEvent[j]].Add(placeToEvent);
                }
            }

            //人物-事件索引
            Relation whomToEvent = BuildRelation(entityTimes, entityPlaces);
            for (int i = 0; i < entityWhoms.Length; i++)
            {
                for (int j = 0; j < entityEvent.Length; j++)
                {
                    if (table[entityWhoms[i]][entityEvent[j]] == null)
                    {
                        List<Relation> relation = new List<Relation>();
                        table[entityWhoms[i]][entityEvent[j]] = relation;
                    }
                    table[entityWhoms[i]][entityEvent[j]].Add(whomToEvent);
                }
            }

        }

        public List<string> Query(Common.Sentence forQueryType, string[] times, string[] places, string[] whoms, string[] events)
        {
            //构建时间
            Entity[] entityTimes = BuildEntity(Common.Sentence.Time, times);

            //构建地点
            Entity[] entityPlaces = BuildEntity(Common.Sentence.Place, places);

            //构建人物
            Entity[] entityWhoms = BuildEntity(Common.Sentence.Whom, whoms);

            //构建事件
            Entity[] entityEvent = BuildEntity(Common.Sentence.Event, events);

            IntTable<Entity, int> result = new IntTable<Entity, int>();

            //时间-地点相关
            List<Entity> tmp = FindRelation(entityTimes, entityPlaces, forQueryType);
            if (tmp != null && tmp.Count > 0)
            {
                result.Add(tmp.ToArray());
            }

            //时间-人物相关
            tmp = FindRelation(entityTimes, entityWhoms, forQueryType);
            if (tmp != null && tmp.Count > 0)
            {
                result.Add(tmp.ToArray());
            }

            //时间-事件相关
            tmp = FindRelation(entityTimes, entityEvent, forQueryType);
            if (tmp != null && tmp.Count > 0)
            {
                result.Add(tmp.ToArray());
            }

            //地点-人物相关
            tmp = FindRelation(entityPlaces, entityWhoms, forQueryType);
            if (tmp != null && tmp.Count > 0)
            {
                result.Add(tmp.ToArray());
            }

            //地点-事件相关
            tmp = FindRelation(entityPlaces, entityEvent, forQueryType);
            if (tmp != null && tmp.Count > 0)
            {
                result.Add(tmp.ToArray());
            }

            //人物-事件相关
            tmp = FindRelation(entityWhoms, entityEvent, forQueryType);
            if (tmp != null && tmp.Count > 0)
            {
                result.Add(tmp.ToArray());
            }

            //按照实体出现次数排序
            List<string> keys = new List<string>();
            foreach (DictionaryEntry de in result)
            {
                keys.Add(((Entity)de.Key).Value.ToString());
            }
            return keys;
        }

        /// <summary>
        /// 生成实体
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private Entity[] BuildEntity(Common.Sentence type, string[] data)
        {
            if (data == null) return null;
            Entity[] entities = new Entity[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                entities[i] = new Entity();//= new Entityv
                entities[i].SetEntity(type, data[i]);
            }
            return entities;
        }

        /// <summary>
        /// 生成关系
        /// </summary>
        /// <param name="entityA"></param>
        /// <param name="entityB"></param>
        /// <param name="entityC"></param>
        /// <returns></returns>
        private Relation BuildRelation(Entity[] entityA, Entity[] entityB)
        {
            Relation relation = new Relation();
            foreach (Entity entity in entityA)
            {
                relation.AddEntity(entity);
            }
            foreach (Entity entity in entityB)
            {
                relation.AddEntity(entity);
            }
            return relation;
        }

        /// <summary>
        /// 获取关系集
        /// </summary>
        /// <param name="entityA"></param>
        /// <param name="entityB"></param>
        /// <returns></returns>
        private List<Entity> FindRelation(Entity[] entityA, Entity[] entityB, Common.Sentence forType)
        {
            List<Entity> entities = new List<Entity>();
            List<Relation> relationSet = new List<Relation>();
            for (int i = 0; entityA != null && i < entityA.Length; i++)
            {
                for (int j = 0; entityB != null && j < entityB.Length; j++)
                {
                    List<Relation> relations = table[entityA[i]][entityB[j]];
                    if (relations != null && relations.Count > 0)
                    {
                        relationSet.AddRange(relations);
                    }
                }
            }

            for (int i = 0; i < relationSet.Count; i++)
            {
                List<Entity> entity = relationSet[i].GetEntity(forType);
                if (entity.Count > 0)
                    entities.AddRange(entity);
            }

            return entities;
        }
    }
}
