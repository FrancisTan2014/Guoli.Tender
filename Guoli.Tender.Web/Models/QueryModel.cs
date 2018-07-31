using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Guoli.Tender.Web.Models
{
    public class QueryModel
    {
        [Required]
        public int page { get; set; }

        [Required]
        public int size { get; set; }
    }
}