using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Iveely.CloudComputting.MergerCommon;
using Iveely.Framework.Log;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;
using log4net;

namespace Iveely.CloudComputting.Merger
{
    internal class Program
    {
        private static Framework.Network.Synchronous.Server MergerSupervisor;

        private static void Main(string[] args)
        {
            //1. 单进程模式运行
            Framework.Process.RunningState.StandAlone();

            //2. 启动运行标识
            StateAPI.StateHelper.Put("ISE://system/state/merger/" + Dns.GetHostName(), "merger start running...");

            //3. 启动监听
            if (MergerSupervisor == null)
            {
                MergerSupervisor = new Server(Dns.GetHostName(), 8801, ProcessMergerClient, 5);
                MergerSupervisor.Listen();
            }
        }

        private static byte[] ProcessMergerClient(byte[] bytes)
        {
            try
            {
                // 1.获取合并包
                var client = Serializer.DeserializeFromBytes<MergePacket>(bytes);

                // 2.处理消息
                if (client.Type == MergePacket.MergeType.Sum)
                {
                    Sum sum = new Sum(client.TimeStamp, client.AppName);
                    double result = sum.Compute(Serializer.DeserializeFromBytes<double>(client.Data));
                    string flag = "sum_" + client.TimeStamp + "_" + client.AppName;
                    Logger.Info(flag + ",result is " + result);
                    sum.Remove(flag);
                    return Serializer.SerializeToBytes(result);
                }
                if (client.Type == MergePacket.MergeType.Average)
                {
                    Average average = new Average(client.TimeStamp, client.AppName);
                    double result = average.Compute(Serializer.DeserializeFromBytes<double>(client.Data));
                    string flag = "average_" + client.TimeStamp + "_" + client.AppName;
                    Logger.Info(flag + ",result is " + result);
                    average.Remove(flag);
                    return Serializer.SerializeToBytes(result);
                }

                if (client.Type == MergePacket.MergeType.Distinct)
                {
                    Distinct distinct = new Distinct(client.TimeStamp, client.AppName);
                    List<object> objects = distinct.Compute(Serializer.DeserializeFromBytes<List<object>>(client.Data));
                    string flag = "distinct_" + client.TimeStamp + "_" + client.AppName;
                    Logger.Info(flag + ", result count is " + objects.Count);
                    return Serializer.SerializeToBytes(objects);
                }

                if (client.Type == MergePacket.MergeType.CombineTable)
                {
                    CombineTable combineTable = new CombineTable(client.TimeStamp, client.AppName);
                    Hashtable objects = combineTable.Compute(Serializer.DeserializeFromBytes<Hashtable>(client.Data));
                    string flag = "combine_table_" + client.TimeStamp + "_" + client.AppName;
                    Logger.Info(flag + ", combine table.");
                    return Serializer.SerializeToBytes(objects);
                }

                if (client.Type == MergePacket.MergeType.CombineList)
                {
                    CombineList combineList = new CombineList(client.TimeStamp, client.AppName);
                    List<object> objects = combineList.Compute(Serializer.DeserializeFromBytes<List<object>>(client.Data));
                    string flag = "combine_list_" + client.TimeStamp + "_" + client.AppName;
                    Logger.Info(flag + ", combine list.");
                    return Serializer.SerializeToBytes(objects);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            return Serializer.SerializeToBytes(-1);
        }
    }
}
