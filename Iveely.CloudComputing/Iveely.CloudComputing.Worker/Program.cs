using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Iveely.CloudComputing.StateAPI;
using Iveely.Framework.Log;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputing.Worker
{
    public class Program
    {
        private static Server _taskSuperviser;

        private static string _machineName;

        private static int _servicePort;

        public static void Main(string[] args)
        {
            //1. 确定worker运行端口号
            int port = 8001;
            if (args.Length > 0)
            {
                port = int.Parse(args[0]);
            }
            _machineName = Dns.GetHostName();
            _servicePort = port;
            if (!Directory.Exists(_servicePort.ToString(CultureInfo.InvariantCulture)))
            {
                Directory.CreateDirectory(_servicePort.ToString(CultureInfo.InvariantCulture));
            }

            //2. 向State Center发送上线消息
            StateHelper.Put("ISE://system/state/worker/" + _machineName + "," + _servicePort, _machineName + ":" + _servicePort + " is ready online!");

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
            ExcutePacket packet = Serializer.DeserializeFromBytes<ExcutePacket>(bytes);
            Logger.Info("Get process task.");
            //如果是执行代码
            if (packet.ExcuteType == ExcutePacket.Type.Code)
            {
                byte[] dataBytes = packet.Data;
                string sourceCode = Encoding.UTF8.GetString(dataBytes);
                string runningPath = "ISE://application/" + packet.TimeStamp + "/" + packet.AppName + "/" +
                                     _machineName + "," + _servicePort;
                Logger.Info("Running path " + runningPath);
                try
                {
                    List<string> references = new List<string>();
                    references.Add("Iveely.CloudComputing.Client.exe");
                    references.Add("Iveely.Framework.dll");
                    references.Add("System.Xml.dll");
                    references.Add("System.Xml.Linq.dll");
                    CodeCompiler.Execode(sourceCode, packet.ClassName, references,
                        new object[] { packet.ReturnIp, packet.Port, _machineName, _servicePort, packet.TimeStamp, packet.AppName });
                    StateHelper.Put(runningPath, "Finished with success!");
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                    StateHelper.Put(runningPath, "Finished with " + exception);
                }
                return bytes;
            }

            //如果是文件片
            else if (packet.ExcuteType == ExcutePacket.Type.FileFragment)
            {
                byte[] fileNameBytes = packet.Data;
                string fileName = Encoding.UTF8.GetString(fileNameBytes);
                if (fileName.Contains("/"))
                {
                    string[] folder = fileName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string tempPath = _servicePort + "/";
                    for (int i = 0; i < folder.Length - 1; i++)
                    {
                        tempPath += folder[i] + "/";
                        if (!Directory.Exists(tempPath))
                        {
                            Directory.CreateDirectory(tempPath);
                        }
                    }
                }
                Logger.Info("Get command to save file fragment by name " + fileName);
                FileTransfer fileTransfer = new FileTransfer();
                fileTransfer.Receive(7001, _servicePort + "/" + fileName);
                return fileNameBytes;
            }

            //如果是下载文件
            else if (packet.ExcuteType == ExcutePacket.Type.Download)
            {
                Logger.Info("Get command download.");
                byte[] fileNameBytes = packet.Data;
                string fileName = Encoding.UTF8.GetString(fileNameBytes);

                Logger.Info("Start send file:" + fileName);
                FileTransfer fileTransfer = new FileTransfer();
                fileTransfer.Send(_servicePort + "/" + fileName, packet.ReturnIp, 7002);
                Logger.Info("Send finished.");

                return fileNameBytes;
            }

            //如果是删除文件
            else if (packet.ExcuteType == ExcutePacket.Type.Delete)
            {
                Logger.Info("Get command delete.");
                byte[] fileNameBytes = packet.Data;
                string fileName = Encoding.UTF8.GetString(fileNameBytes);
                if (File.Exists(_servicePort + "/" + fileName))
                {
                    Logger.Info("Start delete file:" + fileName);
                    File.Delete(_servicePort + "/" + fileName);
                    Logger.Info("Deleted.");
                }
                else
                {
                    Logger.Warn("Delete file:" + fileName + " not found.");
                }
                return fileNameBytes;
            }

            //如果是重命名文件
            else if (packet.ExcuteType == ExcutePacket.Type.Rename)
            {
                Logger.Info("Get command rename.");
                Tuple<string, string> fileTuple = Serializer.DeserializeFromBytes<Tuple<string, string
                    >>(packet.Data);
                string fileName = _servicePort + "/" + fileTuple.Item1;
                string fileNewName = _servicePort + "/" + fileTuple.Item2;
                if (File.Exists(fileName) && !File.Exists(fileNewName) && fileName != fileNewName)
                {
                    File.Move(fileName, fileNewName);
                }
            }
            return null;
        }

#if DEBUG

        [TestMethod]
        public void TestProcessTask()
        {

        }

#endif
    }
}
