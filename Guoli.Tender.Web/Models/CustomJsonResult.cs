using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Guoli.Tender.Web.Models
{
    public sealed class CustomJsonResult: JsonResult
    {
        public string FormatString { get; set; }

        public CustomJsonResult(object data, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet)
        {
            FormatString = "YYYY-MM-DD HH:mm:ss";
            Data = data;
            MaxJsonLength = int.MaxValue;
            JsonRequestBehavior = behavior;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            response.Write(JsonConvert.SerializeObject(Data));
        }
    }
}