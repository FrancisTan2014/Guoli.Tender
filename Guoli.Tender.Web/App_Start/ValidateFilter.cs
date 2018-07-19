using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Guoli.Tender.Web.Models;

namespace Guoli.Tender.Web
{
    public class ValidateFilter: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ctx = filterContext;
            var data = ctx.Controller.ViewData;
            if (!data.ModelState.IsValid)
            {
                ctx.Result = new JsonResult
                {
                    Data = Reply.OfParamsError()
                };
            }

            base.OnActionExecuting(filterContext);
        }
    }
}