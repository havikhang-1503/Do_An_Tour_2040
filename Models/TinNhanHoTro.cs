using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tour_2040.Models
{
    [Table("TinNhanHoTros")]
    public class TinNhanHoTro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaYeuCauHoTro { get; set; } = string.Empty; // Khởi tạo giá trị mặc định

        [ForeignKey("MaYeuCauHoTro")]
        public virtual YeuCauHoTro? YeuCauHoTro { get; set; } // Thêm ?

        [Required]
        public string NoiDung { get; set; } = string.Empty; // Khởi tạo giá trị mặc định

        public string? NguoiGui { get; set; } // Thêm ? (Cho phép null)

        public bool LaNhanVien { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}