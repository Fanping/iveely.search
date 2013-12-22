/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Iveely.CloudComputing.Client;
using Iveely.Framework.Algorithm;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;

namespace Iveely.SearchEngine
{
    public class Host : Application
    {
        public static void Main()
        {
            Host host = new Host();
            host.Run(null);
            //Backstage backstage = new Backstage();
            //backstage.Run(new object[] { 8001, 8001, 8001, 8001, 8001 ,8001,8001});
        }

        public override void Run(object[] args)
        {
            //long timestamp = DateTime.UtcNow.ToFileTimeUtc();
            IEnumerable<string> workers = GetAllWorkers();

            Console.WriteLine("Get Worker count:" + workers.Count());
            if (workers != null)
            {
                while (true)
                {
                    Console.Write("Enter your query:");
                    string query = Console.ReadLine();
                    string result = GetGlobalCache<string>(query);
                    Console.WriteLine(string.Format("query:[{0}] in cache is {1}", query, result));
                    if (string.IsNullOrEmpty(result))
                    {
                        string timestamp = DateTime.UtcNow.ToLongDateString();
                        SetGlobalCache("CurrentQueryIndex", timestamp);
                        SetGlobalCache(timestamp, query);
                        int sendIndex = 9000;
                        List<string> cacheStore = new List<string>();
                        foreach (string worker in workers)
                        {
                            string[] workerInfo = worker.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            int endFlagIndex = workerInfo[0].LastIndexOf("/") + 1;
                            string ip = workerInfo[0].Substring(endFlagIndex, workerInfo[0].Length - endFlagIndex);

                            try
                            {
                                sendIndex += (int.Parse(workerInfo[1]) % 100);
                                Client client = new Client(ip, sendIndex);
                                Packet dataPacket = new Packet(new byte[1]);
                                dataPacket.WaiteCallBack = true;
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
