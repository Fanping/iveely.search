/////////////////////////////////////////////////
///文件名:Pattern
///描  述:
///创建者:刘凡平(Iveely Liu)
///邮  箱:liufanping@iveely.com
///组  织:Iveely
///年  份:2012/3/27 20:44:27
///////////////////////////////////////////////



namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 模式
    /// </summary>
    public class Pattern
    {
        /// <summary>
        /// 模式提问值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 对应回答模板
        /// </summary>
        public Template Template = new Template();

       
    }
}
