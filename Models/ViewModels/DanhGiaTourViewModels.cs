// Folder: Models
// File path: Models/DanhGiaTourViewModels.cs
// File name: DanhGiaTourViewModels.cs
// Labels: A(Index VM), B(User CRUD VM), C(Admin Details VM), D(Compatibility Wrapper)
// Mô tả:
// - FIX: Đủ class trong namespace Tour_2040.Models (đúng như @model trong Views)
// - Có wrapper DanhGiaTourViewModels để những view cũ lỡ gọi vẫn compile.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Tour_2040.Models
{
    // =========================
    // PUBLIC/ADMIN INDEX
    // =========================
    public class DanhGiaTourIndexViewModels
    {
        public string? TuKhoa { get; set; }
        public string? TrangThai { get; set; } = "tatca"; // tatca/dadanhgia/chuadanhgia
        public int? SoSaoLoc { get; set; }

        public int Trang { get; set; } = 1;
        public int KichThuocTrang { get; set; } = 10;
        public int TongSoDong { get; set; }
        public int TongSoTrang { get; set; } = 1;

        public int TongDanhGia { get; set; }
        public double DiemTrungBinh { get; set; }
        public int DanhGiaNamSao { get; set; }

        public List<DanhGiaTourAdminDongVM> DanhSachDong { get; set; } = new();
    }

    public class DanhGiaTourAdminDongVM
    {
        public string MaTour { get; set; } = string.Empty;
        public string TenTour { get; set; } = string.Empty;
        public string? DiaDiem { get; set; }
        public decimal GiaTour { get; set; }

        public int SoLuongDanhGia { get; set; }
        public double DiemTrungBinh { get; set; }
        public DateTime? NgayDanhGiaGanNhat { get; set; }
        public int SoLuongNamSao { get; set; }

        // UI admin
        public bool CoDanhGia => SoLuongDanhGia > 0;
    }

    // =========================
    // USER: CREATE
    // =========================
    public class DanhGiaTourCreateViewModel
    {
        [Required]
        public string MaLichTrinh { get; set; } = string.Empty;

        public string TenTourHoacLichTrinh { get; set; } = string.Empty;

        [Range(1, 5)]
        public int SoSao { get; set; } = 5;

        [StringLength(2000)]
        public string? BinhLuan { get; set; }

        public IFormFile? TepHinhAnh { get; set; }

        // flag để view hiển thị nút/khối đánh giá
        public bool CanDanhGia { get; set; } = true;

        // thông báo mềm (ví dụ chưa hoàn thành tour)
        public string? ThongBao { get; set; }

        public List<DanhGiaTourListItemVM> DanhSachDanhGia { get; set; } = new();
    }

    public class DanhGiaTourListItemVM
    {
        public string MaDanhGia { get; set; } = string.Empty;
        public int SoSao { get; set; }
        public string? BinhLuan { get; set; }
        public string? HinhAnhUrl { get; set; }
        public DateTime NgayTao { get; set; }
        public string TenNguoiDung { get; set; } = "User";
        public bool IsOwner { get; set; }
    }

    // =========================
    // USER: EDIT
    // =========================
    public class DanhGiaTourEditViewModel
    {
        [Required] public string MaDanhGia { get; set; } = string.Empty;
        [Required] public string MaLichTrinh { get; set; } = string.Empty;

        public string TenTourHoacLichTrinh { get; set; } = string.Empty;

        [Range(1, 5)]
        public int SoSao { get; set; } = 5;

        [StringLength(2000)]
        public string? BinhLuan { get; set; }

        public string? HinhAnhUrl { get; set; }
        public IFormFile? TepHinhAnh { get; set; }
    }

    // =========================
    // USER: DELETE
    // =========================
    public class DanhGiaTourItemViewModel
    {
        public string MaDanhGia { get; set; } = string.Empty;
        public string TenNguoiDung { get; set; } = "User";
    }

    // =========================
    // DETAILS THEO TOUR (PublicTourDetails)
    // =========================
    public class DanhGiaTourAdminTourDetailsViewModel
    {
        public string MaTour { get; set; } = string.Empty;
        public string TenTour { get; set; } = string.Empty;
        public string? DiaDiem { get; set; }
        public decimal GiaTour { get; set; }

        public int TongDanhGia { get; set; }
        public double DiemTrungBinh { get; set; }

        // breakdown 1-5 sao
        public int Sao1 { get; set; }
        public int Sao2 { get; set; }
        public int Sao3 { get; set; }
        public int Sao4 { get; set; }
        public int Sao5 { get; set; }

        public int Trang { get; set; } = 1;
        public int KichThuocTrang { get; set; } = 10;
        public int TongSoDong { get; set; }
        public int TongSoTrang { get; set; } = 1;

        public List<DanhGiaTourAdminDanhGiaVM> DanhSachDanhGia { get; set; } = new();
    }

    public class DanhGiaTourAdminDanhGiaVM
    {
        public string MaDanhGia { get; set; } = string.Empty;
        public string MaLichTrinh { get; set; } = string.Empty;
        public string? MaTour { get; set; }
        public string MaNguoiDung { get; set; } = string.Empty;

        public string TenNguoiDung { get; set; } = "User";
        public int SoSao { get; set; }
        public string? BinhLuan { get; set; }
        public string? HinhAnhUrl { get; set; }
        public DateTime NgayDanhGia { get; set; }
        public DateTime NgayTao { get; set; }

        public bool IsOwner { get; set; }
    }

    // =========================
    // COMPATIBILITY WRAPPER
    // =========================
    public class DanhGiaTourViewModels
    {
        public class DanhGiaTourIndexViewModels : Tour_2040.Models.DanhGiaTourIndexViewModels { }
        public class DanhGiaTourAdminDongVM : Tour_2040.Models.DanhGiaTourAdminDongVM { }
        public class DanhGiaTourCreateViewModel : Tour_2040.Models.DanhGiaTourCreateViewModel { }
        public class DanhGiaTourEditViewModel : Tour_2040.Models.DanhGiaTourEditViewModel { }
        public class DanhGiaTourItemViewModel : Tour_2040.Models.DanhGiaTourItemViewModel { }
        public class DanhGiaTourAdminTourDetailsViewModel : Tour_2040.Models.DanhGiaTourAdminTourDetailsViewModel { }
        public class DanhGiaTourAdminDanhGiaVM : Tour_2040.Models.DanhGiaTourAdminDanhGiaVM { }
    }
}
