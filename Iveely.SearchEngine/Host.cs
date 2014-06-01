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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Iveely.CloudComputing.Client;
using Iveely.Database;
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
    public partial class Host
    {
        /// <summary>
        /// 内存索引
        /// </summary>
        private static Hashtable TextIndexData = new Hashtable();

        /// <summary>
        ///  网页数据
        /// </summary>
        private static ITable<string, Crawler.Page> SearchDataTable;

        /// <summary>
        /// 分词组件
        /// </summary>
        private static Iveely.Framework.Text.Segment.DicSplit segment;

        private static void Main(string[] args)
        {
            //Index index = new Index();
            //object[] objects = new object[] { 8001, 8001, 8001, 8001, 8001, 8001, 8001, 8001, 8001 };
            //index.Run(objects);

            //return;
            Init();
            WebSocketServer webSocket = new WebSocketServer(GetTextResult);
            webSocket.StartServer();
        }

        private static void Init()
        {
            // 加载分词组件
            segment = DicSplit.GetInstance();

            // 加载文本索引文件
            LoadTextIndex();

            // 加载数据文件
            LoadRawData();
        }

        private static string GetTextResult(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return string.Empty;
            }
            try
            {
                // 1.分词
                string[] keywords = segment.Do(query);

                // 2.获取相应文档集合
                IntTable<string, double> docsTable = new IntTable<string, double>();
                foreach (var keyword in keywords)
                {
                    object obj = TextIndexData[keyword];
                    if (obj == null)
                    {
                        continue;
                    }
                    List<Iveely.Data.Slots<string, double>> textIndexs = (List<Iveely.Data.Slots<string, double>>)obj;
                    if (textIndexs != null)
                    {
                        foreach (var slotse in textIndexs)
                        {
                            docsTable.Add(slotse.Slot0, slotse.Slot1);
                        }
                    }
                }

                // 3.文档排序

                // 4.提取文档数据
                List<Crawler.Page> list = new List<Crawler.Page>();
                foreach (string url in docsTable.Keys)
                {
                    Crawler.Page page = SearchDataTable.Find(url);
                    if (page != null)
                    {
                        list.Add(page);
                        if (list.Count > 9)
                        {
                            break;
                        }
                    }
                }

                // 5.截取摘要返回
                string result = "";
                foreach (var page in list)
                {
                    result += BuildTextResult(ChangeColor(keywords, page.Title), CheckContent(page.Content, keywords),
                        page.Url, page.Timestamp, "");
                }

                return result;
            }
            catch (Exception exception)
            {
                return exception.ToString();
            }

        }

        private static void LoadTextIndex()
        {
            string indexFilePath =
                @"C:\Private\Iveely\项目\Iveely\Iveely.CloudComputing\Release\8001\ISE\Iveely_Search_Engine_TextIndex.db4";
            using (IStorageEngine textIndexEngine = STSdb.FromFile(indexFilePath))
            {
                ITable<string, List<Iveely.Data.Slots<string, double>>> indexTable = textIndexEngine.OpenXTable<string, List<Iveely.Data.Slots<string, double>>>("WebPage");
                foreach (var kv in indexTable)
                {
                    if (!TextIndexData.ContainsKey(kv.Key))
                    {
                        TextIndexData[kv.Key] = kv.Value;
                    }
                }
            }
        }

        private static void LoadRawData()
        {
            string dataPath =
               @"C:\Private\Iveely\项目\Iveely\Iveely.CloudComputing\Release\8001\ISE\Iveely_Search_Engine_Pages.db4";
            IStorageEngine dataEngine = STSdb.FromFile(dataPath);//.OpenXFile(indexFilePath);
            dataEngine.OpenXFile(dataPath);
            SearchDataTable = dataEngine.OpenXTable<string, Crawler.Page>("WebPage");
        }

        private static string GetRelativeResult(string query)
        {
            return "this is relatice.";
        }

        private static string GetKnownledgeResult(string query)
        {
            return "this is knownledge.";
        }


        /// <summary>
        ///   节选文本中的摘要数据
        /// </summary>
        /// <param name="content"> 文本内容 </param>
        /// <param name="keys"> 关键字集合 </param>
        /// <returns> 返回摘要 </returns>
        private static string CheckContent(string content, string[] keys)
        {
            string text = content;
            string[] matchtext = keys;
            string result = "";
            int chidLen = 80 / matchtext.Length;
            var place = new List<int>();
            foreach (string s in matchtext)
            {
                place.Add(text.IndexOf(s, StringComparison.Ordinal));
            }
            //place.Sort();
            int lastEnd = 0;
            for (int i = 0; i < place.Count; i++)
            {
                int start = place[i] > chidLen / 2 ? place[i] - chidLen / 2 : 0;
                while (start < lastEnd)
                {
                    start++;
                }

                int end = place[i] < text.Length - chidLen / 2 ? place[i] + chidLen / 2 : text.Length;
                if (end - start > keys[i].Length)
                {
                    result += text.Substring(start, end - start) + "  ";
                }
                else if (end - lastEnd > chidLen / 2)
                {
                    result += text.Substring(lastEnd, end - lastEnd);
                }
                lastEnd = end;
            }
            //更换颜色
            result = ChangeColor(keys, result);
            return result;
        }

        /// <summary>
        ///   替换颜色
        /// </summary>
        /// <param name="keys"> 关键字 </param>
        /// <param name="result"> 结果 </param>
        private static string ChangeColor(string[] keys, string result)
        {
            foreach (string key in keys)
            {
                if (key != "")
                {
                    result = result.Replace(key, "<b>" + key + "</b>");
                }
            }
            return result;
        }

        private static string BuildTextResult(string title, string content, string url, string date, string classify)
        {
            string formatInfor =
             string.Format(
                 "<h3 class='title'><a href='" + url + "'  target='_blank'>{0}</a></h3><span class='url'>{2}</span></br><div class='desc'>{1}<div><span class='date'>发布日期:{3}  所属领域:{4}</span>",
                 title,
                 content,
                 url.Trim().Length > 80 ? url.Trim().Substring(0, 79) + "..." : url.Trim(),
                 date,
                 classify);
            return "<li class='record'>" + formatInfor + "</li>";

        }
    }
}
