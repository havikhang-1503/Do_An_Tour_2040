// Folder: Models
// File path: Models/DichVu.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    [Table("DichVu")]
    public class DichVu
    {
        public DichVu()
        {
            this.MaDichVu = MaSoHelper.TaoMa("MDV");
            this.MaTenDichVu = this.MaDichVu;

            this.TrangThai = "Active";
            this.HieuLucTu = DateTime.Now;
            this.TourDichVus = new List<TourDichVu>();
        }

        // PK (theo code cũ của em)
        [Key]
        [Required, StringLength(20)]
        [Display(Name = "Mã định danh dịch vụ")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaTenDichVu { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã dịch vụ")]
        public string MaDichVu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
        [StringLength(200)]
        [Display(Name = "Tên dịch vụ")]
        public string TenDichVu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại dịch vụ")]
        [StringLength(100)]
        [Display(Name = "Loại dịch vụ")]
        public string LoaiDichVu { get; set; } = string.Empty;

        // Nếu là Phương tiện / Nhà hàng / Khách sạn / Khu vui chơi => bắt buộc có MaDiaDiem
        [Display(Name = "Địa điểm liên kết")]
        [StringLength(20)]
        public string? MaDiaDiem { get; set; }

        [ForeignKey(nameof(MaDiaDiem))]
        [Display(Name = "Địa điểm")]
        public virtual DiaDiem? DiaDiem { get; set; }

        [StringLength(300)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hiệu lực từ")]
        public DateTime? HieuLucTu { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hiệu lực đến")]
        public DateTime? HieuLucDen { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [StringLength(50)]
        [Display(Name = "Đơn vị tính")]
        public string? DonViTinh { get; set; }

        [StringLength(300)]
        [Display(Name = "Hình ảnh")]
        public string? HinhAnhUrl { get; set; }

        [StringLength(300)]
        [Display(Name = "Thông tin pháp lý")]
        public string? ThongTinPhapLy { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public virtual ICollection<TourDichVu> TourDichVus { get; set; }
    }
}
