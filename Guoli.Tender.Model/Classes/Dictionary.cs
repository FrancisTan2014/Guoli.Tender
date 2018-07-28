using System;
using System.ComponentModel.DataAnnotations;

namespace Guoli.Tender.Model
{
    /// <summary>
    /// 管理各种类型数据（为了与系统的 Dictionary 区别，采用复数形式）
    /// </summary>
    public class Dictionaries: IEntity<int>
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// 字典类型，当它为 0 时表示元类型（类型的类型）
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Type { get; set; }

        public int ParentId { get; set; }

        public string Remark { get; set; }

        public DateTime AddTime { get; set; }
    }
}
