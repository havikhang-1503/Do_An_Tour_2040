// Folder: Models
// File path: Models/Voucher.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class Voucher
    {
        public Voucher()
        {
            this.MaVoucher = MaSoHelper.TaoMa("MVC");
            this.NgayTao = DateTime.Now;
            this.IsActive = true;
            this.DaSuDung = 0;
            this.TrangThaiDuyet = "Chờ duyệt";
        }

        [Key]
        [Required, StringLength(50)]
        [Display(Name = "Mã Voucher")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaVoucher { get; set; }

        [Required(ErrorMessage = "Tên voucher không được để trống")]
        [StringLength(200)]
        [Display(Name = "Tên Voucher")]
        public string TenVoucher { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Loại giảm")]
        public string LoaiGiam { get; set; } = "SoTien";

        [Required]
        [Display(Name = "Giá trị giảm")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn hoặc bằng 0")]
        public decimal GiaTriGiam { get; set; }

        [Display(Name = "Số tiền giảm tối thiểu")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal? SoTienGiamToiThieu { get; set; }

        [Display(Name = "Số tiền giảm tối đa")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal? SoTienGiamToiDa { get; set; }

        [Display(Name = "Đơn hàng tối thiểu")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal? DonHangToiThieu { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime? NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime? NgayKetThuc { get; set; }

        [Display(Name = "Số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int? SoLuong { get; set; }

        [Display(Name = "Đã sử dụng")]
        public int DaSuDung { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        public DateTime NgayTao { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái duyệt")]
        public string? TrangThaiDuyet { get; set; }
    }
}