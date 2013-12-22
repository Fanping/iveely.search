/////////////////////////////////////////////////
//文件名:System
//描  述:
//创建者:刘凡平(Iveely Liu)
//邮  箱:liufanping@iveely.com
//组  织:Iveely
//年  份:2012/3/28 15:50:14
///////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;


namespace Iveely.Framework.Algorithm.AI.Library
{
    /// <summary>
    /// 系统函数库
    /// </summary>
    public class Sys
    {
        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns></returns>
        public string GetTime()
        {
            //返回当前时间
            Debug.Assert(CultureInfo.InvariantCulture != null, "CultureInfo.InvariantCulture != null");
            return DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 加法计算
        /// </summary>
        /// <param name="a">加数</param>
        /// <param name="b">被加数</param>
        /// <returns>返回结果</returns>
        public string Plus(string a, string b)
        {
            return (int.Parse(a) + int.Parse(b)).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 减法
        /// </summary>
        /// <param name="a">被减数</param>
        /// <param name="b">减数</param>
        /// <returns>减的结果</returns>
        public string Sub(string a, string b)
        {
            return (int.Parse(a) - int.Parse(b)).ToString(CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// 乘法
        /// </summary>
        /// <param name="a">乘数</param>
        /// <param name="b">被乘数</param>
        /// <returns>积</returns>
        public string Mul(string a, string b)
        {
            return (int.Parse(a) * int.Parse(b)).ToString(CultureInfo.InvariantCulture);
        }

        public string Normal(params string[] infors)
        {
            return infors[0];
        }


        /// <summary>
        /// 执行转换功能
        /// </summary>
        public static string ConvertToBig(string number)
        {

            //获取输入值
            string num = number;
            //清除首末的0
            num = Clean(num);
            //判断是否为空
            if (String.IsNullOrWhiteSpace(num))
            {
                Console.WriteLine("数字不能为空或者为空格。");
                return null;
            }
            //检查输入
            if (CheckStandard(num))
            {
                //加个小数点
                num += ".";
                //小数点左边值
                string left = num.Split('.')[0];
                //小数点右边值
                string right = num.Split('.')[1];
                //单位符号数组
                string[] symbol = { "", "十", "百", "千", "万", "十", "百", "千", "亿", "十", "百", "千" };
                //大小写数组
                string[] uppercase = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
                //输出结果
                string result = "";
                //标识是否是中间0
                int middlezero = 0;
                //左边转换为字符数组
                char[] cLeft = left.ToCharArray();
                //如果整数部分只是0
                if (cLeft[0] == '0')
                {
                    //直接零开头
                    result += "零";
                }
                //计算整数（左边）部分
                for (int i = 0; i < cLeft.Length; i++)
                {
                    //如果是中间位的0
                    if (cLeft[i] != '0')
                    {
                        //转换为大写
                        result += uppercase[int.Parse(cLeft[i].ToString(CultureInfo.InvariantCulture))];
                        //添加单位
                        result += symbol[cLeft.Length - 1 - i];
                        //中间0重置
                        middlezero = 0;
                    }
                    else
                    {
                        //如果不是整数的最末位是0
                        if (i != cLeft.Length - 1)
                        {
                            //如果下一位是非0
                            if (cLeft[i + 1] != '0' && (cLeft.Length - 1 - i != 4 && cLeft.Length - 1 - i != 8))
                            {
                                //转换为大写
                                result += uppercase[int.Parse(cLeft[i].ToString(CultureInfo.InvariantCulture))];
                            }
                            //如果下一位依然是0
                            else
                            {
                                //如果是万位置后面的最后0
                                if (cLeft.Length - 1 - i == 4 && middlezero < 3)
                                {
                                    //添加万位符
                                    result += "万";
                                    //中间0结束先
                                    middlezero = 0;
                                    //如果下一位首位为0
                                    if (cLeft[i + 1] == '0')
                                    {
                                        result += "零";

                                        i++;
                                    }

                                }
                                //如果是亿后面的0
                                if (cLeft.Length - 1 - i == 8 && middlezero < 3)
                                {
                                    //添加亿位符
                                    result += "亿";
                                    //中间0结束
                                    middlezero = 0;
                                    //如果下一位首位为0
                                    if (cLeft[i + 1] == '0')
                                    {
                                        result += "零";

                                        i++;
                                    }
                                }
                            }
                        }
                        middlezero++;
                    }

                }
                //小数部分如果不为空
                if (right.Trim() != "")
                {
                    //添加小数点
                    result += "点";
                    //左边转换为字符
                    char[] cRight = right.ToCharArray();
                    //计算右边小数部分
                    result = cRight.Aggregate(result, (current, t) => current + uppercase[int.Parse(t.ToString(CultureInfo.InvariantCulture))]);
                }
                //输出转换后的结果
                return result;
            }
            return null;

        }
        /// <summary>
        /// 检查输入字符是否满足规范
        /// </summary>
        private static bool CheckStandard(string input)
        {
            //临时变量
            //第一步，判断小数点小于等于1个
            int point = 0;
            //第二步，全部都是数字
            int otherChar = 0;
            int num = 0;
            //第三步，小数点整数部分限定最大值为12位
            int left = 0;
            //首先全部转换为字符数组
            char[] myInput = input.ToCharArray();
            //遍历
            foreach (char t in myInput)
            {
//如果不是点，我们就当作数字字符
                if (t != '.')
                {
                    //能成功转换为数字
                    int temp;
                    if (int.TryParse(t.ToString(CultureInfo.InvariantCulture), out temp))
                    {
                        //数字+1
                        num++;
                        //如果是小数点左边
                        if (point < 1)
                        {
                            //表示当前是左边数字
                            left++;
                        }
                    }
                        //其它字符
                    else
                    {
                        //其它字符+1
                        otherChar++;
                    }
                }
                    //是小数点
                else
                {
                    //小数点+1
                    point++;
                }
            }
            //如果小数点多余1个
            if (point > 1)
            {
                Console.WriteLine("您的输入中有多余的小数点！");
                return false;
            }
            //正常情况下
            //如果左边的整数部分超过12位
            if (left > 12)
            {
                Console.WriteLine("整数部分溢出，整数部分不得超过12位！");
                return false;
            }
            //正常情况下
            //是否存在其它字符
            if (otherChar > 0)
            {
                Console.WriteLine("您的输入存在非数字字符，请仔细检查！");
                return false;
            }
            //一切正常
            return true;
        }

        /// <summary>
        /// 清除首部的0和末尾的0
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <returns>返回清除后的字符串</returns>
        private static string Clean(string input)
        {
            //首先清空字符首末的空格符
            input = input.Trim();
            //如果以0开头
            while (input.StartsWith("0"))
            {
                //把0清除
                input = input.Substring(1, input.Length - 1);
            }
            //如果是小数点的0结尾
            while (input.EndsWith("0") && input.Contains("."))
            {
                //清楚末尾的0
                input = input.Substring(0, input.Length - 1);
            }
            //如果开始是小数点
            if (input.StartsWith("."))
            {
                //前面加0
                input = "0" + input;
            }

            //返回Input
            return input;
        }


    }
}
