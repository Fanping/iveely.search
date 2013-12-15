/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iveely.CloudComputing.Client;
using Iveely.CloudComputing.StateAPI;
using Iveely.Framework.Text;


namespace Iveely.SearchEngine
{
    public class Test : Application
    {
        public override void Run(object[] args)
        {
            Init(args);
            SetCache("1", false);
            SetCache("2", false);
            Thread.Sleep(1000);
            Console.WriteLine(GetCache<bool>("1"));
            string[] results = GetKeysByValueFromCache(false, 10, true);
            Console.WriteLine(results.Count());
        }
    }

    public class Searcher
    {
        //private 

        public static void Main()
        {
            Test test = new Test();
            test.Run(new object[] { 0, 0, 0, 0, 0, 0 });
            //Collector collector = new Collector();
            //collector.Run(new object[] { 0, 0, 0, 0, 0, 0 });
            //Client client = new Client("Fanping-pc", 6778);
            //byte[] bytes = new byte[10];
            //Packet packet = new Packet();
            //packet.Data = bytes;
            //client.Send<byte[]>(packet);

        }
    }
}
