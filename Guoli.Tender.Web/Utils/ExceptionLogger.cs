using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;

namespace Guoli.Tender.Web.Utils
{
    public static class ExceptionLogger
    {
        public static void Error(string className, string methodName, string remark, Exception ex)
        {
            var _logRepos = new ExceptionLogRepository();
            var log = new ExceptionLog
            {
                ClassName = className,
                Method = methodName,
                StackTrace = ex.StackTrace,
                Remark = remark,
                AddTime = DateTime.Now
            };
            _logRepos.Insert(log);
        }
    }
}