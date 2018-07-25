using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;

namespace Guoli.Tender.Web
{
    public sealed class ExceptionFilter: HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var repos = new ExceptionLogRepository();
            var ex = new ExceptionLog
            {
                ClassName = nameof(ExceptionFilter),
                Method = "OnException",
                StackTrace = filterContext.Exception.StackTrace,
                Remark = filterContext.Exception.Message,
                AddTime = DateTime.Now
            };
            repos.Insert(ex);

            base.OnException(filterContext);
        }
    }
}