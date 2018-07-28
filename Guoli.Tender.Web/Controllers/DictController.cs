using Guoli.Tender.Model;
using Guoli.Tender.Repos;
using Guoli.Tender.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Guoli.Tender.Web.Controllers
{
    public sealed class DictController: BaseController<Dictionaries, int>
    {
        protected override IRepository<Dictionaries, int> Repos { get; set; }

        public DictController()
        {
            Repos = new DictionariesRepository();
        }

        public override JsonResult Add(Dictionaries model)
        {
            model.AddTime = DateTime.Now;
            return base.Add(model);
        }

        public JsonResult AddType(string name)
        {
            var dict = new Dictionaries
            {
               Name = name,
               ParentId = 0,
               Type = 0,
               AddTime = DateTime.Now
            };
            return base.Add(dict);
        }

        public JsonResult FetchByType(int type)
        {
            var list = Repos.Find(d => d.Type == type);
            return Json(Reply.OfSuccess(list));
        }

        public JsonResult FetchTypes()
        {
            var list = Repos.Find(d => d.Type == 0);
            return Json(Reply.OfSuccess(list));
        }
    }
}