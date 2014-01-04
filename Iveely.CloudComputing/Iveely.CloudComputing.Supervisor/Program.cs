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
            //启动监控器
            Thread thread = new Thread(KeepMonitor);
            thread.Start();

            //心跳检查
            CheckLive();
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
                Thread.Sleep(60 * 1000);
                foreach (DictionaryEntry entry in table)
                {
                    DateTime dateTime = (DateTime)entry.Value;
                    TimeSpan timeSpan = DateTime.UtcNow - dateTime;
                    if (timeSpan.TotalSeconds > 59)
                    {
                        //重新启动该进程
                        Reboot(entry.Key.ToString());
                    }
                }
            }
        }

        private static void Reboot(string processInfor)
        {
            //先杀掉该进程
            string[] processStrings = processInfor.Split(new[] { '|' });
            string processId = processStrings[0];
            string port = processStrings[1];
            Process killProcess = System.Diagnostics.Process.GetProcessById(int.Parse(processId));
            if (killProcess != null)
                killProcess.Kill();

            //重新启动
            Process newProcess = new Process();
            newProcess.StartInfo.FileName = "Iveely.CloudComputing.Worker.exe";
            newProcess.StartInfo.Arguments = port;
            newProcess.Start();
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
                    if (information.Length > 2)
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
