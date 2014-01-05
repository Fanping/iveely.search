/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using Iveely.CloudComputing.Client;

namespace Iveely.CloudComputing.Worker
{
    [Serializable]
    public class RunningStatus
    {
        public string Description;
        public ExcutePacket Packet;

        public RunningStatus(ExcutePacket packet, string status)
        {
            Packet = packet;
            Description = status;
        }
    }
}