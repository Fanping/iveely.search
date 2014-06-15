/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using Iveely.Data;
using Iveely.Database;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Text;
using System.Collections;

namespace Iveely.SearchEngine
{
    public partial class Host
    {
        /// <summary>
        /// 文本内存索引
        /// </summary>
        private static readonly Hashtable TextIndexData = new Hashtable();

        /// <summary>
        /// 知识引擎内存索引
        /// </summary>
        private static readonly Hashtable KnowledgeIndexData = new Hashtable();

        /// <summary>
        ///  网页数据
        /// </summary>
        private static ITable<string, Crawler.Page> _searchDataTable;

        /// <summary>
        /// 知识引擎数据
        /// </summary>
        private static ITable<long, KnowlegeIndex.KnowledgeEntity> _knowledgeDataTable;

        /// <summary>
        /// 分词组件
        /// </summary>
        private static HMMSegment segment;

        private static void Main(string[] args)
        {
            //搜索服务
            Init();
            WebSocketServer webSocket = new WebSocketServer(GetResult);
            webSocket.StartServer();

        }

        private static void Init()
        {
            // 加载分词组件
            Console.WriteLine("[1/6]正在加载分词组件...");
            segment = HMMSegment.GetInstance();

            // 加载文本索引文件
            Console.WriteLine("[2/6]正在加载文本索引数据...");
            LoadTextIndex();

            // 加载数据文件
            Console.WriteLine("[3/6]正在预加载文本数据...");
            LoadRawData();

            // 加载知识引擎数据
            Console.WriteLine("[4/6]正在加载知识引擎数据...");
            LoadKnowledgeData();

            // 加载知识引擎索引数据
            Console.WriteLine("[5/6]正在加载知识索引数据...");
            LoadKnowledgeIndex();
        }

        private static string GetResult(string query)
        {
            if (query.Trim().Length < 1)
            {
                return "您的输入过短.";
            }
            string[] keywords = segment.Split(query);
            string knowledgeResult = GetKnownledgeResult(keywords);
            string textResult = GetTextResult(keywords);
            string result = knowledgeResult + textResult;
            if (result.Trim().Length < 1)
            {
                return "未找到任何结果.";
            }
            return result;
        }

        private static string GetTextResult(string[] keywords)
        {
            try
            {
                // 1.获取相应文档集合
                IntTable<string, double> docsTable = new IntTable<string, double>();
                foreach (var keyword in keywords)
                {
                    object obj = TextIndexData[keyword];
                    if (obj == null)
                    {
                        continue;
                    }
                    List<Slots<string, double>> textIndexs = (List<Slots<string, double>>)obj;
                    if (textIndexs != null)
                    {
                        foreach (var slotse in textIndexs)
                        {
                            docsTable.Add(slotse.Slot0, slotse.Slot1);
                        }
                    }
                }

                // 2.文档排序
                List<string> sortedList = new List<string>();
                ArrayList list = new ArrayList(docsTable.Values);
                list.Sort();
                list.Reverse();
                int maxCount = 9;
                foreach (double svalue in list)
                {
                    maxCount--;
                    if (maxCount < 0)
                    {
                        break;
                    }
                    IDictionaryEnumerator ide = docsTable.GetEnumerator();
                    while (ide.MoveNext())
                    {
                        if (Math.Abs((double)ide.Value - svalue) < 0.00000001)
                        {
                            sortedList.Add(ide.Key.ToString());
                        }
                    }
                }

                // 3.提取文档数据
                List<Crawler.Page> doclist = new List<Crawler.Page>();
                HashSet<string> filter = new HashSet<string>();
                foreach (string url in sortedList)
                {
                    Crawler.Page page = _searchDataTable.Find(url);
                    if (page != null && !filter.Contains(page.Url))
                    {
                        doclist.Add(page);
                        filter.Add(page.Url);
                    }
                }

                // 4.截取摘要返回
                string result = "";
                foreach (var page in doclist)
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
            string indexFilePath = "TextSearch\\ISE_Pages_Index.db4";
            using (IStorageEngine textIndexEngine = STSdb.FromFile(indexFilePath))
            {
                ITable<string, List<Slots<string, double>>> indexTable = textIndexEngine.OpenXTable<string, List<Slots<string, double>>>("WebPage");
                foreach (var kv in indexTable)
                {
                    if (!TextIndexData.ContainsKey(kv.Key))
                    {
                        TextIndexData[kv.Key] = kv.Value;
                    }
                }
            }
        }

        private static void LoadKnowledgeIndex()
        {
            string indexFilePath = "Baike\\Baike_question_index.db4";
            using (IStorageEngine textIndexEngine = STSdb.FromFile(indexFilePath))
            {
                ITable<string, List<Slots<long, double>>> indexTable = textIndexEngine.OpenXTable<string, List<Slots<long, double>>>("WebPage");
                foreach (var kv in indexTable)
                {
                    if (!KnowledgeIndexData.ContainsKey(kv.Key))
                    {
                        KnowledgeIndexData[kv.Key] = kv.Value;
                    }
                }
            }
        }

        private static void LoadKnowledgeData()
        {
            string dataPath = "Baike\\Baike_question.db4";
            IStorageEngine dataEngine = STSdb.FromFile(dataPath);
            dataEngine.OpenXFile(dataPath);
            _knowledgeDataTable = dataEngine.OpenXTable<long, KnowlegeIndex.KnowledgeEntity>("WebPage");
        }

        private static void LoadRawData()
        {
            string dataPath = "TextSearch\\ISE_Pages.db4";
            IStorageEngine dataEngine = STSdb.FromFile(dataPath);//.OpenXFile(indexFilePath);
            dataEngine.OpenXFile(dataPath);
            _searchDataTable = dataEngine.OpenXTable<string, Crawler.Page>("WebPage");
        }

        private static string GetKnownledgeResult(string[] keywords)
        {
            // 1.获取相应文档集合
            IntTable<long, double> docsTable = new IntTable<long, double>();
            foreach (var keyword in keywords)
            {
                object obj = KnowledgeIndexData[keyword];
                if (obj == null)
                {
                    continue;
                }
                List<Slots<long, double>> dataIndexs = (List<Slots<long, double>>)obj;
                if (dataIndexs != null)
                {
                    foreach (var slotse in dataIndexs)
                    {
                        docsTable.Add(slotse.Slot0, slotse.Slot1);
                    }
                }
            }

            // 2.文档排序
            List<long> sortedList = new List<long>();
            ArrayList list = new ArrayList(docsTable.Values);
            list.Sort();
            list.Reverse();
            int maxCount = 5;
            foreach (double svalue in list)
            {
                maxCount--;
                if (maxCount < 0)
                {
                    break;
                }
                IDictionaryEnumerator ide = docsTable.GetEnumerator();
                while (ide.MoveNext())
                {
                    if (Math.Abs((double)ide.Value - svalue) < 0.00000001)
                    {
                        sortedList.Add((long)ide.Key);
                    }
                }
            }

            // 3.提取文档数据
            List<KnowlegeIndex.KnowledgeEntity> doclist = new List<KnowlegeIndex.KnowledgeEntity>();
            int cou = 1;
            HashSet<long> filter = new HashSet<long>();
            foreach (long url in sortedList)
            {
                KnowlegeIndex.KnowledgeEntity entity = _knowledgeDataTable.Find(url);
                if (entity != null && !filter.Contains(entity.Id) && cou > 0)
                {
                    doclist.Add(entity);
                    filter.Add(entity.Id);
                    cou--;
                }
            }

            // 4.截取摘要返回
            string result = "";
            foreach (var page in doclist)
            {
                result += BuildTextResult(ChangeColor(keywords, "[知]"+page.QuestionDesc), "答案:" + page.Answer,
                    page.RefUrl, page.EffectTime, "");
            }

            return result;
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
                 "<h3 class='title'><a href='" + url + "'  target='_blank'>{0}</a></h3><span class='url'>{2}</span></br><div class='desc'>{1}<div><span class='date'>发布日期:{3}</span>",
                 title,
                 content,
                 url.Trim().Length > 120 ? url.Trim().Substring(0, 119) + "..." : url.Trim(),
                 date,
                 classify);
            return "<li class='record'>" + formatInfor + "</li>";

        }
    }
}
