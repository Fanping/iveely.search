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
using System.Net.Mime;
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
using System.Net;
using Iveely.Framework.Process;

namespace Iveely.SearchEngine
{
    public partial class Host : Application
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

            Crawler crawler = new Crawler();
            crawler.Run(new object[] { 8001, 8001, 8001, 8001, 8001, 8001 });

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


        public static void Learn(string corpusFolder)
        {
            Hashtable table = new Hashtable();
            string[] dirs = Directory.GetDirectories(corpusFolder);
            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    string[] context = File.ReadAllText(file).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string lastSem = string.Empty;
                    for (int i = 0; i < context.Length; i++)
                    {
                        string[] text = context[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                        if (text.Length == 2)
                        {
                            //词与词性
                            if (!table.ContainsKey(text[0]))
                            {
                                table.Add(text[0], text[1]);
                            }
                            //_wordSemantic.Add(text[0], text[1]);
                        }
                    }
                }
            }
            Serializer.SerializeToFile(table,"KeyAnd.ser");
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
