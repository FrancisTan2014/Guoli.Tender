using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;
using Guoli.Tender.Web.Models;
using Guoli.Tender.Web.Utils;

namespace Guoli.Tender.Web.Controllers
{
    public sealed class ArticleController: BaseController<Article, int>
    {
        protected override IRepository<Article, int> Repos { get; set; }

        public ArticleController()
        {
            Repos = new ArticleRepository();
        }

        [HttpPost]
        public JsonResult HasRead(int id, bool status)
        {
            var article = Repos.Get(id);
            if (article != null)
            {
                article.HasRead = status;

                var success = Repos.Update(article);
                var res = Reply.Get(success);
                return Json(res);
            }

            return Json(Reply.OfFailed());
        }

        public JsonResult FetchList(ArticleQueryModel query)
        {
	        long total;
	        var list = string.IsNullOrEmpty(query.keyword) 
					? GetFromSqlDb(query, out total) 
					: EsHelper.Search(query, out total);

            var res = Reply.OfSuccess(new
            {
                total,
                list
            });
            return CustomJson(res);
        }

	    private IEnumerable<Article> GetFromSqlDb(ArticleQueryModel query, out long total)
	    {
			var list = Repos.GetAll();
		    if (query.start != null)
		    {
			    list = list.Where(a => a.PubTime > query.start.Value);
		    }
		    if (query.end != null)
		    {
			    list = list.Where(a => a.PubTime < query.end.Value);
		    }
		    if (query.departId > 0)
		    {
			    list = list.Where(a => a.DepartmentId == query.departId);
		    }
		    if (query.readStatus >= 0)
		    {
			    var hasRead = query.readStatus == 1;
			    list = list.Where(a => a.HasRead == hasRead);
		    }
		    if (!string.IsNullOrEmpty(query.title))
		    {
			    list = list.Where(a => a.Title.Contains(query.title));
		    }
		    if (query.day != null)
		    {
			    var d = query.day.Value;
			    list = list.Where(a => a.PubTime.Year == d.Year && a.PubTime.Month == d.Month && a.PubTime.Day == d.Day);
		    }

		    total = list.Count();
		    list = list.OrderByDescending(a => a.PubTime)
			    .Skip((query.page - 1) * query.size)
			    .Take(query.size);

		    return list;
	    }
    }
}