using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Iveely.CloudComputing.Client;
using Iveely.Framework.Algorithm;
using Iveely.Framework.Algorithm.AI;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Log;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;
using Iveely.Framework.Text.Segment;


namespace Iveely.SearchEngine
{
    /// <summary>
    /// 搜索引擎
    /// </summary>
    public class Backstage : Application
    {
        /// <summary>
        /// 网页信息
        /// </summary>
        public class Page
        {
            /// <summary>
            /// 采集日期
            /// </summary>
            public string Date { get; set; }

            /// <summary>
            /// 页面链接
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// 页面标题
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// 页面正文
            /// </summary>
            public string Content { get; set; }


            public override string ToString()
            {
                return string.Format("{0}\t{1}\t{2}\t{3}", Date, Url, Title, Content.Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty));
            }
        }

        /// <summary>
        /// 爬虫爬行队列
        /// </summary>
        public List<string> Urls = new List<string>();

        /// <summary>
        /// 文本索引片段向量
        /// </summary>
        public static InvertFragment TextFragment;

        /// <summary>
        /// 相关性索引片段向量
        /// </summary>
        public static DimensionTable<string, string, double> RelativeTable;

        /// <summary>
        /// 爬虫爬行的跟站点
        /// </summary>
        private const string Host = "www.baike.com";

        /// <summary>
        /// 索引文件
        /// </summary>
        private string _textIndexFile;

        /// <summary>
        /// 相关性索引文件
        /// </summary>
        private string _relativeIndexFile;

        /// <summary>
        /// 搜索端口
        /// </summary>
        private int _searchPort = 9000;

        /// <summary>
        /// 主程序入口
        /// </summary>
        /// <param name="args"></param>
        public override void Run(object[] args)
        {
            //1. 初始化
            Init(args);
            TextFragment = new InvertFragment(GetRootFolder());
            RelativeTable = new DimensionTable<string, string, double>();
            Urls.Add("http://www.baike.com");
            _textIndexFile = GetRootFolder() + "\\InvertFragment.part";
            _relativeIndexFile = GetRootFolder() + "\\RelativeFragment.part";

            //Thread searchThread = new Thread(StartSearcher);
            //searchThread.Start();

            //2. 循环数据采集
            while (Urls.Count > 0)
            {
                List<Page> pages = new List<Page>();
                try
                {
                    //2.1 爬虫开始运行
                    Crawler(ref pages);
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception);
                }

                //2.2 索引器开始运行
                if (pages != null && pages.Count > 0)
                {
                    try
                    {
                        Indexer(ref pages);
                    }
                    catch (Exception exception)
                    {
                        Logger.Warn(exception);
                    }

                }

                //2.3 更新url
                try
                {
                    Urls.AddRange(GetKeysByValueFromCache(false, 10, true));
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception);
                }
            }
        }

        /// <summary>
        /// 爬虫主程序
        /// </summary>
        /// <param name="pages">搜集的网页信息</param>
        public void Crawler(ref List<Page> pages)
        {
            //1. 遍历url集合
            HashSet<string> newUrls = new HashSet<string>();

            for (int i = 0; i < Urls.Count; i++)
            {
                try
                {
                    //1.1 获取标题，网页正文，子链接集
                    //WriteToConsole("Processing " + Urls[i]);
                    string title = string.Empty;
                    string content = string.Empty;
                    List<string> childrenLink = null;
                    GetHtml(Urls[i], ref title, ref content, ref childrenLink);

                    //1.2 过滤子链接集
                    foreach (string link in childrenLink)
                    {
                        if (!newUrls.Contains(link) && (new Uri(link)).Host == Host)
                        {
                            newUrls.Add(link);
                        }
                    }

                    //1.3 记录数据
                    if (title != string.Empty)
                    {
                        Page page = new Page
                        {
                            Url = Urls[i],
                            Title = title,
                            Date = DateTime.Now.ToShortDateString(),
                            Content = content
                        };
                        pages.Add(page);

                    }
                }
                catch (Exception exception)
                {
                    WriteToConsole(exception.ToString());
                }

            }

            //3. 新的url标识为未爬行过，并存放于缓存中
            SetListIntoCache(newUrls.ToArray(), false);

            //4. 重新获取新的url
            Urls.Clear();

        }

        /// <summary>
        /// 索引程序入口
        /// </summary>
        /// <param name="pages">网页信息集合</param>
        public void Indexer(ref List<Page> pages)
        {
            //自动分析网页表达的含义
            //WriteToConsole(string.Format("开始自动分析网页表达的含义，共{0}条记录。", pages.Count));
            List<Template.Question> questions = new List<Template.Question>();
            const string delimiter = ".?。！\t？…●|\r\n])!";
            using (var database = Database.Open(GetRootFolder() + "\\Iveely.Search.Data.part"))
            {
                foreach (Page page in pages)
                {
                    bool isSavePage = false;
                    string[] sentences = page.Content.Split(delimiter.ToCharArray(),
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sentence in sentences)
                    {
                        if (sentence.Length >= 5)
                        {
                            Template.Question result = Bot.GetInstance(GetRootFolder())
                                .BuildQuestion(sentence, page.Url, page.Title);
                            if (result != null && result.Description != null && result.Description.Count > 0)
                            {
                                isSavePage = true;
                                result.Content = sentence;
                                questions.Add(result);
                            }
                        }
                    }
                    if (isSavePage)
                    {
                        database.Store(questions);
                    }
                }
            }
            pages.Clear();

            //对表达的语义建议索引
            // WriteToConsole(string.Format("对表达的语义建议索引，共{0}条记录。", questions.Count));

            if (File.Exists(_textIndexFile))
            {
                TextFragment = Serializer.DeserializeFromFile<InvertFragment>(_textIndexFile);
            }

            if (File.Exists(_relativeIndexFile))
            {
                RelativeTable =
                    Serializer.DeserializeFromFile<DimensionTable<string, string, double>>(_relativeIndexFile);
            }

            using (var database = Database.Open(GetRootFolder() + "\\Iveely.Search.Question"))
            {
                if (questions.Any())
                {
                    foreach (Template.Question question in questions)
                    {
                        int id = question.Content.GetHashCode();
                        question.Id = id;
                        TextFragment.AddDocument(id, question.Content, false);
                        foreach (var entity in question.Entity)
                        {
                            double oldValue = RelativeTable[entity.Item1][entity.Item2] == null
                                ? 0
                                : RelativeTable[entity.Item1][entity.Item2];
                            RelativeTable[entity.Item1][entity.Item2] = oldValue + 0.0001;
                        }
                        database.Store(question);
                    }
                }
            }
            questions.Clear();
            Serializer.SerializeToFile(TextFragment, _textIndexFile);
            Serializer.SerializeToFile(RelativeTable, _relativeIndexFile);
        }

        public void StartSearcher()
        {
            if (File.Exists(_textIndexFile))
            {
                TextFragment = Serializer.DeserializeFromFile<InvertFragment>(_textIndexFile);
            }
            int servicePort = int.Parse(GetRootFolder()) % 100;

            try
            {
                _searchPort += servicePort;
                Server server = new Server(Dns.GetHostName(), _searchPort, ProcessQuery);
                WriteToConsole(_searchPort + " is start search service.");
                server.Listen();
            }
            catch (Exception exception)
            {
                WriteToConsole("Start Server Error." + exception);
            }
        }


        public byte[] ProcessQuery(byte[] bytes)
        {
            string currentQueryIndex = GetGlobalCache<string>("CurrentQueryIndex");
            if (!string.IsNullOrEmpty(currentQueryIndex))
            {
                string query = GetGlobalCache<string>(currentQueryIndex);
                WriteToConsole("Get Query:" + query);
                string[] keywords = NGram.GetGram(query, NGram.Type.BiGram);
                List<string> docs = TextFragment.FindCommonDocumentByKeys(keywords);
                List<Template.Question> result = new List<Template.Question>();
                using (var database = Database.Open(GetRootFolder() + "\\Iveely.Search.Question"))
                {
                    var wordsQuey = database.Query<Template.Question>();
                    int returnCount = 5;
                    foreach (string doc in docs)
                    {
                        if (returnCount == 0)
                            break;
                        returnCount--;
                        wordsQuey.Descend("Id").Constrain(int.Parse(doc)).Equal();
                        var questions = wordsQuey.Execute<Template.Question>();
                        if (questions != null && questions.Count > 0)
                        {
                            result.Add(questions.GetFirst());
                        }
                    }
                }
                string inputResultKey = Dns.GetHostName() + "," + _searchPort + query;
                WriteToConsole("Result write into cache key=" + inputResultKey + ", count=" + result.Count);
                //if (result.Count > 10)
                //{
                //    result.RemoveRange(10, result.Count - 10);
                //}
                string content = string.Join("\r\n", result);
                content = keywords.Aggregate(content, (current, keyword) => current.Replace(keyword, "<strong>" + keyword + "</strong>"));
                SetGlobalCache(inputResultKey, content);
            }
            return Serializer.SerializeToBytes(true);
        }
    }
}
