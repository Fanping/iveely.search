//using System.Text.RegularExpressions;
//using Iveely.Database;
//using Iveely.Framework.NLP;
//using Iveely.Framework.Process;
//using Iveely.Framework.Text;
//using Iveely.Framework.Text.Segment;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Iveely.SearchEngine
//{
//    /// <summary>
//    /// 爬虫
//    /// </summary>
//    public class Crawler : Iveely.CloudComputing.Client.Application
//    {
//        public class Page
//        {
//            public string Url;
//            public string Timestamp;
//            public string Content;
//            public string Title;
//            public string Site;
//        }

//        private static object obj = new object();
//        public class DataSaver
//        {
//            public void SavePage(ref List<Page> docs, string folder, bool isForce = false)
//            {
//                // 如果有多余的，需要存放到数据库
//                lock (obj)
//                {
//                    if (docs.Count > 50 || isForce)
//                    {
//                        if (!Directory.Exists(folder))
//                        {
//                            Directory.CreateDirectory(folder);
//                        }
//                        string fileFlag = folder + "\\" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour+ ".db4";
//                        using (IStorageEngine engine = STSdb.FromFile(fileFlag))
//                        {
//                            // 插入数据
//                            ITable<string, Page> table = engine.OpenXTable<string, Page>("WebPage");
//                            for (int i = 0; i < docs.Count; i++)
//                            {
//                                table[docs[i].Url] = docs[i];
//                            }

//                            // 记录
//                            ITable<DateTime, int> crawlerMonitor = engine.OpenXTable<DateTime, int>("Crawler_Monitor");
//                            crawlerMonitor[DateTime.Now] = docs.Count;

//                            engine.Commit();

//                        }
//                        docs.Clear();
//                    }
//                }
//            }
//        }

//        public override void Run(object[] args)
//        {
//            Init(args);
//            Console.WriteLine("Starting...");

//            //1. 读取5w个链接
//            string[] basicUrls = File.ReadAllLines(GetRootFolder() + "\\urls.txt");
//            foreach (var url in basicUrls)
//            {
//                try
//                {
//                    GetData(url);
//                }
//                catch (Exception exception)
//                {
//                    Console.WriteLine(exception);
//                }
//            }
//        }

//        public object GetData(string url)
//        {
//            DataSaver dataSaver = new DataSaver();
//            List<Page> docs = new List<Page>();

//            // 当前需要爬行的链接
//            List<string> CurrentUrls = new List<string>();

//            // 已经爬行过的链接
//            HashSet<string> VisitedUrls = new HashSet<string>();

//            //每个站点至多访问数量
//            int maxVisit = 2000;

//            string[] urlInfo = url.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//            string schemaUrl = "http://" + urlInfo[0];
//            IconGetter iconGetter = new IconGetter(GetRootFolder() + "\\icons");

//            //Uri可能转换失败
//            try
//            {
//                iconGetter.Extract(schemaUrl);
//                Uri hostUrl = new Uri(schemaUrl);
//                CurrentUrls.Add(schemaUrl);
//                string site = string.Empty;
//                int hasVisited = 0;
//                int hasUrlsCount = 1;

//                //如果当前拥有则爬行
//                while (CurrentUrls.Count > 0 && hasVisited < maxVisit)
//                {
//                    hasVisited++;
//                    HashSet<string> newLinks = new HashSet<string>();
//                    try
//                    {
//                        //2. 获取网页信息
//                        Console.WriteLine(DateTime.Now.ToString() + "[" + Thread.CurrentThread.ManagedThreadId + "]" + ":Visit " + CurrentUrls[0]);
//                        VisitedUrls.Add(CurrentUrls[0]);
//                        bool isGetContentSuc = false;
//                        Html2Article.ArticleDocument document = Html2Article.GetArticle(CurrentUrls[0], ref isGetContentSuc);
//                        if (document != null && document.Content.Length > 10)
//                        {
//                            if (string.IsNullOrEmpty(site))
//                            {
//                                string[] titleArray = document.Title.Split(new char[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
//                                site = titleArray[titleArray.Length - 1];
//                            }
//                            Page page = new Page();
//                            page.Url = CurrentUrls[0];
//                            page.Site = site;
//                            page.Content = document.Content;
//                            page.Title = document.Title;
//                            page.Timestamp = DateTime.Now.ToString();// or UTC
//                            docs.Add(page);
//                            dataSaver.SavePage(ref docs, GetRootFolder()+"\\RawData");
//                        }

//                        //3. 获取新链接
//                        if (document != null && hasUrlsCount < maxVisit)
//                        {
//                            for (int j = 0; j < document.ChildrenLink.Count; j++)
//                            {
//                                try
//                                {
//                                    string link = document.ChildrenLink[j];
//                                    if (link.Contains("#"))
//                                    {
//                                        link = link.Substring(0, link.IndexOf("#", System.StringComparison.Ordinal) - 1);
//                                    }
//                                    string host = (new Uri(document.ChildrenLink[j])).Host;
//                                    if (host == hostUrl.Host && !newLinks.Contains(link) &&
//                                        !VisitedUrls.Contains(link))
//                                    {

//                                        newLinks.Add(link);
//                                        VisitedUrls.Add(link);
//                                    }
//                                }
//                                catch (Exception exception)
//                                {
//                                    Console.WriteLine(exception);
//                                }

//                            }
//                        }
//                    }
//                    catch (Exception exception)
//                    {
//                        Console.WriteLine(exception);
//                    }
//                    CurrentUrls.RemoveAt(0);
//                    if (newLinks.Count > 0)
//                    {
//                        CurrentUrls.AddRange(newLinks.ToArray());
//                        hasUrlsCount += newLinks.Count;
//                    }
//                }
//                if (docs.Count > 0)
//                {
//                    dataSaver.SavePage(ref docs, GetRootFolder()+"\\RawData");
//                    docs.Clear();
//                }
//                string recordFolder = GetRootFolder() + "\\urls_hisotry";
//                if (!Directory.Exists(recordFolder))
//                {
//                    Directory.CreateDirectory(recordFolder);
//                }
//                File.WriteAllLines(recordFolder + "\\Urls_" + hostUrl.Host + ".txt", VisitedUrls);
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine(exception);
//            }

//            return true;
//        }
//    }

//    public class IconGetter
//    {

//        private string saveFolder;

//        /// <summary>
//        /// 下载器
//        /// </summary>
//        private Iveely.Framework.Network.Downloader downloader = new Framework.Network.Downloader();

//        /// <summary>
//        /// 获取网站集合的ICON
//        /// </summary>
//        /// <param name="urls"></param>
//        public void Extract(string[] urls)
//        {
//            ThreadManager<string> threadTasks = new ThreadManager<string>();
//            threadTasks.SetData(new List<string>(urls));
//            threadTasks.SetFunction(Extract);
//            threadTasks.Start();
//        }

//        public object Extract(string url)
//        {
//            try
//            {
//                //0. 规范化
//                if (url.Contains(" ") || url.Contains("\t"))
//                {
//                    url = url.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
//                }
//                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
//                {
//                    url = "http://" + url;
//                }
//                Console.WriteLine(url);

//                //1. 默认路径host下的icon

//                Uri uri = new Uri(url);
//                string savePath = uri.Host + ".ico";
//                if (!downloader.SyncDownload(url + "/favicon.ico", this.saveFolder + "\\" + savePath))
//                {
//                    //2. 分析网页源码提取
//                    Iveely.Framework.Text.Html html = Iveely.Framework.Text.Html.CreatHtml(uri);
//                    if (html == null)
//                    {
//                        return false;
//                    }
//                    string sourceCode = html.SourceCode;
//                    Match match = Regex.Match(sourceCode, "(href=\").*?(.ico)");
//                    if (match.Success)
//                    {
//                        string strUrl = match.Value.Replace("href=\"", "");
//                        Uri icoUrl = new Uri(uri, strUrl);
//                        downloader.SyncDownload(icoUrl.ToString(), this.saveFolder + "\\" + savePath);
//                    }
//                }
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine(exception);
//                return false;
//            }
//            return true;
//        }

//        public IconGetter(string savefolder)
//        {
//            if (!Directory.Exists(saveFolder))
//            {
//                Directory.CreateDirectory(savefolder);
//            }
//            this.saveFolder = savefolder;
//        }
//    }
//}
