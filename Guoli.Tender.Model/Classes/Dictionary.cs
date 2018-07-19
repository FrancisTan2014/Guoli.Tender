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

        [Required]
        [Range(1, int.MaxValue)]
        public int Type { get; set; }

        public int ParentId { get; set; }
    }
}
