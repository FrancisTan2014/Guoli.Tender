using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Guoli.Tender.Web.Models
{
    public class ArticleQueryModel: QueryModel
    {
        public int departId { get; set; }

        public DateTime? start { get; set; }

        public DateTime? end { get; set; }

        public int readStatus { get; set; }
    }
}