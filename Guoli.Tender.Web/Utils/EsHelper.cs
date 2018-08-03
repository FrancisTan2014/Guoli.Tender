using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Guoli.Tender.Model;
using Guoli.Tender.Repos;
using Guoli.Tender.Web.Models;
using Guoli.Tender.Web.Utils;
using Nest;

namespace Guoli.Tender.Web
{
    public static class EsHelper
    {
        private const string indexName = "tender";

        private static ElasticClient GetClient()
        {
            var esUrl = ConfigurationManager.AppSettings["EsUrl"];
            var settings = new ConnectionSettings(new Uri(esUrl))
                .DefaultIndex(indexName)
                .DeadTimeout(TimeSpan.FromMinutes(2))
                .DisableDirectStreaming();
            var client = new ElasticClient(settings);
            return client;
        }

        private static bool CreateIndex()
        {
            var client = GetClient();
            var analyzer = ConfigurationManager.AppSettings["EsAnalyzer"];
            var response  = client.CreateIndex(indexName, 
                    c => c.Mappings(
                        ms => ms.Map<Article>(
                            m => m.Properties(
                                p => p.Text(
                                        s => s.Name(a => a.Title)
                                              .Analyzer(analyzer)
                                              .SearchAnalyzer(analyzer))
                                      .Text(
                                        s => s.Name(a => a.ContentWithoutHtml)
                                              .Analyzer(analyzer)
                                              .SearchAnalyzer(analyzer))
                                      .Text(s => s.Name(a => a.QueryId))
                                      .Date(s => s.Name(a => a.PubTime))
                                      .Boolean(s => s.Name(a => a.HasRead))
                                      .Number(s => s.Name(a => a.DepartmentId))
                ))));

            return response.ShardsAcknowledged;
        }

        public static void EnsureIndexHasCreated()
        {
            var client = GetClient();
            var res = client.IndexExists(indexName);
            if (!res.Exists)
            {
                CreateIndex();
            }
        }

        public static void Index(IEnumerable<Article> articles)
        {
            try
            {
                EnsureIndexHasCreated();

                var client = GetClient();
                var res = client.Bulk(b => b.CreateMany(articles)
                    .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(2))));
                var ex = res.OriginalException;
                ExceptionLogger.Error(nameof(EsHelper), nameof(Index), $"{ex.Message}", ex);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Error(nameof(EsHelper), nameof(Index), ex.Message, ex);
            }
        }

        public static IEnumerable<Article> Search(ArticleQueryModel queryParams, out long total)
        {
            var client = GetClient();
	        var request = BuildQueryRequest(queryParams);
            var response = client.Search<Article>(request);

	        total = response.Total;
	        return response.Hits.Select(s =>
	        {
	            var a = s.Source;
	            var hit = s.Highlights;

	            var titleKey = LowerFirstLetter(nameof(Article.Title));
	            if (hit.ContainsKey(titleKey))
	            {
	                a.Title = ConcatHighlights(hit[titleKey]);
                }

	            var contentKey = LowerFirstLetter(nameof(Article.ContentWithoutHtml));
	            if (hit.ContainsKey(contentKey))
	            {
	                a.ContentWithoutHtml = ConcatHighlights(hit[contentKey]);
                }
	            return a;
	        });
        }

        private static string LowerFirstLetter(string s)
        {
            var first = s.Substring(0, 1).ToLower();
            if (s.Length > 1)
            {
                return first + s.Substring(1);
            }
            else
            {
                return first;
            }
        }

        private static string ConcatHighlights(HighlightHit hit)
        {
            return string.Join("", hit.Highlights);
        }

        private static ISearchRequest BuildQueryRequest(ArticleQueryModel queryParams)
        {
            var q = queryParams;

	        QueryContainer query = null;

			#region filter context

            Expression<Func<Article, object>> fieldEx = a => a.DepartmentId;
			var filters = new List<QueryContainer>();
			if (q.departId > 0)
			{
				filters.Add(new TermQuery
				{
					Field = new Field(fieldEx),
					Value = q.departId
				});
			}
			//if (q.readStatus >= 0)
			//{
			//	filters.Add(new TermQuery
			//	{
			//		Field = nameof(Article.HasRead),
			//		Value = q.readStatus
			//	});
			//}
			if (q.day != null)
			{
				filters.Add(new DateRangeQuery
				{
					GreaterThan = q.day.Value.AddMilliseconds(-10),
					LessThan = q.day.Value.AddDays(1)
				});
			}
			query &= new BoolQuery { Filter = filters };

			if (q.start != null && q.end != null)
			{
				query &= new DateRangeQuery
				{
					GreaterThan = q.start.Value,
					LessThan = q.end.Value,
				};
			} 
			#endregion

			var descriptor = new QueryContainerDescriptor<Article>();
			query &= descriptor.MultiMatch(
                    m => m.Fields(f => f.Field(a => a.Title).Field(a => a.ContentWithoutHtml))
                        .Query(queryParams.keyword));

	        var tags = GetHighlightTags();
            Expression<Func<Article, object>> sortFieldEx = a => a.PubTime;
            Expression<Func<Article, object>> titleEx = a => a.Title;
            Expression<Func<Article, object>> contentEx = a => a.ContentWithoutHtml;
            return new SearchRequest<Article>
			{
				From = (q.page - 1) * q.size,
				Size = q.size,
				Query = query,
				Sort = new List<ISort>
				{
					new SortField { Field = new Field(sortFieldEx), Order = SortOrder.Descending }
				},
				Highlight = new Highlight
				{
					PreTags = new [] { tags[0] },
					PostTags = new [] { tags[1] },
					Encoder = HighlighterEncoder.Html,
                    Fields = new Dictionary<Field, IHighlightField>
                    {
                        { new Field(titleEx), new HighlightField() },
                        { new Field(contentEx), new HighlightField() },
                    }
                }
			};
        }

	    private static string[] GetHighlightTags()
	    {
		    var name = "EsHighlightTag";
			var config = ConfigurationManager.AppSettings[name];
		    if (string.IsNullOrEmpty(config))
		    {
			    throw new ConfigurationErrorsException($"AppSettings configuration item \"{name}\" does not exist.");
		    }

		    var highlightTags = HtmlHelper.Decode(config);
		    var index = highlightTags.IndexOf("><");
		    var preTag = highlightTags.Substring(0, index + 1);
		    var postTag = highlightTags.Substring(index + 1);
		    return new[] {preTag, postTag};
	    }

        public static void SyncFromSqlServer()
        {
            var client = GetClient();
            var res = client.IndexExists(indexName);
            if (res.Exists)
            {
                client.DeleteIndex(indexName);
                CreateIndex();
            }

            var repos = new ArticleRepository();
            var page = 1;
            var size = 1000;
            List<Article> list;
            do
            {
                var skipCnt = (page - 1) * size;
                list = repos.GetAll().OrderBy(a => a.Id).Skip(skipCnt).Take(size).ToList();
                Index(list);
                page++;
            } while (list.Count > 0);
        }
    }
}