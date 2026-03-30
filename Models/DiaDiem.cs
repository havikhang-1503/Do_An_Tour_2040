// Folder: Models
// File path: Models/DiaDiem.cs
// File name: DiaDiem.cs
// Class: DiaDiem
// Labels: A(PK), B(FK XaPhuong), C(Google Maps), D(Fields), E(Relations), F(Default)
// Desc: Địa điểm có đủ PlaceId + Lat/Lng để UI chọn trên Google Maps và backend lưu chính xác.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    [Table("DiaDiem")]
    public class DiaDiem
    {
        public DiaDiem()
        {
            // Auto-generate Location Code: MDD + 6 digits
            MaDiaDiem = MaSoHelper.TaoMa("MDD");
            DichVus = new List<DichVu>();
            ConSuDung = true;
        }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Mã địa điểm")]
        [Column("MaDiaDiem")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaDiaDiem { get; set; }

        [Required(ErrorMessage = "Tên địa điểm không được để trống")]
        [StringLength(200)]
        [Display(Name = "Tên địa điểm")]
        [Column("TenDiaDiem")]
        public string TenDiaDiem { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Xã / Phường")]
        [Column("XaPhuongId")]
        public int XaPhuongId { get; set; }

        [ForeignKey(nameof(XaPhuongId))]
        public virtual XaPhuong? XaPhuong { get; set; }

        [StringLength(500)]
        [Display(Name = "Địa chỉ chi tiết")]
        [Column("DiaChiChiTiet")]
        public string? DiaChiChiTiet { get; set; }

        // ================== GOOGLE MAPS (OPTIONAL) ==================

        [StringLength(200)]
        [Display(Name = "Google Place Id")]
        [Column("GooglePlaceId")]
        public string? GooglePlaceId { get; set; }

        [StringLength(700)]
        [Display(Name = "Địa chỉ đầy đủ")]
        [Column("DiaChiDayDu")]
        public string? DiaChiDayDu { get; set; }

        [Display(Name = "Vĩ độ")]
        [Column("Lat", TypeName = "decimal(10,7)")]
        public decimal? Lat { get; set; }

        [Display(Name = "Kinh độ")]
        [Column("Lng", TypeName = "decimal(10,7)")]
        public decimal? Lng { get; set; }

        [Display(Name = "Mô tả")]
        [Column("MoTa")]
        public string? MoTa { get; set; }

        [Display(Name = "Còn sử dụng")]
        [Column("ConSuDung")]
        public bool ConSuDung { get; set; }

        public virtual ICollection<DichVu> DichVus { get; set; }

        [NotMapped]
        public string? GoogleMapsUrl
        {
            get
            {
                if (Lat.HasValue && Lng.HasValue)
                {
                    return $"https://www.google.com/maps?q={Lat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{Lng.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                }

                if (!string.IsNullOrWhiteSpace(GooglePlaceId))
                {
                    return $"https://www.google.com/maps/search/?api=1&query_place_id={Uri.EscapeDataString(GooglePlaceId)}";
                }

                return null;
            }
        }
    }
}
