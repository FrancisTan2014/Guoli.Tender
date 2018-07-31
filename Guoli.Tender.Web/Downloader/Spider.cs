using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;
using HtmlAgilityPack;

namespace Guoli.Tender.Web
{
    public sealed class Spider
    {
        private bool _enableProxy = false;
        private ConcurrentQueue<Host> _proxyHost = new ConcurrentQueue<Host>();

        private void LoadProxy()
        {
            var api = "http://api3.xiguadaili.com/ip/?tid=557359382051037&num=5000";
            var request = (HttpWebRequest)WebRequest.Create(api);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var hosts = sr.ReadToEnd();
                    if (string.IsNullOrEmpty(hosts))
                    {
                        throw new Exception("未能获取到代理 IP");
                    }

                    hosts = Regex.Replace(hosts, "\\s+", ",");

                    var arr = hosts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var h in arr)
                    {
                        var host = new Host(h);
                        _proxyHost.Enqueue(host);
                    }
                }
            }
        }

        private Host GetProxy()
        {
            if (_proxyHost.Count == 0)
            {
                LoadProxy();
            }

            Host host = null;
            while (host == null)
            {
                if (_proxyHost.TryDequeue(out host))
                {
                    break;
                }

                Thread.Sleep(50);
            }

            return host;
        }

        public string DownloadHtml(string url, Encoding encoding = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                Host proxyHost = null;
                if (_enableProxy)
                {
                    proxyHost = GetProxy();
                    request.Proxy = new WebProxy(proxyHost.Ip, proxyHost.Port);
                }

                request.Method = "GET";
                request.Timeout = 2 * 60 * 1000;
                request.AllowAutoRedirect = true;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var stream = response.GetResponseStream();
                    using (var sr = new StreamReader(stream, encoding ?? Encoding.UTF8))
                    {
                        var html = sr.ReadToEnd();
                        if (proxyHost != null)
                        {
                            _proxyHost.Enqueue(proxyHost);
                        }
                        return html;
                    }
                }
            }
            catch (Exception ex)
            {
                var _logRepos = new ExceptionLogRepository();
                var log = new ExceptionLog
                {
                    ClassName = nameof(Spider),
                    Method = nameof(GetArticleList),
                    StackTrace = ex.StackTrace,
                    Remark = $"msg={ex.Message}; url={url}",
                    AddTime = DateTime.Now
                };
                _logRepos.Insert(log);
                return string.Empty;
            }
        }

        public List<Article> GetArticleList(string url)
        {
            var list = new List<Article>();

            try
            {
                var html = DownloadHtml(url);
                var document = new HtmlDocument();
                document.LoadHtml(html);

                var listTableClassName = "listInfoTable";
                var table = document.DocumentNode
                    .Descendants("table")
                    .SingleOrDefault(e => e.Attributes["class"].Value == listTableClassName);
                if (table == null)
                {
                    return list;
                }

                var trs = table.Descendants("tr").ToList();
                for (var i = 1; i < trs.Count; i++)
                {
                    var row = trs[i];
                    var a = row.SelectNodes("td[1]/a")[0];
                    var title = a.InnerText.Trim();
                    var td = row.SelectNodes("td[2]")[0];
                    var queryId = td.InnerText.Trim();

                    var href = a.Attributes["href"].Value;
                    var articleLink = HtmlHelper.ConcatUrl(url, href);

                    list.Add(new Article
                    {
                        Title = title,
                        SourceUrl = articleLink,
                        QueryId = queryId
                    });
                }
            }
            catch (Exception ex)
            {
                var _logRepos = new ExceptionLogRepository();
                var log = new ExceptionLog
                {
                    ClassName = nameof(Spider),
                    Method = nameof(GetArticleList),
                    StackTrace = ex.StackTrace,
                    Remark = $"msg={ex.Message}; url={url}",
                    AddTime = DateTime.Now
                };
                _logRepos.Insert(log);
            }

            return list;
        }

        public bool GetArticleContent(Article article)
        {
            try
            {
                var html = DownloadHtml(article.SourceUrl);
                var document = new HtmlDocument();
                document.LoadHtml(html);

                var contentDiv = document.DocumentNode
                    .Descendants("div")
                    .SingleOrDefault(e => e.Attributes["class"]?.Value?.Contains("noticeBox") ?? false);
                if (contentDiv == null)
                {
                    return false;
                }

                var time = contentDiv.SelectNodes("div[1]/p[2]/span[2]")[0].InnerText.Trim();
                var content = contentDiv.SelectNodes("div[2]")[0].InnerHtml;
                var txt = HtmlHelper.WithoutHtmlTags(content);

                var str = HtmlHelper.WithoutWhiteSpaces(txt);
                var len = str.Length >= 150 ? 150 : str.Length;
                var summary = str.Substring(0, len);

                article.Content = content;
                article.ContentWithoutHtml = txt;
                article.PubTime = DateTime.Parse(time);
                article.Summary = summary;

                return true;
            }
            catch (Exception ex)
            {
                var _logRepos = new ExceptionLogRepository();
                var log = new ExceptionLog
                {
                    ClassName = nameof(Spider),
                    Method = nameof(GetArticleContent),
                    StackTrace = ex.StackTrace,
                    Remark = $"msg={ex.Message};   url={article.SourceUrl}",
                    AddTime = DateTime.Now
                };
                _logRepos.Insert(log);
                return false;
            }
        }
    }

    sealed class Host
    {
        public string Ip { get; set; }
        public int Port { get; set; }

        public Host(string host)
        {
            var arr = host.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            Ip = arr[0];
            Port = Convert.ToInt32(arr[1]);
        }
    }
}