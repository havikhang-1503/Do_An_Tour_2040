// Folder: Models
// File path: Models/YeuCauHoTro.cs
// File name: YeuCauHoTro.cs
// Class: YeuCauHoTro
// Labels: A(PK), B(FK User), C(FK KhachHang - FIX), D(FK GiaoDich), E(Default)
// FIX NOTES:
// - Đồng bộ độ dài KhachHangId = 20 để khớp KhachHang.MaKhachHang (nvarchar(20)).

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    [Table("YeuCauHoTros")]
    public class YeuCauHoTro
    {
        public YeuCauHoTro()
        {
            MaYeuCauHoTro = MaSoHelper.TaoMa("MYC");
            NgayGui = DateTime.Now;
            TrangThai = "Chờ xử lý";
            LoaiHoTro = string.Empty;
            ChuDe = string.Empty;
            NoiDung = string.Empty;
            TinNhanHoTros = new List<TinNhanHoTro>();
        }

        [Key]
        [Required]
        [StringLength(50)]
        [Display(Name = "Mã yêu cầu")]
        public string MaYeuCauHoTro { get; set; } = string.Empty;

        [StringLength(450)]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        // ✅ FIX: 20 cho khớp KhachHang.MaKhachHang
        [Display(Name = "Mã khách hàng")]
        [StringLength(20)]
        public string? KhachHangId { get; set; }

        [ForeignKey(nameof(KhachHangId))]
        public virtual KhachHang? KhachHang { get; set; }

        [StringLength(50)]
        [Display(Name = "Mã giao dịch")]
        public string? MaGiaoDich { get; set; }

        [ForeignKey(nameof(MaGiaoDich))]
        public virtual GiaoDich? GiaoDich { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại hỗ trợ")]
        [StringLength(100)]
        [Display(Name = "Phân loại")]
        public string LoaiHoTro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập chủ đề")]
        [StringLength(200)]
        [Display(Name = "Chủ đề")]
        public string ChuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung chi tiết")]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Display(Name = "Ngày gửi")]
        public DateTime NgayGui { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chờ xử lý";

        [Display(Name = "Nội dung phản hồi")]
        public string? TraLoi { get; set; }

        [Display(Name = "Ngày phản hồi")]
        public DateTime? NgayTraLoi { get; set; }

        [NotMapped]
        public string Id
        {
            get => MaYeuCauHoTro;
            set => MaYeuCauHoTro = value;
        }

        public virtual ICollection<TinNhanHoTro> TinNhanHoTros { get; set; }
    }
}
