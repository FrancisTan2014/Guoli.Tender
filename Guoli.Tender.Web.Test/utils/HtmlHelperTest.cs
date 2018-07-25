using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Guoli.Tender.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Guoli.Tender.Web.Test.utils
{
    [TestClass]
    public class HtmlHelperTest
    {
        [TestMethod]
        public void ShouldRemoveAllWhiteSpaces()
        {
            var text = "test remove \r\nwhite \tspaces";
            var textWithoutWhiteSpaces = "testremovewhitespaces";
            var res = HtmlHelper.WithoutWhiteSpaces(text);
            res.Should().Be(textWithoutWhiteSpaces);
        }

        [TestMethod]
        public void ShouldRemoveAllHtmlTags()
        {
            var html = "<html><b>t</b></ br>e<strong>s</strong>t</html>";
            var expected = "test";
            var res = HtmlHelper.WithoutHtmlTags(html);
            res.Should().Be(expected);
        }

        [TestMethod]
        public void ShouldRemoveAllQuery()
        {
            var url = "http://wz.harb.95306.cn:9000/mainPageNoticeList.do?method=list&cur=1";
            var expected = "http://wz.harb.95306.cn:9000/mainPageNoticeList.do";
            var res = HtmlHelper.WithoutQuery(url);
            res.Should().Be(expected);

            url = "http://wz.harb.95306.cn:9000/mainPageNoticeList.do";
            res = HtmlHelper.WithoutQuery(url);
            expected = url;
            res.Should().Be(expected);
        }

        [TestMethod]
        public void ShouldGetTheRightUrl()
        {
            var url = "http://wz.harb.95306.cn:9000/mainPageNoticeList.do?method=list&cur=1";
            var args = new Dictionary<string, string>
            {
                { "page", "10" },
                { "size", "20" }
            };
            var expected = url + "&page=10&size=20";
            var res = HtmlHelper.ConcatQuery(url, args);
            res.Should().Be(expected);

            url = "http://wz.harb.95306.cn:9000/mainPageNoticeList.do";
            expected = url + "?&page=10&size=20";
            res = HtmlHelper.ConcatQuery(url, args);
            res.Should().Be(expected);
        }

        [TestMethod]
        public void ShouldGetTheRightUrl2()
        {
            var url = "http://wz.sheny.95306.cn/mainPageNoticeList.do?method=list&cur=1";
            var path = "mainPageNotice.do?method=info&id=OT002018072303304211%40OT002018072303304212%4010";
            var expected =
                "http://wz.sheny.95306.cn/mainPageNotice.do?method=info&id=OT002018072303304211%40OT002018072303304212%4010";
            var res = HtmlHelper.ConcatUrl(url, path);
            res.Should().Be(expected);
        }
    }
}
