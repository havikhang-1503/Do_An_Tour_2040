// Folder: Models/ViewModels
// File path: Models/ViewModels/UngVienNhanVienViewModel.cs
// File name: UngVienNhanVienViewModel.cs
// Class: UngVienNhanVienViewModel
// Labels: A(ViewModel), B(Identity Base), C(Staff Info), D(Status/Role)
// Desc: ViewModel ứng viên nhân viên (Việt hoá) để Admin duyệt + gán vai trò/chức vụ nhanh.

using System;
using System.ComponentModel.DataAnnotations;

namespace Tour_2040.Models.ViewModels
{
    public class UngVienNhanVienViewModel
    {
        // =========================
        // Định danh Identity
        // =========================

        [Display(Name = "User ID")]
        public string NguoiDungId { get; set; } = string.Empty;

        [Display(Name = "Tên đăng nhập")]
        public string? TenDangNhap { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        // =========================
        // Thông tin cơ bản (lấy từ ApplicationUser)
        // =========================

        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [Display(Name = "CCCD/CMND")]
        public string? CCCD { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AnhDaiDienUrl { get; set; }

        // =========================
        // Thông tin nhân sự (phục vụ duyệt nhân viên)
        // =========================

        [Display(Name = "Chức vụ đề xuất")]
        public string? ChucVuDeXuat { get; set; }  // VD: Kế toán / Kho / CSKH / Điều phối

        [Display(Name = "Phòng ban")]
        public string? PhongBan { get; set; }      // VD: Kế toán / Kho / Điều hành

        [Display(Name = "Vai trò hệ thống đề xuất")]
        public string? VaiTroDeXuat { get; set; }  // VD: Staff / Accountant / Warehouse

        [Display(Name = "Ngày vào làm")]
        public DateTime? NgayVaoLam { get; set; }

        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }     // VD: Chờ duyệt / Đang làm / Tạm khóa

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}
