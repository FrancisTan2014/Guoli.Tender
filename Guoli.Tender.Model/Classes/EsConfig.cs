using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guoli.Tender.Model
{
    public sealed class EsConfig: IEntity<int>
    {
        public int Id { get; set; }

        /// <summary>
        /// 上次同步 Article 表格的最大 Id
        /// </summary>
        public int LastIdOfArticle { get; set; }

        
    }
}
