using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Guoli.Tender.Web.Downloader;
using HtmlAgilityPack;

namespace Guoli.Tender.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            //var url =
            //    "http://wz.guangzh.95306.cn:80/mainPageNoticeList.do?method=init&id=7200001&cur=1&keyword=&inforCode=&time0=&time1=";

            //var html = Spider.GetArticleList(url);

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
            {
                Response.Flush();
            }
        }
    }
}
