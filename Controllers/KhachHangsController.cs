// Folder: Controllers
// File path: Controllers/KhachHangsController.cs
// File name: KhachHangsController.cs
// Class: KhachHangsController
// Labels: A(Fix Details load bookings), B(Dynamic FK detect), C(Fallback by phone), D(KPI), E(CRUD stable)
// FIX NOTES:
// - Details now loads bookings from GiaoDiches for the selected customer.
// - Auto-detect FK column name: KhachHangId or MaKhachHang.
// - Fallback by phone if FK is missing (common in old code).
// - Exposes ViewBag.Bookings / TotalSpend / TotalTrips / HienTrang for Details view.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Utils;
using Microsoft.AspNetCore.Identity;

namespace Tour_2040.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class KhachHangsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public KhachHangsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================== HELPER: ĐỒNG BỘ USER SANG KHÁCH HÀNG ==================
        private async Task SyncUsersToCustomers()
        {
            var existingEmails = await _context.KhachHangs
                .Where(k => k.Email != null && k.Email != "")
                .Select(k => k.Email!)
                .ToListAsync();

            var newUsers = await _context.Users
                .Where(u => u.Email != null && u.Email != "" && !existingEmails.Contains(u.Email))
                .ToListAsync();

            if (!newUsers.Any()) return;

            foreach (var user in newUsers)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
                if (isAdmin || isStaff) continue;

                var kh = new KhachHang
                {
                    MaKhachHang = MaSoHelper.TaoMa("KH"),
                    HoTen = user.HoTen ?? user.UserName ?? "Khách hàng mới",
                    Email = user.Email,
                    SoDienThoai = user.PhoneNumber,
                    NgayTao = DateTime.Now,
                    NhomKhach = "Thường",
                    TrangThaiTour = "Chưa đặt tour",
                    IsVip = false
                };

                _context.KhachHangs.Add(kh);
            }

            await _context.SaveChangesAsync();
        }

        // ================== LIST ==================
        public async Task<IActionResult> Index(string? keyword, string? nhomKhach, string? trangThaiTour)
        {
            await SyncUsersToCustomers();

            var query = _context.KhachHangs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.HoTen != null && x.HoTen.ToLower().Contains(k)) ||
                    (x.MaKhachHang != null && x.MaKhachHang.ToLower().Contains(k)) ||
                    (x.Email != null && x.Email.ToLower().Contains(k)) ||
                    (x.SoDienThoai != null && x.SoDienThoai.Contains(k))
                );
            }

            if (!string.IsNullOrEmpty(nhomKhach) && nhomKhach != "All")
                query = query.Where(x => x.NhomKhach == nhomKhach);

            if (!string.IsNullOrEmpty(trangThaiTour) && trangThaiTour != "All")
                query = query.Where(x => x.TrangThaiTour == trangThaiTour);

            var list = await query.OrderByDescending(x => x.NgayTao).ToListAsync();

            ViewData["Keyword"] = keyword ?? "";
            ViewData["NhomKhach"] = nhomKhach ?? "All";
            ViewData["TrangThaiTour"] = trangThaiTour ?? "All";

            return View(list);
        }

        // ================== DETAILS (✅ FIX: LOAD BOOKINGS + KPI) ==================
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var kh = await _context.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaKhachHang == id);

            if (kh == null) return NotFound();

            // ---- Detect property names in GiaoDich entity safely ----
            var gdEntity = _context.Model.FindEntityType(typeof(GiaoDich));
            bool hasKhachHangId = gdEntity?.FindProperty("KhachHangId") != null;
            bool hasMaKhachHang = gdEntity?.FindProperty("MaKhachHang") != null;
            bool hasSoDienThoai = gdEntity?.FindProperty("SoDienThoai") != null;
            bool hasNgayTao = gdEntity?.FindProperty("NgayTao") != null;

            IQueryable<GiaoDich> baseQuery = _context.GiaoDiches
                .AsNoTracking()
                .Include(g => g.Tour)
                .Include(g => g.ThanhToans);

            IQueryable<GiaoDich> q;

            // 1) Prefer FK match (best)
            if (hasKhachHangId)
            {
                q = baseQuery.Where(g => EF.Property<string>(g, "KhachHangId") == kh.MaKhachHang);
            }
            else if (hasMaKhachHang)
            {
                q = baseQuery.Where(g => EF.Property<string>(g, "MaKhachHang") == kh.MaKhachHang);
            }
            else
            {
                q = baseQuery.Where(g => false);
            }

            // 2) If FK yields none => fallback by phone (common legacy data)
            var anyByFk = await q.AnyAsync();
            if (!anyByFk && hasSoDienThoai)
            {
                var phoneRaw = (kh.SoDienThoai ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(phoneRaw))
                {
                    // build variants: "0xxx", "+84xxx", "84xxx" (basic normalize)
                    string p = phoneRaw.Replace(" ", "").Replace(".", "").Replace("-", "");
                    var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { p };

                    if (p.StartsWith("+84"))
                    {
                        variants.Add("0" + p.Substring(3));
                        variants.Add("84" + p.Substring(3));
                    }
                    else if (p.StartsWith("84"))
                    {
                        variants.Add("0" + p.Substring(2));
                        variants.Add("+84" + p.Substring(2));
                    }
                    else if (p.StartsWith("0"))
                    {
                        variants.Add("+84" + p.Substring(1));
                        variants.Add("84" + p.Substring(1));
                    }

                    q = baseQuery.Where(g => variants.Contains(EF.Property<string>(g, "SoDienThoai")));
                }
            }

            // 3) Order
            if (hasNgayTao)
                q = q.OrderByDescending(g => EF.Property<DateTime>(g, "NgayTao"));
            else
                q = q.OrderByDescending(g => g.MaGiaoDich);

            var bookings = await q.ToListAsync();

            // ---- KPI ----
            var valid = bookings.Where(x => x.TrangThai != "Huy").ToList();
            decimal totalSpend = valid.Sum(x => x.TongTien);
            int totalTrips = valid.Count;

            // “Hiện trạng” đơn giản (em muốn rule khác thì đổi)
            string hienTrang = totalSpend >= 20000000 ? "VIP"
                            : totalSpend >= 10000000 ? "Thân thiết"
                            : "Thường";

            ViewBag.Bookings = bookings;
            ViewBag.TotalSpend = totalSpend;
            ViewBag.TotalTrips = totalTrips;
            ViewBag.HienTrang = hienTrang;

            return View(kh);
        }

        // ================== CREATE ==================
        public IActionResult Create()
        {
            var model = new KhachHang
            {
                MaKhachHang = MaSoHelper.TaoMa("KH"),
                NgayTao = DateTime.Now,
                NhomKhach = "Thường",
                TrangThaiTour = "Chưa đặt tour",
                IsVip = false
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHang khachHang)
        {
            if (string.IsNullOrWhiteSpace(khachHang.MaKhachHang))
                khachHang.MaKhachHang = MaSoHelper.TaoMa("KH");

            if (khachHang.NgayTao == default)
                khachHang.NgayTao = DateTime.Now;

            if (!ModelState.IsValid)
                return View(khachHang);

            if (await _context.KhachHangs.AnyAsync(k => k.MaKhachHang == khachHang.MaKhachHang))
            {
                ModelState.AddModelError("MaKhachHang", "Mã khách hàng này đã tồn tại.");
                return View(khachHang);
            }

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================== EDIT ==================
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var kh = await _context.KhachHangs.FirstOrDefaultAsync(x => x.MaKhachHang == id);
            if (kh == null) return NotFound();

            return View(kh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, KhachHang khachHang)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            if (id != khachHang.MaKhachHang) return NotFound();

            if (!ModelState.IsValid)
                return View(khachHang);

            var existing = await _context.KhachHangs.FirstOrDefaultAsync(x => x.MaKhachHang == id);
            if (existing == null) return NotFound();

            existing.HoTen = khachHang.HoTen;
            existing.CCCD = khachHang.CCCD;
            existing.Email = khachHang.Email;
            existing.SoDienThoai = khachHang.SoDienThoai;
            existing.DiaChi = khachHang.DiaChi;
            existing.NgaySinh = khachHang.NgaySinh;

            existing.NhomKhach = khachHang.NhomKhach;
            existing.TrangThaiTour = khachHang.TrangThaiTour;
            existing.IsVip = khachHang.IsVip;
            existing.GhiChu = khachHang.GhiChu;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================== DELETE ==================
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var kh = await _context.KhachHangs.FirstOrDefaultAsync(x => x.MaKhachHang == id);
            if (kh == null) return NotFound();

            return View(kh);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var kh = await _context.KhachHangs.FirstOrDefaultAsync(x => x.MaKhachHang == id);
            if (kh == null) return RedirectToAction(nameof(Index));

            // Chặn xóa nếu có giao dịch (bắt cả 2 kiểu FK phổ biến)
            var gdEntity = _context.Model.FindEntityType(typeof(GiaoDich));
            bool hasKhachHangId = gdEntity?.FindProperty("KhachHangId") != null;
            bool hasMaKhachHang = gdEntity?.FindProperty("MaKhachHang") != null;

            IQueryable<GiaoDich> gdq = _context.GiaoDiches.AsNoTracking();
            bool hasDependents = false;

            if (hasKhachHangId)
                hasDependents = await gdq.AnyAsync(g => EF.Property<string>(g, "KhachHangId") == kh.MaKhachHang);
            else if (hasMaKhachHang)
                hasDependents = await gdq.AnyAsync(g => EF.Property<string>(g, "MaKhachHang") == kh.MaKhachHang);

            if (hasDependents)
            {
                TempData["Error"] = "Không thể xóa khách hàng đã có giao dịch.";
                return RedirectToAction(nameof(Index));
            }

            _context.KhachHangs.Remove(kh);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
