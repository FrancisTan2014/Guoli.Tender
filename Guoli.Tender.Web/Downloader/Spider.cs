using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Guoli.Tender.Model;
using HtmlAgilityPack;

namespace Guoli.Tender.Web
{
    public sealed class Spider
    {
        private static WebClient _client;
        private static object _lockObj = new object();

        private static WebClient GetClient(Encoding encoding = null)
        {
            if (_client == null)
            {
                lock (_lockObj)
                {
                    if (_client == null)
                    {
                        _client = new WebClient();
                    }
                }
            }

            _client.Encoding = encoding ?? Encoding.UTF8;

            return _client;
        }

        public static HtmlDocument DownloadHtml(string url, Encoding encoding = null)
        {
            var client = GetClient();
            var html = client.DownloadString(url);

            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }

        public static IList<Article> GetArticleList(string url)
        {
            var list = new List<Article>();
            var document = DownloadHtml(url);

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
                var href = a.Attributes["href"].Value;
                var td = row.SelectNodes("td[2]")[0];
                var queryId = td.InnerText.Trim();
                list.Add(new Article
                {
                    Title = title,
                    SourceUrl = href,
                    QueryId = queryId
                });
            }

            return list;
        }

        public static Article GetArticleContent(Article article)
        {
            var document = DownloadHtml(article.SourceUrl);

            var contentDiv = document.DocumentNode.Descendants("div")
                .SingleOrDefault(e => e.Attributes["class"].Value.Contains("noticeBox"));
            //if (contentDiv == null)
            //{
            //    return article;
            //}

            var time = contentDiv.SelectNodes("div[1]/p[2]/span[2]")[0].InnerText.Trim();
            var content = contentDiv.SelectNodes("div[2]")[0].InnerHtml;
            var txt = HtmlHelper.WithoutHtmlTags(content);
            var summary = txt.Substring(0, 150);

            article.Content = content;
            article.ContentWithoutHtml = txt;
            article.PubTime = DateTime.Parse(time);
            article.Summary = summary;

            return article;
        }
    }
}