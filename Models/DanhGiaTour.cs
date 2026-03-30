// Folder: Models
// File path: Models/DanhGiaTour.cs
// File name: DanhGiaTour.cs
// Class: DanhGiaTour
// Labels: A(PK), B(FK LichTrinh), C(FK Tour), D(FK User), E(Fields), F(Default)
// NOTE: Giữ LichTrinhId length=20 (khớp MaLichTrinh). UserId length=450 (Identity chuẩn).

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class DanhGiaTour
    {
        public DanhGiaTour()
        {
            MaDanhGia = MaSoHelper.TaoMa("MDG");

            // FIX: tránh SoSao=0 gây fail Range ở create form
            SoSao = 5;

            NgayDanhGia = DateTime.Now;
            NgayTao = DateTime.Now;
        }

        [Key]
        [Required, StringLength(50)]
        [Display(Name = "Mã đánh giá")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaDanhGia { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã lịch trình")]
        public string LichTrinhId { get; set; } = string.Empty;

        [ForeignKey(nameof(LichTrinhId))]
        public virtual LichTrinh? LichTrinh { get; set; }

        [StringLength(20)]
        [Display(Name = "Mã tour")]
        public string? TourId { get; set; }

        [ForeignKey(nameof(TourId))]
        public virtual Tour? Tour { get; set; }

        [Required, StringLength(450)]
        [Display(Name = "Người dùng")]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Vui lòng đánh giá từ 1 đến 5 sao")]
        [Display(Name = "Số sao")]
        public int SoSao { get; set; } = 5;

        [StringLength(2000)]
        [Display(Name = "Bình luận")]
        public string? BinhLuan { get; set; }

        [StringLength(500)]
        [Display(Name = "Ảnh minh họa")]
        public string? HinhAnhUrl { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime NgayDanhGia { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }

        [Display(Name = "Ngày tạo hệ thống")]
        public DateTime NgayTao { get; set; }
    }
}
