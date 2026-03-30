// Folder: Models
// File path: Models/GiaoDichChiTiet.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class GiaoDichChiTiet
    {
        public GiaoDichChiTiet()
        {
            // Auto-generate Detail Code: MCT + 6 digits
            this.MaChiTietGiaoDich = MaSoHelper.TaoMa("MCT");
        }

        // Sử dụng duy nhất một cột chuỗi làm Khóa chính
        [Key]
        [Required, StringLength(100)]
        [Display(Name = "Mã chi tiết giao dịch")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự sinh số
        public string MaChiTietGiaoDich { get; set; }

        // FK -> GiaoDich (Chuyển sang string để khớp với GiaoDich.MaGiaoDich)
        [Required, StringLength(50)]
        public string GiaoDichId { get; set; } = string.Empty;

        [ForeignKey(nameof(GiaoDichId))]
        public virtual GiaoDich GiaoDich { get; set; } = default!;

        // FK -> DichVu (Chuyển sang string để khớp với DichVu.MaDichVu)
        [StringLength(20)]
        public string? DichVuId { get; set; }

        [ForeignKey(nameof(DichVuId))]
        public virtual DichVu? DichVu { get; set; }

        // Snapshot tên để sau này đổi danh mục không ảnh hưởng lịch sử
        [StringLength(200)]
        [Display(Name = "Tên dịch vụ")]
        public string? TenDichVu { get; set; }

        [StringLength(200)]
        [Display(Name = "Tên phương tiện")]
        public string? TenPhuongTien { get; set; }

        [Display(Name = "Số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        [Range(0, double.MaxValue)]
        public decimal DonGia { get; set; }

        [NotMapped]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien => SoLuong * DonGia;
    }
}