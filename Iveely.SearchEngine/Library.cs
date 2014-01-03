/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.CloudComputing.Client;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;

namespace Iveely.SearchEngine
{
    public class Library : Application
    {
        public string TextQuery(string keywords)
        {
            return Query(keywords, "Current-Text-Query");
        }

        public string RelativeQuery(string keyword)
        {
            return Query(keyword, "Current-Relative-Query");
        }

        public override void Run(object[] args)
        {
            throw new NotImplementedException();
        }

        private string Query(string keywords, string queryType)
        {
            IEnumerable<string> workers = GetAllWorkers();
            string result = string.Empty;
            var enumerable = workers as string[] ?? workers.ToArray();
            Console.WriteLine("Get Worker count:" + enumerable.Count());
            if (workers != null)
            {
                result = GetGlobalCache<string>(keywords);
                Console.WriteLine("query:[{0}] in cache is {1}", keywords, result);
                if (string.IsNullOrEmpty(result))
                {
                    string timestamp = DateTime.UtcNow.ToLongDateString();
                    SetGlobalCache(queryType, timestamp);
                    SetGlobalCache(timestamp, keywords);
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

                    foreach (string ca in cacheStore)
                    {
                        try
                        {
                            string outputResult = ca + keywords;
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
            Console.WriteLine("Not found any workers!");
            return result;
        }
    }
}
