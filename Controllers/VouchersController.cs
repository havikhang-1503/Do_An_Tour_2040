// Folder: Controllers
// File path: Controllers/VouchersController.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Utils;

namespace Tour_2040.Controllers
{
    [Authorize]
    public class VouchersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VouchersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========== ADMIN / STAFF ==========

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Vouchers
                .OrderByDescending(v => v.NgayTao)
                .ToListAsync();
            return View(list);
        }

        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher model)
        {
            if (string.IsNullOrWhiteSpace(model.MaVoucher))
            {
                // If not provided, auto-generate
                model.MaVoucher = MaSoHelper.TaoMa("MVC");
            }
            else
            {
                // Check duplicate
                var code = model.MaVoucher.Trim().ToUpperInvariant();
                if (await _context.Vouchers.AnyAsync(v => v.MaVoucher == code))
                {
                    ModelState.AddModelError(nameof(Voucher.MaVoucher), "Mã voucher này đã tồn tại.");
                }
                model.MaVoucher = code;
            }

            if (model.LoaiGiam != "PhanTram" && model.LoaiGiam != "SoTien")
            {
                ModelState.AddModelError(nameof(Voucher.LoaiGiam), "Loại giảm phải là 'PhanTram' hoặc 'SoTien'.");
            }

            if (!ModelState.IsValid) return View(model);

            model.TrangThaiDuyet = "Chờ duyệt";
            model.IsActive = false;
            model.DaSuDung = 0;
            model.NgayTao = DateTime.Now;

            _context.Vouchers.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id) // Changed int -> string
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            voucher.TrangThaiDuyet = "Đã duyệt";
            voucher.IsActive = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id) // Changed int -> string
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            voucher.IsActive = !voucher.IsActive;

            if (!voucher.IsActive && voucher.TrangThaiDuyet == "Đã duyệt")
            {
                voucher.TrangThaiDuyet = "Tạm khóa";
            }
            else if (voucher.IsActive && voucher.TrangThaiDuyet == "Tạm khóa")
            {
                voucher.TrangThaiDuyet = "Đã duyệt";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ========== USER: AVAILABLE VOUCHERS ==========

        public async Task<IActionResult> Available()
        {
            var now = DateTime.Now;

            var list = await _context.Vouchers
                .Where(v =>
                    v.IsActive &&
                    v.TrangThaiDuyet == "Đã duyệt" &&
                    (!v.NgayBatDau.HasValue || v.NgayBatDau <= now) &&
                    (!v.NgayKetThuc.HasValue || v.NgayKetThuc >= now) &&
                    (!v.SoLuong.HasValue || v.DaSuDung < v.SoLuong.Value))
                .OrderByDescending(v => v.NgayTao)
                .ToListAsync();

            return View(list);
        }

        // ========== USER: MY VOUCHERS ==========

        public async Task<IActionResult> My()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var list = await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .Where(uv => uv.UserId == userId)
                .OrderByDescending(uv => uv.NgayLuu)
                .ToListAsync();

            return View(list);
        }

        // ========== USER: SAVE VOUCHER (BY ID) ==========

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveToMyVoucher(string id) // Changed int -> string
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var now = DateTime.Now;

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v =>
                v.MaVoucher == id &&
                v.IsActive &&
                v.TrangThaiDuyet == "Đã duyệt" &&
                (!v.NgayBatDau.HasValue || v.NgayBatDau <= now) &&
                (!v.NgayKetThuc.HasValue || v.NgayKetThuc >= now) &&
                (!v.SoLuong.HasValue || v.DaSuDung < v.SoLuong.Value));

            if (voucher == null)
            {
                TempData["StatusMessage"] = "Voucher không khả dụng hoặc đã hết lượt.";
                return RedirectToAction(nameof(Available));
            }

            var existed = await _context.UserVouchers
                .AnyAsync(uv => uv.UserId == userId && uv.VoucherId == voucher.MaVoucher && !uv.IsUsed);

            if (!existed)
            {
                var uv = new UserVoucher
                {
                    MaUserVoucher = MaSoHelper.TaoMa("MUV"), // Generate Primary Key
                    UserId = userId,
                    VoucherId = voucher.MaVoucher,
                    NgayLuu = DateTime.Now,
                    IsUsed = false
                };

                _context.UserVouchers.Add(uv);
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = $"Đã lưu voucher {voucher.MaVoucher}!";
            }
            else
            {
                TempData["StatusMessage"] = "Bạn đã có voucher này rồi.";
            }

            return RedirectToAction(nameof(My));
        }

        // ========== USER: SAVE VOUCHER (BY CODE) ==========

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveByCode(string code, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["StatusMessage"] = "Mã voucher không hợp lệ.";
                return SafeRedirect(returnUrl, nameof(Available));
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            code = code.Trim().ToUpperInvariant();
            var now = DateTime.Now;

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v =>
                v.MaVoucher == code &&
                v.IsActive &&
                v.TrangThaiDuyet == "Đã duyệt" &&
                (!v.NgayBatDau.HasValue || v.NgayBatDau <= now) &&
                (!v.NgayKetThuc.HasValue || v.NgayKetThuc >= now) &&
                (!v.SoLuong.HasValue || v.DaSuDung < v.SoLuong.Value));

            if (voucher == null)
            {
                TempData["StatusMessage"] = "Voucher không tồn tại hoặc hết hạn.";
                return SafeRedirect(returnUrl, nameof(Available));
            }

            var existed = await _context.UserVouchers
                .AnyAsync(uv => uv.UserId == userId && uv.VoucherId == voucher.MaVoucher && !uv.IsUsed);

            if (!existed)
            {
                var uv = new UserVoucher
                {
                    MaUserVoucher = MaSoHelper.TaoMa("MUV"),
                    UserId = userId,
                    VoucherId = voucher.MaVoucher,
                    NgayLuu = DateTime.Now,
                    IsUsed = false
                };

                _context.UserVouchers.Add(uv);
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = $"Đã lưu mã {code}!";
            }
            else
            {
                TempData["StatusMessage"] = "Bạn đã lưu mã này rồi.";
            }

            return SafeRedirect(returnUrl, nameof(Available));
        }

        private IActionResult SafeRedirect(string? returnUrl, string defaultAction)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(defaultAction);
        }
    }
}