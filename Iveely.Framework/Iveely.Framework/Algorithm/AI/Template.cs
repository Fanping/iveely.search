/////////////////////////////////////////////////
///文件名:Template
///描  述:
///创建者:刘凡平(Iveely Liu)
///邮  箱:liufanping@iveely.com
///组  织:Iveely
///年  份:2012/3/27 20:45:04
///////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.Framework.DataStructure;


namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 模板
    /// </summary>
    public class Template
    {
        /// <summary>
        /// 生成的问题
        /// </summary>
        public class Question
        {
            /// <summary>
            /// 问题描述
            /// </summary>
            public string Description { get; internal set; }

            /// <summary>
            /// 问题答案
            /// </summary>
            public string Answer { get; internal set; }

            /// <summary>
            /// 参考信息
            /// </summary>
            public string Reference { get; internal set; }
        }

        /// <summary>
        /// 模版值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 模板随机值
        /// </summary>
        public Rand Rand { get; set; }

        /// <summary>
        /// 匹配*标记位置
        /// </summary>
        public SortedList<int> Star { get; set; }

        /// <summary>
        /// 用户的第几次输出
        /// </summary>
        public int Input { get; set; }

        /// <summary>
        /// 变量
        /// </summary>
        public Variable SetVariable;

        /// <summary>
        /// 获取的变量
        /// </summary>
        public Variable GetVariable;

        /// <summary>
        /// 函数名
        /// </summary>
        public Function Function { get; set; }

        public List<Question> Questions { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public Template()
        {
            //随机选项初始化
            Rand = new Rand();
            //设置变量初始化
            SetVariable = new Variable();
            //获取变量初始化
            GetVariable = new Variable();
            //方法集合初始化
            Function = new Function();
            //*号匹配符初始化
            Star = new SortedList<int>();
            //获取用户输入初始化
            Input = -1;
            //初始化疑问
            Questions = new List<Question>();
        }

        /// <summary>
        /// 回复信息
        /// </summary>
        /// <returns></returns>
        public string Reply()
        {
            //回复信息
            var result = Value;
            //如果是带有记忆的*号标识
            if (Star != null)
            {
                //如果名字不为空
                if (SetVariable.Name != "" || SetVariable.Name != null)
                {
                    //遍历每一个*符号
                    for (int i = 0; i < Star.Count; i++)
                    {
                        //存储变量数目
                        User.StoreNum++;
                        //设定记忆变量
                        Memory.Set(User.UserId + "" + User.StoreNum, SetVariable.Name, AI.Star.List[i]);
                        //break;
                    }
                    //返回值
                    //return this.Value;
                }
                //输出信息
                //return this.Value + Smart.Star.List[Star-1];
                //遍历每一个*符号
                //for (int i = 0; i < this.Star.Count; i++)
                //{
                //    result += Smart.Star.List[i];
                //}
            }

            //如果有动态执行函数功能
            if (Function != null)
            {
                //需要传递的参数集合
                var parm = new SortedList<string>();
                //将里面的索引值取出来
                var star = Star;
                if (star != null)
                {
                    int[] va = star.ToArray();
                    //将真实值取出来
                    parm.AddRange(va.Select(a => AI.Star.List[a - 1]));
                }
                //执行，依然要把参数传递过去
                return Library.CodeCompiler.Execute(Function.Name, parm.ToArray());
            }
            //获取存储的变量值
            if (GetVariable.Name != null)
            {

                //获取变量
                result
                    += Memory.Get(User.UserId + "" + User.StoreNum, GetVariable.Name);
            }
            //如果输出用户曾经的输出
            if (Input != -1)
            {
                //输出信息
                result += AI.Input.List[Input - 1];
            }
            if (String.IsNullOrEmpty(result))
            {
                //随机数的方式返回
                var rnd = new Random();
                //返回
                if (Rand != null) return Rand.List[rnd.Next(0, Rand.List.Count)];
            }
            return result;
        }


        public void AddQuestion(Question question)
        {
            this.Questions.Add(question);
        }

        public List<Question> BuildQuestion(string reference)
        {
            List<Question> formalQuestions = new List<Question>();
            string[] values = AI.Star.List;
            foreach (var question in Questions)
            {
                string doubt = question.Description;
                string answer = question.Answer;
                for (int i = 0; i < values.Count(); i++)
                {
                    doubt = doubt.Replace("[" + i + "]", values[i]);
                    answer = answer.Replace("[" + i + "]", values[i]);
                }

                //doubt = ReplaceStar(doubt, values);
                answer = ReplaceStar(answer, values);
                Question formalQuestion = new Question();
                formalQuestion.Answer = answer;
                formalQuestion.Description = doubt;
                formalQuestion.Reference = reference;
                formalQuestions.Add(formalQuestion);
            }
            return formalQuestions;
        }

        private string ReplaceStar(string text, string[] values)
        {
            int indexStar = text.IndexOf("*", System.StringComparison.Ordinal);
            while (indexStar > -1)
            {
                int numberLeftIndex = text.IndexOf("{", StringComparison.Ordinal);
                int numberRightIndex = text.IndexOf("}", StringComparison.Ordinal);
                string[] indexs = text.Substring(numberLeftIndex + 1, numberRightIndex - numberLeftIndex - 1).Split(new[] { ',', '，' });
                int start = int.Parse(indexs[0]);
                int end = start;
                if (indexs.Length > 1)
                {
                    end = int.Parse(indexs[1]);
                }
                text = text.Remove(numberLeftIndex, numberRightIndex - numberLeftIndex + 1);
                while (end >= start)
                {
                    text = text.Remove(indexStar, 1).Insert(indexStar, values[start]);
                    indexStar = text.IndexOf("*", System.StringComparison.Ordinal);
                    start++;
                }

            }
            return text;
        }
    }
}
