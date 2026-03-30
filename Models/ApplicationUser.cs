// Folder: Models
// File path: Models/ApplicationUser.cs
// File name: ApplicationUser.cs
// Class: ApplicationUser
// Labels: A(IdentityUser), B(Profile), C(CRM/Loyalty), D(Preferences), E(Consent), F(Relations)
// Desc: Mở rộng thông tin khách hàng để cá nhân hoá ưu đãi (điểm, hạng, hành vi, sở thích) + tuân thủ consent.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tour_2040.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            GiaoDiches = new List<GiaoDich>();
            HopDongs = new List<HopDong>();
            YeuCauHoTros = new List<YeuCauHoTro>();

            // Default CRM
            HangThanhVien = "Member";
            DiemTichLuy = 0;
            TongChiTieu = 0;
            SoLanDatTour = 0;
            SoLanHuy = 0;

            NhanEmailMarketing = false;
            NhanSMSMarketing = false;
            NhanZaloMarketing = false;
        }

        // =========================
        // THÔNG TIN CƠ BẢN
        // =========================

        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [StringLength(50)]
        [Display(Name = "Mã người dùng")]
        public string? MaNguoiDung { get; set; }

        [StringLength(12)]
        [Display(Name = "CCCD/CMND")]
        public string? CCCD { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(20)]
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; } // VD: Nam/Nữ/Khác/Không muốn nói

        [Display(Name = "Ngày sinh")]
        [Column(TypeName = "datetime2(0)")]
        public DateTime? NgaySinh { get; set; }

        // Ảnh CCCD (nếu em dùng cho booking)
        [StringLength(500)]
        public string? AnhCCCDUrl { get; set; }

        // =========================
        // ẢNH ĐẠI DIỆN
        // =========================

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Ảnh đại diện")]
        [NotMapped]
        public IFormFile? AvatarFile { get; set; }

        // =========================
        // THÔNG TIN CRM / LOYALTY (ƯU ĐÃI)
        // =========================

        [StringLength(30)]
        [Display(Name = "Hạng thành viên")]
        public string? HangThanhVien { get; set; } // Member/Silver/Gold/VIP

        [Display(Name = "Điểm tích lũy")]
        public int DiemTichLuy { get; set; }

        [Display(Name = "Tổng chi tiêu (lifetime)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongChiTieu { get; set; }

        [Display(Name = "Số lần đặt tour")]
        public int SoLanDatTour { get; set; }

        [Display(Name = "Lần đặt gần nhất")]
        [Column(TypeName = "datetime2(0)")]
        public DateTime? LanDatGanNhat { get; set; }

        [Display(Name = "Số lần hủy")]
        public int SoLanHuy { get; set; }

        [Display(Name = "Ngày sinh nhật đã cấp voucher")]
        [Column(TypeName = "datetime2(0)")]
        public DateTime? NgayCapVoucherSinhNhatGanNhat { get; set; }

        // =========================
        // SỞ THÍCH (ĐỀ XUẤT TOUR)
        // - Lưu dạng chuỗi phân tách ; để nhanh và dễ (sau này muốn chuẩn hoá thì tách bảng)
        // =========================

        [StringLength(300)]
        [Display(Name = "Sở thích loại tour")]
        public string? SoThichLoaiTour { get; set; } // VD: "Nghỉ dưỡng;Trải nghiệm;Phượt"

        [StringLength(50)]
        [Display(Name = "Mức giá ưa thích")]
        public string? SoThichMucGia { get; set; } // Budget/Mid/Luxury

        [StringLength(50)]
        [Display(Name = "Mùa ưa thích")]
        public string? SoThichMua { get; set; } // Xuân/Hạ/Thu/Đông

        [StringLength(100)]
        [Display(Name = "Phương tiện ưa thích")]
        public string? SoThichPhuongTien { get; set; } // "Xe;Máy bay;Tàu"

        [StringLength(200)]
        [Display(Name = "Ghi chú sở thích ăn uống")]
        public string? SoThichAnUong { get; set; }

        // =========================
        // KÊNH ĐẾN / MARKETING CONSENT
        // =========================

        [StringLength(50)]
        [Display(Name = "Khách đến từ")]
        public string? KenhDen { get; set; } // FB/TikTok/Referral/Google/...

        [Display(Name = "Nhận email ưu đãi")]
        public bool NhanEmailMarketing { get; set; }

        [Display(Name = "Nhận SMS ưu đãi")]
        public bool NhanSMSMarketing { get; set; }

        [Display(Name = "Nhận Zalo ưu đãi")]
        public bool NhanZaloMarketing { get; set; }

        [Display(Name = "Ngày đồng ý nhận ưu đãi")]
        [Column(TypeName = "datetime2(0)")]
        public DateTime? NgayDongYMarketing { get; set; }

        // =========================
        // NAVIGATION
        // =========================

        public virtual ICollection<GiaoDich> GiaoDiches { get; set; }
        public virtual ICollection<HopDong> HopDongs { get; set; }
        public virtual ICollection<YeuCauHoTro> YeuCauHoTros { get; set; }
    }
}
