/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：
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

namespace Iveely.Framework.Algorithm.AI
{
    /// <summary>
    /// 疑问句
    /// </summary>
    public class Interrogative
    {

        //public Tuple<List<Tuple<string, string>>, List<Tuple<string, string>>> Understand(
        //    string input)
        //{
        //    return null;
        //    //List<WordResult[]> words = Text.Segment.IctclasSegment.GetInstance().SplitToArray(input);
        //    //SentenceGetter sentenceGetter = new SentenceGetter();
        //    //List<Tuple<string, string>> questions = new List<Tuple<string, string>>();
        //    //List<Tuple<string, string>> relativeBody = new List<Tuple<string, string>>();
        //    //List<string> bodys = new List<string>();

        //    //// 1.构建疑问句
        //    //string[] locations = sentenceGetter.GetLocations(words);
        //    //foreach (var location in locations)
        //    //{
        //    //    int index = input.IndexOf(location);
        //    //    if (index == -1)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    string str = input.Substring(0, index);
        //    //    str += "什么地方";
        //    //    str += input.Substring(index + location.Length, input.Length - index - location.Length);
        //    //    Tuple<string, string> tuple = new Tuple<string, string>(str, location);
        //    //    questions.Add(tuple);
        //    //}
        //    //if (locations.Any())
        //    //    bodys.AddRange(locations);

        //    //string[] names = sentenceGetter.GetNames(words);
        //    //foreach (var name in names)
        //    //{
        //    //    int index = input.IndexOf(name);
        //    //    if (index == -1)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    string str = input.Substring(0, index);
        //    //    str += "谁";
        //    //    str += input.Substring(index + name.Length, input.Length - index - name.Length);
        //    //    Tuple<string, string> tuple = new Tuple<string, string>(str, name);
        //    //    questions.Add(tuple);
        //    //}
        //    //if (names.Any())
        //    //    bodys.AddRange(names);

        //    //string[] times = sentenceGetter.GetTime(words);
        //    //foreach (var time in times)
        //    //{
        //    //    int index = input.IndexOf(time);
        //    //    if (index == -1)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    string str = input.Substring(0, index);
        //    //    str += "什么时候";
        //    //    str += input.Substring(index + time.Length, input.Length - index - time.Length);
        //    //    Tuple<string, string> tuple = new Tuple<string, string>(str, time);
        //    //    questions.Add(tuple);
        //    //}
        //    //if (times.Any())
        //    //    bodys.AddRange(times);

        //    //string[] events = sentenceGetter.GetEvent(words);
        //    //foreach (var @event in events)
        //    //{
        //    //    int index = input.IndexOf(@event);
        //    //    if (index == -1)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    string str = input.Substring(0, index);
        //    //    str += "什么";
        //    //    str += input.Substring(index + @event.Length, input.Length - index - @event.Length);
        //    //    Tuple<string, string> tuple = new Tuple<string, string>(str, @event);
        //    //    questions.Add(tuple);
        //    //}
        //    //if (events.Any())
        //    //    bodys.AddRange(events);

        //    //string[] agences = sentenceGetter.GetAgency(words);
        //    //foreach (var agence in agences)
        //    //{
        //    //    int index = input.IndexOf(agence);
        //    //    if (index == -1)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    string str = input.Substring(0, index);
        //    //    str += "谁";
        //    //    str += input.Substring(index + agence.Length, input.Length - index - agence.Length);
        //    //    Tuple<string, string> tuple = new Tuple<string, string>(str, agence);
        //    //    questions.Add(tuple);
        //    //}
        //    //if (agences.Any())
        //    //    bodys.AddRange(agences);

        //    //// 2 构建对象关系
        //    //foreach (var bodyA in bodys)
        //    //{
        //    //    foreach (var bodyB in bodys)
        //    //    {
        //    //        if (bodyA != bodyB)
        //    //        {
        //    //            Tuple<string, string> tuple = new Tuple<string, string>(bodyA, bodyB);
        //    //            relativeBody.Add(tuple);
        //    //        }

        //    //    }
        //    //}

        //    //Tuple<List<Tuple<string, string>>, List<Tuple<string, string>>> result =
        //    //    new Tuple<List<Tuple<string, string>>, List<Tuple<string, string>>>(questions, relativeBody);
        //    //return result;
        //}

        ///// <summary>
        ///// 是否有条件构建疑问句
        ///// </summary>
        ///// <param name="result"></param>
        ///// <returns></returns>
        //private bool IntegrityCheck(List<WordResult[]> result)
        //{
        //    List<string> flags = new List<string>();
        //    //必须有动词
        //    bool hasV = false;
        //    //必须有名词
        //    bool hasN = false;
        //    for (int i = 0; i < result.Count; i++)
        //    {
        //        for (int j = 1; j < result[i].Length - 1; j++)
        //        {
        //            string str = Utility.GetPOSString(result[i][j].nPOS);
        //            if (!flags.Contains(str))
        //                flags.Add(str);
        //            if (str.Contains("v"))
        //            {
        //                hasV = true;
        //            }
        //            if (str.Contains("r"))
        //            {
        //                //不能有代词
        //                return false;
        //            }
        //            if (str.Contains("n"))
        //            {
        //                hasN = true;
        //            }
        //        }
        //    }
        //    return flags.Count > 2 && hasV && hasN;
        //}

    }
}
