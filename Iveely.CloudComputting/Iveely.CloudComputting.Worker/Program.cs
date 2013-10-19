using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Iveely.CloudComputting.StateAPI;
using Iveely.Framework.Log;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputting.Worker
{
    public class Program
    {
        private static Server _taskSuperviser;

        private static string MachineName;

        private static int ServicePort;

        public static void Main(string[] args)
        {
            //1. 确定worker运行端口号
            int port = 8001;
            if (args.Length > 0)
            {
                port = int.Parse(args[0]);
            }
            MachineName = Dns.GetHostName();
            ServicePort = port;

            //2. 向State Center发送上线消息
            StateHelper.Put("ISE://system/state/worker/" + MachineName + "," + ServicePort, MachineName + ":" + ServicePort + " is ready online!");

            //3. 启动任务接收监听
            if (_taskSuperviser == null)
            {
                Logger.Info("Starting listen the worker's task...");
                _taskSuperviser = new Server(Dns.GetHostName(), port, ProcessTask);
                Logger.Info("worker's task supervisor instance build success...");
                _taskSuperviser.Listen();
            }
        }

        private static byte[] ProcessTask(byte[] bytes)
        {
            CodePacket packet = Serializer.DeserializeFromBytes<CodePacket>(bytes);
            byte[] dataBytes = packet.Data;
            string sourceCode = System.Text.Encoding.UTF8.GetString(dataBytes);
            string runningPath = "ISE://application/" + packet.TimeStamp + "/" + packet.AppName + "/" +
                                 MachineName + "," + ServicePort;
            Logger.Info("Running path " + runningPath);
            try
            {
                List<string> references = new List<string>();
                references.Add("Iveely.CloudComputting.Client.exe");
                references.Add("System.Xml.dll");
                references.Add("System.Xml.Linq.dll");
                CodeCompiler.Execode(sourceCode, packet.ClassName, references, new object[] { packet.ReturnIp, packet.Port, MachineName, ServicePort, packet.TimeStamp, packet.AppName });
                StateHelper.Put(runningPath, "Finished with success!");
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                StateHelper.Put(runningPath, "Finished with " + exception.ToString());
            }
            return bytes;
        }

#if DEBUG

        [TestMethod]
        public void TestProcessTask()
        {

        }

#endif
    }
}
