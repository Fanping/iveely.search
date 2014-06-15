using Iveely.CloudComputing.Client;
using Iveely.Database;
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
    public class BaikeDataCrawler : Application
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
                        string fileFlag = folder + "\\Baike_data.db4";
                        using (IStorageEngine engine = STSdb.FromFile(fileFlag))
                        {
                            // 插入数据
                            ITable<string, Page> table = engine.OpenXTable<string, Page>("WebPage");
                            for (int i = 0; i < docs.Count; i++)
                            {
                                if (docs[i].Url.Contains("view"))
                                {
                                    table[docs[i].Title] = docs[i];
                                }
                            }
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
            //try
            //{
            //    GetData("baike.baidu.com");
            //}
            //catch (Exception exception)
            //{
            //    Console.WriteLine(exception);
            //}
            Index();
        }

        public object GetData(string url)
        {
            DataSaver dataSaver = new DataSaver();
            List<Page> docs = new List<Page>();

            // 当前需要爬行的链接
            List<string> currentUrls = new List<string>();

            // 已经爬行过的链接
            HashSet<string> visitedUrls = new HashSet<string>();


            string[] urlInfo = url.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string schemaUrl = "http://" + urlInfo[0];

            //Uri可能转换失败
            int hasVisited = 0;
            try
            {
                Uri hostUrl = new Uri(schemaUrl);
                currentUrls.Add(schemaUrl);
                string site = string.Empty;

                //如果当前拥有则爬行
                while (currentUrls.Count > 0)
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
                                string[] titleArray = document.Title.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                                site = titleArray[titleArray.Length - 1];
                            }
                            Page page = new Page();
                            page.Url = currentUrls[0];
                            page.Site = site;
                            page.Content = document.Content;
                            page.Title = document.Title;
                            page.Timestamp = DateTime.Now.ToString();// or UTC
                            docs.Add(page);
                            dataSaver.SavePage(ref docs, GetRootFolder() + "\\RawData");
                        }

                        //3. 获取新链接
                        if (document != null)
                        {
                            for (int j = 0; j < document.ChildrenLink.Count; j++)
                            {
                                try
                                {
                                    string link = document.ChildrenLink[j];
                                    if (link.Contains("#"))
                                    {
                                        link = link.Substring(0, link.IndexOf("#", StringComparison.Ordinal) - 1);
                                    }
                                    if (link.EndsWith("/"))
                                    {
                                        link = link.Substring(0, link.Length - 1);
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
                    }
                }
                if (docs.Count > 0)
                {
                    dataSaver.SavePage(ref docs, GetRootFolder() + "\\RawData");
                    docs.Clear();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return true;
        }

        public void Index()
        {
            string fileFlag = GetRootFolder() + "\\RawData\\Baike_data.db4";
            using (IStorageEngine engine = STSdb.FromFile(fileFlag))
            {
                // 插入数据
                ITable<string, Page> table = engine.OpenXTable<string, Page>("WebPage");
                foreach (var kv in table)
                {
                    Page page = kv.Value;
                    Console.WriteLine(kv.Key+" "+page.Url);
                }
            }
        }
    }
}
