using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guoli.Tender.Model
{
    public sealed class Keyword: IEntity<int>
    {
        public int Id { get; set; }

        /// <summary>
        /// 完整的关键词，可能比较长（如 LKJ 监控装置相关配件）
        /// </summary>
        public string FullKeyword { get; set; }

        /// <summary>
        /// 对关键词进行细粒度的拆分，以达到更好的搜索效果，不同的词之间用逗号隔开
        /// </summary>
        public string SplitedKeywords { get; set; }

        public DateTime AddTime { get; set; }

        /// <summary>
        /// 是否是热点关键词，若为 true，那么此关键词可能会被特别关注
        /// 例如为其缓存搜索结果数量、与其相关信息提醒等
        /// </summary>
        public bool IsHot { get; set; }

        [NotMapped]
        public string[] SplitedKeywordsArray => SplitedKeywords?.Split(new[] {',', '，' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
    }
}
