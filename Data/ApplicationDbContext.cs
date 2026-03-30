    // Folder: Data
    // File path: Data/ApplicationDbContext.cs

    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Tour_2040.Models;

    namespace Tour_2040.Data
    {
        public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            // ====== MAIN TABLES ======
            public DbSet<Tour> Tours { get; set; } = default!;
            public DbSet<DichVu> DichVus { get; set; } = default!;
            public DbSet<TourDichVu> TourDichVus { get; set; } = default!;

            public DbSet<GiaoDich> GiaoDiches { get; set; } = default!;
            public DbSet<GiaoDichChiTiet> GiaoDichChiTiets { get; set; } = default!;

            public DbSet<HopDong> HopDongs { get; set; } = default!;
            public DbSet<ThanhToan> ThanhToans { get; set; } = default!;
            public DbSet<YeuCauHoTro> YeuCauHoTros { get; set; } = default!;

            public DbSet<DiaDiem> DiaDiems { get; set; } = default!;
            public DbSet<TinhThanh> TinhThanhs { get; set; } = default!;
            public DbSet<XaPhuong> XaPhuongs { get; set; } = default!;

            public DbSet<NhanVien> NhanViens { get; set; } = default!;
            public DbSet<KhachHang> KhachHangs { get; set; } = default!;

            public DbSet<LichTrinh> LichTrinhs { get; set; } = default!;
            public DbSet<LichTrinhChiTiet> LichTrinhChiTiets { get; set; } = default!;
            public DbSet<LichTrinhChiTietDichVu> LichTrinhChiTietDichVus { get; set; } = default!;

            public DbSet<DanhGiaTour> DanhGiaTours { get; set; } = default!;

            public DbSet<Voucher> Vouchers { get; set; } = default!;
            public DbSet<UserVoucher> UserVouchers { get; set; } = default!;
            public DbSet<Tour_2040.Models.TinNhanHoTro> TinNhanHoTros { get; set; }
            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);

                // ====== TOUR - SERVICE RELATIONSHIP (N-N via TourDichVu) ======
                builder.Entity<TourDichVu>()
                    .HasOne(td => td.Tour)
                    .WithMany(t => t.TourDichVus)
                    .HasForeignKey(td => td.TourId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<TourDichVu>()
                    .HasOne(td => td.DichVu)
                    .WithMany(d => d.TourDichVus)
                    .HasForeignKey(td => td.DichVuId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ====== TRANSACTION DETAILS ======
                builder.Entity<GiaoDichChiTiet>()
                    .HasOne(gd => gd.GiaoDich)
                    .WithMany(g => g.ChiTietDichVus)
                    .HasForeignKey(gd => gd.GiaoDichId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<GiaoDichChiTiet>()
                    .HasOne(gd => gd.DichVu)
                    .WithMany()
                    .HasForeignKey(gd => gd.DichVuId)
                    .OnDelete(DeleteBehavior.SetNull); // Keep history even if service is deleted

                // ====== SCHEDULE DETAILS ======
                builder.Entity<LichTrinhChiTiet>()
                    .HasOne(lt => lt.LichTrinh)
                    .WithMany(l => l.LichTrinhChiTiets)
                    .HasForeignKey(lt => lt.LichTrinhId)
                    .OnDelete(DeleteBehavior.Cascade);

                // ====== DECIMAL CONFIGURATION (MONEY) ======
                var decimalProps = new[]
                {
                    (typeof(DichVu), nameof(DichVu.DonGia)),
                    (typeof(GiaoDich), nameof(GiaoDich.TongTien)),
                    (typeof(GiaoDichChiTiet), nameof(GiaoDichChiTiet.DonGia)),
                    (typeof(ThanhToan), nameof(ThanhToan.SoTien)),
                    (typeof(Tour), nameof(Tour.GiaTour)),
                    (typeof(LichTrinh), nameof(LichTrinh.TongTien)),
                    (typeof(TourDichVu), nameof(TourDichVu.DonGiaHienTai)),
                    (typeof(Voucher), nameof(Voucher.GiaTriGiam)),
                    (typeof(Voucher), nameof(Voucher.DonHangToiThieu)),
                    (typeof(Voucher), nameof(Voucher.SoTienGiamToiDa)),
                    (typeof(Voucher), nameof(Voucher.SoTienGiamToiThieu))
                };

                foreach (var (type, propName) in decimalProps)
                {
                    builder.Entity(type).Property(propName).HasColumnType("decimal(18,2)");
                }

                // ====== CUSTOMER RELATIONSHIPS ======
                // Transaction -> Customer
                builder.Entity<GiaoDich>()
                    .HasOne(g => g.KhachHang)
                    .WithMany(k => k.GiaoDiches)
                    .HasForeignKey(g => g.KhachHangId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Payment -> Customer
                builder.Entity<ThanhToan>()
                    .HasOne(t => t.KhachHang)
                    .WithMany(k => k.ThanhToans)
                    .HasForeignKey(t => t.KhachHangId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Contract -> Customer
                builder.Entity<HopDong>()
                    .HasOne(h => h.KhachHang)
                    .WithMany(k => k.HopDongs)
                    .HasForeignKey(h => h.KhachHangId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Support Request -> Customer
                builder.Entity<YeuCauHoTro>()
                    .HasOne(y => y.KhachHang)
                    .WithMany(k => k.YeuCauHoTros)
                    .HasForeignKey(y => y.KhachHangId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ====== ADMINISTRATIVE UNITS ======
                // Province -> Ward (1-N)
                builder.Entity<XaPhuong>()
                    .HasOne(xp => xp.TinhThanh)
                    .WithMany(t => t.XaPhuongs)
                    .HasForeignKey(xp => xp.TinhThanhId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ward -> Location (1-N)
                builder.Entity<DiaDiem>()
                    .HasOne(d => d.XaPhuong)
                    .WithMany(xp => xp.DiaDiems)
                    .HasForeignKey(d => d.XaPhuongId)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }
    //}   