/////////////////////////////////////////////////
///文件名:Fucntion
///描  述:
///创建者:刘凡平(Iveely Liu)
///邮  箱:liufanping@iveely.com
///组  织:Iveely
///年  份:2012/3/28 16:53:27
/////////////////////////////////////////////

using System.Collections.Generic;
using Iveely.Framework.DataStructure;


namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 函数执行体
    /// </summary>
    public class Function
    {
        /// <summary>
        /// 函数名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 函数执行参数集合
        /// </summary>
        public SortedList<string> Parameters = new SortedList<string>();
    }
}
