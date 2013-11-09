/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using Iveely.Framework.Network;

namespace Iveely.CloudComputting.CacheCommon
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
