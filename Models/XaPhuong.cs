// Folder: Models
// File path: Models/XaPhuong.cs

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tour_2040.Models
{
    [Table("XaPhuong")]
    public class XaPhuong
    {
        [Key]
        [Column("MaXaPhuong")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaXaPhuong { get; set; }

        [Required]
        public int TinhThanhId { get; set; }

        [ForeignKey("TinhThanhId")]
        public virtual TinhThanh? TinhThanh { get; set; }

        [Required(ErrorMessage = "Tên xã phường là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Xã/Phường")]
        [Column("TenXaPhuong")]
        public string TenXaPhuong { get; set; } = string.Empty;

        public virtual ICollection<DiaDiem> DiaDiems { get; set; } = new List<DiaDiem>();
    }
}