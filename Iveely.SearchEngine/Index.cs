using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iveely.CloudComputing.Client;
using Iveely.Data;
using Iveely.Database;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Text;

namespace Iveely.SearchEngine
{
    public class Crawler
    {
        /// <summary>
        /// 页面基本信息
        /// </summary>
        public class Page
        {
            public string Url;
            public string Timestamp;
            public string Content;
            public string Title;
            public string Site;
        }
    }

    public class Index : Application
    {
        /// <summary>
        /// 索引信息
        /// </summary>
        public class TextIndex
        {
            public string Keyword;
            public string Url;
            public double Weight;
        }

        /// <summary>
        /// 分词组件
        /// </summary>
        private static Iveely.Framework.Text.HMMSegment segment;

        /// <summary>
        /// 正文锁
        /// </summary>
        private static object PageObj = new object();

        /// <summary>
        /// 索引锁
        /// </summary>
        private static object IndexObj = new object();

        /// <summary>
        /// 临时存放网页数据
        /// </summary>
        private static List<Crawler.Page> pages = new List<Crawler.Page>();

        /// <summary>
        /// 临时存放文本索引数据
        /// </summary>
        private static List<TextIndex> indexs = new List<TextIndex>();

        /// <summary>
        /// 已经访问过的原始文件
        /// </summary>
        private HashSet<string> fileVisited = new HashSet<string>();

        /// <summary>
        /// 执行索引程序入口
        /// </summary>
        /// <param name="args"></param>
        public override void Run(object[] args)
        {
            Init(args);
            segment = HMMSegment.GetInstance();
            DataSaver dataSaver = new DataSaver();

            string rawDatafolder = GetRootFolder() + "\\RawData";
            string folder = GetRootFolder() + "\\ISE";
            try
            {
                string[] files = Directory.GetFiles(rawDatafolder);
                //if (files.Length < 2)
                //{
                //    // 如果个数小于等于1，则休眠10min
                //    Thread.Sleep(1000 * 60 * 10);
                //}
                for (int i = 0; i < files.Length; i++)
                {
                    if (!fileVisited.Contains(files[i]))
                    {
                        fileVisited.Add(files[i]);
                        try
                        {
                            Console.WriteLine(i + "/" + files.Length);
                            dataSaver.AnalysisData(folder, files[i]);
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
        }

        public class DataSaver
        {
            /// <summary>
            /// 分析原始网页数据
            /// </summary>
            /// <param name="folder"></param>
            public void AnalysisData(string folder, string filePath)
            {
                // 如果有多余的，需要存放到数据库
                lock (PageObj)
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    string fileFlag = filePath;
                    using (IStorageEngine engine = STSdb.FromFile(fileFlag))
                    {
                        // 插入数据
                        ITable<string, Crawler.Page> table = engine.OpenXTable<string, Crawler.Page>("WebPage");
                        foreach (var keyValuePair in table)
                        {
                            Crawler.Page page = (Crawler.Page)keyValuePair.Value;
                            if (page != null && page.Content.Trim().Length > 0)
                            {
                                Console.WriteLine(page.Url);
                                var frequency = new IntTable<string, int>();
                                string[] results = segment.Split(page.Title + page.Title + page.Content);
                                if (results.Length < 1)
                                {
                                    continue;
                                }
                                frequency.Add(results);
                                foreach (DictionaryEntry de in frequency)
                                {
                                    TextIndex textIndex = new TextIndex();
                                    textIndex.Keyword = de.Key.ToString();
                                    textIndex.Weight = int.Parse(de.Value.ToString()) * 1.0 / results.Length;
                                    textIndex.Url = page.Url;
                                    indexs.Add(textIndex);
                                    SaveIndex(ref indexs, folder, false);
                                }
                                pages.Add(page);
                                SaveContent(ref pages, folder, false);
                            }
                        }

                    }
                }
                SaveIndex(ref indexs, folder, true);
                SaveContent(ref pages, folder, true);
            }

            /// <summary>
            /// 保存较为正式的网页数据
            /// </summary>
            /// <param name="docs"></param>
            /// <param name="folder"></param>
            /// <param name="isForce"></param>
            public void SaveContent(ref List<Crawler.Page> docs, string folder, bool isForce = false)
            {
                // 如果有多余的，需要存放到数据库
                lock (PageObj)
                {
                    try
                    {
                        if (docs.Count > 50 || isForce)
                        {
                            if (!Directory.Exists(folder))
                            {
                                Directory.CreateDirectory(folder);
                            }
                            string fileFlag = folder + "\\Iveely_Search_Engine_Pages.db4";
                            using (IStorageEngine engine = STSdb.FromFile(fileFlag))
                            {
                                // 插入数据
                                ITable<string, Crawler.Page> table = engine.OpenXTable<string, Crawler.Page>("WebPage");
                                for (int i = 0; i < docs.Count; i++)
                                {
                                    table[docs[i].Url] = docs[i];
                                }
                                engine.Commit();

                            }
                            docs.Clear();
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        docs.Clear();
                    }
                }
            }

            /// <summary>
            /// 保存索引数据
            /// </summary>
            /// <param name="indexDocs"></param>
            /// <param name="folder"></param>
            /// <param name="isForce"></param>
            public void SaveIndex(ref List<TextIndex> indexDocs, string folder, bool isForce = false)
            {
                // 如果有多余的，需要存放到数据库
                lock (IndexObj)
                {
                    try
                    {
                        if (indexDocs.Count > 2000 || isForce)
                        {
                            if (!Directory.Exists(folder))
                            {
                                Directory.CreateDirectory(folder);
                            }
                            string fileFlag = folder + "\\Iveely_Search_Engine_TextIndex.db4";
                            using (IStorageEngine engine = STSdb.FromFile(fileFlag))
                            {
                                // 插入数据
                                ITable<string, List<Iveely.Data.Slots<string, double>>> table = engine.OpenXTable<string, List<Iveely.Data.Slots<string, double>>>("WebPage");
                                for (int i = 0; i < indexDocs.Count; i++)
                                {
                                    // 如果包含则追加
                                    List<Iveely.Data.Slots<string, double>> list = table.Find(indexDocs[i].Keyword);
                                    if (list != null && list.Count > 0)
                                    {
                                        Iveely.Data.Slots<string, double> slot=new Slots<string, double>(indexDocs[i].Url,indexDocs[i].Weight);
                                        list.Add(slot);
                                    }
                                    // 否则新增
                                    else
                                    {
                                        list = new List<Slots<string, double>>();
                                        Iveely.Data.Slots<string, double> slot = new Slots<string, double>(indexDocs[i].Url, indexDocs[i].Weight);
                                        list.Add(slot);
                                        table[indexDocs[i].Keyword] = list;
                                    }
                                }
                                engine.Commit();

                            }
                            indexDocs.Clear();
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        indexDocs.Clear();
                    }

                }
            }
        }
    }
}
