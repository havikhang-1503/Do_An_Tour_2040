// Folder: Models
// File path: Models/HopDong.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class HopDong
    {
        public HopDong()
        {
            // Tự động sinh mã hợp đồng: MHD + 6 số (Ví dụ: MHD123456)
            this.MaHopDong = MaSoHelper.TaoMa("MHD");
            this.NgayTao = DateTime.Now;
            this.TrangThai = "ChoDuyet";
        }

        // --- KHÓA CHÍNH ---
        [Key]
        [Required, StringLength(50)]
        [Display(Name = "Mã hợp đồng")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public string MaHopDong { get; set; }

        [StringLength(200)]
        [Display(Name = "Tên hợp đồng")]
        public string? TenHopDong { get; set; }

        // --- CÁC KHÓA NGOẠI (STRING) ---

        // 1. Liên kết Giao Dịch
        [Required]
        [Display(Name = "Mã giao dịch")]
        [StringLength(50)]
        public string GiaoDichId { get; set; } = string.Empty;

        [ForeignKey("GiaoDichId")]
        public virtual GiaoDich? GiaoDich { get; set; }

        // 2. Liên kết Tour
        [Display(Name = "Mã Tour")]
        [StringLength(20)]
        public string? TourId { get; set; }

        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }

        // 3. Liên kết Khách hàng (CRM)
        [Display(Name = "Mã khách hàng")]
        [StringLength(20)]
        public string? KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        public virtual KhachHang? KhachHang { get; set; }

        // 4. Liên kết User (Tài khoản Identity)
        [Display(Name = "Người dùng đại diện")]
        public string? ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }

        // --- THÔNG TIN CHI TIẾT ---

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; }

        [Display(Name = "Ngày hiệu lực")]
        public DateTime? NgayHieuLuc { get; set; }

        [Display(Name = "Ngày kết thúc")]
        public DateTime? NgayKetThuc { get; set; }

        [StringLength(2000)]
        [Display(Name = "Nội dung điều khoản")]
        public string? NoiDung { get; set; }

        // [QUAN TRỌNG] Thêm cột này để sửa lỗi bên Controller/View
        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }

        [Display(Name = "File đính kèm (PDF)")]
        public string? FilePath { get; set; }
    }
}