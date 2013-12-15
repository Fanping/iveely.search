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
using Iveely.Framework.Network.Synchronous;

namespace Iveely.SearchEngine
{
    public class Host : Application
    {
        public static void Main()
        {
            Host host = new Host();
            host.Run(null);
        }

        public override void Run(object[] args)
        {
            //long timestamp = DateTime.UtcNow.ToFileTimeUtc();
            IEnumerable<string> workers = GetAllWorkers();
            int i = 0;
            if (workers != null)
            {
                while (true)
                {
                    Console.Write("Enter your query:");
                    string query = Console.ReadLine();
                    string result = GetGlobalCache<string>(query);
                    if (string.IsNullOrEmpty(result))
                    {
                        string timestamp = DateTime.UtcNow.ToLongDateString();
                        SetGlobalCache("CurrentQueryIndex", timestamp);
                        SetGlobalCache(timestamp, query);
                        Thread.Sleep(100);
                        foreach (string worker in workers)
                        {
                            result += GetGlobalCache<string>(worker + ":" + query);
                        }
                    }
                    Console.WriteLine(result);
                }
            }
            Console.WriteLine("Not found any workers!");
        }
    }
}
