using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 实体关系
    /// </summary>
    [Serializable]
    public class Relation
    {
        /// <summary>
        /// 关系编号
        /// </summary>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 关系中的实体集
        /// </summary>
        private List<Entity> Entities;

        public Relation()
        {
            Entities = new List<Entity>();
        }

        /// <summary>
        /// 添加关系实体
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(Entity entity)
        {
            this.Entities.Add(entity);
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<Entity> GetEntity(Common.Sentence sentence)
        {
            List<Entity> list = new List<Entity>();
            foreach(Entity entity in Entities)
            {
                if(entity.Type==sentence)
                {
                    list.Add(entity);
                }
            }
            return list;
        }
    }
}
