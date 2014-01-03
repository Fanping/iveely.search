/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Iveely.CloudComputing.Client;
using Iveely.Framework.Algorithm.AI;
using Iveely.Framework.Algorithm.AI.Library;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text.Segment;

namespace Iveely.SearchEngine
{
    public class Host : Application
    {
        public static void Main()
        {
            //Backstage backstage = new Backstage();
            //backstage.Run(new object[] { 8001, 8001, 8001, 8001, 8001, 8001 });
            const string delimiter = ".?。！\t？…●|\r\n])!";
            string[] sentences = new[] { "台北主题 台北主题首页" };//File.ReadAllText("TestData.txt", Encoding.UTF8).Split(delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string sentence in sentences)
            {
                if (sentence.Length >= 5)
                {
                    List<Template.Question> result = Bot.GetInstance("Init\\").BuildQuestion(sentence, "http://", "tutl");
                    foreach (var question in result)
                    {
                        Console.WriteLine(sentence);
                        Console.WriteLine(question.Description + "? " + question.Answer);
                        Console.WriteLine();
                    }
                }
            }
            Console.ReadKey();
            //Host host = new Host();
            //host.Run(null);
        }

        public override void Run(object[] args)
        {
            IEnumerable<string> workers = GetAllWorkers();
            var enumerable = workers as string[] ?? workers.ToArray();
            Console.WriteLine("Get Worker count:" + enumerable.Count());
            if (workers != null)
            {
                while (true)
                {
                    Console.Write("Enter your query:");
                    string query = Console.ReadLine();
                    string result = GetGlobalCache<string>(query);
                    Console.WriteLine("query:[{0}] in cache is {1}", query, result);
                    if (string.IsNullOrEmpty(result))
                    {
                        string timestamp = DateTime.UtcNow.ToLongDateString();
                        SetGlobalCache("CurrentQueryIndex", timestamp);
                        SetGlobalCache(timestamp, query);
                        int sendIndex = 9000;
                        List<string> cacheStore = new List<string>();
                        foreach (string worker in enumerable)
                        {
                            string[] workerInfo = worker.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            int endFlagIndex = workerInfo[0].LastIndexOf("/", StringComparison.Ordinal) + 1;
                            string ip = workerInfo[0].Substring(endFlagIndex, workerInfo[0].Length - endFlagIndex);

                            try
                            {
                                sendIndex += (int.Parse(workerInfo[1]) % 100);
                                Client client = new Client(ip, sendIndex);
                                Packet dataPacket = new Packet(new byte[1]) { WaiteCallBack = true };
                                client.Send<bool>(dataPacket);
                                cacheStore.Add(ip + "," + sendIndex);
                                sendIndex = 9000;
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }
                        }
                        Thread.Sleep(2000);
                        foreach (string ca in cacheStore)
                        {
                            try
                            {
                                string outputResult = ca + query;
                                Console.WriteLine(outputResult);
                                result += GetGlobalCache<string>(outputResult);
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }

                        }
                    }
                    Console.WriteLine("Finnal result :" + result);
                }
            }
            Console.WriteLine("Not found any workers!");
        }
    }
}
