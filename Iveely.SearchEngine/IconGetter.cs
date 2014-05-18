using Iveely.Framework.Process;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Iveely.SearchEngine
{
    /// <summary>
    /// ICON读取
    /// </summary>
    public class IconGetter
    {

        private string saveFolder;

        /// <summary>
        /// 下载器
        /// </summary>
        private Iveely.Framework.Network.Downloader downloader = new Framework.Network.Downloader();

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
                if (!downloader.SyncDownload(url + "/favicon.ico", this.saveFolder + "\\" + savePath))
                {
                    //2. 分析网页源码提取
                    Iveely.Framework.Text.Html html = Iveely.Framework.Text.Html.CreatHtml(uri);
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
                        downloader.SyncDownload(icoUrl.ToString(),this.saveFolder + "\\"+savePath);
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
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(savefolder);
            }
            this.saveFolder = savefolder;
        }
    }
}
