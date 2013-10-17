/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.CloudComputting.Configuration;
using Iveely.CloudComputting.MergerCommon;
using Iveely.Framework.Text;

namespace Iveely.CloudComputting.Client
{
    public class Mathematics
    {
        private static Framework.Network.Synchronous.Client client;


        public static double Sum(double val, object[] args)
        {
            Init();
            MergerCommon.MergePacket packet = new MergePacket(Serializer.SerializeToBytes(val), MergePacket.MergeType.Sum, args[4].ToString(), args[5].ToString());
            packet.WaiteCallBack = true;
            return client.Send<double>(packet);
        }



        private static void Init()
        {
            if (client == null)
            {
                string remoteServer = SettingItem.GetInstance().MergeServerIP;
                int remotePort = 8801;
                client = new Framework.Network.Synchronous.Client(remoteServer, remotePort);
            }
        }
    }
}
