using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iveely.Framework.Algorithm.AI.Library;
using Iveely.Framework.Log;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;

namespace Iveely.CloudComputing.Supervisor
{
    /// <summary>
    /// worker的守护进程
    /// </summary>
    public class Program
    {
        private static Hashtable table = new Hashtable();

        static void Main(string[] args)
        {
            //心跳检查
            Logger.Info("Check heatbeat...");
            Thread liveThread = new Thread(CheckLive);
            liveThread.Start();

            //启动监控器
            Logger.Info("Start the monitor...");
            Thread monitorThread = new Thread(KeepMonitor);
            monitorThread.Start();
        }

        private static void KeepMonitor()
        {
            Server server = new Server(Dns.GetHostName(), 8600, Heartbeat);
            server.Listen();
        }

        /// <summary>
        /// 每分钟一次的心跳检测
        /// </summary>
        private static void CheckLive()
        {
            while (true)
            {
                List<object> removedProcess = new List<object>();
                foreach (DictionaryEntry entry in table)
                {
                    DateTime dateTime = (DateTime)entry.Value;
                    TimeSpan timeSpan = DateTime.UtcNow - dateTime;
                    if (timeSpan.TotalSeconds > 59)
                    {
                        //重新启动该进程
                        Logger.Warn(entry.Key + " too long time breathy...would be reboot.");
                        Reboot(entry.Key.ToString());
                        removedProcess.Add(entry.Key);
                    }
                    else
                    {
                        Logger.Info(entry.Key + " is work fine!");
                    }
                }

                if (removedProcess.Count > 0)
                {
                    foreach (var pro in removedProcess)
                    {
                        table.Remove(pro);
                    }
                }
                Thread.Sleep(1000 * 60);
            }
        }

        private static void Reboot(string processInfor)
        {
            //先杀掉该进程
            string[] processStrings = processInfor.Split(new[] { '|' });
            string processId = processStrings[0];
            string port = processStrings[1];
            try
            {
                Process killProcess = System.Diagnostics.Process.GetProcessById(int.Parse(processId));
                if (killProcess != null)
                    killProcess.Kill();
            }
            catch (Exception exception)
            {
                Logger.Warn("When try to kill process,exception happen:" + exception);
            }

            //重新启动
            Process newProcess = new Process();
            newProcess.StartInfo.FileName = "Iveely.CloudComputing.Worker.exe";
            newProcess.StartInfo.Arguments = port;
            newProcess.Start();
            Logger.Info("reboot success!");
        }

        /// <summary>
        /// 心跳接收器
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static byte[] Heartbeat(byte[] bytes)
        {
            try
            {
                Packet packet = Serializer.DeserializeFromBytes<Packet>(bytes);
                string flag = Encoding.UTF8.GetString(packet.Data);
                if (flag != null)
                {
                    string[] information = flag.Split(new[] { '?' });
                    if (information.Length == 2)
                    {
                        string machine = information[0];
                        DateTime timestamp = DateTime.Parse(information[1]);
                        if (table.ContainsKey(machine))
                        {
                            table[machine] = timestamp;
                        }
                        else
                        {
                            table.Add(machine, timestamp);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception);
            }
            return bytes;
        }
    }
}
