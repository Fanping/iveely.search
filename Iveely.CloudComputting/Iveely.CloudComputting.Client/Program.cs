/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Iveely.CloudComputting.StateAPI;
using Iveely.CloudComputting.StateCommon;
using Iveely.Framework.Log;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;

namespace Iveely.CloudComputting.Client
{
    public class Program
    {
        private static Server _server;

        /// <summary>
        /// 供提交客户端应用程序
        /// 格式：submit filepath namespace.classname appname
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //0. 检查传递参数
            if (args == null || args.Length < 4)
            {
                Console.WriteLine("arguments can not be null,press any key to exit.");
                Console.ReadKey();
                return;
            }

            //1. 编译应用程序
            Logger.Info("Start Compile your code...");
            string appName = args[3];
            string className = args[2];
            string filePath = args[1];
            string timeStamp = DateTime.Now.ToFileTimeUtc().ToString();
            string compileResult = CompileCode(filePath);
            if (compileResult != string.Empty)
            {
                Logger.Error(compileResult);
                Logger.Info("Please modify your code as the error said. press any key to exit.");
                Console.ReadLine();
                return;
            }

            //2. 读取编译后的文件
            Logger.Info("Preparing for send your application to platform...");
            string sourceCode = File.ReadAllText(filePath);
            byte[] bytes = Encoding.UTF8.GetBytes(sourceCode);

            //2. 上传程序至各个节点
            Thread thread = new Thread(StartListen);
            thread.Start();

            StateHelper.Put("ISE://history/" + timeStamp + "/" + appName,
              Dns.GetHostName());

            IEnumerable<string> ipPathes = StateHelper.GetChildren("ISE://system/state/worker");
            foreach (var ipPath in ipPathes)
            {
                string[] ip = ipPath.Substring(ipPath.LastIndexOf('/') + 1, ipPath.Length - ipPath.LastIndexOf('/') - 1).Split(',');
                Framework.Network.Synchronous.Client transfer = new Framework.Network.Synchronous.Client(ip[0], int.Parse(ip[1]));
                CodePacket codePacket = new CodePacket(bytes, className, appName, timeStamp);
                codePacket.SetReturnAddress(Dns.GetHostName(), 8800);
                transfer.Send<object>(codePacket);
            }

            //3. 结点运行程序，直至结束
            DateTime submitTime = DateTime.UtcNow;
            while (!IsDelay(submitTime, 60))
            {
                if (CheckApplicationExit(timeStamp, appName, ipPathes.Count()))
                {
                    Console.WriteLine("Application has finished,press any key to exit.");
                    Console.ReadLine();
                    return;
                }
                Thread.Sleep(60 * 1000);
            }
            Console.WriteLine("Application failured as run too much time,press any key to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// 启动返回数据监听
        /// </summary>
        private static void StartListen()
        {
            if (_server == null)
            {
                _server = new Server(Dns.GetHostName(), 8800, ProcessResponse);
                _server.Listen();
            }
        }

        /// <summary>
        /// 是否已经运行超时
        /// </summary>
        /// <param name="sumbitTime">应用程序提交时间</param>
        /// <param name="allowMaxTime">允许最长运行时间(单位：分)</param>
        /// <returns></returns>
        private static bool IsDelay(DateTime sumbitTime, long allowMaxTime)
        {
            return (DateTime.UtcNow - sumbitTime).TotalMinutes > allowMaxTime;
        }

        /// <summary>
        /// 检查应用程序是否已经退出
        /// </summary>
        /// <returns></returns>
        private static bool CheckApplicationExit(string timeStamp, string appName, int workerCount)
        {
            IEnumerable<string> finishedStates = StateHelper.GetChildren("ISE://application/" + timeStamp + "/" + appName);
            if (finishedStates.Any() && finishedStates.Count() == workerCount)
            {
                foreach (var finishedState in finishedStates)
                {
                    Logger.Info(finishedState + ":" + StateHelper.Get<string>(finishedState));
                }
                return true;
            }
            return false;
        }

        private static string CompileCode(string fileName)
        {
            if (File.Exists(fileName))
            {
                //Framework.Text.CodeCompiler compiler = new CodeCompiler();
                List<string> references = new List<string>();
                references.Add("Iveely.CloudComputting.Client.exe");
                return CodeCompiler.Compile(File.ReadAllLines(fileName), references);
            }
            throw new FileNotFoundException(fileName + " is not found!");
        }

        private static byte[] ProcessResponse(byte[] bytes)
        {
            Packet packet = Serializer.DeserializeFromBytes<Packet>(bytes);
            byte[] dataBytes = packet.Data;
            string information = Serializer.DeserializeFromBytes<string>(dataBytes);

            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(string.Format("[Response {0}] {1}", DateTime.UtcNow.ToString(), information));
            Console.ForegroundColor = color;
            return null;
        }
    }
}
