// Folder: Models
// File path: Models/ThanhToan.cs
// File name: ThanhToan.cs
// Class: ThanhToan
// Labels: A(PK), B(FK GiaoDich), C(FK KhachHang - FIX), D(Money), E(Default)
// FIX NOTES:
// - Đồng bộ độ dài KhachHangId = 20 để khớp KhachHang.MaKhachHang (nvarchar(20)).
// - Không dùng MaTenKhachHang (NotMapped) làm FK.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class ThanhToan
    {
        public ThanhToan()
        {
            // Tự động sinh mã thanh toán: MTT + 6 số
            MaThanhToan = MaSoHelper.TaoMa("MTT");
            NgayThanhToan = DateTime.Now;
            TrangThai = "Thành công";
        }

        // --- KHÓA CHÍNH ---
        [Key]
        [Required, StringLength(50)]
        [Display(Name = "Mã thanh toán")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaThanhToan { get; set; } = string.Empty;

        // --- KHÓA NGOẠI: GIAO DỊCH ---
        [Required, StringLength(50)]
        [Display(Name = "Mã giao dịch")]
        public string GiaoDichId { get; set; } = string.Empty;

        [ForeignKey(nameof(GiaoDichId))]
        public virtual GiaoDich? GiaoDich { get; set; }

        // --- THÔNG TIN TÀI CHÍNH ---
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền thanh toán")]
        [Range(0, double.MaxValue)]
        public decimal SoTien { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Phương thức")]
        public string PhuongThuc { get; set; } = string.Empty;

        // --- THÔNG TIN BỔ SUNG ---
        [StringLength(200)]
        [Display(Name = "Tên tài khoản thanh toán")]
        public string? TenTaiKhoan { get; set; }

        [StringLength(100)]
        [Display(Name = "Số tài khoản / Ví")]
        public string? SoTaiKhoan { get; set; }

        [StringLength(500)]
        [Display(Name = "Nội dung / Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Ngày thanh toán")]
        public DateTime NgayThanhToan { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }

        // --- LIÊN KẾT KHÁCH HÀNG (CRM) ---
        [Display(Name = "Khách hàng")]
        [StringLength(20)] // ✅ FIX: khớp KhachHang.MaKhachHang (nvarchar(20))
        public string? KhachHangId { get; set; }

        [ForeignKey(nameof(KhachHangId))]
        public virtual KhachHang? KhachHang { get; set; }
    }
}
