using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tour_2040.Migrations
{
    /// <inheritdoc />
    public partial class inntt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    AnhCCCDUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HangThanhVien = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DiemTichLuy = table.Column<int>(type: "int", nullable: false),
                    TongChiTieu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLanDatTour = table.Column<int>(type: "int", nullable: false),
                    LanDatGanNhat = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    SoLanHuy = table.Column<int>(type: "int", nullable: false),
                    NgayCapVoucherSinhNhatGanNhat = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    SoThichLoaiTour = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SoThichMucGia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoThichMua = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoThichPhuongTien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoThichAnUong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    KenhDen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NhanEmailMarketing = table.Column<bool>(type: "bit", nullable: false),
                    NhanSMSMarketing = table.Column<bool>(type: "bit", nullable: false),
                    NhanZaloMarketing = table.Column<bool>(type: "bit", nullable: false),
                    NgayDongYMarketing = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    MaKhachHang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CCCD = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhomKhach = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThaiTour = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVip = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.MaKhachHang);
                });

            migrationBuilder.CreateTable(
                name: "TinhThanh",
                columns: table => new
                {
                    MaTinh = table.Column<int>(type: "int", nullable: false),
                    TenTinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinhThanh", x => x.MaTinh);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    MaVoucher = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenVoucher = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LoaiGiam = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GiaTriGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoTienGiamToiThieu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoTienGiamToiDa = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DonHangToiThieu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    DaSuDung = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiDuyet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.MaVoucher);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    MaNhanVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayVaoLam = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConLamViec = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.MaNhanVien);
                    table.ForeignKey(
                        name: "FK_NhanViens_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XaPhuong",
                columns: table => new
                {
                    MaXaPhuong = table.Column<int>(type: "int", nullable: false),
                    TinhThanhId = table.Column<int>(type: "int", nullable: false),
                    TenXaPhuong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XaPhuong", x => x.MaXaPhuong);
                    table.ForeignKey(
                        name: "FK_XaPhuong_TinhThanh_TinhThanhId",
                        column: x => x.TinhThanhId,
                        principalTable: "TinhThanh",
                        principalColumn: "MaTinh",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserVouchers",
                columns: table => new
                {
                    MaUserVoucher = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VoucherId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayLuu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVouchers", x => x.MaUserVoucher);
                    table.ForeignKey(
                        name: "FK_UserVouchers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVouchers_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "MaVoucher",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaDiem",
                columns: table => new
                {
                    MaDiaDiem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenDiaDiem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    XaPhuongId = table.Column<int>(type: "int", nullable: false),
                    DiaChiChiTiet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GooglePlaceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DiaChiDayDu = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    Lat = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    Lng = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConSuDung = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaDiem", x => x.MaDiaDiem);
                    table.ForeignKey(
                        name: "FK_DiaDiem_XaPhuong_XaPhuongId",
                        column: x => x.XaPhuongId,
                        principalTable: "XaPhuong",
                        principalColumn: "MaXaPhuong",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DichVu",
                columns: table => new
                {
                    MaTenDichVu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaDichVu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenDichVu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LoaiDichVu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaDiaDiem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HieuLucTu = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HieuLucDen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HinhAnhUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ThongTinPhapLy = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichVu", x => x.MaTenDichVu);
                    table.ForeignKey(
                        name: "FK_DichVu_DiaDiem_MaDiaDiem",
                        column: x => x.MaDiaDiem,
                        principalTable: "DiaDiem",
                        principalColumn: "MaDiaDiem");
                });

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    MaTour = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenTour = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    LoaiTour = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NoiKhoiHanh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DiaDiem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NoiKhoiHanhDiaDiemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DiaDiemChinhId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GiaTour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    SoNguoiToiDa = table.Column<int>(type: "int", nullable: true),
                    SoNguoiHienTai = table.Column<int>(type: "int", nullable: true),
                    NgayDi = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    NgayVe = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    LichTrinhJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsPersonal = table.Column<bool>(type: "bit", nullable: false),
                    HinhAnhTourUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    YeuCauKhachSan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    YeuCauNhaHang = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    YeuCauPhuongTien = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    YeuCauVuiChoi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    YeuCauKhac = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.MaTour);
                    table.ForeignKey(
                        name: "FK_Tours_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tours_DiaDiem_DiaDiemChinhId",
                        column: x => x.DiaDiemChinhId,
                        principalTable: "DiaDiem",
                        principalColumn: "MaDiaDiem");
                    table.ForeignKey(
                        name: "FK_Tours_DiaDiem_NoiKhoiHanhDiaDiemId",
                        column: x => x.NoiKhoiHanhDiaDiemId,
                        principalTable: "DiaDiem",
                        principalColumn: "MaDiaDiem");
                });

            migrationBuilder.CreateTable(
                name: "GiaoDiches",
                columns: table => new
                {
                    MaGiaoDich = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KhachHangId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TourId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SoNguoiLon = table.Column<int>(type: "int", nullable: false),
                    SoTreEm = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HoTenNguoiDaiDien = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiaChiLienHe = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnhCCCDUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDiches", x => x.MaGiaoDich);
                    table.ForeignKey(
                        name: "FK_GiaoDiches_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GiaoDiches_KhachHangs_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaoDiches_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "MaTour");
                });

            migrationBuilder.CreateTable(
                name: "TourDichVus",
                columns: table => new
                {
                    MaDichVu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DichVuId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayThu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGiaHienTai = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDichVus", x => x.MaDichVu);
                    table.ForeignKey(
                        name: "FK_TourDichVus_DichVu_DichVuId",
                        column: x => x.DichVuId,
                        principalTable: "DichVu",
                        principalColumn: "MaTenDichVu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourDichVus_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "MaTour",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TourLichTrinhNgay",
                columns: table => new
                {
                    MaTourLichTrinhNgay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TourId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayThu = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourLichTrinhNgay", x => x.MaTourLichTrinhNgay);
                    table.ForeignKey(
                        name: "FK_TourLichTrinhNgay_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "MaTour",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GiaoDichChiTiets",
                columns: table => new
                {
                    MaChiTietGiaoDich = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiaoDichId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DichVuId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenDichVu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenPhuongTien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDichChiTiets", x => x.MaChiTietGiaoDich);
                    table.ForeignKey(
                        name: "FK_GiaoDichChiTiets_DichVu_DichVuId",
                        column: x => x.DichVuId,
                        principalTable: "DichVu",
                        principalColumn: "MaTenDichVu",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GiaoDichChiTiets_GiaoDiches_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalTable: "GiaoDiches",
                        principalColumn: "MaGiaoDich",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HopDongs",
                columns: table => new
                {
                    MaHopDong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenHopDong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GiaoDichId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TourId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    KhachHangId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayHieuLuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopDongs", x => x.MaHopDong);
                    table.ForeignKey(
                        name: "FK_HopDongs_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HopDongs_GiaoDiches_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalTable: "GiaoDiches",
                        principalColumn: "MaGiaoDich",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HopDongs_KhachHangs_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HopDongs_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "MaTour");
                });

            migrationBuilder.CreateTable(
                name: "LichTrinhs",
                columns: table => new
                {
                    MaLichTrinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TourId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TenLichTrinh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoNguoiLon = table.Column<int>(type: "int", nullable: false),
                    SoTreEm = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChuTrangThai = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayHuy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoHuy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GiaoDichId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichTrinhs", x => x.MaLichTrinh);
                    table.ForeignKey(
                        name: "FK_LichTrinhs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LichTrinhs_GiaoDiches_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalTable: "GiaoDiches",
                        principalColumn: "MaGiaoDich");
                    table.ForeignKey(
                        name: "FK_LichTrinhs_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "MaTour",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToans",
                columns: table => new
                {
                    MaThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GiaoDichId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuongThuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenTaiKhoan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoTaiKhoan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    KhachHangId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToans", x => x.MaThanhToan);
                    table.ForeignKey(
                        name: "FK_ThanhToans_GiaoDiches_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalTable: "GiaoDiches",
                        principalColumn: "MaGiaoDich",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThanhToans_KhachHangs_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauHoTros",
                columns: table => new
                {
                    MaYeuCauHoTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    KhachHangId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaGiaoDich = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LoaiHoTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TraLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTraLoi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauHoTros", x => x.MaYeuCauHoTro);
                    table.ForeignKey(
                        name: "FK_YeuCauHoTros_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauHoTros_GiaoDiches_MaGiaoDich",
                        column: x => x.MaGiaoDich,
                        principalTable: "GiaoDiches",
                        principalColumn: "MaGiaoDich");
                    table.ForeignKey(
                        name: "FK_YeuCauHoTros_KhachHangs_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaTours",
                columns: table => new
                {
                    MaDanhGia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LichTrinhId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TourId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    BinhLuan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HinhAnhUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaTours", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGiaTours_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGiaTours_LichTrinhs_LichTrinhId",
                        column: x => x.LichTrinhId,
                        principalTable: "LichTrinhs",
                        principalColumn: "MaLichTrinh",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGiaTours_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "MaTour");
                });

            migrationBuilder.CreateTable(
                name: "LichTrinhChiTiets",
                columns: table => new
                {
                    MaLichTrinhChiTiet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LichTrinhId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayThu = table.Column<int>(type: "int", nullable: false),
                    DiaDiemBatDauId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DiaDiemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Gio = table.Column<TimeSpan>(type: "time", nullable: false),
                    BatDauLuc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    KetThucLuc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ThoiLuongPhut = table.Column<int>(type: "int", nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LoaiHoatDong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichTrinhChiTiets", x => x.MaLichTrinhChiTiet);
                    table.ForeignKey(
                        name: "FK_LichTrinhChiTiets_DiaDiem_DiaDiemBatDauId",
                        column: x => x.DiaDiemBatDauId,
                        principalTable: "DiaDiem",
                        principalColumn: "MaDiaDiem");
                    table.ForeignKey(
                        name: "FK_LichTrinhChiTiets_DiaDiem_DiaDiemId",
                        column: x => x.DiaDiemId,
                        principalTable: "DiaDiem",
                        principalColumn: "MaDiaDiem");
                    table.ForeignKey(
                        name: "FK_LichTrinhChiTiets_LichTrinhs_LichTrinhId",
                        column: x => x.LichTrinhId,
                        principalTable: "LichTrinhs",
                        principalColumn: "MaLichTrinh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TinNhanHoTros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaYeuCauHoTro = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiGui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LaNhanVien = table.Column<bool>(type: "bit", nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhanHoTros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinNhanHoTros_YeuCauHoTros_MaYeuCauHoTro",
                        column: x => x.MaYeuCauHoTro,
                        principalTable: "YeuCauHoTros",
                        principalColumn: "MaYeuCauHoTro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichTrinhChiTietDichVus",
                columns: table => new
                {
                    MaLichTrinhChiTietDichVu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LichTrinhChiTietId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DichVuId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichTrinhChiTietDichVus", x => x.MaLichTrinhChiTietDichVu);
                    table.ForeignKey(
                        name: "FK_LichTrinhChiTietDichVus_DichVu_DichVuId",
                        column: x => x.DichVuId,
                        principalTable: "DichVu",
                        principalColumn: "MaTenDichVu",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichTrinhChiTietDichVus_LichTrinhChiTiets_LichTrinhChiTietId",
                        column: x => x.LichTrinhChiTietId,
                        principalTable: "LichTrinhChiTiets",
                        principalColumn: "MaLichTrinhChiTiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTours_LichTrinhId",
                table: "DanhGiaTours",
                column: "LichTrinhId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTours_TourId",
                table: "DanhGiaTours",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTours_UserId",
                table: "DanhGiaTours",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaDiem_XaPhuongId",
                table: "DiaDiem",
                column: "XaPhuongId");

            migrationBuilder.CreateIndex(
                name: "IX_DichVu_MaDiaDiem",
                table: "DichVu",
                column: "MaDiaDiem");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichChiTiets_DichVuId",
                table: "GiaoDichChiTiets",
                column: "DichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichChiTiets_GiaoDichId",
                table: "GiaoDichChiTiets",
                column: "GiaoDichId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDiches_KhachHangId",
                table: "GiaoDiches",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDiches_TourId",
                table: "GiaoDiches",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDiches_UserId",
                table: "GiaoDiches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongs_ApplicationUserId",
                table: "HopDongs",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongs_GiaoDichId",
                table: "HopDongs",
                column: "GiaoDichId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongs_KhachHangId",
                table: "HopDongs",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongs_TourId",
                table: "HopDongs",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinhChiTietDichVus_DichVuId",
                table: "LichTrinhChiTietDichVus",
                column: "DichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinhChiTietDichVus_LichTrinhChiTietId",
                table: "LichTrinhChiTietDichVus",
                column: "LichTrinhChiTietId");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinhChiTiets_DiaDiemBatDauId",
                table: "LichTrinhChiTiets",
                column: "DiaDiemBatDauId");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinhChiTiets_DiaDiemId",
                table: "LichTrinhChiTiets",
                column: "DiaDiemId");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinhChiTiets_LichTrinhId",
                table: "LichTrinhChiTiets",
                column: "LichTrinhId");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinhs_GiaoDichId",
                table: "LichTrinhs",
                column: "GiaoDichId");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinhs_TourId",
                table: "LichTrinhs",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinhs_UserId",
                table: "LichTrinhs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_ApplicationUserId",
                table: "NhanViens",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_GiaoDichId",
                table: "ThanhToans",
                column: "GiaoDichId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_KhachHangId",
                table: "ThanhToans",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhanHoTros_MaYeuCauHoTro",
                table: "TinNhanHoTros",
                column: "MaYeuCauHoTro");

            migrationBuilder.CreateIndex(
                name: "IX_TourDichVus_DichVuId",
                table: "TourDichVus",
                column: "DichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDichVus_TourId",
                table: "TourDichVus",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_TourLichTrinhNgay_TourId",
                table: "TourLichTrinhNgay",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_DiaDiemChinhId",
                table: "Tours",
                column: "DiaDiemChinhId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_NoiKhoiHanhDiaDiemId",
                table: "Tours",
                column: "NoiKhoiHanhDiaDiemId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_UserId",
                table: "Tours",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_UserId",
                table: "UserVouchers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_VoucherId",
                table: "UserVouchers",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_XaPhuong_TinhThanhId",
                table: "XaPhuong",
                column: "TinhThanhId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoTros_KhachHangId",
                table: "YeuCauHoTros",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoTros_MaGiaoDich",
                table: "YeuCauHoTros",
                column: "MaGiaoDich");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoTros_UserId",
                table: "YeuCauHoTros",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DanhGiaTours");

            migrationBuilder.DropTable(
                name: "GiaoDichChiTiets");

            migrationBuilder.DropTable(
                name: "HopDongs");

            migrationBuilder.DropTable(
                name: "LichTrinhChiTietDichVus");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "ThanhToans");

            migrationBuilder.DropTable(
                name: "TinNhanHoTros");

            migrationBuilder.DropTable(
                name: "TourDichVus");

            migrationBuilder.DropTable(
                name: "TourLichTrinhNgay");

            migrationBuilder.DropTable(
                name: "UserVouchers");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "LichTrinhChiTiets");

            migrationBuilder.DropTable(
                name: "YeuCauHoTros");

            migrationBuilder.DropTable(
                name: "DichVu");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "LichTrinhs");

            migrationBuilder.DropTable(
                name: "GiaoDiches");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DiaDiem");

            migrationBuilder.DropTable(
                name: "XaPhuong");

            migrationBuilder.DropTable(
                name: "TinhThanh");
        }
    }
}
