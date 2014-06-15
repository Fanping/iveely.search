using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.SearchEngine
{
    /// <summary>
    /// 信息抽取
    /// </summary>
    public class EntityExtrator
    {
        /// <summary>
        /// 实体抽取
        /// </summary>
        private List<string> EntityPatterns = new List<string>();

        /// <summary>
        /// 实体关系抽取
        /// </summary>
        private List<string> RelationPatterns = new List<string>();

        /// <summary>
        /// 词性分析组件
        /// </summary>
        Iveely.Framework.Text.HMMSegment mse =  Framework.Text.HMMSegment.GetInstance();

        public EntityExtrator()
        {
            //patterns.Add("");
        }

        public string[] GetInfo(string text)
        {
            Tuple<string[], string[]> tuple = mse.SplitToArray(text);
            return tuple.Item2;
        }
    }
}
