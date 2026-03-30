//// Folder: Models
// File path: Models/StartThanhToanViewModel.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace Tour_2040.Models
{
    public class StartThanhToanViewModel
    {
        // ===== THÔNG TIN TOUR =====

        // Lưu ý: Đảm bảo MaTour là ID định danh chính
        [Required(ErrorMessage = "Mã tour là bắt buộc")]
        [Display(Name = "Mã tour")]
        public string MaTour { get; set; } = string.Empty;

        // Nếu Controller của bạn dùng tên biến là TourId (kiểu string), hãy thêm dòng này:
        public string TourId { get { return MaTour; } set { MaTour = value; } }

        [Display(Name = "Tên tour")]
        public string TenTour { get; set; } = string.Empty;

        [Display(Name = "Địa điểm")]
        public string? DiaDiem { get; set; }

        [Display(Name = "Ngày đi")]
        [DataType(DataType.Date)]
        public DateTime? NgayDi { get; set; }

        [Display(Name = "Ngày về")]
        [DataType(DataType.Date)]
        public DateTime? NgayVe { get; set; }

        // ===== THÔNG TIN NGƯỜI ĐẠI DIỆN =====
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên người đại diện")]
        public string HoTenNguoiDaiDien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại quá dài")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ")]
        [Display(Name = "Email liên hệ")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "CCCD / CMND")]
        public string? CCCD { get; set; }

        [Display(Name = "Địa chỉ liên hệ")]
        public string? DiaChiLienHe { get; set; }

        [Display(Name = "Ảnh CCCD")]
        public string? AnhCCCDUrl { get; set; }

        // ✅ THÊM: Ngày sinh (để lưu snapshot vào GiaoDich nếu cần)
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        // ✅ THÊM: Ghi chú (để lưu vào GiaoDich.GhiChu nếu form có truyền)
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // ===== SỐ LƯỢNG KHÁCH =====
        [Display(Name = "Số người lớn")]
        [Range(1, 100, ErrorMessage = "Phải có ít nhất 1 người lớn")]
        public int SoNguoiLon { get; set; } = 1;

        [Display(Name = "Số trẻ em")]
        [Range(0, 100, ErrorMessage = "Số lượng trẻ em không hợp lệ")]
        public int SoTreEm { get; set; } = 0;

        // ===== TIỀN TOUR + DỊCH VỤ =====
        [Display(Name = "Tổng tiền dịch vụ")]
        public decimal TongTienDichVu { get; set; }

        [Display(Name = "Tổng tiền dự kiến")]
        public decimal TongTienDuKien { get; set; }

        // ===== VOUCHER =====
        [Display(Name = "Mã Voucher người dùng")]
        public string? SelectedUserVoucherId { get; set; }

        [Display(Name = "Tiền giảm")]
        public decimal TienGiam { get; set; }

        [Display(Name = "Tổng sau giảm")]
        public decimal TongSauGiam { get; set; }

        // ===== PHƯƠNG THỨC THANH TOÁN =====
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = "ChuyenKhoan";
    }
}
