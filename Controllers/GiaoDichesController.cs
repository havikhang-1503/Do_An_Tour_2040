//// Folder: Controllers
// File path: Controllers/GiaoDichesController.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Utils; // MaSoHelper

namespace Tour_2040.Controllers
{
    [Authorize]
    public class GiaoDichesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiaoDichesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminOrStaff =>
            User.Identity != null &&
            User.Identity.IsAuthenticated &&
            (User.IsInRole("Admin") || User.IsInRole("Staff"));

        // ======================= INDEX =======================
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.GiaoDiches
                .Include(x => x.Tour)
                .Include(x => x.User)
                .Include(x => x.ThanhToans)
                .Include(x => x.HopDongs)
                .AsQueryable();

            if (!IsAdminOrStaff)
            {
                query = query.Where(x => x.UserId == userId);
            }

            var list = await query
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(list);
        }

        // ======================= DETAILS =======================
        // GET: GiaoDiches/Details?maGd=...
        // ======================= DETAILS (FULL) =======================
        public async Task<IActionResult> Details(string maGd)
        {
            if (string.IsNullOrWhiteSpace(maGd)) return NotFound();

            var record = await _context.GiaoDiches
                .Include(x => x.Tour)
                    .ThenInclude(t => t.TourDichVus)
                        .ThenInclude(td => td.DichVu)
                .Include(x => x.User)
                //.Include(x => x.UserVoucher).ThenInclude(uv => uv.Voucher) // Mở dòng này nếu bạn có quan hệ Voucher
                .Include(x => x.ThanhToans)
                .Include(x => x.HopDongs)
                .Include(x => x.LichTrinhs) // <--- QUAN TRỌNG: Để lấy mã lịch trình cho nút chuyển trang
                .FirstOrDefaultAsync(x => x.MaGiaoDich == maGd);

            if (record == null) return NotFound();

            // Check quyền xem
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (record.UserId != userId) return Forbid();
            }

            // Sắp xếp dữ liệu hiển thị
            record.ThanhToans = record.ThanhToans.OrderByDescending(t => t.NgayThanhToan).ToList();

            return View(record);
        }

        // ======================= SEED DEMO =======================
        [Authorize]
        public async Task<IActionResult> SeedDemo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Forbid();

            var tour = await _context.Tours.FirstOrDefaultAsync();

            var maGd = MaSoHelper.TaoMa("MGD");

            var giaoDich = new GiaoDich
            {
                MaGiaoDich = maGd,
                UserId = userId,
                TourId = tour?.MaTour, // Use string MaTour
                NgayTao = DateTime.Now,
                TongTien = 2_000_000,
                TrangThai = "Hoàn thành (demo)",
                GhiChu = "Giao dịch demo"
            };

            _context.GiaoDiches.Add(giaoDich);
            await _context.SaveChangesAsync();

            var thanhToan = new ThanhToan
            {
                MaThanhToan = MaSoHelper.TaoMa("MTT"),

                // --- SỬA LỖI TẠI ĐÂY ---
                // Xóa dòng: MaGiaoDich = giaoDich.MaGiaoDich, (Vì thuộc tính này không còn trong Model ThanhToan)

                GiaoDichId = giaoDich.MaGiaoDich, // Chỉ giữ lại dòng này là đủ

                SoTien = 2_000_000,
                NgayThanhToan = DateTime.Now,
                PhuongThuc = "Chuyển khoản",
                TenTaiKhoan = "Khách demo",
                SoTaiKhoan = "123456789",
                GhiChu = "Thanh toán full tiền tour (demo)",
                TrangThai = "Thành công"
            };

            _context.ThanhToans.Add(thanhToan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { maGd = giaoDich.MaGiaoDich });
        }
    }
}
