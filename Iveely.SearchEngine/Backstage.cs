using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using Iveely.CloudComputing.Client;
using Iveely.Framework.Algorithm;
using Iveely.Framework.Algorithm.AI;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Log;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;


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
                return string.Format("{0}\t{1}\t{2}\t{3}", Date, Url, Title,
                    Content.Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty));
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

        public static Framework.Text.HMMSegment segment = HMMSegment.GetInstance();

        public static LocalStore<Template.Question> DataStore;

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
            DataStore = new LocalStore<Template.Question>(GetRootFolder() + "\\QuestionData.index",
                GetRootFolder() + "\\QuestionData", 100);

            //1.1 文本索引文件
            _textIndexFile = GetRootFolder() + "\\InvertFragment.part";
            if (File.Exists(_textIndexFile))
                TextFragment = Serializer.DeserializeFromFile<InvertFragment>(_textIndexFile);
            if (TextFragment == null)
            {
                TextFragment = new InvertFragment(GetRootFolder());
            }

            //1.2 相关索引文件
            _relativeIndexFile = GetRootFolder() + "\\RelativeFragment.part";
            if (File.Exists(_relativeIndexFile))
                RelativeTable = Serializer.DeserializeFromFile<DimensionTable<string, string, double>>(_relativeIndexFile);
            if (RelativeTable == null)
            {
                RelativeTable = new DimensionTable<string, string, double>();
            }
            Urls.Add("http://www.baike.com");

            //StartSearcher();
            Thread searchThread = new Thread(StartSearcher);
            searchThread.Start();

            Thread.Sleep(10000);

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

            foreach (Page page in pages)
            {
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
                            result.Content = sentence;
                            questions.Add(result);
                        }
                    }
                }

            }
            pages.Clear();

            //对表达的语义建议索引
            // WriteToConsole(string.Format("对表达的语义建议索引，共{0}条记录。", questions.Count));

            if (File.Exists(_textIndexFile) && TextFragment == null)
            {
                TextFragment = Serializer.DeserializeFromFile<InvertFragment>(_textIndexFile);
            }

            if (File.Exists(_relativeIndexFile) && RelativeTable == null)
            {
                RelativeTable =
                    Serializer.DeserializeFromFile<DimensionTable<string, string, double>>(_relativeIndexFile);
            }


            if (questions.Any())
            {
                foreach (Template.Question question in questions)
                {
                    int id = question.GetHashCode();
                    question.Id = id;
                    TextFragment.AddDocument(id, question.Content, false);
                    foreach (var entity in question.Entity)
                    {
                        double oldValue = RelativeTable[entity.Item1][entity.Item2] == null
                            ? 0
                            : RelativeTable[entity.Item1][entity.Item2];
                        RelativeTable[entity.Item1][entity.Item2] = oldValue + 0.0001;
                    }
                    DataStore.Write(question);
                }
            }
            questions.Clear();
            Serializer.SerializeToFile(TextFragment, _textIndexFile);
            Serializer.SerializeToFile(RelativeTable, _relativeIndexFile);
        }

        public void StartSearcher()
        {
            if (File.Exists(_textIndexFile) && TextFragment == null)
            {
                TextFragment = Serializer.DeserializeFromFile<InvertFragment>(_textIndexFile);
            }
            if (File.Exists(_relativeIndexFile) && RelativeTable == null)
            {
                RelativeTable =
                    Serializer.DeserializeFromFile<DimensionTable<string, string, double>>(_relativeIndexFile);
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
            try
            {
                Packet packet = Serializer.DeserializeFromBytes<Packet>(bytes);
                string type = System.Text.Encoding.UTF8.GetString(packet.Data);

                //如果是文本搜索
                if (type == "Text-Query")
                {
                    string currentTextQuery = GetGlobalCache<string>("Current-Text-Query");
                    if (!string.IsNullOrEmpty(currentTextQuery))
                    {
                        string query = GetGlobalCache<string>(currentTextQuery);
                        WriteToConsole("Get Text Query:" + query);
                        string[] keywords = segment.Split(query);
                        List<string> docs = TextFragment.FindCommonDocumentByKeys(keywords, 10);


                        string content = string.Empty;
                        string bestQuestion = string.Empty;
                        decimal bestQuesVal = 0;
                        foreach (var doc in docs)
                        {
                            Template.Question question = DataStore.Read(int.Parse(doc));
                            Tuple<string, decimal> tuple = question.GetBestQuestion(query);
                            if (tuple.Item2 > bestQuesVal)
                            {
                                bestQuesVal = tuple.Item2;
                                bestQuestion = tuple.Item1;
                            }
                            content += question.ToString() + ";";
                        }

                        string inputResultKey = Dns.GetHostName() + "," + _searchPort + query;
                        WriteToConsole("Result write into cache key=" + inputResultKey + ", count=" + docs.Count);

                        foreach (var keyword in keywords)
                        {
                            if (keyword.Length > 0)
                            {
                                content = content.Replace(keyword, "<strong>" + keyword + "</strong>");
                            }
                        }

                        if (bestQuestion.Length > 0 && bestQuesVal > (Decimal)0.1)
                        {
                            content = bestQuestion + ";" + content;
                        }

                        SetGlobalCache(inputResultKey, content);
                    }
                }

                    //如果是相关搜索
                else if (type == "Relative-Query")
                {
                    string result = string.Empty;
                    string currentRelativeQuery = GetGlobalCache<string>("Current-Relative-Query");
                    if (!string.IsNullOrEmpty(currentRelativeQuery))
                    {
                        string query = GetGlobalCache<string>(currentRelativeQuery);
                        WriteToConsole("Get Relative Query:" + query);
                        List<Tuple<string, double>> tuples = RelativeTable[query].GetAllKeyValue(20);
                        string inputResultKey = Dns.GetHostName() + "," + _searchPort + query;
                        if (tuples == null)
                        {
                            tuples = new List<Tuple<string, double>>();
                        }
                        foreach (var tuple in tuples)
                        {
                            result += tuple.Item1 + ":" + tuple.Item2 + ";";
                        }
                        SetGlobalCache(inputResultKey, result);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception);
            }
            return Serializer.SerializeToBytes(true);
        }
    }
}
