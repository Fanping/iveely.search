/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using Iveely.CloudComputing.Configuration;
using Iveely.CloudComputing.MergerCommon;
using Iveely.Framework.Log;
using Iveely.Framework.Text;

namespace Iveely.CloudComputing.Client
{
    public class Mathematics
    {
        private static Framework.Network.Synchronous.Client _client;


        public static T Sum<T>(double val)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(val), MergePacket.MergeType.Sum, Application.Parameters[4].ToString(), Application.Parameters[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send sum commond,value is " + val);
            return (T)Convert.ChangeType(_client.Send<object>(packet), typeof(T));
        }

        public static double Average(double val)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(val), MergePacket.MergeType.Average,
                Application.Parameters[4].ToString(), Application.Parameters[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send average commond,value is " + val);
            return _client.Send<double>(packet);
        }

        public static List<T> CombineList<T>(List<T> objects)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(objects), MergePacket.MergeType.CombineList,
                Application.Parameters[4].ToString(), Application.Parameters[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(Application.Parameters[2] + "," + Application.Parameters[3] + " send combine list commond.");
            return _client.Send<List<T>>(packet);
        }

        public static Hashtable CombineTable(Hashtable table, object[] args)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(table), MergePacket.MergeType.CombineTable,
                args[4].ToString(), args[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(args[2] + "," + args[3] + " send combine table commond.");
            return _client.Send<Hashtable>(packet);
        }

        public static List<T> Distinct<T>(List<T> objects, object[] args)
        {
            Init();
            MergePacket packet = new MergePacket(Serializer.SerializeToBytes(objects), MergePacket.MergeType.Distinct,
                args[4].ToString(), args[5].ToString());
            packet.WaiteCallBack = true;
            Logger.Info(args[2] + "," + args[3] + " send distinct commond. ");
            List<object> results = _client.Send<List<object>>(packet);
            List<T> list = new List<T>();
            foreach (var result in results)
            {
                list.Add((T)result);
            }
            return list;
        }

        private static void Init()
        {
            if (_client == null)
            {
                string remoteServer = SettingItem.GetInstance().MergeServerIP;
                int remotePort = 8801;
                _client = new Framework.Network.Synchronous.Client(remoteServer, remotePort);
            }
        }
    }
}
