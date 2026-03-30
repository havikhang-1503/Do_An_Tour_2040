// Folder: Models
// File path: Models/TourRegisterVoucherItem.cs

using System.ComponentModel.DataAnnotations;

namespace Tour_2040.Models
{
    public class TourRegisterVoucherItem
    {
        [Display(Name = "Mã dòng voucher")]
        public string MaTourRegisterVoucherItem { get; set; } = string.Empty;

        [Display(Name = "Mã Voucher")]
        public string MaVoucher { get; set; } = string.Empty;

        [Display(Name = "Tên Voucher")]
        public string TenVoucher { get; set; } = string.Empty;

        [Display(Name = "Loại giảm giá")]
        public string LoaiGiam { get; set; } = "SoTien";

        [Display(Name = "Giá trị giảm")]
        public decimal GiaTriGiam { get; set; }

        [Display(Name = "Giảm tối đa")]
        public decimal? SoTienGiamToiDa { get; set; }

        [Display(Name = "Giảm tối thiểu")]
        public decimal? SoTienGiamToiThieu { get; set; }

        [Display(Name = "Đơn hàng tối thiểu")]
        public decimal? DonHangToiThieu { get; set; }
    }
}