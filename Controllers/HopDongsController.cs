// Folder: Controllers
// File path: Controllers/HopDongsController.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using Tour_2040.Data;
using Tour_2040.Models;

namespace Tour_2040.Controllers
{
    [Authorize]
    public class HopDongsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HopDongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminOrStaff =>
            User.Identity != null &&
            User.Identity.IsAuthenticated &&
            (User.IsInRole("Admin") || User.IsInRole("Staff"));

        // ===================== INDEX ======================
        public async Task<IActionResult> Index(string? keyword, string? status)
        {
            var query = _context.HopDongs
                .Include(x => x.Tour)
                .Include(x => x.GiaoDich)
                    .ThenInclude(g => g.Tour)
                .Include(x => x.GiaoDich)
                    .ThenInclude(g => g.User)
                .Include(x => x.GiaoDich)
                    .ThenInclude(g => g.KhachHang)
                .AsQueryable();

            // 1. Permissions: User sees their own contracts
            if (!IsAdminOrStaff)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // Note: HopDong has ApplicationUserId directly now too? Or via GiaoDich?
                // Assuming via GiaoDich based on previous context, but let's be safe
                query = query.Where(h => (h.GiaoDich != null && h.GiaoDich.UserId == userId) || h.ApplicationUserId == userId);
            }

            // 2. Search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(h =>
                    h.MaHopDong.ToLower().Contains(keyword) ||
                    (h.GiaoDich != null && h.GiaoDich.Tour != null && h.GiaoDich.Tour.TenTour.ToLower().Contains(keyword)) ||
                    (h.GiaoDich != null && h.GiaoDich.KhachHang != null && h.GiaoDich.KhachHang.HoTen.ToLower().Contains(keyword))
                );
            }

            // 3. Filter Status
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(h => h.TrangThai == status);
            }

            // 4. Sort
            query = query.OrderByDescending(x => x.NgayTao);

            ViewData["CurrentKeyword"] = keyword;
            ViewData["CurrentStatus"] = status ?? "All";

            return View(await query.ToListAsync());
        }

        // ====================== DETAILS =========================
        public async Task<IActionResult> Details(string id) // Changed int -> string
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var hd = await _context.HopDongs
                .Include(x => x.GiaoDich).ThenInclude(g => g.Tour)
                .Include(x => x.GiaoDich).ThenInclude(g => g.User)
                .Include(x => x.GiaoDich).ThenInclude(g => g.KhachHang)
                .FirstOrDefaultAsync(m => m.MaHopDong == id); // Use MaHopDong string

            if (hd == null) return NotFound();

            if (!IsAdminOrStaff)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if ((hd.GiaoDich == null || hd.GiaoDich.UserId != userId) && hd.ApplicationUserId != userId)
                    return Forbid();
            }

            return View(hd);
        }

        // ====================== EXPORT PDF =========================
        public async Task<IActionResult> DownloadPdf(string id) // Changed int -> string
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var hd = await _context.HopDongs
                 .Include(x => x.GiaoDich).ThenInclude(g => g.Tour)
                 .Include(x => x.GiaoDich).ThenInclude(g => g.User)
                 .Include(x => x.GiaoDich).ThenInclude(g => g.KhachHang)
                 .FirstOrDefaultAsync(m => m.MaHopDong == id);

            if (hd == null) return NotFound();

            if (!IsAdminOrStaff)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if ((hd.GiaoDich == null || hd.GiaoDich.UserId != userId) && hd.ApplicationUserId != userId)
                    return Forbid();
            }

            var fileName = $"HopDong_{hd.MaHopDong}.pdf";

            return new ViewAsPdf("Details", hd)
            {
                FileName = fileName,
                PageSize = Size.A4,
                PageMargins = { Left = 20, Bottom = 20, Right = 20, Top = 20 }
            };
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}