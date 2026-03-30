    // Folder: Models
    // File path: Models/TinhThanh.cs

    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Tour_2040.Models
    {
        [Table("TinhThanh")]
        public class TinhThanh
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.None)] // Standard administrative IDs are usually manually imported
            [Column("MaTinh")]
            public int MaTinhThanh { get; set; }

            [Required(ErrorMessage = "Tên tỉnh thành không được để trống")]
            [StringLength(100)]
            [Display(Name = "Tỉnh/Thành Phố")]
            public string TenTinh { get; set; } = string.Empty;

            // Quan hệ 1-N: Một Tỉnh có nhiều Xã/Phường
            public virtual ICollection<XaPhuong> XaPhuongs { get; set; } = new List<XaPhuong>();
        }
    }