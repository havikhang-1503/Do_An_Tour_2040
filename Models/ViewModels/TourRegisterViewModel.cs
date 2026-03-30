using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Tour_2040.Models;

namespace Tour_2040.Models.ViewModels
{
    /// <summary>
    /// ViewModel xử lý việc đăng ký Tour cho khách hàng.
    /// Bao gồm: Hiển thị thông tin Tour, Ràng buộc dữ liệu người đại diện, Tính toán số lượng khách, và Dịch vụ đi kèm.
    /// </summary>
    public class TourRegisterViewModel
    {
        #region ================== 1. THÔNG TIN TOUR (HIỂN THỊ) ==================
        // ✅ [1.1] ID tour (required - dùng để gửi form)
        [Required]
        public string TourId { get; set; } = string.Empty;

        // ✅ [1.2] Mã hành trình (hiển thị)
        [Display(Name = "Mã hành trình")]
        public string? MaTour { get; set; }

        // ✅ [1.3] Tên hành trình (hiển thị)
        [Display(Name = "Tên hành trình")]
        public string TenTour { get; set; } = string.Empty;

        // ✅ [1.4] Ảnh đại diện tour
        public string? HinhAnhTourUrl { get; set; }

        // ✅ [1.5] Nơi khởi hành (hiển thị)
        [Display(Name = "Nơi khởi hành")]
        public string? NoiKhoiHanh { get; set; }

        // ✅ [1.6] Điểm đến (hiển thị)
        [Display(Name = "Điểm đến")]
        public string? DiaDiem { get; set; }

        // ✅ [1.7] Ngày khởi hành (hiển thị)
        [Display(Name = "Ngày khởi hành")]
        [DataType(DataType.Date)]
        public DateTime? NgayDi { get; set; }

        // ✅ [1.8] Ngày về (hiển thị)
        [Display(Name = "Ngày về")]
        [DataType(DataType.Date)]
        public DateTime? NgayVe { get; set; }

        // ✅ [1.9] Giá vé gốc (hiển thị)
        [Display(Name = "Giá vé gốc")]
        public decimal GiaTour { get; set; }

        // ✅ [1.10] Danh sách dịch vụ đi kèm (để hiển thị trong trang Register)
        public List<TourDichVu> TourDichVus { get; set; } = new();
        #endregion

        #region ================== 2. THÔNG TIN ĐỊNH DANH (VALIDATION) ==================
        // ✅ [2.1] Họ tên người đại diện (bắt buộc)
        [Display(Name = "Họ tên người đại diện")]
        [Required(ErrorMessage = "Quý khách vui lòng nhập họ tên.")]
        [RegularExpression(@"^[^0-9]*$", ErrorMessage = "Họ tên không được chứa ký tự số.")]
        public string HoTenDaiDien { get; set; } = string.Empty;

        // ✅ [2.2] Ngày sinh (bắt buộc + kiểm tra tuổi)
        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Quý khách vui lòng chọn ngày sinh.")]
        [DataType(DataType.Date)]
        [MinAge(16, ErrorMessage = "Người đại diện phải từ 16 tuổi trở lên.")]
        public DateTime? NgaySinh { get; set; }

        // ✅ [2.3] Số điện thoại (bắt buộc + validate format)
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên lạc.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải bao gồm 10 chữ số.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "SĐT không hợp lệ (Phải bắt đầu bằng số 0).")]
        public string SoDienThoai { get; set; } = string.Empty;

        // ✅ [2.4] Email (bắt buộc + format)
        [Display(Name = "Địa chỉ Email")]
        [Required(ErrorMessage = "Vui lòng cung cấp Email để nhận vé điện tử.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        // ✅ [2.5] CCCD/Hộ chiếu (bắt buộc + validate)
        [Display(Name = "Số CCCD/Hộ chiếu")]
        [Required(ErrorMessage = "Quý khách vui lòng nhập số CCCD.")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD định danh phải bao gồm 12 chữ số.")]
        public string CCCD { get; set; } = string.Empty;

        // ✅ [2.6] Địa chỉ liên hệ (bắt buộc)
        [Display(Name = "Địa chỉ thường trú")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ liên hệ.")]
        public string DiaChiLienHe { get; set; } = string.Empty;

        // ✅ [2.7] Ảnh CCCD (bắt buộc)
        [Display(Name = "Ảnh CCCD mặt trước")]
        [DataType(DataType.Upload)]
        public IFormFile? AnhCCCDFile { get; set; }
        #endregion

        #region ================== 3. TÍNH TOÁN HÀNH KHÁCH ==================
        // ✅ [3.1] Số người lớn (bắt buộc >= 1)
        [Display(Name = "Số người lớn")]
        [Range(1, 100, ErrorMessage = "Hành trình yêu cầu ít nhất 01 người lớn.")]
        public int SoNguoiLon { get; set; } = 1;

        // ✅ [3.2] Số trẻ em (tuỳ chọn)
        [Display(Name = "Số trẻ em")]
        [Range(0, 100, ErrorMessage = "Số lượng trẻ em không hợp lệ.")]
        public int SoTreEm { get; set; } = 0;

        // ✅ [3.3] Ghi chú đặc biệt (tuỳ chọn)
        [Display(Name = "Ghi chú đặc biệt")]
        public string? GhiChu { get; set; }

        // ✅ [3.4] Tổng chi phí dự kiến (hiển thị)
        [Display(Name = "Tổng chi phí dự kiến")]
        public decimal TongTienDuKien { get; set; }
        #endregion
    }

    #region ================== CUSTOM VALIDATION ATTRIBUTE ==================

    /// <summary>
    /// Thuộc tính kiểm tra độ tuổi tối thiểu của người dùng.
    /// Cú pháp: [MinAge(16)]
    /// </summary>
    public class MinAgeAttribute : ValidationAttribute
    {
        private readonly int _limit;

        public MinAgeAttribute(int limit)
        {
            _limit = limit;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is DateTime dt)
            {
                // Nếu ngày sinh + số năm giới hạn > ngày hiện tại => Chưa đủ tuổi
                if (dt.AddYears(_limit) > DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage);
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Định dạng ngày tháng không hợp lệ.");
        }
    }

    #endregion
}