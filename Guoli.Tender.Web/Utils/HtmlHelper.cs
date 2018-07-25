using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Guoli.Tender.Web
{
    public sealed class HtmlHelper
    {
        public static string WithoutHtmlTags(string html)
        {
            var pattern = "</*\\s*\\w+>";
            return Regex.Replace(html, pattern, "");
        }

        public static string WithoutWhiteSpaces(string txt)
        {
            var pattern = "\\s+";
            return Regex.Replace(txt, pattern, "");
        }

        public static string WithoutQuery(string url)
        {
            var pattern = "\\?.*";
            return Regex.Replace(url, pattern, "");
        }

        public static string ConcatQuery(string url, Dictionary<string, string> args)
        {
            var list = from couple in args select $"{couple.Key}={couple.Value}";
            var query = "&" + string.Join("&", list);
            if (url.IndexOf("?") == -1)
            {
                query = "?" + query;
            }

            return url + query;
        }

        public static string ConcatUrl(string url, string relativePath)
        {
            var uri = new Uri(url);
            var b = new StringBuilder();
            b.Append(uri.Scheme);
            b.Append("://");
            b.Append(uri.Host);
            if (uri.Port != 80)
            {
                b.Append(":" + uri.Port);
            }
            if (!relativePath.StartsWith("/"))
            {
                b.Append("/");
            }
            b.Append(relativePath);

            return b.ToString();
        }
    }
}