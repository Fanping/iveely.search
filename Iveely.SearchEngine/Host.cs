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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iveely.CloudComputing.Client;
using Iveely.CloudComputing.StateAPI;
using Iveely.Framework.Algorithm.AI;
using Iveely.Framework.Text;
using Iveely.Framework.Text.Segment;

namespace Iveely.SearchEngine
{
    public class Host
    {

        public static void Main()
        {
            //Test test = new Test();
            //test.Run(new object[] { 0, 0, 0, 0, 0, 0 });
            Backstage collector = new Backstage();
            collector.Run(new object[] { 0, 0, 0, 0, 0, 0 });
            //Framework.Text.Segment.Participle.Test();
            //Framework.Text.Segment.Participle.Test();
            ////Client client = new Client("Fanping-pc", 6778);
            ////byte[] bytes = new byte[10];
            ////Packet packet = new Packet();
            ////packet.Data = bytes;
            ////client.Send<byte[]>(packet);

            Console.ReadKey();
        }

        
    }
}
