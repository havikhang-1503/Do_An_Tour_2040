using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    // Lịch trình mẫu theo ngày gắn với Tour (template itinerary)
    public class TourLichTrinhNgay
    {
        public TourLichTrinhNgay()
        {
            MaTourLichTrinhNgay = MaSoHelper.TaoMa("TLT");
            NgayThu = 1;
        }

        [Key]
        [Required, StringLength(50)]
        [Display(Name = "Mã lịch trình mẫu theo ngày")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaTourLichTrinhNgay { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Display(Name = "Mã Tour")]
        public string TourId { get; set; } = string.Empty;

        [Range(1, 365)]
        [Display(Name = "Ngày thứ")]
        public int NgayThu { get; set; }

        [StringLength(250)]
        [Display(Name = "Tiêu đề")]
        public string? TieuDe { get; set; }

        [Display(Name = "Nội dung")]
        public string? NoiDung { get; set; }

        [ForeignKey(nameof(TourId))]
        public virtual Tour? Tour { get; set; }
    }
}
