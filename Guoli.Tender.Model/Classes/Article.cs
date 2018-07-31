using System;
using System.ComponentModel.DataAnnotations;

namespace Guoli.Tender.Model
{
    public class Article: IEntity<int>
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        [Required]
        public string Summary { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string ContentWithoutHtml { get; set; }

        [Required]
        public DateTime PubTime { get; set; }

        /// <summary>
        /// 原网页地址
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// 公告所属单位
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// 竞标类型（如采购、销售或公示等）
        /// </summary>
        public int TenderType { get; set; }

        /// <summary>
        /// 公告类型（如项目公告、中标公告、变更公告、补遗公告等）
        /// </summary>
        public int NoticeType { get; set; }

        /// <summary>
        /// 属性类型（如线上项目、线下项目）
        /// </summary>
        public int PropType { get; set; }

        /// <summary>
        /// 此信息在其对应平台上的唯一查询ID
        /// </summary>
        public string QueryId { get; set; }

        public bool HasRead { get; set; }

        public Article()
        {
            
        }
    }
}
