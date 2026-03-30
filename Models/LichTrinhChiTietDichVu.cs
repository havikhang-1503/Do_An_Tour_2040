// Folder: Models
// File path: Models/LichTrinhChiTietDichVu.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tour_2040.Models
{
    public class LichTrinhChiTietDichVu
    {
        [Key]
        public int MaLichTrinhChiTietDichVu { get; set; }

        [Required]
        [StringLength(50)]
        public string LichTrinhChiTietId { get; set; } = string.Empty;

        [ForeignKey("LichTrinhChiTietId")]
        public virtual LichTrinhChiTiet? LichTrinhChiTiet { get; set; }

        [Required]
        [StringLength(20)]
        public string DichVuId { get; set; } = string.Empty;

        [ForeignKey("DichVuId")]
        public virtual DichVu? DichVu { get; set; }
    }
}