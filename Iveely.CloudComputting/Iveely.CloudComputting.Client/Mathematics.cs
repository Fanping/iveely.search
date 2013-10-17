/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.CloudComputting.Configuration;
using Iveely.CloudComputting.MergerCommon;
using Iveely.Framework.Log;
using Iveely.Framework.Text;

namespace Iveely.CloudComputting.Client
{
    public class Mathematics
    {
        private static Framework.Network.Synchronous.Client client;


        public static T Sum<T>(double val, object[] args)
        {
            Init();
            MergerCommon.MergePacket packet = new MergePacket(Serializer.SerializeToBytes(val), MergePacket.MergeType.Sum, args[4].ToString(), args[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(args[2].ToString() + "," + args[3].ToString() + " send sum commond,value is " + val);
            return (T)Convert.ChangeType(client.Send<object>(packet), typeof(T));
        }

        public static double Average(double val, object[] args)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(val), MergePacket.MergeType.Average,
                args[4].ToString(), args[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(args[2].ToString() + "," + args[3].ToString() + " send average commond,value is " + val);
            return client.Send<double>(packet);
        }

        public static List<T> CombineList<T>(List<T> objects, object[] args)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(objects), MergePacket.MergeType.CombineList,
                args[4].ToString(), args[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(args[2].ToString() + "," + args[3].ToString() + " send combine list commond.");
            return client.Send<List<T>>(packet);
        }

        public static Hashtable CombineTable(Hashtable table, object[] args)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(table), MergePacket.MergeType.CombineTable,
                args[4].ToString(), args[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(args[2].ToString() + "," + args[3].ToString() + " send combine table commond.");
            return client.Send<Hashtable>(packet);
        }

        public static List<T> Distinct<T>(List<T> objects, object[] args)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(objects), MergePacket.MergeType.Distinct,
                args[4].ToString(), args[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(args[2].ToString() + "," + args[3].ToString() + " send distinct commond. ");
            List<object> results = client.Send<List<object>>(packet);
            List<T> list = new List<T>();
            foreach (var result in results)
            {
                list.Add((T)result);
            }
            return list;
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
