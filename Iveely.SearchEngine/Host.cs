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
using Iveely.Framework.DataStructure;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;

namespace Iveely.SearchEngine
{
    public class Host : Application
    {
        public static void Main()
        {
            Host host = new Host();
            host.Run(null);
            //Backstage backstage = new Backstage();
            //backstage.Run(new object[] { 8001, 8001, 8001, 8001, 8001 });
        }

        public override void Run(object[] args)
        {
            //long timestamp = DateTime.UtcNow.ToFileTimeUtc();
            IEnumerable<string> workers = GetAllWorkers();

            Console.WriteLine("Get Worker count:" + workers.Count());
            int i = 0;
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
                        int sendIndex = 1;
                        string lastIp = string.Empty;
                        List<string> cacheStore = new List<string>();
                        foreach (string worker in workers)
                        {
                            string[] workerInfo = worker.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            int endFlagIndex = workerInfo[0].LastIndexOf("/") + 1;
                            string ip = workerInfo[0].Substring(endFlagIndex, workerInfo[0].Length - endFlagIndex);
                            if (lastIp == ip)
                            {
                                sendIndex++;
                            }
                            else
                            {
                                sendIndex = 1;
                                lastIp = ip;
                            }
                            try
                            {
                                Client client = new Client(ip, 9000 + sendIndex);
                                Packet dataPacket = new Packet(new byte[1]);
                                dataPacket.WaiteCallBack = false;
                                client.Send<string>(dataPacket);
                                cacheStore.Add(ip + "," + (9000 + sendIndex));
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }
                        }

                        foreach (string ca in cacheStore)
                        {
                            string outputResult = ca + query;
                            Console.WriteLine(outputResult);
                            result += GetGlobalCache<string>(outputResult);
                        }
                    }
                    Console.WriteLine("Finnal result :" + result);
                }
            }
            Console.WriteLine("Not found any workers!");
        }
    }
}
