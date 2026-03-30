// Folder: Models
// File path: Models/TourDichVu.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tour_2040.Models
{
    [Table("TourDichVus")]
    public class TourDichVu
    {
        [Key]
        public int MaDichVu { get; set; } // Giữ ID tự tăng cho bảng trung gian

        [Required, StringLength(20)]
        public string TourId { get; set; } = string.Empty;

        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }

        [Required, StringLength(20)]
        public string DichVuId { get; set; } = string.Empty;

        [ForeignKey("DichVuId")]
        public virtual DichVu? DichVu { get; set; }

        [Display(Name = "Ngày thứ")]
        [Range(1, 100)]
        public int NgayThu { get; set; } = 1;

        [Display(Name = "Số lượng")]
        [Range(1, int.MaxValue)]
        public int SoLuong { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá hiện tại")]
        public decimal DonGiaHienTai { get; set; }

        [StringLength(200)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}