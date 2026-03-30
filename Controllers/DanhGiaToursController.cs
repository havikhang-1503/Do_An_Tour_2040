using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Models.ViewModels;

namespace Tour_2040.Controllers
{
    public class DanhGiaToursController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public DanhGiaToursController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // =========================
        // A) PUBLIC LIST (Trang chủ danh sách đánh giá)
        // Route mặc định: /DanhGiaTours
        // View: Index.cshtml
        // =========================
        [AllowAnonymous]
        // ĐÃ SỬA: Đổi tên từ Public -> Index để khớp với đường dẫn mặc định
        public async Task<IActionResult> Index(string? tuKhoa, string? trangThai, int? soSao, int trang = 1, int kichThuocTrang = 10)
        {
            trang = trang <= 0 ? 1 : trang;
            kichThuocTrang = kichThuocTrang is < 5 or > 50 ? 10 : kichThuocTrang;
            trangThai ??= "tatca";

            var toursQ = _db.Tours.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim().ToLower();
                toursQ = toursQ.Where(t =>
                    (t.MaTour != null && t.MaTour.ToLower().Contains(k)) ||
                    (t.TenTour != null && t.TenTour.ToLower().Contains(k)) ||
                    (t.DiaDiem != null && t.DiaDiem.ToLower().Contains(k))
                );
            }

            // Join đánh giá
            var dgQ = _db.DanhGiaTours.AsNoTracking();

            // Lọc sao theo tour
            if (soSao.HasValue && soSao.Value >= 1 && soSao.Value <= 5)
            {
                var s = soSao.Value;
                var tourIds = await dgQ.Where(d => d.SoSao == s && d.TourId != null).Select(d => d.TourId!).Distinct().ToListAsync();
                toursQ = toursQ.Where(t => tourIds.Contains(t.MaTour));
            }

            // Build stats
            var joined = from t in toursQ
                         join d in dgQ on t.MaTour equals d.TourId into g
                         select new DanhGiaTourAdminDongVM
                         {
                             MaTour = t.MaTour,
                             TenTour = t.TenTour ?? t.MaTour,
                             DiaDiem = t.DiaDiem,
                             GiaTour = t.GiaTour,
                             SoLuongDanhGia = g.Count(),
                             DiemTrungBinh = g.Any() ? g.Average(x => x.SoSao) : 0,
                             NgayDanhGiaGanNhat = g.Any() ? g.Max(x => x.NgayDanhGia) : (DateTime?)null,
                             SoLuongNamSao = g.Count(x => x.SoSao == 5)
                         };

            if (trangThai == "dadanhgia") joined = joined.Where(x => x.SoLuongDanhGia > 0);
            else if (trangThai == "chuadanhgia") joined = joined.Where(x => x.SoLuongDanhGia == 0);

            var tongDong = await joined.CountAsync();
            var tongTrang = (int)Math.Ceiling(tongDong / (double)kichThuocTrang);
            if (tongTrang <= 0) tongTrang = 1;
            if (trang > tongTrang) trang = tongTrang;

            var list = await joined
                .OrderByDescending(x => x.SoLuongDanhGia)
                .ThenBy(x => x.TenTour)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            // KPI tổng
            var allRatings = await _db.DanhGiaTours.AsNoTracking().ToListAsync();
            var tongDG = allRatings.Count;
            var diemTB = tongDG > 0 ? allRatings.Average(x => x.SoSao) : 0;
            var namSao = allRatings.Count(x => x.SoSao == 5);

            var vm = new DanhGiaTourIndexViewModels
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                SoSaoLoc = soSao,
                Trang = trang,
                KichThuocTrang = kichThuocTrang,
                TongSoDong = tongDong,
                TongSoTrang = tongTrang,
                TongDanhGia = tongDG,
                DiemTrungBinh = Math.Round(diemTB, 2),
                DanhGiaNamSao = namSao,
                DanhSachDong = list
            };

            // Trả về view Index.cshtml
            return View(vm);
        }

        // =========================
        // B) PUBLIC DETAILS: xem đánh giá theo tour
        // Route: /DanhGiaTours/PublicTourDetails/{maTour}
        // View: PublicTourDetails.cshtml
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> PublicTourDetails(string maTour, int trang = 1, int kichThuocTrang = 10)
        {
            trang = trang <= 0 ? 1 : trang;
            kichThuocTrang = kichThuocTrang is < 5 or > 50 ? 10 : kichThuocTrang;

            var tour = await _db.Tours.AsNoTracking().FirstOrDefaultAsync(t => t.MaTour == maTour);
            if (tour == null) return NotFound();

            var q = _db.DanhGiaTours.AsNoTracking()
                .Where(d => d.TourId == maTour)
                .OrderByDescending(d => d.NgayDanhGia);

            var tong = await q.CountAsync();
            var tongTrang = (int)Math.Ceiling(tong / (double)kichThuocTrang);
            if (tongTrang <= 0) tongTrang = 1;
            if (trang > tongTrang) trang = tongTrang;

            var list = await q
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .Select(d => new DanhGiaTourAdminDanhGiaVM
                {
                    MaDanhGia = d.MaDanhGia,
                    MaLichTrinh = d.LichTrinhId,
                    MaTour = d.TourId,
                    MaNguoiDung = d.UserId,
                    TenNguoiDung = d.User != null ? (d.User.UserName ?? "User") : "User",
                    SoSao = d.SoSao,
                    BinhLuan = d.BinhLuan,
                    HinhAnhUrl = d.HinhAnhUrl,
                    NgayDanhGia = d.NgayDanhGia,
                    NgayTao = d.NgayTao,
                    IsOwner = false
                })
                .ToListAsync();

            // stats
            var all = await _db.DanhGiaTours.AsNoTracking().Where(x => x.TourId == maTour).ToListAsync();
            var tongDG = all.Count;
            var diemTB = tongDG > 0 ? all.Average(x => x.SoSao) : 0;

            var vm = new DanhGiaTourAdminTourDetailsViewModel
            {
                MaTour = tour.MaTour,
                TenTour = tour.TenTour ?? tour.MaTour,
                DiaDiem = tour.DiaDiem,
                GiaTour = tour.GiaTour,

                TongDanhGia = tongDG,
                DiemTrungBinh = Math.Round(diemTB, 2),

                Sao1 = all.Count(x => x.SoSao == 1),
                Sao2 = all.Count(x => x.SoSao == 2),
                Sao3 = all.Count(x => x.SoSao == 3),
                Sao4 = all.Count(x => x.SoSao == 4),
                Sao5 = all.Count(x => x.SoSao == 5),

                Trang = trang,
                KichThuocTrang = kichThuocTrang,
                TongSoDong = tong,
                TongSoTrang = tongTrang,
                DanhSachDanhGia = list
            };

            return View(vm);
        }

        // =========================
        // C) USER CREATE (sau khi xong tour)
        // View: Create.cshtml
        // =========================
        [Authorize]
        public async Task<IActionResult> Create(string maLichTrinh)
        {
            var userId = _userManager.GetUserId(User) ?? "";

            var lichTrinh = await _db.LichTrinhs.AsNoTracking().FirstOrDefaultAsync(l => l.MaLichTrinh == maLichTrinh);
            if (lichTrinh == null) return NotFound();

            // Check owner nếu cần
            var propUserId = lichTrinh.GetType().GetProperty("UserId");
            if (propUserId != null)
            {
                var owner = propUserId.GetValue(lichTrinh)?.ToString();
                if (!string.IsNullOrWhiteSpace(owner) && owner != userId) return Forbid();
            }

            var ds = await _db.DanhGiaTours.AsNoTracking()
                .Where(d => d.LichTrinhId == maLichTrinh)
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new DanhGiaTourListItemVM
                {
                    MaDanhGia = d.MaDanhGia,
                    SoSao = d.SoSao,
                    BinhLuan = d.BinhLuan,
                    HinhAnhUrl = d.HinhAnhUrl,
                    NgayTao = d.NgayTao,
                    TenNguoiDung = d.User != null ? (d.User.UserName ?? "User") : "User",
                    IsOwner = d.UserId == userId
                })
                .ToListAsync();

            string? tourId = null;
            var propTourId = lichTrinh.GetType().GetProperty("TourId") ?? lichTrinh.GetType().GetProperty("MaTour");
            if (propTourId != null) tourId = propTourId.GetValue(lichTrinh)?.ToString();

            var tenTour = !string.IsNullOrWhiteSpace(tourId)
                ? (await _db.Tours.AsNoTracking().Where(t => t.MaTour == tourId).Select(t => t.TenTour).FirstOrDefaultAsync()) ?? tourId
                : maLichTrinh;

            var vm = new DanhGiaTourCreateViewModel
            {
                MaLichTrinh = maLichTrinh,
                TenTourHoacLichTrinh = tenTour,
                CanDanhGia = true,
                DanhSachDanhGia = ds
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhGiaTourCreateViewModel vm)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            if (!ModelState.IsValid)
            {
                vm.DanhSachDanhGia = await _db.DanhGiaTours.AsNoTracking()
                    .Where(d => d.LichTrinhId == vm.MaLichTrinh)
                    .OrderByDescending(d => d.NgayDanhGia)
                    .Select(d => new DanhGiaTourListItemVM
                    {
                        MaDanhGia = d.MaDanhGia,
                        SoSao = d.SoSao,
                        BinhLuan = d.BinhLuan,
                        HinhAnhUrl = d.HinhAnhUrl,
                        NgayTao = d.NgayTao,
                        TenNguoiDung = d.User != null ? (d.User.UserName ?? "User") : "User",
                        IsOwner = d.UserId == userId
                    })
                    .ToListAsync();

                return View(vm);
            }

            var lichTrinh = await _db.LichTrinhs.FirstOrDefaultAsync(l => l.MaLichTrinh == vm.MaLichTrinh);
            if (lichTrinh == null) return NotFound();

            string? tourId = null;
            var propTourId = lichTrinh.GetType().GetProperty("TourId") ?? lichTrinh.GetType().GetProperty("MaTour");
            if (propTourId != null) tourId = propTourId.GetValue(lichTrinh)?.ToString();

            string? imgUrl = null;
            if (vm.TepHinhAnh != null && vm.TepHinhAnh.Length > 0)
            {
                imgUrl = await SaveReviewImage(vm.TepHinhAnh);
            }

            var entity = new DanhGiaTour
            {
                LichTrinhId = vm.MaLichTrinh,
                TourId = tourId,
                UserId = userId,
                SoSao = vm.SoSao,
                BinhLuan = vm.BinhLuan,
                HinhAnhUrl = imgUrl,
                NgayDanhGia = DateTime.Now,
                NgayTao = DateTime.Now
            };

            _db.DanhGiaTours.Add(entity);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Đã gửi đánh giá ✅";
            return RedirectToAction(nameof(Create), new { maLichTrinh = vm.MaLichTrinh });
        }

        // =========================
        // USER EDIT
        // View: Edit.cshtml
        // =========================
        [Authorize]
        public async Task<IActionResult> Edit(string maDanhGia)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var dg = await _db.DanhGiaTours.AsNoTracking().FirstOrDefaultAsync(x => x.MaDanhGia == maDanhGia);
            if (dg == null) return NotFound();
            if (dg.UserId != userId) return Forbid();

            var ten = dg.TourId != null
                ? (await _db.Tours.AsNoTracking().Where(t => t.MaTour == dg.TourId).Select(t => t.TenTour).FirstOrDefaultAsync()) ?? dg.TourId
                : dg.LichTrinhId;

            var vm = new DanhGiaTourEditViewModel
            {
                MaDanhGia = dg.MaDanhGia,
                MaLichTrinh = dg.LichTrinhId,
                TenTourHoacLichTrinh = ten,
                SoSao = dg.SoSao,
                BinhLuan = dg.BinhLuan,
                HinhAnhUrl = dg.HinhAnhUrl
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DanhGiaTourEditViewModel vm)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var dg = await _db.DanhGiaTours.FirstOrDefaultAsync(x => x.MaDanhGia == vm.MaDanhGia);
            if (dg == null) return NotFound();
            if (dg.UserId != userId) return Forbid();

            if (!ModelState.IsValid) return View(vm);

            if (vm.TepHinhAnh != null && vm.TepHinhAnh.Length > 0)
            {
                TryDeleteLocalUpload(dg.HinhAnhUrl);
                dg.HinhAnhUrl = await SaveReviewImage(vm.TepHinhAnh);
            }

            dg.SoSao = vm.SoSao;
            dg.BinhLuan = vm.BinhLuan;
            dg.NgayCapNhat = DateTime.Now;
            dg.NgayDanhGia = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["ok"] = "Đã cập nhật đánh giá ✅";
            return RedirectToAction(nameof(Create), new { maLichTrinh = dg.LichTrinhId });
        }

        // =========================
        // USER DELETE
        // View: Delete.cshtml
        // =========================
        [Authorize]
        public async Task<IActionResult> Delete(string maDanhGia)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var dg = await _db.DanhGiaTours.AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.MaDanhGia == maDanhGia);

            if (dg == null) return NotFound();
            if (dg.UserId != userId) return Forbid();

            var vm = new DanhGiaTourItemViewModel
            {
                MaDanhGia = dg.MaDanhGia,
                TenNguoiDung = dg.User?.UserName ?? "User"
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string maDanhGia)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var dg = await _db.DanhGiaTours.FirstOrDefaultAsync(x => x.MaDanhGia == maDanhGia);
            if (dg == null) return NotFound();
            if (dg.UserId != userId) return Forbid();

            var maLichTrinh = dg.LichTrinhId;
            TryDeleteLocalUpload(dg.HinhAnhUrl);

            _db.DanhGiaTours.Remove(dg);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Đã xoá đánh giá ✅";
            return RedirectToAction(nameof(Create), new { maLichTrinh });
        }

        // =========================
        // ADMIN DASHBOARD
        // View: AdminIndex.cshtml (dựa theo screenshot của bạn)
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin(string? tuKhoa, string? trangThai, int? soSao, int trang = 1, int kichThuocTrang = 10)
        {
            var vm = await BuildIndexVM(tuKhoa, trangThai, soSao, trang, kichThuocTrang);
            return View("AdminIndex", vm);
        }

        // =========================
        // ADMIN TOUR DETAILS
        // View: AdminTourDetails.cshtml (dựa theo screenshot của bạn)
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminTourDetails(string maTour, int trang = 1, int kichThuocTrang = 10)
        {
            var vm = await PublicTourDetails(maTour, trang, kichThuocTrang) as ViewResult;
            if (vm?.Model is DanhGiaTourAdminTourDetailsViewModel m)
            {
                foreach (var d in m.DanhSachDanhGia) d.IsOwner = false;
                return View("AdminTourDetails", m);
            }
            return NotFound();
        }

        // =========================
        // HELPERS
        // =========================
        private async Task<DanhGiaTourIndexViewModels> BuildIndexVM(string? tuKhoa, string? trangThai, int? soSao, int trang, int kichThuocTrang)
        {
            trang = trang <= 0 ? 1 : trang;
            kichThuocTrang = kichThuocTrang is < 5 or > 50 ? 10 : kichThuocTrang;
            trangThai ??= "tatca";

            var toursQ = _db.Tours.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim().ToLower();
                toursQ = toursQ.Where(t =>
                    (t.MaTour != null && t.MaTour.ToLower().Contains(k)) ||
                    (t.TenTour != null && t.TenTour.ToLower().Contains(k)) ||
                    (t.DiaDiem != null && t.DiaDiem.ToLower().Contains(k))
                );
            }

            var dgQ = _db.DanhGiaTours.AsNoTracking();

            if (soSao.HasValue && soSao.Value >= 1 && soSao.Value <= 5)
            {
                var s = soSao.Value;
                var tourIds = await dgQ.Where(d => d.SoSao == s && d.TourId != null).Select(d => d.TourId!).Distinct().ToListAsync();
                toursQ = toursQ.Where(t => tourIds.Contains(t.MaTour));
            }

            var joined = from t in toursQ
                         join d in dgQ on t.MaTour equals d.TourId into g
                         select new DanhGiaTourAdminDongVM
                         {
                             MaTour = t.MaTour,
                             TenTour = t.TenTour ?? t.MaTour,
                             DiaDiem = t.DiaDiem,
                             GiaTour = t.GiaTour,
                             SoLuongDanhGia = g.Count(),
                             DiemTrungBinh = g.Any() ? g.Average(x => x.SoSao) : 0,
                             NgayDanhGiaGanNhat = g.Any() ? g.Max(x => x.NgayDanhGia) : (DateTime?)null,
                             SoLuongNamSao = g.Count(x => x.SoSao == 5)
                         };

            if (trangThai == "dadanhgia") joined = joined.Where(x => x.SoLuongDanhGia > 0);
            else if (trangThai == "chuadanhgia") joined = joined.Where(x => x.SoLuongDanhGia == 0);

            var tongDong = await joined.CountAsync();
            var tongTrang = (int)Math.Ceiling(tongDong / (double)kichThuocTrang);
            if (tongTrang <= 0) tongTrang = 1;
            if (trang > tongTrang) trang = tongTrang;

            var list = await joined
                .OrderByDescending(x => x.SoLuongDanhGia)
                .ThenBy(x => x.TenTour)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            var allRatings = await _db.DanhGiaTours.AsNoTracking().ToListAsync();
            var tongDG = allRatings.Count;
            var diemTB = tongDG > 0 ? allRatings.Average(x => x.SoSao) : 0;
            var namSao = allRatings.Count(x => x.SoSao == 5);

            return new DanhGiaTourIndexViewModels
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                SoSaoLoc = soSao,
                Trang = trang,
                KichThuocTrang = kichThuocTrang,
                TongSoDong = tongDong,
                TongSoTrang = tongTrang,
                TongDanhGia = tongDG,
                DiemTrungBinh = Math.Round(diemTB, 2),
                DanhGiaNamSao = namSao,
                DanhSachDong = list
            };
        }

        private async Task<string> SaveReviewImage(IFormFile file)
        {
            var root = Path.Combine(_env.WebRootPath, "uploads", "reviews");
            Directory.CreateDirectory(root);

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

            var name = $"rv_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(root, name);

            using (var fs = new FileStream(full, FileMode.Create))
                await file.CopyToAsync(fs);

            return $"/uploads/reviews/{name}";
        }

        private void TryDeleteLocalUpload(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!url.StartsWith("/uploads/reviews/", StringComparison.OrdinalIgnoreCase)) return;

            var name = url.Replace("/uploads/reviews/", "").Trim();
            var full = Path.Combine(_env.WebRootPath, "uploads", "reviews", name);
            if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
        }
    }
}