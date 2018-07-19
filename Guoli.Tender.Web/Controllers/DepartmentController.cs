using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;

namespace Guoli.Tender.Web.Controllers
{
    public sealed class DepartmentController: BaseController<Department, int>
    {
        protected override IRepository<Department, int> Repos { get; set; }

        public DepartmentController()
        {
            Repos = new DepartmentRepository();
        }
    }
}