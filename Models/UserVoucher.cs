// Folder: Models
// File path: Models/UserVoucher.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tour_2040.Utils;

namespace Tour_2040.Models
{
    public class UserVoucher
    {
        public UserVoucher()
        {
            this.MaUserVoucher = MaSoHelper.TaoMa("MUV");
            this.NgayLuu = DateTime.Now;
            this.IsUsed = false;
        }

        [Key]
        [Required, StringLength(50)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MaUserVoucher { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string VoucherId { get; set; } = string.Empty;

        [Display(Name = "Ngày lưu")]
        public DateTime NgayLuu { get; set; }

        [Display(Name = "Đã sử dụng")]
        public bool IsUsed { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey(nameof(VoucherId))]
        public virtual Voucher? Voucher { get; set; }
    }
}