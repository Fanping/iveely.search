/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using Iveely.Framework.Network;

namespace Iveely.CloudComputing.CacheCommon
{
    /// <summary>
    /// 缓存网络传输包
    /// </summary>
    [Serializable]
    public class CachePacket : Packet
    {
        public CachePacket(byte[] data)
            : base(data)
        {

        }
    }
}
