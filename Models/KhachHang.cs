// Folder: Models
// File path: Models/KhachHang.cs
// File name: KhachHang.cs
// Class: KhachHang
// Labels: A(PK), B(Tạo mã), C(Hiển thị gộp), D(Navigation)
// FIX NOTES:
// - Khóa chính: MaKhachHang (ổn định, dùng làm route id và FK).
// - MaTenKhachHang: chỉ hiển thị (NotMapped) -> KHÔNG được dùng làm FK.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class KhachHang
    {
        public KhachHang()
        {
            // ✅ Tự sinh mã khách hàng: KH + 6 số
            if (string.IsNullOrWhiteSpace(MaKhachHang))
            {
                MaKhachHang = MaSoHelper.TaoMa("KH");
            }

            NgayTao = DateTime.Now;
            NhomKhach = "Thường";
            TrangThaiTour = "Chưa đặt tour";
            IsVip = false;

            GiaoDiches = new List<GiaoDich>();
            ThanhToans = new List<ThanhToan>();
            YeuCauHoTros = new List<YeuCauHoTro>();
            HopDongs = new List<HopDong>();
        }

        // =========================
        // ✅ KHÓA CHÍNH
        // =========================
        [Key]
        [Required, StringLength(20)]
        [Display(Name = "Mã khách hàng")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaKhachHang { get; set; } = string.Empty;

        // =========================
        // ✅ CHỈ HIỂN THỊ (KHÔNG LƯU DB)
        // =========================
        [NotMapped]
        [Display(Name = "Mã và Tên khách hàng")]
        public string MaTenKhachHang => $"{MaKhachHang} - {HoTen}";

        [Required(ErrorMessage = "Họ tên khách hàng không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(12)]
        [Display(Name = "CCCD/CMND")]
        public string? CCCD { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Nhóm khách")]
        public string NhomKhach { get; set; } = "Thường";

        [Required, StringLength(50)]
        [Display(Name = "Trạng thái tour")]
        public string TrangThaiTour { get; set; } = "Chưa đặt tour";

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Khách VIP")]
        public bool IsVip { get; set; } = false;

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // ===== QUAN HỆ =====
        public virtual ICollection<GiaoDich> GiaoDiches { get; set; }
        public virtual ICollection<ThanhToan> ThanhToans { get; set; }
        public virtual ICollection<YeuCauHoTro> YeuCauHoTros { get; set; }
        public virtual ICollection<HopDong> HopDongs { get; set; }
    }
}
