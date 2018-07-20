using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;

namespace Guoli.Tender.Web
{
    public sealed class DownloadTask
    {
        private ConcurrentQueue<Article> _queue = new ConcurrentQueue<Article>();

        private IRepository<Department, int> _departRepos = new DepartmentRepository();

        public void DownloadAll()
        {
            var departs = _departRepos.GetAll();
            foreach (var d in departs)
            {
                
            }
        }
    }
}