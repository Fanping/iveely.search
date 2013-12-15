/////////////////////////////////////////////////
///文件名:Memory
///描  述:
///创建者:刘凡平(Iveely Liu)
///邮  箱:liufanping@iveely.com
///组  织:Iveely
///年  份:2012/3/28 11:16:46
///////////////////////////////////////////////


using System.Collections;

namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 变量
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// 变量名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 变量值
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// 全局记忆
    /// </summary>
    public class Memory
    {
        /// <summary>
        /// 全局记忆表
        /// </summary>
        private static readonly Hashtable Table = new Hashtable();

        /// <summary>
        /// 设置知识
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <param name="variable">变量名</param>
        /// <param name="value">变量值</param>
        public static void Set(string userId, string variable, string value)
        {
            //设置变量
            var vari = new Variable { Name = variable, Value = value };
            //变量名
            //变量值
            //往全局记忆表中插入
            Table.Add(userId, vari);
        }

        /// <summary>
        /// 获取记忆知识
        /// </summary>
        /// <param name="userId">用户名</param>
        /// <param name="variable">变量名</param>
        /// <returns></returns>
        public static string Get(string userId, string variable)
        {
            //遍历每一个元素
            foreach (DictionaryEntry item in Table)
            {
                //如果用户名相等
                if (item.Key.ToString()==userId)
                {
                    //转换为变量
                    var vari = (Variable)item.Value;
                    //如果变量值相等
                    if (vari.Name == variable)
                    {
                        //返回变量的值
                        return vari.Value;
                    }
                }
            }
            //返回其它
            return "对不起，我忘了，呜呜...";
        }
    }
}
