using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;

namespace Guoli.Tender.Web
{
    public sealed class DownloadTask
    {
        private const int DOWNLOAD_TIME_SPAN = 500;
        private const int MAX_COUNT_TO_PERSISTENCE = 1000;

        private ConcurrentQueue<Article> _waitForDownloadingQueue = new ConcurrentQueue<Article>();
        private ConcurrentQueue<Article> _downloadedQueue = new ConcurrentQueue<Article>();
        private Hashtable _queryIds = new Hashtable();

        private IRepository<Department, int> _departRepos = new DepartmentRepository();
        private IRepository<Article, int> _articleRepos = new ArticleRepository();

        private bool _listPageDownloadFinished = false;
        private bool _articleDownloadFinished = false;

        private object _lockObj = new object();

        public bool Finished { get; private set; }

        public DownloadTask()
        {
            Finished = false;
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(o => DownloadListPage());
            ThreadPool.QueueUserWorkItem(o => DownloadArticles());
        }

        private async void DownloadListPage()
        {
            var departs = _departRepos.GetAll();
            var ids = GetIds();
            var argList = from id in ids
                          select new Dictionary<string, string>
                {
                    { "method", "list" },
                    { "cur", "1" },
                    { "id", id }
                };
            foreach (var d in departs)
            {
                await DownloadListPage(d, argList);
            }

            _listPageDownloadFinished = true;
        }

        private async Task DownloadListPage(Department depart, IEnumerable<Dictionary<string, string>> argList)
        {
            var link = HtmlHelper.WithoutQuery(depart.ListPageUrl);
            foreach (var args in argList)
            {
                var page = 1;
                var hasArticles = true;
                while (hasArticles)
                {
                    args["cur"] = page.ToString();

                    var url = HtmlHelper.ConcatQuery(link, args);
                    var articles = await Spider.GetArticleList(url);
                    articles.ForEach(a =>
                    {
                        a.DepartmentId = depart.Id;
                        GetTypesForArticle(a, args["id"]);
                    });

                    if (articles.Count == 0)
                    {
                        hasArticles = false;
                    }
                    else
                    {
                        ProcessArticles(articles);
                    }

                    page++;
                    Thread.Sleep(DOWNLOAD_TIME_SPAN);
                }
            }
        }

        private void GetTypesForArticle(Article article, string id)
        {
            article.TenderType = id[0];
            article.NoticeType = id[1];
            article.PropType = id[2];
        }

        private void DownloadArticles()
        {
            while (true)
            {
                Article article;
                if (_waitForDownloadingQueue.TryDequeue(out article))
                {
                    ThreadPool.QueueUserWorkItem(async o =>
                    {
                        var success = await Spider.GetArticleContent(article);
                        if (success)
                        {
                            _downloadedQueue.Enqueue(article);
                        }
                    });
                }

                if (_listPageDownloadFinished && _waitForDownloadingQueue.Count == 0)
                {
                    _articleDownloadFinished = true;
                    break;
                }
            }
        }

        private void WriteToDb()
        {
            while (true)
            {
                if (_downloadedQueue.Count >= MAX_COUNT_TO_PERSISTENCE)
                {
                    var list = _downloadedQueue.ToList();
                    _downloadedQueue = new ConcurrentQueue<Article>();
                    _articleRepos.BulkInsert(list);
                }
            }
        }

        private void ProcessArticles(List<Article> articles)
        {
            foreach (var a in articles)
            {
                // 将文章的 QueryId 作为其唯一标识
                // 来进行排除重复项
                if (!_queryIds.ContainsKey(a.QueryId))
                {
                    // 利用 hashtable 查询速度快的特点
                    // 进行快速比对，这里 0 这个值没有
                    // 意义，仅作为一个占位符
                    _queryIds.Add(a.QueryId, 0);
                    _waitForDownloadingQueue.Enqueue(a);
                }
            }
        }

        private List<string> GetIds()
        {
            //[{n:'采购',c:'1',w:false},{n:'招标',c:'3',m1:true,w:true},{n:'竞买',c:'4',w:true},{n:'集中竞价',c:'6',m2:true},{n:'销售',c:'2',w:false},{n:'竞卖',c:'5',m1:true,m2:true},{n:'公示',c:'7',w:false},{n:'审前公示',c:'8',m1:true}, { n: '采购公示',c: '9',m2: true}
            var types = new[] { 3, 4, 5, 6, 8, 9 };
            // [{n:'项目公告',c:'0'},{n:'中标公告',c:'2'},{n:'变更公告',c:'3'},{n:'补遗公告',c:'4'},{n:'结果补遗',c:'5'},{n:'流标公告',c:'6'}]
            var flags = new[] { 0, 2, 3, 4, 5, 6 };
            // [{n:'全部',c:'0'},{n:'线上项目',c:'1',w:false},{n:'正式项目',c:'3',m1:true,w:true},{n:'模拟项目',c:'4',m2:true},{n:'线下项目',c:'2'}]
            var projs = new[] { 2, 3, 4 };
            // [{n:'全部',c:'000'},{n:'中国铁路沈阳局集团有限公司',c:'T00'}]
            var markets = new[] { "000" };

            return (from t in types from f in flags from p in projs from m in markets select t + f + p + m).ToList();
        }
    }
}