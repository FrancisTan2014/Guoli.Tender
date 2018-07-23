using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guoli.Tender.Model
{
    public class ExceptionLog
    {
        public int Id { get; set; }
        public string ClassName { get; set; }
        public string Method { get; set; }
        public string StackTrace { get; set; }
        public string Remark { get; set; }
        public DateTime AddTime { get; set; }
    }
}
