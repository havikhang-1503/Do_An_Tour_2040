// Folder: Models
// File path: Models/LichTrinh.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class LichTrinh
    {
        public LichTrinh()
        {
            // Tự động sinh mã: MLT + 6 số
            MaLichTrinh = MaSoHelper.TaoMa("MLT");

            // Default
            NgayDat = DateTime.Now;
            TrangThai = "Chờ xác nhận";

            // Tránh null collections
            LichTrinhChiTiets = new HashSet<LichTrinhChiTiet>();
            DanhGiaTours = new HashSet<DanhGiaTour>();
        }

        [Key]
        [Required, StringLength(20)]
        [Display(Name = "Mã lịch trình")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaLichTrinh { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã Tour")]
        public string TourId { get; set; } = string.Empty;

        [ForeignKey(nameof(TourId))]
        public virtual Tour? Tour { get; set; }

        [StringLength(450)]
        [Display(Name = "Người đặt")]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [Display(Name = "Tên lịch trình")]
        [StringLength(200)]
        public string? TenLichTrinh { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; }

        [Range(0, 1000)]
        [Display(Name = "Số người lớn")]
        public int SoNguoiLon { get; set; }

        [Range(0, 1000)]
        [Display(Name = "Số trẻ em")]
        public int SoTreEm { get; set; }

        // ✅ Cột tổng tiền (để fix lỗi thiếu cột + phục vụ KPI doanh thu)
        [Range(typeof(decimal), "0", "999999999999")]
        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }

        // ✅ NEW: ghi chú theo trạng thái (admin/staff cập nhật)
        [StringLength(500)]
        [Display(Name = "Ghi chú trạng thái")]
        public string? GhiChuTrangThai { get; set; }

        // ✅ NEW: phục vụ hủy lịch (user cancel)
        [Display(Name = "Ngày hủy")]
        public DateTime? NgayHuy { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do hủy")]
        public string? LyDoHuy { get; set; }

        [StringLength(50)]
        [Display(Name = "Mã giao dịch")]
        public string? GiaoDichId { get; set; }

        [ForeignKey(nameof(GiaoDichId))]
        public virtual GiaoDich? GiaoDich { get; set; }

        public virtual ICollection<LichTrinhChiTiet> LichTrinhChiTiets { get; set; }
        public virtual ICollection<DanhGiaTour> DanhGiaTours { get; set; }

        // Tiện hiển thị (không map DB)
        [NotMapped]
        public int TongSoNguoi => SoNguoiLon + SoTreEm;
    }
}
