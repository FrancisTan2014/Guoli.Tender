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
    public sealed class UserController: BaseController<User, int>
    {
        protected override IRepository<User, int> Repos { get; set; }

        public UserController()
        {
            Repos = new UserRepository();
        }

        public override JsonResult Add(User model)
        {
            throw new NotImplementedException();
        }

        public JsonResult Login(User model)
        {
            var user = Repos.Find(u => u.Username == model.Username)
                        .SingleOrDefault();
            if (user == null)
            {
                
            }

            return Json(Reply.OfSuccess());
        }
    }
}