using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.Text.Segment;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 问题检测
    /// </summary>
    public class QuestionChecker : HMM
    {
        /// <summary>
        /// 问题类型
        /// </summary>
        public enum Type
        {
            //时间疑问句(你几点钟睡觉)
            ForTime,

            //地点疑问句(你在哪儿上学)
            ForLocation,

            //过程疑问句(蛋炒饭怎么做)
            ForProccess,

            //人物疑问句(唐朝第一个皇帝是谁)
            ForWhom,

            //选择疑问句(你累不累？钓鱼岛是不是中国的？)
            ForSelect,

            //属性疑问句(你今年多大了)
            ForAttribute,

            //状态疑问句(水烧开了吗)
            ForStatus
        }

        /// <summary>
        /// 状态集
        /// </summary>
        private readonly string[] _states = { "Head", "Middle", "End" };

        private static QuestionChecker checker;

        public static QuestionChecker GetInstance()
        {
            if (checker == null)
            {
                checker = new QuestionChecker();
            }
            return checker;
        }

        private QuestionChecker()
        {
            TrainStudy();
        }

        private void TrainStudy()
        {
            SetState(_states);
            string lastwordType = _states[0];
            string currentWordType = string.Empty;
            string[] allLines = File.ReadAllLines("Init\\question_study.txt", Encoding.UTF8);
            foreach (string line in allLines)
            {
                string[] context = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < context.Length; i = i + 2)
                {
                    //添加观察者
                    AddObserver(context[i]);
                    if (i == 1)
                    {
                        currentWordType = _states[0];
                    }

                    else if (i < context.Length - 1)
                    {
                        currentWordType = _states[1];
                    }

                    else
                    {
                        currentWordType = _states[2];
                    }

                    //初始状态转移矩阵
                    AddInitialStateProbability(currentWordType, 1);
                    //观察状态转移矩阵
                    AddComplexProbability(currentWordType, context[1]);
                    //隐含状态转移矩阵
                    AddTransferProbability(lastwordType, currentWordType, 1);
                    lastwordType = currentWordType;
                }
            }
        }

        /// <summary>
        /// 判断是否是一个疑问句
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public bool IsQuestion(string sentence)
        {
            List<string> semanticList = Text.Segment.IctclasSegment.GetInstance().GetSemantic(sentence);
            if (semanticList.Count == 0)
            {
                return false;
            }
            double pro = 0;
            int[] path=Decode(semanticList.ToArray(), out pro);
            //Console.WriteLine(path.Length);
            return pro < 0.1;
        }
    }
}
