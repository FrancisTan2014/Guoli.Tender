using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;
using Guoli.Tender.Web.Models;
using Guoli.Tender.Web.Utils;

namespace Guoli.Tender.Web.Controllers
{
    public sealed class KeywordController: BaseController<Keyword, int>
    {
        protected override IRepository<Keyword, int> Repos { get; set; }

        public KeywordController()
        {
            Repos = new KeywordRepository();
        }

        public override JsonResult Add(Keyword model)
        {
            model.AddTime = DateTime.Now;

            var success = Repos.Insert(model).Id > 0;

            var newKeywords = new List<string>();
            foreach (var k in model.SplitedKeywordsArray)
            {
                if (!KeywordHelper.Exists(k))
                {
                    newKeywords.Add(k);
                }
            }
            if (newKeywords.Count > 0)
            {
                KeywordHelper.Add(newKeywords.ToArray());
            }

            var reply = Reply.Get(success);
            return Json(reply);
        }

        public JsonResult FetchList(QueryModel query)
        {
            var total = Repos.Count();
            var skipCnt = (query.page - 1) * query.size;
            var list = Repos.GetAll().OrderByDescending(k => k.AddTime).Skip(skipCnt).Take(query.size);
            var reply = Reply.OfSuccess(new
            {
                total,
                list
            });
            return Json(reply, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ChangeStatus(int id, bool status)
        {
            var model = Repos.Get(id);
            if (model != null)
            {
                model.IsHot = status;
                var success = Repos.Update(model);
                var reply = Reply.Get(success);
                return Json(reply);
            }

            return Json(Reply.OfFailed());
        }
    }
}