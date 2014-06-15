using System.Globalization;
using System.Text.RegularExpressions;
using Iveely.Database;
using Iveely.Framework.Process;
using Iveely.Framework.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Iveely.SearchEngine
{
    /// <summary>
    /// 爬虫
    /// </summary>
    public class Crawler : Iveely.CloudComputing.Client.Application
    {
        public class Page
        {
            public string Url;
            public string Timestamp;
            public string Content;
            public string Title;
            public string Site;
        }

        private static readonly object obj = new object();
        public class DataSaver
        {
            public void SavePage(ref List<Page> docs, string folder, bool isForce = false)
            {
                // 如果有多余的，需要存放到数据库
                lock (obj)
                {
                    if (docs.Count > 50 || isForce)
                    {
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        string fileFlag = folder + "\\" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + ".db4";
                        using (IStorageEngine engine = STSdb.FromFile(fileFlag))
                        {
                            // 插入数据
                            ITable<string, Page> table = engine.OpenXTable<string, Page>("WebPage");
                            for (int i = 0; i < docs.Count; i++)
                            {
                                table[docs[i].Url] = docs[i];
                            }

                            // 记录
                            ITable<DateTime, int> crawlerMonitor = engine.OpenXTable<DateTime, int>("Crawler_Monitor");
                            crawlerMonitor[DateTime.Now] = docs.Count;

                            engine.Commit();

                        }
                        docs.Clear();
                    }
                }
            }
        }

        public override void Run(object[] args)
        {
            Init(args);
            Console.WriteLine("Starting...");

            //1. 读取5w个链接
            string[] basicUrls = File.ReadAllLines(GetRootFolder() + "\\urls.txt");
            foreach (var url in basicUrls)
            {
                try
                {
                    GetData(url);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        public object GetData(string url)
        {
            DataSaver dataSaver = new DataSaver();
            List<Page> docs = new List<Page>();

            // 当前需要爬行的链接
            List<string> currentUrls = new List<string>();

            // 已经爬行过的链接
            HashSet<string> visitedUrls = new HashSet<string>();

            //每个站点至多访问数量
            int maxVisit = 2000;

            string[] urlInfo = url.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string schemaUrl = "http://" + urlInfo[0];
            IconGetter iconGetter = new IconGetter(GetRootFolder() + "\\icons");

            //Uri可能转换失败
            try
            {
                iconGetter.Extract(schemaUrl);
                Uri hostUrl = new Uri(schemaUrl);
                currentUrls.Add(schemaUrl);
                string site = string.Empty;
                int hasVisited = 0;
                int hasUrlsCount = 1;

                //如果当前拥有则爬行
                while (currentUrls.Count > 0 && hasVisited < maxVisit)
                {
                    hasVisited++;
                    HashSet<string> newLinks = new HashSet<string>();
                    try
                    {
                        //2. 获取网页信息
                        Console.WriteLine(DateTime.Now + "[" + Thread.CurrentThread.ManagedThreadId + "]" + ":Visit " + currentUrls[0]);
                        visitedUrls.Add(currentUrls[0]);
                        bool isGetContentSuc = false;
                        Html2Article.ArticleDocument document = Html2Article.GetArticle(currentUrls[0], ref isGetContentSuc);
                        if (document != null && document.Content.Length > 10)
                        {
                            if (string.IsNullOrEmpty(site))
                            {
                                string[] titleArray = document.Title.Split(new char[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                                site = titleArray[titleArray.Length - 1];
                            }
                            Page page = new Page();
                            page.Url = currentUrls[0];
                            page.Site = site;
                            page.Content = document.Content;
                            page.Title = document.Title;
                            page.Timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);// or UTC
                            docs.Add(page);
                            dataSaver.SavePage(ref docs, GetRootFolder() + "\\RawData");
                        }

                        //3. 获取新链接
                        if (document != null && hasUrlsCount < maxVisit)
                        {
                            for (int j = 0; j < document.ChildrenLink.Count; j++)
                            {
                                try
                                {
                                    string link = document.ChildrenLink[j];
                                    if (link.Contains("#"))
                                    {
                                        link = link.Substring(0, link.IndexOf("#", System.StringComparison.Ordinal) - 1);
                                    }
                                    string host = (new Uri(document.ChildrenLink[j])).Host;
                                    if (host == hostUrl.Host && !newLinks.Contains(link) &&
                                        !visitedUrls.Contains(link))
                                    {

                                        newLinks.Add(link);
                                        visitedUrls.Add(link);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception);
                                }

                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                    currentUrls.RemoveAt(0);
                    if (newLinks.Count > 0)
                    {
                        currentUrls.AddRange(newLinks.ToArray());
                        hasUrlsCount += newLinks.Count;
                    }
                }
                if (docs.Count > 0)
                {
                    dataSaver.SavePage(ref docs, GetRootFolder() + "\\RawData");
                    docs.Clear();
                }
                string recordFolder = GetRootFolder() + "\\urls_hisotry";
                if (!Directory.Exists(recordFolder))
                {
                    Directory.CreateDirectory(recordFolder);
                }
                File.WriteAllLines(recordFolder + "\\Urls_" + hostUrl.Host + ".txt", visitedUrls);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return true;
        }
    }

    public class IconGetter
    {

        private readonly string _saveFolder;

        /// <summary>
        /// 下载器
        /// </summary>
        private readonly Iveely.Framework.Network.Downloader _downloader = new Framework.Network.Downloader();

        /// <summary>
        /// 获取网站集合的ICON
        /// </summary>
        /// <param name="urls"></param>
        public void Extract(string[] urls)
        {
            ThreadManager<string> threadTasks = new ThreadManager<string>();
            threadTasks.SetData(new List<string>(urls));
            threadTasks.SetFunction(Extract);
            threadTasks.Start();
        }

        public object Extract(string url)
        {
            try
            {
                //0. 规范化
                if (url.Contains(" ") || url.Contains("\t"))
                {
                    url = url.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
                }
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
                Console.WriteLine(url);

                //1. 默认路径host下的icon

                Uri uri = new Uri(url);
                string savePath = uri.Host + ".ico";
                if (!_downloader.SyncDownload(url + "/favicon.ico", this._saveFolder + "\\" + savePath))
                {
                    //2. 分析网页源码提取
                    Html html = Html.CreatHtml(uri);
                    if (html == null)
                    {
                        return false;
                    }
                    string sourceCode = html.SourceCode;
                    Match match = Regex.Match(sourceCode, "(href=\").*?(.ico)");
                    if (match.Success)
                    {
                        string strUrl = match.Value.Replace("href=\"", "");
                        Uri icoUrl = new Uri(uri, strUrl);
                        _downloader.SyncDownload(icoUrl.ToString(), this._saveFolder + "\\" + savePath);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
            return true;
        }

        public IconGetter(string savefolder)
        {
            if (!Directory.Exists(_saveFolder))
            {
                Directory.CreateDirectory(savefolder);
            }
            this._saveFolder = savefolder;
        }
    }
}
