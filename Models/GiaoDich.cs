// Folder: Models
// File path: Models/GiaoDich.cs
// File name: GiaoDich.cs
// Class: GiaoDich
// Labels: A(PK), B(FK User), C(FK KhachHang - FIX), D(FK Tour), E(Navigation init)
// Mô tả: Model giao dịch tour.
// FIX: KhachHangId phải khớp độ dài với KhachHang.MaKhachHang (nvarchar(20)) để tạo FK không lỗi.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class GiaoDich
    {
        public GiaoDich()
        {
            MaGiaoDich = MaSoHelper.TaoMa("MGD");
            NgayTao = DateTime.Now;

            TrangThai = "Pending";
            SoNguoiLon = 1;
            SoTreEm = 0;

            ChiTietDichVus = new List<GiaoDichChiTiet>();
            ThanhToans = new List<ThanhToan>();
            HopDongs = new List<HopDong>();
            YeuCauHoTros = new List<YeuCauHoTro>();
            LichTrinhs = new List<LichTrinh>();
        }

        // --- KHÓA CHÍNH ---
        [Key]
        [Required, StringLength(50)]
        [Display(Name = "Mã giao dịch")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaGiaoDich { get; set; } = string.Empty;

        // --- LIÊN KẾT USER (Identity) ---
        [Required]
        [Display(Name = "Mã người dùng")]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        // --- LIÊN KẾT KHÁCH HÀNG (CRM) ---
        // ✅ FIX: FK phải trỏ về KhachHang.MaKhachHang (nvarchar(20)) => KhachHangId cũng phải nvarchar(20)
        [Display(Name = "Khách hàng")]
        [StringLength(20)]
        public string? KhachHangId { get; set; }

        [ForeignKey(nameof(KhachHangId))]
        public virtual KhachHang? KhachHang { get; set; }

        // --- LIÊN KẾT TOUR ---
        [Display(Name = "Tour")]
        [StringLength(20)]
        public string? TourId { get; set; }

        [ForeignKey(nameof(TourId))]
        public virtual Tour? Tour { get; set; }

        // --- THÔNG TIN CHI TIẾT ---
        [Display(Name = "Số người lớn")]
        [Range(1, int.MaxValue, ErrorMessage = "Số người lớn tối thiểu là 1")]
        public int SoNguoiLon { get; set; }

        [Display(Name = "Số trẻ em")]
        [Range(0, int.MaxValue)]
        public int SoTreEm { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; }

        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal TongTien { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // ==========================================================
        // SNAPSHOT THÔNG TIN KHÁCH HÀNG
        // ==========================================================
        [StringLength(150)]
        [Display(Name = "Họ tên người đại diện")]
        public string? HoTenNguoiDaiDien { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(150)]
        [Display(Name = "Email liên hệ")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Display(Name = "CCCD / CMND")]
        public string? CCCD { get; set; }

        [StringLength(300)]
        [Display(Name = "Địa chỉ liên hệ")]
        public string? DiaChiLienHe { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(500)]
        [Display(Name = "Ảnh CCCD")]
        public string? AnhCCCDUrl { get; set; }

        // --- DANH SÁCH LIÊN KẾT (1-N) ---
        public virtual ICollection<GiaoDichChiTiet> ChiTietDichVus { get; set; }
        public virtual ICollection<ThanhToan> ThanhToans { get; set; }
        public virtual ICollection<HopDong> HopDongs { get; set; }
        public virtual ICollection<YeuCauHoTro> YeuCauHoTros { get; set; }
        public virtual ICollection<LichTrinh> LichTrinhs { get; set; }
    }
}
