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

        private List<string> MakeUrl(string listUrl)
        {
            //[{n:'采购',c:'1',w:false},{n:'招标',c:'3',m1:true,w:true},{n:'竞买',c:'4',w:true},{n:'集中竞价',c:'6',m2:true},{n:'销售',c:'2',w:false},{n:'竞卖',c:'5',m1:true,m2:true},{n:'公示',c:'7',w:false},{n:'审前公示',c:'8',m1:true}, { n: '采购公示',c: '9',m2: true}
            var types = new[] { 3, 4, 5, 6, 8, 9 };
            // [{n:'项目公告',c:'0'},{n:'中标公告',c:'2'},{n:'变更公告',c:'3'},{n:'补遗公告',c:'4'},{n:'结果补遗',c:'5'},{n:'流标公告',c:'6'}]
            var flags = new[] { 0, 2, 3, 4, 5, 6 };
            // [{n:'全部',c:'0'},{n:'线上项目',c:'1',w:false},{n:'正式项目',c:'3',m1:true,w:true},{n:'模拟项目',c:'4',m2:true},{n:'线下项目',c:'2'}]
            var projs = new[] { 2, 3, 4 };
            // [{n:'全部',c:'000'},{n:'中国铁路沈阳局集团有限公司',c:'T00'}]
            var markets = new[] { "000" };

        }
    }
}