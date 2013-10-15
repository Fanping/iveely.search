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

        public static void Main()
        {
            //1. 建立Application文件夹，存放用户发来的文件
            if (!Directory.Exists("Application"))
            {
                Directory.CreateDirectory("Application");
            }

            //2. 向State Center发送上线消息
            string ip = Dns.GetHostName();
            StateHelper.Put("ISE://system/state/worker/" + ip, ip + " is ready online!");

            //3. 启动任务接收监听
            if (_taskSuperviser == null)
            {
                Logger.Info("Starting listen the worker's task...");
                _taskSuperviser = new Server(Dns.GetHostName(), 8001, ProcessTask);
                _taskSuperviser.Listen();
            }
        }

        private static byte[] ProcessTask(byte[] bytes)
        {
            CodePacket packet = Serializer.DeserializeFromBytes<CodePacket>(bytes);
            byte[] dataBytes = packet.Data;
            string sourceCode = System.Text.Encoding.UTF8.GetString(dataBytes);
            string runningPath = "ISE://application/" + packet.TimeStamp + "/" + packet.AppName + "/" +
                                 Dns.GetHostName();
            try
            {
                List<string> references = new List<string>();
                references.Add("Iveely.CloudComputting.Client.exe");
                CodeCompiler.Execode(sourceCode, packet.ClassName, references, null);
                StateHelper.Put(runningPath, "Finished with success!");
            }
            catch (Exception exception)
            {
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
