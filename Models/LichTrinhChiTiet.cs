// Folder: Models
// File path: Models/LichTrinhChiTiet.cs
// File name: LichTrinhChiTiet.cs
// Class: LichTrinhChiTiet
// Labels: A(PK), B(FK LichTrinh), C(FK DiaDiem), D(DateTime), E(Fields), F(Default)
// Desc: Chi tiết lịch trình theo ngày, có giờ/phút chính xác (BatDauLuc/KetThucLuc) + địa điểm.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class LichTrinhChiTiet
    {
        public LichTrinhChiTiet()
        {
            MaLichTrinhChiTiet = MaSoHelper.TaoMa("MCTLT");
            NgayThu = 1;
            DichVuKemTheo = new HashSet<LichTrinhChiTietDichVu>();
        }

        [Key]
        [Required, StringLength(50)]
        [Display(Name = "Mã chi tiết lịch trình")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaLichTrinhChiTiet { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã lịch trình")]
        public string LichTrinhId { get; set; } = string.Empty;

        [ForeignKey(nameof(LichTrinhId))]
        public virtual LichTrinh? LichTrinh { get; set; }

        [Range(1, 365)]
        [Display(Name = "Ngày thứ")]
        public int NgayThu { get; set; }

        // ✅ NEW: điểm bắt đầu (A)
        [StringLength(20)]
        [Display(Name = "Điểm bắt đầu")]
        public string? DiaDiemBatDauId { get; set; }

        [ForeignKey(nameof(DiaDiemBatDauId))]
        public virtual DiaDiem? DiaDiemBatDau { get; set; }

        // ✅ Điểm đến (B)
        [StringLength(20)]
        [Display(Name = "Điểm đến")]
        public string? DiaDiemId { get; set; }

        [ForeignKey(nameof(DiaDiemId))]
        public virtual DiaDiem? DiaDiem { get; set; }

        // LEGACY: giữ lại để không vỡ code cũ
        [Required(ErrorMessage = "Vui lòng chọn giờ")]
        [Column(TypeName = "time")]
        [Display(Name = "Giờ (legacy)")]
        public TimeSpan Gio { get; set; }

        // ✅ NEW: lưu đúng datetime theo phút
        [Display(Name = "Bắt đầu lúc")]
        [Column(TypeName = "datetime2(0)")]
        public DateTime? BatDauLuc { get; set; }

        [Display(Name = "Kết thúc lúc")]
        [Column(TypeName = "datetime2(0)")]
        public DateTime? KetThucLuc { get; set; }

        [Range(0, 14400)]
        [Display(Name = "Thời lượng (phút)")]
        public int? ThoiLuongPhut { get; set; }

        [Range(0, 9999)]
        [Display(Name = "Thứ tự")]
        public int ThuTu { get; set; }

        [StringLength(200)]
        [Display(Name = "Nội dung")]
        public string? NoiDung { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [StringLength(50)]
        [Display(Name = "Loại hoạt động")]
        public string? LoaiHoatDong { get; set; }

        public virtual ICollection<LichTrinhChiTietDichVu> DichVuKemTheo { get; set; }
    }
}
