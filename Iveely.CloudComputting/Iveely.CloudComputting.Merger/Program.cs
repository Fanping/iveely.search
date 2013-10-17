using System;
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
                MergerSupervisor = new Server(Dns.GetHostName(), 8801, ProcessMergerClient);
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
                    string flag = "Sum_" + client.TimeStamp + "_" + client.AppName;
                    LogHelper.Info(flag + ",result is " + result);
                    sum.Remove(flag);
                    return Serializer.SerializeToBytes(result);
                }
                //if (client.Type == MergePacket.MergeType.Average)
                //{
                //    double result = MergerMath.Average(client.Name, Serializer.Deserialize<double>(client.Data));
                //    LogHelper.Info("Data Avg");
                //    return Serializer.SerializeToBytes(result);
                //}
                //if (client.Type == MergePacket.MergeType.Count)
                //{
                //    double result = MergerMath.Count(client.Name, Serializer.Deserialize<double>(client.Data));
                //    //LogHelper.Info("Delete from path-" + client.Path);
                //    return Serializer.SerializeToBytes(result);
                //}
                //if (client.Type == MergePacket.MergeType.Distinct)
                //{
                //    var result = new List<object>();
                //    result.AddRange(MergerMath.Distinct<object>(client.Name, Serializer.Deserialize<IEnumerable<object>>(client.Data)));
                //    //LogHelper.Info("Get from path-" + client.Path);
                //    return Serializer.SerializeToBytes(result);
                //}
                //if (client.Type == MergePacket.MergeType.TopN)
                //{

                //    var result = Serializer.DeserializeFromBytes<List<double>>(client.Data);//(List<double>)client.Data;
                //    long n = result.ToArray().Length;
                //    result = MergerMath.TopN(client.Name, Serializer.Deserialize<List<double>>(client.Data), n);
                //    //LogHelper.Info("IsExists from path-" + client.Path);
                //    return Serializer.SerializeToBytes(result);
                //}
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception);
            }
            return Serializer.SerializeToBytes(-1);
        }
    }
}
