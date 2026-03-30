using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Tour_2040.Models;

namespace Tour_2040.Models.ViewModels
{
    /// <summary>
    /// ViewModel dùng cho Create/Edit Tour
    /// Bao gồm: Thông tin tour, cascade dropdown (Tỉnh/Xã/Địa điểm), và dịch vụ đi kèm
    /// </summary>
    public class TourCreateViewModel
    {
        #region ================== 1. THÔNG TIN ĐỊNH DANH ==================
        // ✅ [1.1] Mã tour (auto-generate)
        [Display(Name = "Mã Tour")]
        [StringLength(50)]
        public string? MaTour { get; set; }

        // ✅ [1.2] Tên tour (bắt buộc)
        [Required(ErrorMessage = "Vui lòng nhập tên tour du lịch")]
        [StringLength(250, ErrorMessage = "Tên tour tối đa 250 ký tự")]
        [Display(Name = "Tên hành trình")]
        public string TenTour { get; set; } = string.Empty;

        // ✅ [1.3] Loại tour
        [Required(ErrorMessage = "Vui lòng chọn loại hình tour")]
        [Display(Name = "Phân loại Tour")]
        public string LoaiTour { get; set; } = string.Empty;
        #endregion

        #region ================== 2. ĐIỂM KHỞI HÀNH & TỈNH/XÃ ==================
        // ✅ [2.1] Điểm khởi hành (text)
        [Required(ErrorMessage = "Vui lòng nhập điểm xuất phát")]
        [StringLength(200)]
        [Display(Name = "Điểm khởi hành")]
        public string NoiKhoiHanh { get; set; } = string.Empty;

        // ✅ [2.2] Tỉnh/TP của điểm khởi hành (dropdown)
        [Display(Name = "Tỉnh (khởi hành)")]
        public string NoiKhoiHanhTinh { get; set; } = string.Empty;

        // ✅ [2.3] Xã/Phường của điểm khởi hành (dropdown cascade)
        [Display(Name = "Xã (khởi hành)")]
        public string NoiKhoiHanhXa { get; set; } = string.Empty;
        #endregion

        #region ================== 3. ĐIỂM ĐẾN & TỈNH/XÃ ==================
        // ✅ [3.1] Địa điểm đến (text)
        [Required(ErrorMessage = "Vui lòng nhập địa điểm đến")]
        [StringLength(200)]
        [Display(Name = "Điểm đến chính")]
        public string DiaDiem { get; set; } = string.Empty;

        // ✅ [3.2] Tỉnh/TP của điểm đến (dropdown)
        [Display(Name = "Tỉnh (điểm đến)")]
        public string DiaDiemTinh { get; set; } = string.Empty;

        // ✅ [3.3] Xã/Phường của điểm đến (dropdown cascade)
        [Display(Name = "Xã (điểm đến)")]
        public string DiaDiemXa { get; set; } = string.Empty;
        #endregion

        #region ================== 4. THỜI GIAN & LỊCH TRÌNH ==================
        // ✅ [4.1] Ngày khởi hành
        [Required(ErrorMessage = "Vui lòng chọn ngày đi")]
        [Display(Name = "Ngày khởi hành")]
        [DataType(DataType.Date)]
        public DateTime NgayDi { get; set; } = DateTime.Today.AddDays(7);

        // ✅ [4.2] Ngày kết thúc
        [Required(ErrorMessage = "Vui lòng chọn ngày về")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime NgayVe { get; set; } = DateTime.Today.AddDays(10);

        // ✅ [4.3] Mô tả/Lịch trình chi tiết
        [Display(Name = "Mô tả lịch trình chi tiết")]
        public string MoTa { get; set; } = string.Empty;
        #endregion
        // ================== 4.X (NEW) - GIỜ KHỞI HÀNH / KẾT THÚC + LỊCH TRÌNH JSON ==================

        [Display(Name = "Giờ khởi hành (HH:mm)")]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Giờ khởi hành phải đúng định dạng HH:mm")]
        public string GioKhoiHanh { get; set; } = "06:30";

        [Display(Name = "Giờ kết thúc (HH:mm)")]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Giờ kết thúc phải đúng định dạng HH:mm")]
        public string GioKetThuc { get; set; } = "18:00";

        /// <summary>
        /// Lịch trình theo ngày dạng JSON (frontend build).
        /// Mục đích: tự build text đổ vào MoTa + (tùy chọn) lưu riêng nếu em muốn.
        /// </summary>
        [Display(Name = "Lịch trình (JSON)")]
        public string LichTrinhJson { get; set; } = "{}";

        [Display(Name = "Mẫu lịch trình")]
        public string TemplateKey { get; set; } = string.Empty;

        #region ================== 5. SỐ LƯỢNG KHÁCH & TÀI CHÍNH ==================
        // ✅ [5.1] Số chỗ tối đa
        [Required(ErrorMessage = "Vui lòng nhập số khách tối đa")]
        [Range(1, 500, ErrorMessage = "Số lượng khách phải từ 1 đến 500")]
        [Display(Name = "Số chỗ tối đa")]
        public int SoNguoiToiDa { get; set; } = 20;

        // ✅ [5.2] Số khách hiện tại (dùng khi Edit, hiển thị ở form)
        [Display(Name = "Số khách hiện tại")]
        public int SoNguoiHienTai { get; set; }

        // ✅ [5.3] Giá tour
        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0, 1000000000, ErrorMessage = "Giá tour không hợp lệ")]
        [Display(Name = "Giá trọn gói (VNĐ)")]
        [DataType(DataType.Currency)]
        public decimal GiaTour { get; set; }
        #endregion

        #region ================== 6. FILE ẢNH ==================
        // ✅ [6.1] File upload ảnh đại diện
        [Display(Name = "Ảnh đại diện Tour")]
        public IFormFile? AnhTourFile { get; set; }

        // ✅ [6.2] Đường dẫn ảnh hiện tại (dùng khi Edit)
        [Display(Name = "Đường dẫn ảnh hiện tại")]
        public string? HinhAnhTourUrl { get; set; }
        #endregion

        #region ================== 7. DỊCH VỤ ĐI KÈM ==================
        // ✅ [7.1] Danh sách ID dịch vụ đã chọn (primary)
        [Display(Name = "Dịch vụ đính kèm")]
        public List<string> SelectedDichVuIds { get; set; } = new List<string>();

        // ✅ [7.2] Danh sách ID dịch vụ đã chọn (backup - giữ lại để tương thích)
        public List<string> SelectedServiceIds { get; set; } = new List<string>();

        // ✅ [7.3] Danh sách tất cả dịch vụ có sẵn (để render checkbox ở View)
        public List<DichVu> AvailableDichVus { get; set; } = new List<DichVu>();
        #endregion
    }
}