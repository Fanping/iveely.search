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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Iveely.CloudComputing.Client;
using Iveely.Framework.Algorithm.AI;
using Iveely.Framework.Algorithm.AI.Library;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.NLP;
using Iveely.Framework.Text;
using Iveely.Framework.Text.Segment;
using System.Collections;

namespace Iveely.SearchEngine
{
    public class Host : Application
    {
        private Library library = new Library();

        public class Temp
        {
            public string Name;

            public override int GetHashCode()
            {
                Name = "123";
                int val = Name.GetHashCode();
                return val;
            }
        }

        public static void Main(string[] args)
        {

            #region 信息抽取

            InvertEntity invert = new InvertEntity();
            string[] times = { "1998年" };
            string[] places = { "台湾" };
            string[] whoms = { "马英九", "国民政府" };
            string[] events = { "加入" };
            invert.Add(times, places, whoms, events);
            string[] mywhom = { "马英九" };
            List<string> result= invert.Query(Common.Sentence.Time, null, places, mywhom, events);
            foreach(string str in result)
            {
                Console.WriteLine(str);
            }

            #endregion

            Console.WriteLine("end");
            //QuestionGetter getter = new QuestionGetter();
            //getter.Run(new object[] { 8001, 8001, 8001, 8001, 8001, 8001 });

            //if (args.Length > 0)
            //{
            //    Host host = new Host();
            //    host.Run(null);
            //}
            //else
            //{
            //    Backstage backstage = new Backstage();
            //    backstage.Run(new object[] { 8001, 8001, 8001, 8001, 8001, 8001 });
            //}
            Console.ReadKey();
        }

        public override void Run(object[] args)
        {
            while (true)
            {
                Console.Write("Text Query Words:");
                string query = Console.ReadLine();
                Console.WriteLine(library.TextQuery(query));

                Console.Write("Relative Query Word:");
                query = Console.ReadLine();
                Console.WriteLine(library.RelativeQuery(query));
            }
        }
    }
}
