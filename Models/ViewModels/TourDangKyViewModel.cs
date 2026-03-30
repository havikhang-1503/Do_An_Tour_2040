// Folder: Models/ViewModels
// File path: Models/ViewModels/TourDangKyViewModel.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace Tour_2040.Models.ViewModels
{
    public class TourDangKyViewModel
    {
        // Thông tin tour
        [Display(Name = "Mã tour")]
        public string? MaTour { get; set; }

        [Display(Name = "Tên tour")]
        public string? TenTour { get; set; }

        [Display(Name = "Địa điểm chính")]
        public string? DiaDiem { get; set; }

        [Display(Name = "Giá tour")]
        public decimal GiaTour { get; set; }

        // Thông tin người đại diện
        [Required(ErrorMessage = "Họ tên người đại diện là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Họ tên người đại diện")]
        public string HoTenDaiDien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [StringLength(200)]
        [Display(Name = "Email liên hệ")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Địa chỉ liên hệ")]
        public string? DiaChiLienHe { get; set; }

        // Số lượng
        [Range(1, 1000, ErrorMessage = "Số người lớn ít nhất là 1")]
        [Display(Name = "Số người lớn")]
        public int SoNguoiLon { get; set; } = 1;

        [Range(0, 1000, ErrorMessage = "Số trẻ em không hợp lệ")]
        [Display(Name = "Số trẻ em")]
        public int SoTreEm { get; set; } = 0;

        [Display(Name = "Danh sách người đi kèm")]
        public string? DanhSachNguoiDiKem { get; set; } // Định dạng: Họ tên - Năm sinh...

        [Display(Name = "Ghi chú thêm")]
        public string? GhiChu { get; set; }

        // Điều khoản
        [Display(Name = "Tôi đồng ý với điều khoản & nội dung hợp đồng mẫu")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn phải đồng ý với điều khoản")]
        public bool DongYDieuKhoan { get; set; }
    }
}