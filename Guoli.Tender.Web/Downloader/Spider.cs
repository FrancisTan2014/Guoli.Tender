using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Guoli.Tender.Model;
using HtmlAgilityPack;

namespace Guoli.Tender.Web.Downloader
{
    public class Spider
    {
        public static IList<Article> GetArticleList(string url)
        {
            var list = new List<Article>();
            var client = new WebClient {Encoding = Encoding.UTF8};
            var html = client.DownloadString(url);

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
    }
}