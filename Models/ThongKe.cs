using System;
using System.Collections.Generic;

namespace Tour_2040.Models
{
    public class ThongKeDoanhThuItem
    {
        public string Nhan { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
    }

    public class ThongKeDichVuItem
    {
        public string LoaiDichVu { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal TongDonGia { get; set; }
    }

    public class ThongKeDichVuChiTietItem
    {
        public string MaGiaoDich { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public string TenTour { get; set; } = string.Empty;
        public string TenKhach { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
    }

    public class ThongKeViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string KieuThongKe { get; set; } = "Ngay";

        public int TongSoTour { get; set; }
        public int TongSoTourMacDinh { get; set; }
        public int TongSoTourCaNhan { get; set; }
        public int TongSoKhachHang { get; set; }
        public int TongSoNhanVien { get; set; }
        public int TongSoUser { get; set; }

        public int TongSoGiaoDich { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int TongSoHopDong { get; set; }
        public int SoHopDongCoThanhToan { get; set; }
        public decimal DoanhThuTuHopDong { get; set; }

        public int TongSoYeuCauHoTro { get; set; }
        public int TongSoDiaDiem { get; set; }
        public int TongSoDichVu { get; set; }

        public decimal DoanhThuTourMacDinh { get; set; }
        public decimal DoanhThuTourCaNhan { get; set; }

        public List<ThongKeDichVuItem> ThongKeDichVus { get; set; } = new List<ThongKeDichVuItem>();
        public List<ThongKeDoanhThuItem> DoanhThuTheoMoc { get; set; } = new List<ThongKeDoanhThuItem>();
    }

    public class ThongKeDichVuChiTietViewModel
    {
        public string LoaiDichVu { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TongSoGiaoDich { get; set; }
        public decimal TongTien { get; set; }
        public List<ThongKeDichVuChiTietItem> Items { get; set; } = new List<ThongKeDichVuChiTietItem>();
    }
}