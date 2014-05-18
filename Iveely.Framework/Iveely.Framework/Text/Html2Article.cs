using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Iveely.Framework.Text
{
    public class Html2Article
    {
        public class ArticleDocument
        {
            public string Url { get; set; }

            public string Site { get; set; }

            public string Title { get; set; }

            /// <summary>
            /// 正文文本
            /// </summary>
            public string Content { get; set; }

            /// <summary>
            /// 带标签正文
            /// </summary>
            public string ContentWithTags { get; set; }

            /// <summary>
            /// 发布日期
            /// </summary>
            public DateTime PublishDate { get; set; }

            public string Timestamp { get; set; }

            /// <summary>
            /// 包含的字链接
            /// </summary>
            public List<string> ChildrenLink { get; set; }

            public override string ToString()
            {
                return Url + "\t" + Site + "\t" + Title + "\t" + PublishDate.ToShortDateString() + "\t" + Content + "\t" +
                       ContentWithTags;
            }
        }


        #region 参数设置

        // 正则表达式过滤：正则表达式，要替换成的文本
        private static readonly string[][] Filters = new string[][]
        {
            new string[] {@"(?is)<script.*?>.*?</script>", ""},
            new string[] {@"(?is)<style.*?>.*?</style>", ""},
            // 针对链接密集型的网站的处理，主要是门户类的网站，降低链接干扰
            new string[] {@"(?is)</a>", "</a>\n"}
        };

        private static bool _appendMode = false;

        /// <summary>
        /// 是否使用追加模式，默认为false
        /// 使用追加模式后，会将符合过滤条件的所有文本提取出来
        /// </summary>
        public static bool AppendMode
        {
            get { return _appendMode; }
            set { _appendMode = value; }
        }

        private static int _depth = 6;

        /// <summary>
        /// 按行分析的深度，默认为6
        /// </summary>
        public static int Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        private static int _limitCount = 180;

        /// <summary>
        /// 字符限定数，当分析的文本数量达到限定数则认为进入正文内容
        /// 默认180个字符数
        /// </summary>
        public static int LimitCount
        {
            get { return _limitCount; }
            set { _limitCount = value; }
        }

        // 确定文章正文头部时，向上查找，连续的空行到达_headEmptyLines，则停止查找
        private static int _headEmptyLines = 2;
        // 用于确定文章结束的字符数
        private static int _endLimitCharCount = 20;

        #endregion


        public static ArticleDocument GetArticle(string url, ref bool isGetContentSuccess)
        {
            try
            {
                Html html = Html.CreatHtml(new Uri(url));
                if (html == null)
                {
                    return null;
                }
                ArticleDocument doc = GetRequestArticle(html.SourceCode);
                isGetContentSuccess = doc.Content.Length > 0;
                if (doc != null)
                {
                    doc.Url = url;
                    doc.Content = doc.Content.Length > 0 ? doc.Content : html.Content;
                    doc.Timestamp = html.Timestamp;
                    doc.Site = GetSiteName(html.Title);
                    doc.ChildrenLink = html.ChildrenLink.Select(o => o.ToString()).ToList();

                }

                return doc;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            return null;
        }

        private static string GetSiteName(string siteTitle)
        {
            if (siteTitle.Contains("-"))
            {
                string[] infos = siteTitle.Split(new char[] { '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (infos.Length >= 1)
                {
                    return infos[infos.Length - 1];
                }
                return siteTitle;
            }
            else
            {
                return siteTitle;
            }
        }

        /// <summary>
        /// 从给定的Html原始文本中获取正文信息
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static ArticleDocument GetRequestArticle(string html)
        {
            // 如果换行符的数量小于10，则认为html为压缩后的html
            // 由于处理算法是按照行进行处理，需要为html标签添加换行符，便于处理
            if (html.Count(c => c == '\n') < 10)
            {
                html = html.Replace(">", ">\n");
            }

            // 获取html，body标签内容
            string body = "";
            string bodyFilter = @"(?is)<body.*?</body>";
            Match m = Regex.Match(html, bodyFilter);
            if (m.Success)
            {
                body = m.ToString();
            }
            // 过滤样式，脚本等不相干标签
            foreach (var filter in Html2Article.Filters)
            {
                body = Regex.Replace(body, filter[0], filter[1]);
            }
            // 标签规整化处理，将标签属性格式化处理到同一行
            // 处理形如以下的标签：
            //  <a 
            //   href='http://www.baidu.com'
            //   class='test'
            // 处理后为
            //  <a href='http://www.baidu.com' class='test'>
            body = Regex.Replace(body, @"(<[^<>]+)\s*\n\s*", FormatTag);

            string content;
            string contentWithTags;
            string title1;
            GetContent(body, out content, out title1, out contentWithTags);
            string title2 = GetTitle(html);
            string title = GetMostProTitle(title1, title2, content);
            ArticleDocument article = new ArticleDocument
            {
                Title = title,
                PublishDate = GetPublishDate(html),
                Content = content,
                ContentWithTags = contentWithTags
            };

            return article;
        }

        /// <summary>
        /// 获取最大可能的标题
        /// </summary>
        /// <param name="title1"></param>
        /// <param name="title2"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private static string GetMostProTitle(string title1, string title2, string body)
        {
            // 如果任意一个为empty，则返回另外一个
            if (string.IsNullOrEmpty(title1)) { return title2; }
            if (string.IsNullOrEmpty(title2)) { return title1; }

            // 计算title1积分
            char[] titleArray = title1.ToArray();
            int count1 = 0;
            foreach (char c in titleArray)
            {
                if (body.Contains(c.ToString()))
                {
                    count1++;
                }
            }

            // 计算title2积分
            titleArray = title2.ToArray();
            int count2 = 0;
            foreach (char c in titleArray)
            {
                if (body.Contains(c.ToString()))
                {
                    count2++;
                }
            }

            if (count1 > count2)
            {
                return title1;
            }
            return title2;
        }

        /// <summary>
        /// 格式化标签，剔除匹配标签中的回车符
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private static string FormatTag(Match match)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ch in match.Value)
            {
                if (ch == '\r' || ch == '\n')
                {
                    continue;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取时间
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static string GetTitle(string html)
        {
            //默认标题
            string titleFilter = @"<title>[\s\S]*?</title>";
            string clearFilter = @"<.*?>";

            string title = "";
            Match match = Regex.Match(html, titleFilter, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                title = Regex.Replace(match.Groups[0].Value, clearFilter, "");
            }

            //尝试1： 正文的标题一般在h1中，比title中的标题更干净
            string h1Filter = @"<h1.*?>.*?</h1>";
            match = Regex.Match(html, h1Filter, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string h1 = Regex.Replace(match.Groups[0].Value, clearFilter, "");
                if (!String.IsNullOrEmpty(h1) && title.StartsWith(h1))
                {
                    return h1;
                }
            }

            title = title.Split(new char[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries)[0];
            title = title.Replace("\r\n", "").Trim();
            return title;
        }

        /// <summary>
        /// 获取文章发布日期
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static DateTime GetPublishDate(string html)
        {
            // 过滤html标签，防止标签对日期提取产生影响
            string text = Regex.Replace(html, "(?is)<.*?>", "");
            Match match = Regex.Match(
                text,
                @"((\d{4}|\d{2})(\-|\/)\d{1,2}\3\d{1,2})(\s?\d{2}:\d{2})?|(\d{4}年\d{1,2}月\d{1,2}日)(\s?\d{2}:\d{2})?",
                RegexOptions.IgnoreCase);

            DateTime result = new DateTime(1900, 1, 1);
            if (match.Success)
            {
                try
                {
                    string dateStr = "";
                    for (int i = 0; i < match.Groups.Count; i++)
                    {
                        dateStr = match.Groups[i].Value;
                        if (!String.IsNullOrEmpty(dateStr))
                        {
                            break;
                        }
                    }
                    // 对中文日期的处理
                    if (dateStr.Contains("年"))
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var ch in dateStr)
                        {
                            if (ch == '年' || ch == '月')
                            {
                                sb.Append("/");
                                continue;
                            }
                            if (ch == '日')
                            {
                                sb.Append(' ');
                                continue;
                            }
                            sb.Append(ch);
                        }
                        dateStr = sb.ToString();
                    }
                    result = Convert.ToDateTime(dateStr);
                }
                catch (Exception)
                {
                }
                if (result.Year < 1900)
                {
                    result = new DateTime(1900, 1, 1);
                }
            }
            return result;
        }

        /// <summary>
        /// 从body标签文本中分析正文内容
        /// </summary>
        /// <param name="bodyText">只过滤了script和style标签的body文本内容</param>
        /// <param name="content">返回文本正文，不包含标签</param>
        /// <param name="contentWithTags">返回文本正文包含标签</param>
        private static void GetContent(string bodyText, out string content, out string title, out string contentWithTags)
        {
            string[] orgLines = null; // 保存原始内容，按行存储
            string[] lines = null; // 保存干净的文本内容，不包含标签
            title = "";
            orgLines = bodyText.Split('\n');
            lines = new string[orgLines.Length];
            // 去除每行的空白字符,剔除标签
            for (int i = 0; i < orgLines.Length; i++)
            {
                string lineInfo = orgLines[i];
                // 处理回车，使用[crlf]做为回车标记符，最后统一处理
                lineInfo = Regex.Replace(lineInfo, "(?is)</p>|<br.*?/>", "[crlf]");
                lines[i] = Regex.Replace(lineInfo, "(?is)<.*?>", "").Trim();
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder orgSb = new StringBuilder();

            int preTextLen = 0; // 记录上一次统计的字符数量
            int startPos = -1; // 记录文章正文的起始位置
            for (int i = 0; i < lines.Length - _depth; i++)
            {
                int len = 0;
                for (int j = 0; j < _depth; j++)
                {
                    len += lines[i + j].Length;
                }

                if (startPos == -1) // 还没有找到文章起始位置，需要判断起始位置
                {
                    if (preTextLen > _limitCount && len > 0) // 如果上次查找的文本数量超过了限定字数，且当前行数字符数不为0，则认为是开始位置
                    {
                        // 查找文章起始位置, 如果向上查找，发现2行连续的空行则认为是头部
                        int emptyCount = 0;
                        for (int j = i - 1; j > 0; j--)
                        {
                            if (String.IsNullOrEmpty(lines[j]))
                            {
                                emptyCount++;
                            }
                            else
                            {
                                emptyCount = 0;
                            }
                            if (emptyCount == _headEmptyLines)
                            {
                                startPos = j + _headEmptyLines;
                                break;
                            }
                        }
                        // 如果没有定位到文章头，则以当前查找位置作为文章头
                        if (startPos == -1)
                        {
                            startPos = i;
                        }
                        // 填充发现的文章起始部分
                        if (startPos > 2)
                        {
                            title = lines[startPos - 3];
                        }
                        for (int j = startPos; j <= i; j++)
                        {
                            sb.Append(lines[j]);
                            orgSb.Append(orgLines[j]);
                        }
                    }
                }
                else
                {
                    //if (len == 0 && preTextLen == 0)    // 当前长度为0，且上一个长度也为0，则认为已经结束
                    if (len <= _endLimitCharCount && preTextLen < _endLimitCharCount) // 当前长度为0，且上一个长度也为0，则认为已经结束
                    {
                        if (!_appendMode)
                        {
                            break;
                        }
                        startPos = -1;
                    }
                    sb.Append(lines[i]);
                    orgSb.Append(orgLines[i]);
                }
                preTextLen = len;
            }

            string result = sb.ToString();
            // 处理回车符，更好的将文本格式化输出
            content = result.Replace("[crlf]", Environment.NewLine);
            content = content.Replace("&nbsp;", "");
            content = content.Replace("&ldquo;", "");
            content = content.Replace("&rdquo;", "");
            content = System.Web.HttpUtility.HtmlDecode(content);
            // 输出带标签文本
            contentWithTags = orgSb.ToString();
        }
    }
}
