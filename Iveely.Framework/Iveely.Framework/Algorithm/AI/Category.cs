/////////////////////////////////////////////////
///文件名:Category
///描  述:
///创建者:刘凡平(Iveely Liu)
///邮  箱:liufanping@iveely.com
///组  织:Iveely
///年  份:2012/3/27 20:41:46
///////////////////////////////////////////////


using System.Collections.Generic;
using Iveely.Framework.DataStructure;


namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 类别
    /// </summary>
    public class Category
    {
        /// <summary>
        /// 模式集合
        /// </summary>
        public SortedList<Pattern> Patterns = new SortedList<Pattern>();
    }
}
