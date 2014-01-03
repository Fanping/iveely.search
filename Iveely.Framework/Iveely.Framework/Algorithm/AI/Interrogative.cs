/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using SharpICTCLAS;

namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 疑问句
    /// </summary>
    public class Interrogative
    {

        public Tuple<List<Tuple<string, string>>, List<Tuple<string, string, string>>> Understand(
            List<WordResult[]> words)
        {
            SentenceGetter sentenceGetter = new SentenceGetter();
          
            string[] locations = sentenceGetter.GetLocations(words);
            foreach (var location in locations)
            {
                Console.WriteLine("location:" + location);
            }

            string[] names = sentenceGetter.GetNames(words);
            foreach (var name in names)
            {
                Console.WriteLine("name:" + name);
            }

            string[] times = sentenceGetter.GetTime(words);
            foreach (var time in times)
            {
                Console.WriteLine("time:" + time);
            }

            string[] events = sentenceGetter.GetEvent(words);
            foreach (var @event in events)
            {
                Console.WriteLine("event:" + @event);
            }

            string[] agences = sentenceGetter.GetAgency(words);
            foreach (var agence in agences)
            {
                Console.WriteLine(agence);
            }

            return null;
        }

        /// <summary>
        /// 是否有条件构建疑问句
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool IntegrityCheck(List<WordResult[]> result)
        {
            List<string> flags = new List<string>();
            //必须有动词
            bool hasV = false;
            //必须有名词
            bool hasN = false;
            for (int i = 0; i < result.Count; i++)
            {
                for (int j = 1; j < result[i].Length - 1; j++)
                {
                    string str = Utility.GetPOSString(result[i][j].nPOS);
                    if (!flags.Contains(str))
                        flags.Add(str);
                    if (str.Contains("v"))
                    {
                        hasV = true;
                    }
                    if (str.Contains("r"))
                    {
                        //不能有代词
                        return false;
                    }
                    if (str.Contains("n"))
                    {
                        hasN = true;
                    }
                }
            }
            return flags.Count > 2 && hasV && hasN;
        }

    }
}
