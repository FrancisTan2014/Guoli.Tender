using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}