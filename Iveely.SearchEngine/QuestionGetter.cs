/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.CloudComputing.Client;
using Iveely.Framework.Algorithm;
using Iveely.Framework.Algorithm.AI;
using Iveely.Framework.DataStructure;
using Iveely.Framework.Log;
using Iveely.Framework.Text;

namespace Iveely.SearchEngine
{
    /// <summary>
    /// 问题提取器
    /// </summary>
    public class QuestionGetter : Application
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
        /// 爬虫爬行的跟站点
        /// </summary>
        private const string Host = "www.baike.com";

        public static LocalStore<string> DataStore;

        public override void Run(object[] args)
        {
            Init(args);
            Urls.Add("http://www.baike.com");
            DataStore = new LocalStore<string>(GetRootFolder() + "\\QuestionData.index",
    GetRootFolder() + "\\ALLQuestionData", 10);
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
                        GetQuestions(ref pages);
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

        /// 索引程序入口
        /// </summary>
        /// <param name="pages">网页信息集合</param>
        public void GetQuestions(ref List<Page> pages)
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


            if (questions.Any())
            {
                foreach (Template.Question question in questions)
                {
                    foreach (string q in question.GetAllQuestions())
                    {
                        DataStore.Write(q);
                    }

                }
            }
            questions.Clear();
        }
    }
}
