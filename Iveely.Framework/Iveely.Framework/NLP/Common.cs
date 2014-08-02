using System;

namespace Iveely.Framework.NLP
{
    [Serializable]
    public class Common
    {
        /// <summary>
        /// 词性语法
        /// </summary>
        public enum Semantic
        {

        }

        /// <summary>
        /// 语句成分
        /// </summary>
        public enum Sentence
        {
            /// <summary>
            /// 时间
            /// </summary>
            Time,

            /// <summary>
            /// 地点
            /// </summary>
            Place,

            /// <summary>
            /// 人物
            /// </summary>
            Whom,

            /// <summary>
            /// 事件
            /// </summary>
            Event
        }
    }
}
