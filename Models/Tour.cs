// Folder: Models
// File path: Models/Tour.cs
// File name: Tour.cs
// Class: Tour
// Labels: A(PK), B(FK User), C(DateTime Full), D(Google Maps FK), E(Relations), F(Default)
// Desc: Model Tour lưu đúng ngày-giờ-phút (datetime2) + bổ sung FK DiaDiem để dùng Google Maps thật, vẫn giữ field text cũ để tương thích.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class Tour
    {
        public Tour()
        {
            MaTour = MaSoHelper.TaoMa("MT");
            NgayTao = DateTime.Now;

            SoNguoiHienTai = 0;
            IsDefault = true;
            IsPersonal = false;
            IsHidden = false;
            TrangThai = "ChoDuyet";

            GiaoDiches = new List<GiaoDich>();
            HopDongs = new List<HopDong>();
            TourDichVus = new List<TourDichVu>();

            // Nếu project của em có file TourLichTrinhNgay.cs (chị thấy có),
            // thì bật navigation này để gắn "lịch trình mẫu theo ngày" cho tour.
            TourLichTrinhNgays = new List<TourLichTrinhNgay>();
        }

        // =========================
        // ĐỊNH DANH
        // =========================

        [Key]
        [Required, StringLength(20)]
        [Display(Name = "Mã Tour")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaTour { get; set; }

        [Required(ErrorMessage = "Tên tour là bắt buộc")]
        [StringLength(250)]
        [Display(Name = "Tên tour")]
        public string TenTour { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Loại tour")]
        public string? LoaiTour { get; set; }

        // =========================
        // ĐỊA ĐIỂM (TEXT - TƯƠNG THÍCH CODE CŨ)
        // =========================

        [StringLength(200)]
        [Display(Name = "Điểm xuất phát")]
        public string? NoiKhoiHanh { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa điểm chính")]
        public string? DiaDiem { get; set; }

        // =========================
        // ĐỊA ĐIỂM (FK - DÙNG GOOGLE MAPS THẬT)
        // - Optional: em có thể bật dần dần, không bắt buộc ngay.
        // =========================

        [StringLength(20)]
        [Display(Name = "Điểm xuất phát (ID)")]
        public string? NoiKhoiHanhDiaDiemId { get; set; }

        [ForeignKey(nameof(NoiKhoiHanhDiaDiemId))]
        public virtual DiaDiem? NoiKhoiHanhDiaDiem { get; set; }

        [StringLength(20)]
        [Display(Name = "Địa điểm chính (ID)")]
        public string? DiaDiemChinhId { get; set; }

        [ForeignKey(nameof(DiaDiemChinhId))]
        public virtual DiaDiem? DiaDiemChinh { get; set; }

        // =========================
        // MÔ TẢ / GIÁ
        // =========================

        [StringLength(2000)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Giá tour không được để trống")]
        [Display(Name = "Giá tour")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal GiaTour { get; set; }

        // =========================
        // USER / TRẠNG THÁI
        // =========================

        [StringLength(450)]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái duyệt")]
        public string? TrangThai { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column(TypeName = "datetime2(0)")]
        public DateTime NgayTao { get; set; }

        // =========================
        // SỐ LƯỢNG
        // =========================

        [Display(Name = "Số người tối đa")]
        public int? SoNguoiToiDa { get; set; }

        [Display(Name = "Số người hiện tại")]
        public int? SoNguoiHienTai { get; set; }

        // =========================
        // THỜI GIAN (ĐÚNG NGÀY-GIỜ-PHÚT)
        // - BỎ DataType.Date để tránh mất giờ/phút
        // - datetime2(0) lưu đến phút (giây = 0)
        // =========================

        [Display(Name = "Khởi hành (ngày/giờ)")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "datetime2(0)")]
        public DateTime? NgayDi { get; set; }

        [Display(Name = "Kết thúc (ngày/giờ)")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "datetime2(0)")]
        public DateTime? NgayVe { get; set; }
        // =========================
        // (NEW) LỊCH TRÌNH DẠNG JSON (OPTIONAL)
        // =========================

        [StringLength(8000)]
        [Display(Name = "Lịch trình (JSON)")]
        public string? LichTrinhJson { get; set; }


        // =========================
        // CỜ HỆ THỐNG
        // =========================

        [Display(Name = "Tour mặc định")]
        public bool IsDefault { get; set; }

        [Display(Name = "Tour cá nhân")]
        public bool IsPersonal { get; set; }

        [StringLength(500)]
        [Display(Name = "Ảnh đại diện")]
        public string? HinhAnhTourUrl { get; set; }

        [Display(Name = "Ẩn tour")]
        public bool IsHidden { get; set; }

        // =========================
        // YÊU CẦU KHÁCH (TÙY CHỌN)
        // =========================

        [StringLength(1000)]
        [Display(Name = "Yêu cầu lưu trú")]
        public string? YeuCauKhachSan { get; set; }

        [StringLength(1000)]
        [Display(Name = "Yêu cầu ẩm thực")]
        public string? YeuCauNhaHang { get; set; }

        [StringLength(1000)]
        [Display(Name = "Yêu cầu di chuyển")]
        public string? YeuCauPhuongTien { get; set; }

        [StringLength(1000)]
        [Display(Name = "Yêu cầu trải nghiệm")]
        public string? YeuCauVuiChoi { get; set; }

        [StringLength(1000)]
        [Display(Name = "Ghi chú khác")]
        public string? YeuCauKhac { get; set; }

        // =========================
        // RELATIONS
        // =========================

        public virtual ICollection<GiaoDich> GiaoDiches { get; set; }
        public virtual ICollection<HopDong> HopDongs { get; set; }
        public virtual ICollection<TourDichVu> TourDichVus { get; set; }

        // Lịch trình mẫu theo ngày (nếu em dùng)
        public virtual ICollection<TourLichTrinhNgay> TourLichTrinhNgays { get; set; }

    }
}
