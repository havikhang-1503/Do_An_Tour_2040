// Folder: Models
// File path: Models/NhanVien.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class NhanVien
    {
        public NhanVien()
        {
            // Auto-generate Employee Code: MNV + 6 digits
            this.MaNhanVien = MaSoHelper.TaoMa("MNV");
            this.NgayVaoLam = DateTime.Now;
            this.ConLamViec = true;
        }

        [Key]
        [Required, StringLength(20)]
        [Display(Name = "Mã nhân viên")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaNhanVien { get; set; }

        [Required]
        [Display(Name = "Tài khoản hệ thống")]
        public string ApplicationUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(50)]
        [Display(Name = "Chức vụ")]
        public string? ChucVu { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày vào làm")]
        public DateTime? NgayVaoLam { get; set; }

        [Display(Name = "Còn làm việc")]
        public bool ConLamViec { get; set; }

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}