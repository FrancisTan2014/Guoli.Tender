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
    /// <summary>
    /// 封装抓取文章信息的任务逻辑，本类是单例的
    /// 内部维护了一个多线程下载的环境，因此不需
    /// 要在外部启动多个下载任务
    /// </summary>
    public sealed class DownloadTask
    {
        private Spider _spider;

        // 下载时间间隔值（ms），防止 503 问题
        private const int ARTICLE_DOWNLOAD_TIMESPAN = 1000;
        private const int LIST_DOWNLOAD_TIMESPAN = ARTICLE_DOWNLOAD_TIMESPAN * 20;
        // 指示当队列中的文章数达到此阀值时，进行持久化操作
        // 批量的持久化操作，有利于提高写数据库的效率
        private const int MAX_COUNT_TO_PERSISTENCE = 1000;
        // 持久化操作间隔（ms），假设每偏文章下载需要 300ms，同时有 18（共有18个机务段） 个线程在下载文章
        private const int PERSISTENCE_TIMESPAN = (MAX_COUNT_TO_PERSISTENCE * 300) / 18;

        // 将各单位的文章分别放入不同的桶中，分别进行下载，以控制下载单篇文件的时间间隔
        // 防止因短时间进行大量的请求，导致服务器拒绝服务等问题（503）
        private Dictionary<int, Queue<Article>> _buckets = new Dictionary<int, Queue<Article>>();
        //private ConcurrentQueue<Article> _waitedForDownloadingQueue = new ConcurrentQueue<Article>();
        private ConcurrentQueue<Article> _downloadedQueue = new ConcurrentQueue<Article>();
        // 用于存储已下载过的文章的唯一标识，所有的值均设置为0，
        // 在这里只需要使用到其查询速度快的特点，进行快速去重
        private Hashtable _queryIds = new Hashtable();

        private IRepository<Department, int> _departRepos = new DepartmentRepository();
        private IRepository<Article, int> _articleRepos = new ArticleRepository();
        private IRepository<ExceptionLog, int> _exceptionRepos = new ExceptionLogRepository();

        private int _departCount;
        private int _finishedDepartCount;
        private bool _listDownloadFinished = false;
        private bool _articleDownloadFinished = false;
        private bool _persitenceFinished = false;

        // 是否是第一次启动下载任务
        private bool _isFirstTime = false;
        // 任务在程序启动之后是否被启动过
        private bool _wasStartedBefore = false;

        private object _lockObj = new object();
        private static DownloadTask _singleInstance;

        public EventHandler<EventArgs> AfterTaskFinished;

        private DownloadTask()
        {
            _spider = new Spider();
        }

        static DownloadTask()
        {
            _singleInstance = new DownloadTask();
        }

        public static DownloadTask GetInstance()
        {
            return _singleInstance;
        }

        public void Start()
        {
            if (!_wasStartedBefore)
            {
                _wasStartedBefore = true;
                LoadQueryIds();
            }
            else
            {
                if (!IsFinished())
                {
                    return;
                }
            }

            _isFirstTime = !_articleRepos.GetAll().Any();

            DownloadPages();
            DownloadArticles();
            Persistence();
        }

        public bool IsFinished()
        {
            return _listDownloadFinished && _articleDownloadFinished && _persitenceFinished;
        }

        private void LoadQueryIds()
        {
            var queryIds = _articleRepos.GetAll().Select(a => a.QueryId);
            foreach (var id in queryIds)
            {
                if (!_queryIds.ContainsKey(id))
                {
                    _queryIds.Add(id, 0);
                }
            }
        }

        /// <summary>
        /// 下载所有单位的所有公告信息列表页面
        /// 的文章信息，每一个单位的下载任务
        /// 交由线程池中的一个线程来完成
        /// </summary>
        private void DownloadPages()
        {
            var departs = _departRepos.GetAll().ToList();
            _departCount = departs.Count;

            //var ids = GetIds();
            foreach (var d in departs)
            {
                if (!_wasStartedBefore)
                {
                    _buckets.Add(d.Id, new Queue<Article>());
                }
                ThreadPool.QueueUserWorkItem(o => DownloadPages(d));
            }
        }

        /// <summary>
        /// 下载指定单位的所有公告信息列表页面
        /// 的文章信息，在下载完成之后将状态同
        /// 步到 _listDownloadFinished 中
        /// </summary>
        /// <param name="depart"></param>
        private void DownloadPages(Department depart)
        {
            try
            {
                var args = new Dictionary<string, string>
                    {
                        { "method", "list" },
                        { "cur", "1" },
                    };
                var page = 1;
                var hasNewArticles = true;
                while (hasNewArticles)
                {
                    var articles = DownloadSinglePage(depart, args, page);

                    hasNewArticles = HasNewArticles(articles);
                    if (hasNewArticles)
                    {
                        var bucket = _buckets[depart.Id];
                        PutIntoBucket(articles, bucket);
                    }

                    page++;
                    Thread.Sleep(LIST_DOWNLOAD_TIMESPAN);
                }
            }
            catch (Exception ex)
            {
                // 为确保即使下载过程中发生异常
                // 任务也能被正常停止，在此处将
                // 异常记录之后继续后面的操作
                var log = new ExceptionLog
                {
                    ClassName = nameof(DownloadTask),
                    Method = nameof(DownloadPages),
                    Remark = ex.Message,
                    StackTrace = ex.StackTrace,
                    AddTime = DateTime.Now
                };
                _exceptionRepos.Insert(log);
            }
            finally
            {
                // 确保下载任务能被正确停止
                lock (_lockObj)
                {
                    _finishedDepartCount += 1;
                    if (_finishedDepartCount == _departCount)
                    {
                        _listDownloadFinished = true;
                    }
                }
            }
        }

        private bool HasNewArticles(List<Article> articles)
        {
            if (articles.Count > 0)
            {
                if (_isFirstTime)
                {
                    // 第一次启动任务，所有文章都是最新的
                    return true;
                }

                // 如果不是第一次启动任务，判断是否有
                // 新的文章的依据是时间，这里将逻辑简
                // 化为与当前日期进行对比，认定今天的
                // 文章为新文章，否则为旧文章
                foreach (var a in articles)
                {
                    var timeSpan = DateTime.Now - a.PubTime;
                    if (timeSpan.Days == 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            return false;
        }

        private List<Article> DownloadSinglePage(Department depart, Dictionary<string, string> args, int page)
        {
            args["cur"] = page.ToString();

            var link = HtmlHelper.WithoutQuery(depart.ListPageUrl);
            var url = HtmlHelper.ConcatQuery(link, args);
            var articles = _spider.GetArticleList(url);
            articles.ForEach(a =>
            {
                a.DepartmentId = depart.Id;
                //GetTypesForArticle(a, args["id"]);
            });

            return articles;
        }

        private void GetTypesForArticle(Article article, string id)
        {
            article.TenderType = Convert.ToInt32(id[0]);
            article.NoticeType = Convert.ToInt32(id[1]);
            article.PropType = Convert.ToInt32(id[2]);
        }

        private void DownloadArticles()
        {
            foreach (var bucket in _buckets.Values)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    while (!_articleDownloadFinished)
                    {
                        if (bucket.Count > 0)
                        {
                            var article = bucket.Dequeue();
                            var success = _spider.GetArticleContent(article);
                            if (success)
                            {
                                _downloadedQueue.Enqueue(article);
                            }
                        }

                        if (bucket.Count == 0)
                        {
                            lock (_lockObj)
                            {
                                _articleDownloadFinished = _listDownloadFinished && IsBucketsEmpty();
                            }
                        }

                        Thread.Sleep(ARTICLE_DOWNLOAD_TIMESPAN);
                    }
                });
            }
        }

        private bool IsBucketsEmpty()
        {
            return !_buckets.Any(q => q.Value.Count > 0);
        }

        /// <summary>
        /// 启动数据持久化任务，这是下载任务的最后一环
        /// 当此任务检测到下载完成，且已将所有数据写入
        /// 数据库后，则发起任务完成通知
        /// </summary>
        private void Persistence()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    if (_downloadedQueue.Count >= MAX_COUNT_TO_PERSISTENCE)
                    {
                        List<Article> list;
                        lock (_lockObj)
                        {
                            list = _downloadedQueue.ToList();
                            _downloadedQueue = new ConcurrentQueue<Article>();
                        }
                        _articleRepos.BulkInsert(list);
                    }

                    if (IsDownloadFinished() && _downloadedQueue.Count == 0)
                    {
                        _persitenceFinished = true;
                        TaskFinished();
                        break;
                    }

                    Thread.Sleep(PERSISTENCE_TIMESPAN);
                }
            });
        }

        /// <summary>
        /// 在任务完成之后统一处理后续逻辑
        /// </summary>
        private void TaskFinished()
        {
            AfterTaskFinished?.Invoke(this, new EventArgs());
        }

        private bool IsDownloadFinished()
        {
            return _listDownloadFinished && _articleDownloadFinished;
        }

        private void PutIntoBucket(List<Article> articles, Queue<Article> bucket)
        {
            lock (_queryIds)
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
                        //_waitedForDownloadingQueue.Enqueue(a);
                        bucket.Enqueue(a);
                    }
                }
            }
        }

        private List<string> GetIds()
        {
            //[{n:'采购',c:'1',w:false},{n:'招标',c:'3',m1:true,w:true},{n:'竞买',c:'4',w:true},{n:'集中竞价',c:'6',m2:true},{n:'销售',c:'2',w:false},{n:'竞卖',c:'5',m1:true,m2:true},{n:'公示',c:'7',w:false},{n:'审前公示',c:'8',m1:true}, { n: '采购公示',c: '9',m2: true}
            var types = new[] { 1, 2, 7 };
            // [{n:'项目公告',c:'0'},{n:'中标公告',c:'2'},{n:'变更公告',c:'3'},{n:'补遗公告',c:'4'},{n:'结果补遗',c:'5'},{n:'流标公告',c:'6'}]
            var flags = new[] { 0, 2, 3, 4, 5, 6 };
            // [{n:'全部',c:'0'},{n:'线上项目',c:'1',w:false},{n:'正式项目',c:'3',m1:true,w:true},{n:'模拟项目',c:'4',m2:true},{n:'线下项目',c:'2'}]
            var projs = new[] { 0 };
            // [{n:'全部',c:'000'},{n:'中国铁路沈阳局集团有限公司',c:'T00'}]
            var markets = new[] { "000" };

            return (from t in types from f in flags from p in projs from m in markets select t + f + p + m).ToList();
        }
    }
}