using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;
using Guoli.Tender.Web.Models;

namespace Guoli.Tender.Web.Controllers
{
    /// <summary>
    /// 封装基本的增删改查等模板方法
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class BaseController<TEntity, TKey>: Controller
        where TEntity: class 
    {
        protected abstract IRepository<TEntity, TKey> Repos { get; set; }

        private Reply GetReply(bool success)
        {
            return success ? Reply.OfSuccess() : Reply.OfFailed();
        }

        [HttpPost]
        public virtual JsonResult Add(TEntity model)
        {
            var m = (IEntity<int>)Repos.Insert(model);

            var success = m.Id > 0;
            var res = GetReply(success);
            return Json(res);
        }

        [HttpPost]
        public virtual JsonResult Update(TEntity model)
        {
            var success = Repos.Update(model);
            var res = GetReply(success);
            return Json(res);
        }

        [HttpPost]
        public virtual JsonResult Remove(TKey id)
        {
            var success = Repos.Remove(id);
            var res = GetReply(success);
            return Json(res);
        }

        public virtual JsonResult FetchAll()
        {
            var list = Repos.GetAll();
            var res = Reply.OfSuccess(list);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult Fetch(TKey id)
        {
            var model = Repos.Get(id);
            var res = Reply.OfSuccess(model);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
    }
}