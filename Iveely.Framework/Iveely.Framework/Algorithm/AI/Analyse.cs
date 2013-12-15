
/////////////////////////////////////////////////
///文件名:Analyse
///描  述:
///创建者:刘凡平(Iveely Liu)
///邮  箱:liufanping@iveely.com
///组  织:Iveely
///年  份:2012/3/27 21:52:15
///////////////////////////////////////////////


using System.Linq;
using System.Collections;
using Iveely.Framework.Text.Segment;

namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 数值分析（用于输入字符与模式字符进行匹配）
    /// </summary>
    public class Analyse
    {
        /// <summary>
        /// 字符串匹配
        /// </summary>
        /// <param name="pattern">模式值</param>
        /// <param name="input">输入字符串</param>
        /// <returns>True为匹配成功，False为匹配失败</returns>
        public static bool Match(string pattern, string input)
        {
            //如果输入的字符长度小于比较的
            return input.Length >= pattern.Length && Compare(pattern.ToArray(), input.ToArray());

        }

        /// <summary>
        /// 模式比较
        /// </summary>
        /// <param name="pattern">模式字符数组</param>
        /// <param name="input">输入字符数组</param>
        /// <returns></returns>
        public static bool Compare(char[] pattern, char[] input)
        {
            //起始位置
            int start = pattern.Length;
            //定义二维矩阵
            var lcs = new int[input.Length, pattern.Length];
            //宽
            for (int i = 0; i < pattern.Length; i++)
            {
                //高
                for (int j = 0; j < input.Length; j++)
                {
                    //初始化
                    lcs[j, i] = new int();
                    //如果相等
                    if (pattern[i]==input[j])
                    {
                        //将矩阵值设为1
                        lcs[j, i] = 1;
                        //起始位
                        if (start > i)
                        {
                            start = i;
                        }
                    }

                    //如果不想等
                    else
                    {
                        //将矩阵值设为0
                        lcs[j, i] = 0;
                    }
                }
            }
            //用栈来存储
            var stack = new Stack();
            //是否找到起始位
            var find = false;
            //行
            for (int i = 0; i < input.Length; i++)
            {
                //列
                for (int j = 0; j < pattern.Length; j++)
                {
                    //如果为1
                    if (lcs[i, j] == 1)
                    {
                        //如果起始位小于当前位
                        if (start < j && find)
                        {
                            //把当前入栈
                            stack.Push(pattern.ToArray()[j]);
                            //把起始位移到当前
                            start = j;
                        }
                        //如果起始位大于当前位
                        else
                        {
                            if (start == j)
                            {
                                //已经找到起始位
                                find = true;
                                //把当前入栈
                                stack.Push(pattern.ToArray()[j]);
                                //把起始位移到当前
                                start = j;
                            }
                            else if (start > j)
                            {
                                //出栈
                                if (stack.Count != 0)
                                {
                                    stack.Pop();
                                }
                                //把起始位移动到当前
                                start = j;
                            }
                        }
                    }
                    ////如果是2（*号内容部分）
                    //else if (LCS[i, j] == 2)
                    //{
                    //    //如果不是这一个star了，也就是换了一个star
                    //    if (lastj != j && lastj!=-1)
                    //    {
                    //        //把以前的加进去
                    //        Star.list.Add(star);
                    //    }
                    //    //如果相等，说明是一个连续的star
                    //    else
                    //    {
                    //        star += input.ToArray()[i];
                    //    }
                    //}
                }
            }
            //定义结果字符串
            string result = "";
            //栈中数量
            int num = stack.Count;
            //输入结果
            for (int m = 0; m < num; m++)
            {
                //累加起来
                result = stack.Pop() + result;
            }
            //重组模式
            string pat = pattern.Aggregate("", (current, s) => current + s);
            //模式
            string _pattern = pat.Replace("*", "");
            //重组输入
            string inp = input.Aggregate("", (current, s) => current + s);
            //输入替换为空格
            inp = result.ToArray().Aggregate(inp, (current, s) => current.Replace(s, ' '));
            //将两个替换为一个
            inp = inp.Replace("  ", " ").Trim();
            //切分字符
            string[] starInput = inp.Split(' ');
            //记录下他
            Star.List = starInput;
            //模式与结果的比较
            return _pattern==result;

        }
    }
}
