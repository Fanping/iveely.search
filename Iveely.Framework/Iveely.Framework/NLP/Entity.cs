using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 实体
    /// </summary>
    [Serializable]
    public struct Entity
    {
        /// <summary>
        /// 所属语句成分
        /// </summary>
        public Common.Sentence Type
        {
            get;
            private set;
        }

        /// <summary>
        /// 实体值
        /// </summary>
        public string Value
        {
            get;
            private set;
        }

        public void SetEntity( Common.Sentence type,string val)
        {
            this.Type = type;
            this.Value = val;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode()+(int)Type;
        }
    }
}
