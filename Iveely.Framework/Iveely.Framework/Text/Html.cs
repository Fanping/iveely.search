/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Iveely.Framework.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Text
{
    /// <summary>
    /// Html标识
    /// (用于分析html文本中的内容)
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public sealed class Html
    {
        private class BasicContent : IDisposable
        {
            /// <summary>
            /// HTML类型
            /// </summary>
            private static readonly ReadOnlyCollection<string> _HtmlMimeTypes = new ReadOnlyCollection<string>(new[]
                                                                                                               {
                                                                                                                   "application/xhtml+xml"
                                                                                                                   ,
                                                                                                                   "text/html"
                                                                                                                   ,
                                                                                                               });

            /// <summary>
            /// XML类型
            /// </summary>
            private static readonly ReadOnlyCollection<string> _XmlMimeTypes = new ReadOnlyCollection<string>(new[]
                                                                                                              {
                                                                                                                  "text/xml"
                                                                                                              });

            /// <summary>
            /// CSS类型
            /// </summary>
            private static readonly ReadOnlyCollection<string> _CssMimeTypes = new ReadOnlyCollection<string>(new[]
                                                                                                              {
                                                                                                                  "text/css"
                                                                                                              });

            /// <summary>
            /// 内存流
            /// </summary>
            private readonly MemoryStream _stream = new MemoryStream();

            /// <summary>
            /// 一般内容处理模式
            /// </summary>
            /// <param name="requestUrl"> 请求的URL </param>
            /// <param name="webResponse"> 请求URL的响应结果 </param>
            public BasicContent(Uri requestUrl, HttpWebResponse webResponse)
            {
                //如果请求URL为空
                if (requestUrl == null)
                {
                    throw new ArgumentNullException("requestUrl 不能为空!");
                }
                //如果请求响应为空
                if (webResponse == null)
                {
                    throw new ArgumentNullException("webResponse 不能为空!");
                }
                RequestUrl = requestUrl;

                ResponseUrl = webResponse.ResponseUri;
                MimeType = GetMimeType(webResponse);
                CharSet = webResponse.CharacterSet;
                using (var stream = webResponse.GetResponseStream())
                {
                    stream.CopyTo(_stream);
                }
                _stream.Position = 0;
            }

            /// <summary>
            /// 页面信息编码
            /// </summary>
            private string CharSet { get; set; }

            /// <summary>
            /// HTML类型
            /// </summary>
            private static ReadOnlyCollection<string> HtmlMimeTypes
            {
                get { return _HtmlMimeTypes; }
            }

            /// <summary>
            /// XML类型
            /// </summary>
            private static ReadOnlyCollection<string> XmlMimeTypes
            {
                get { return _XmlMimeTypes; }
            }

            private static ReadOnlyCollection<string> CssMimeTypes
            {
                get { return _CssMimeTypes; }
            }

            /// <summary>
            /// 请求链接
            /// </summary>
            private Uri RequestUrl { get; set; }

            /// <summary>
            /// 响应链接
            /// </summary>
            private Uri ResponseUrl { get; set; }

            /// <summary>
            /// 媒体类型
            /// </summary>
            private string MimeType { get; set; }

            protected MemoryStream Stream
            {
                get { return _stream; }
            }

            /// <summary>
            /// 文件大小
            /// </summary>
            public virtual long Size
            {
                get { return _stream.Length; }
            }

            /// <summary>
            /// 文本内容
            /// </summary>
            public virtual string Text
            {
                get
                {
                    return Encoding.GetEncoding(CharSet).GetString(_stream.ToArray());
                    // Encoding.UTF8.GetString(_Stream.ToArray());
                }
            }

            #region IDisposable Members

            public void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            #endregion

            /// <summary>
            /// 回去媒体类型
            /// </summary>
            /// <param name="response"> Web请求的响应 </param>
            /// <returns> </returns>
            private static string GetMimeType(WebResponse response)
            {
                //标头
                var contentType = response.Headers[HttpResponseHeader.ContentType];
                //媒体类型
                var mimeType = contentType.Split(';')[0];
                //返回
                return mimeType;
            }

            /// <summary>
            /// 媒体标题
            /// </summary>
            /// <returns> </returns>
            public virtual string GetTitle()
            {
                return "";
            }

            /// <summary>
            /// 正文
            /// </summary>
            /// <returns> </returns>
            public virtual string GetContent()
            {
                return "";
            }

            /// <summary>
            /// 是否是HTML类型
            /// </summary>
            /// <param name="mimeType"> </param>
            /// <returns> True为是HTML类型 </returns>
            public static bool IsHtml(string mimeType)
            {
                return HtmlMimeTypes.Contains(mimeType);
            }

            /// <summary>
            /// 是否是XML类型
            /// </summary>
            /// <param name="mimeType"> </param>
            /// <returns> </returns>
            private static bool IsXml(string mimeType)
            {
                return XmlMimeTypes.Contains(mimeType);
            }

            /// <summary>
            /// 是否是CSS类型
            /// </summary>
            /// <param name="mimeType"> </param>
            /// <returns> </returns>
            private static bool IsCss(string mimeType)
            {
                return CssMimeTypes.Contains(mimeType);
            }

            /// <summary>
            /// 是否是HTML类型
            /// </summary>
            /// <returns> </returns>
            public bool IsHtml()
            {
                return IsHtml(MimeType);
            }

            /// <summary>
            ///  是否是XML类型
            /// </summary>
            /// <returns> </returns>
            public bool IsXml()
            {
                return IsXml(MimeType);
            }

            /// <summary>
            /// 是否是CSS类型
            /// </summary>
            /// <returns> </returns>
            public bool IsCss()
            {
                return IsCss(MimeType);
            }

            public virtual void Save(Stream stream)
            {
                _stream.Position = 0;
                _stream.CopyTo(stream);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _stream.Dispose();
                }
            }
        }

        private class HtmlContent : BasicContent
        {
            public HtmlContent(Uri requestUrl, HttpWebResponse webResponse)
                : base(requestUrl, webResponse)
            {
                Html = new HtmlDocument();
                Encoding en = webResponse.CharacterSet != null && webResponse.CharacterSet.Contains("ISO")
                                  ? Encoding.UTF8
                                  : Encoding.GetEncoding(webResponse.CharacterSet);
                Html.Load(Stream, en);
                //AutoDetectEncoding = true;
                //Html.Load(Stream, AutoDetectEncoding);
            }

            public static bool AutoDetectEncoding { get; set; }

            public HtmlDocument Html { get; protected set; }

            public override string Text
            {
                get { return Html.DocumentNode.OuterHtml; }
            }

            public override void Save(Stream stream)
            {
                Html.Save(stream);
            }

            /// <summary>
            /// 获取网页标题
            /// </summary>
            /// <returns> 返回标题内容 </returns>
            public override string GetTitle()
            {
                try
                {
                    return Html.DocumentNode.SelectNodes("/html/head/title")[0].InnerText;
                }
                catch
                {
                    return "";
                }
            }

            /// <summary>
            /// 获取网页正文内容
            /// </summary>
            /// <returns> 返回网页正文 </returns>
            public override string GetContent()
            {
                try
                {
                    string content = Html.DocumentNode.SelectNodes("/html/body")[0].InnerHtml;
                    return new HtmlParser(content).Text();
                }
                catch
                {
                    return "";
                }
            }
        }

        private class HtmlParser
        {
            //把html转为数组形式用于分析
            private readonly char[] _htmlcode;
            //输出的结果
            private readonly StringBuilder _result = new StringBuilder();
            private readonly string[] _specialTag = { "script", "style", "!--" };
            //分析文本时候的指针位置 
            //标记现在的指针是不是在尖括号内
            private bool _inTag;
            private string[] _keepTag;
            //是否要提取正文
            private bool _needContent = true;
            private int _seek;
            //当前尖括号的名字
            private string _tagName;
            //特殊的尖括号内容，一般这些标签的正文是不要的

            /// <summary>
            /// 初始化类
            /// </summary>
            /// <param name="html"> 要分析的html代码 </param>
            public HtmlParser(string html)
            {
                _htmlcode = html.Replace("\n", "").Replace("\t", "").ToArray();
                KeepTag(new string[] { });
            }

            /// <summary>
            /// 当指针进入尖括号内，就会触发这个属性。这里主要逻辑是提取尖括号里的标签名字
            /// </summary>
            private bool InTag
            {
                get { return _inTag; }
                set
                {
                    _inTag = value;
                    if (!value)
                        return;
                    bool ok = true;
                    _tagName = "";
                    while (ok)
                    {
                        string word = Read();
                        if (word != " " && word != ">")
                        {
                            _tagName += word;
                        }
                        else if (word == " " && _tagName.Length > 0)
                        {
                            ok = false;
                        }
                        else if (word == ">")
                        {
                            ok = false;
                            InTag = false;
                            _seek -= 1;
                        }
                    }
                }
            }

            /// <summary>
            ///  设置要保存那些标签不要被过滤掉
            /// </summary>
            /// <param name="tags"> </param>
            private void KeepTag(string[] tags)
            {
                _keepTag = tags;
            }

            /// <summary>
            /// </summary>
            /// <returns> 输出处理后的文本 </returns>
            public string Text()
            {
                int startTag = 0;
                string lastChar = "";
                while (_seek < _htmlcode.Length)
                {
                    string word = Read();
                    if (word.ToLower() == "<")
                    {
                        startTag = _seek;
                        InTag = true;
                    }
                    else if (word.ToLower() == ">")
                    {
                        int endTag = _seek;
                        InTag = false;
                        if (IskeepTag(_tagName.Replace("/", "")))
                        {
                            for (int i = startTag - 1; i < endTag; i++)
                            {
                                _result.Append(_htmlcode[i].ToString(CultureInfo.InvariantCulture));
                            }
                        }
                        else if (_tagName.StartsWith("!--"))
                        {
                            bool ok = true;
                            while (ok && _seek < _htmlcode.Length)
                            {
                                if (Read() == "-")
                                {
                                    if (Read() == "-")
                                    {
                                        if (Read() == ">")
                                        {
                                            ok = false;
                                        }
                                        else
                                        {
                                            _seek -= 1;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (string str in _specialTag)
                            {
                                if (_tagName == str)
                                {
                                    _needContent = false;
                                    break;
                                }
                                _needContent = true;
                            }
                        }
                    }
                    else if (!InTag && _needContent)
                    {
                        if (word == lastChar && word == " ")
                        {
                            continue;
                        }
                        _result.Append(word);
                        lastChar = word;
                    }
                }
                return _result.ToString();
            }

            /// <summary>
            /// 判断是否要保存这个标签
            /// </summary>
            /// <param name="tag"> A <see cref="System.String" /> </param>
            /// <returns> A <see cref="System.Boolean" /> </returns>
            private bool IskeepTag(string tag)
            {
                return _keepTag.Any(ta => tag.ToLower() == ta.ToLower());
            }

            private string Read()
            {
                return _htmlcode[_seek++].ToString(CultureInfo.InvariantCulture);
            }
        }

        private interface IProcessor
        {
            bool CanProcess(BasicContent content);

            IEnumerable<Uri> Process(BasicContent content, Uri refrenceUri);
        }

        private class HtmlProcessor : IProcessor
        {
            #region IProcessor Members

            public bool CanProcess(BasicContent content)
            {
                return content is HtmlContent;
            }

            public IEnumerable<Uri> Process(BasicContent content, Uri refrenceUri)
            {
                var htmlContent = (HtmlContent)content;
                var doc = htmlContent.Html;

                doc.LoadHtml(EscapeIEConditionalComments(htmlContent.Text));

                var foundUrls = new HashSet<Uri>();

                CollectUrls(foundUrls, doc, "a", "href", refrenceUri);
                CollectUrls(foundUrls, doc, "area", "href", refrenceUri);
                CollectUrls(foundUrls, doc, "frame", "src", refrenceUri);
                CollectUrls(foundUrls, doc, "iframe", "src", refrenceUri);
                CollectUrls(foundUrls, doc, "img", "src", refrenceUri);
                CollectUrls(foundUrls, doc, "input", "src", refrenceUri);
                CollectUrls(foundUrls, doc, "link", "href", refrenceUri);
                CollectUrls(foundUrls, doc, "object", "data", refrenceUri);
                CollectUrls(foundUrls, doc, "script", "src", refrenceUri);

                doc.LoadHtml(UnescapeIEConditionalComments(htmlContent.Text));

                return foundUrls;
            }

            #endregion

            #region IE Conditional Support

            private const string IeConditionalReplaceString = "$1--$2";
            private const string EscapedIeConditionalReplaceString = "$1$3";

            private static readonly Regex IeConditionalStartRegex = new Regex(@"(<!--\[if(?:.*?)IE(?:.*?)\])(>)",
                                                                               RegexOptions.Compiled |
                                                                               RegexOptions.Singleline);

            private static readonly Regex IeConditionalEndRegex = new Regex(@"(<!)(\[endif\]-->)",
                                                                             RegexOptions.Compiled | RegexOptions.Singleline);

            private static readonly Regex EscapedIeConditionalStartRegex = new Regex(
                @"(<!--\[if(?:.*?)IE(?:.*?)\])(--)(>)", RegexOptions.Compiled | RegexOptions.Singleline);

            private static readonly Regex EscapedIeConditionalEndRegex = new Regex(@"(<!)(--)(\[endif\]-->)",
                                                                                    RegexOptions.Compiled |
                                                                                    RegexOptions.Singleline);

            protected static string EscapeIEConditionalComments(string input)
            {
                var startingTagsEscaped = IeConditionalStartRegex.Replace(input, IeConditionalReplaceString);

                return IeConditionalEndRegex.Replace(startingTagsEscaped, IeConditionalReplaceString);
            }

            protected static string UnescapeIEConditionalComments(string input)
            {
                var startingTagsUnescaped = EscapedIeConditionalStartRegex.Replace(input,
                                                                                    EscapedIeConditionalReplaceString);

                return EscapedIeConditionalEndRegex.Replace(startingTagsUnescaped, EscapedIeConditionalReplaceString);
            }

            #endregion

            private static void CollectUrls(HashSet<Uri> links, HtmlDocument htmlDocument, string elementName,
                                            string attributeName, Uri referenceUri)
            {
                var elements = htmlDocument.DocumentNode.SelectNodes("//" + elementName + "[@" + attributeName + "]");

                if (elements == null)
                {
                    return;
                }

                var urls = elements.Select(e => e.Attributes[attributeName]).Select(a => new Uri(referenceUri, HttpUtility.HtmlDecode(a.Value)));

                AddRange(ref links, urls);
            }

            private static void AddRange(ref HashSet<Uri> hashSet, IEnumerable<Uri> collection)
            {
                if (hashSet == null)
                {
                    throw new ArgumentNullException("hashSet");
                }

                if (collection == null)
                {
                    throw new ArgumentNullException("collection");
                }

                foreach (var item in collection)
                {
                    hashSet.Add(item);
                }
            }
        }

        /// <summary>
        /// 信息内容处理工厂（工厂模式）
        /// </summary>
        private static class ProcessFactory
        {
            /// <summary>
            /// 信息内容处理
            /// </summary>
            /// <param name="requestUrl"> 请求的URL </param>
            /// <param name="webResponse"> web请求的响应结果 </param>
            /// <returns> </returns>
            public static BasicContent Create(Uri requestUrl, HttpWebResponse webResponse)
            {
                //获取媒体类型
                var mimeType = GetMimeType(webResponse);
                //如果是HTML
                if (BasicContent.IsHtml(mimeType))
                {
                    //交给HTML处理器
                    return new HtmlContent(requestUrl, webResponse);
                }
                //else if (HttpCrawlerContent.IsXml(mimeType))
                //{
                //    return new XmlHttpCrawlerContent(requestUrl, referrerUrl, webResponse);
                //}

                return new BasicContent(requestUrl, webResponse);
            }

            public static string GetMimeType(WebResponse response)
            {
                var contentType = response.Headers[HttpResponseHeader.ContentType];

                var mimeType = contentType.Split(';')[0];

                return mimeType;
            }
        }

        /// <summary>
        /// 网页标题
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 网页内容
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// 网页源码
        /// </summary>
        public string SourceCode { get; private set; }

        public List<Uri> ChildrenLink { get; private set; }

        private static IProcessor[] _processors;

        /// <summary>
        /// 创建一个分析请求
        /// </summary>
        /// <param name="uri">分析的Uri</param>
        /// <returns>返回Html对象</returns>
        public static Html CreatHtml(Uri uri)
        {
            try
            {

                if (_processors == null)
                {
                    InitProcessors(new HtmlProcessor());
                }

                var request = CreateRequest(uri);
                request.UserAgent =
                    "[Iveely Computing]-Create By LiuFanping@iveely.com, Share in Open Source Community!";
                //request.Credentials = new NetworkCredential("spidertest", "iveely123456");
                //获取响应
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    //处理获取到得响应结果
                    using (var crawlerContent = ProcessFactory.Create(uri, response))
                    {
                        Html html = new Html();
                        html.Title = crawlerContent.GetTitle();
                        html.Content = crawlerContent.GetContent();
                        //html.SourceCode = crawlerContent.Text;
                        html.ChildrenLink = new List<Uri>(ProcessContent(crawlerContent, uri));
                        return html;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(uri + "->" + exception);
                return null;
            }
        }

        private static void InitProcessors(params IProcessor[] processors)
        {
            _processors = processors;
        }

        /// <summary>
        /// 创建请求
        /// </summary>
        /// <param name="url"> 请求的URL </param>
        /// <returns> 返回请求结果 </returns>
        private static HttpWebRequest CreateRequest(Uri url)
        {
            //创建请求
            var request = (HttpWebRequest)WebRequest.Create(url);
            //跟随重定向响应
            request.AllowAutoRedirect = true;
            //重定向最大数目
            request.MaximumAutomaticRedirections = 3;
            //request.UserAgent = _UserAgent;
            //request.Timeout = _TimeoutInMilliseconds;

            //返回请求结果
            return request;
        }

        private static IEnumerable<Uri> ProcessContent(BasicContent crawlerContent, Uri referenceUri)
        {
            var urlCandidates = new HashSet<Uri>();

            foreach (var processor in _processors)
            {
                if (processor.CanProcess(crawlerContent))
                {
                    //try
                    //{
                    var foundUrlCandidates = processor.Process(crawlerContent, referenceUri);

                    if (foundUrlCandidates != null)
                    {
                        AddRange(ref urlCandidates, (foundUrlCandidates));
                    }
                    //}
                    //catch (Exception exception)
                    //{
                    //    Logger.Warn("Spider process content throw exception", exception);
                    //}
                }
            }

            return urlCandidates;
        }

        private static void AddRange(ref HashSet<Uri> hashSet, IEnumerable<Uri> collection)
        {
            if (hashSet == null)
            {
                throw new ArgumentNullException("hashSet");
            }

            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            //添加到hashSet中
            foreach (var item in collection)
            {
                //Uri uri = new Uri(referenceUri, item);
                if (!hashSet.Contains(item))
                    hashSet.Add(item);
            }
        }

#if DEBUG

        [TestMethod]
        public void TestCreatHtml()
        {
            Html html = CreatHtml(new Uri("http://www.baidu.com"));
            Assert.IsTrue(html.Title == "百度一下，你就知道");
            Assert.IsTrue(html.Content.Length > 10);
            Assert.IsTrue(html.SourceCode.Length > 10);
            Assert.IsTrue(html.ChildrenLink != null && html.ChildrenLink.Count > 0);
        }

#endif

    }
}
