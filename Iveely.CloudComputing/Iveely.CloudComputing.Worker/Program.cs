using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Iveely.CloudComputing.Client;
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

        private static Hashtable _statusCenter;

        private static Hashtable _runner;

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
            _statusCenter = new Hashtable();
            _runner = new Hashtable();
            string processFolder = _servicePort.ToString(CultureInfo.InvariantCulture);
            if (!Directory.Exists(processFolder))
            {
                Directory.CreateDirectory(processFolder);
                CopyDirectory("Init", processFolder + "\\");
            }
            CheckCrash();

            //2. 向State Center发送上线消息
            StateHelper.Put("ISE://system/state/worker/" + _machineName + "," + _servicePort,
                _machineName + ":" + _servicePort + " is ready online!");

            //3. 启动心跳线程
            Thread thread = new Thread(SendHeartbeat);
            thread.Start();

            //3. 启动任务接收监听
            if (_taskSuperviser == null)
            {
                Logger.Info("Starting listen the worker's task...");
                _taskSuperviser = new Server(_machineName, _servicePort, ProcessTask);
                Logger.Info("worker's task supervisor instance build success...");
                _taskSuperviser.Listen();
            }
        }

        private static byte[] ProcessTask(byte[] bytes)
        {
            var packet = Serializer.DeserializeFromBytes<ExcutePacket>(bytes);
            Logger.Info("Get process task.");
            //如果是执行代码
            if (packet.ExcuteType == ExcutePacket.Type.Code)
            {
                string appName = packet.AppName;
                if (_statusCenter.ContainsKey(appName))
                {
                    _statusCenter.Remove(appName);
                    _runner.Remove(appName);
                }
                var status = new RunningStatus(packet, "Running");
                var runner = new Runner(ref status);
                runner.StartRun(_machineName, _servicePort);
                _statusCenter.Add(appName, status);
                _runner.Add(appName, runner);
                Backup();
                return Serializer.SerializeToBytes("Submit Success.");
            }

            if (packet.ExcuteType == ExcutePacket.Type.Kill)
            {
                string appName = packet.AppName;
                string flag = "[" + _machineName + "," + _servicePort + "] :";
                if (_statusCenter.ContainsKey(appName))
                {
                    if (_runner.ContainsKey(appName))
                    {
                        var runner = (Runner)_runner[appName];
                        _runner.Remove(appName);
                        runner.Kill();
                    }
                    _statusCenter.Remove(appName);
                    Backup();
                    return Serializer.SerializeToBytes(flag + "Kill Success.");
                }
                return Serializer.SerializeToBytes(flag + "Not found your application");
            }

            if (packet.ExcuteType == ExcutePacket.Type.Task)
            {
                var runningApps = new List<string>();
                if (_statusCenter.Count > 0)
                {
                    runningApps.AddRange(from DictionaryEntry dictionaryEntry in _statusCenter
                                         let key = dictionaryEntry.Key.ToString()
                                         let status = ((RunningStatus)dictionaryEntry.Value).Description
                                         select "[" + _machineName + "," + _servicePort + "] :" + key + " -> " + status);
                }
                return Serializer.SerializeToBytes(runningApps);
            }

            //如果是文件片
            if (packet.ExcuteType == ExcutePacket.Type.FileFragment)
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
                var fileTransfer = new FileTransfer();
                fileTransfer.Receive(7001, _servicePort + "/" + fileName);
                return fileNameBytes;
            }

            //如果是下载文件
            if (packet.ExcuteType == ExcutePacket.Type.Download)
            {
                Logger.Info("Get command download.");
                byte[] fileNameBytes = packet.Data;
                string fileName = Encoding.UTF8.GetString(fileNameBytes);

                Logger.Info("Start send file:" + fileName);
                var fileTransfer = new FileTransfer();
                fileTransfer.Send(_servicePort + "/" + fileName, packet.ReturnIp, 7002);
                Logger.Info("Send finished.");

                return fileNameBytes;
            }

            //如果是删除文件
            if (packet.ExcuteType == ExcutePacket.Type.Delete)
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
            if (packet.ExcuteType == ExcutePacket.Type.Rename)
            {
                Logger.Info("Get command rename.");
                var fileTuple = Serializer.DeserializeFromBytes<Tuple<string, string
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

        private static void CopyFile(string srcDir, string desDir)
        {
            string[] files = Directory.GetFiles(srcDir);
            foreach (var file in files)
            {
                File.Copy(file, desDir + "\\" + new FileInfo(file).Name);
            }
        }

        /// <summary>
        /// 拷贝文件夹
        /// </summary>
        /// <param name="srcdir"></param>
        /// <param name="desdir"></param>
        private static void CopyDirectory(string srcdir, string desdir)
        {
            string folderName = srcdir.Substring(srcdir.LastIndexOf("\\") + 1);

            string desfolderdir = desdir + "\\";

            if (desdir.LastIndexOf("\\") == (desdir.Length - 1))
            {
                desfolderdir = desdir + folderName;
            }
            string[] filenames = Directory.GetFileSystemEntries(srcdir);

            foreach (string file in filenames)
            {
                if (Directory.Exists(file))
                {

                    string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
                    if (!Directory.Exists(currentdir))
                    {
                        Directory.CreateDirectory(currentdir);
                    }

                    CopyDirectory(file, desfolderdir);
                }

                else
                {
                    string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);

                    srcfileName = desfolderdir + "\\" + srcfileName;

                    if (!Directory.Exists(desfolderdir))
                    {
                        Directory.CreateDirectory(desfolderdir);
                    }

                    File.Copy(file, srcfileName);
                }
            }
        }


        /// <summary>
        /// 检查异常中断中的任务
        /// </summary>
        private static void CheckCrash()
        {
            string runnerFile = _servicePort + "\\sys.ruuners";
            if (File.Exists(runnerFile))
            {
                _statusCenter = Serializer.DeserializeFromFile<Hashtable>(runnerFile);
                foreach (DictionaryEntry dictionaryEntry in _statusCenter)
                {
                    var status = (RunningStatus)dictionaryEntry.Value;
                    //Runner runner = (Runner)dictionaryEntry.Value;
                    if (status.Description == "Running")
                    {
                        var runner = new Runner(ref status);
                        _runner.Add(status.Packet.AppName, runner);
                        runner.StartRun(_machineName, _servicePort);
                    }
                }
            }
        }

        public static void SetStatus(string key, string status)
        {
            if (_statusCenter[key] != null)
            {
                ((RunningStatus)_statusCenter[key]).Description = status;
                Backup();
            }
        }

        /// <summary>
        /// 备份执行状态
        /// </summary>
        private static void Backup()
        {
            string runnerFile = _servicePort + "\\sys.ruuners";
            if (File.Exists(runnerFile))
            {
                File.Delete(runnerFile);
            }
            if (_statusCenter.Count > 0)
            {
                Serializer.SerializeToFile(_statusCenter, runnerFile);
            }
        }

        /// <summary>
        /// 发送心跳
        /// </summary>
        private static void SendHeartbeat()
        {
            while (true)
            {
                Framework.Network.Synchronous.Client client = new Framework.Network.Synchronous.Client(Dns.GetHostName(),
            8600);
                string information = Process.GetCurrentProcess().Id + "|" + _servicePort + "?" + DateTime.UtcNow.ToString();
                Packet packet = new Packet(Encoding.UTF8.GetBytes(information));
                packet.WaiteCallBack = false;
                client.Send<bool>(packet);
                Thread.Sleep(30 * 1000);
            }
        }

#if DEBUG

        [TestMethod]
        public void TestProcessTask()
        {
        }

#endif
    }
}