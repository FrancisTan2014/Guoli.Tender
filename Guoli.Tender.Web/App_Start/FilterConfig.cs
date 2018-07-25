using System.Web;
using System.Web.Mvc;

namespace Guoli.Tender.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ValidateFilter());
            filters.Add(new ExceptionFilter());
        }
    }
}
