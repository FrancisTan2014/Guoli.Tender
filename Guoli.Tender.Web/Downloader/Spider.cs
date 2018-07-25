using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;
using HtmlAgilityPack;

namespace Guoli.Tender.Web
{
    public sealed class Spider
    {
        private static IRepository<ExceptionLog, int> _logRepos = new ExceptionLogRepository();

        private static WebClient GetClient(Encoding encoding = null)
        {
            var client = new WebClient { Encoding = encoding ?? Encoding.UTF8 };
            return client;
        }

        public static async Task<HtmlDocument> DownloadHtmlAsync(string url, Encoding encoding = null)
        {
            var client = GetClient();

            string html;
            try
            {
                html = await client.DownloadStringTaskAsync(url);
            }
            catch (Exception ex)
            {
                _logRepos.Insert(new ExceptionLog
                {
                    ClassName = nameof(Spider),
                    Method = nameof(DownloadHtmlAsync),
                    Remark = ex.Message,
                    StackTrace = ex.StackTrace,
                    AddTime = DateTime.Now
                });
                throw ex;
            }

            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }

        public static async Task<List<Article>> GetArticleList(string url)
        {
            var list = new List<Article>();

            var document = await DownloadHtmlAsync(url);

            var listTableClassName = "listInfoTable";
            var table = document.DocumentNode
                .Descendants("table")
                .SingleOrDefault(e => e.Attributes["class"].Value == listTableClassName);
            if (table == null)
            {
                return list;
            }

            var trs = table.Descendants("tr").ToList();
            try
            {
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
                var log = new ExceptionLog
                {
                    ClassName = nameof(Spider),
                    Method = nameof(GetArticleList),
                    StackTrace = ex.StackTrace,
                    Remark = $"url={url}\r\nhtml={document}",
                    AddTime = DateTime.Now
                };
                _logRepos.Insert(log);
            }

            return list;
        }

        public static async Task<bool> GetArticleContent(Article article)
        {
            var document = await DownloadHtmlAsync(article.SourceUrl);

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
            var summary = HtmlHelper.WithoutWhiteSpaces(txt).Substring(0, 150);

            article.Content = content;
            article.ContentWithoutHtml = txt;
            article.PubTime = DateTime.Parse(time);
            article.Summary = summary;

            return true;
        }
    }
}