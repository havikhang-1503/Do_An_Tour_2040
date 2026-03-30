using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Utils;

namespace Tour_2040.Controllers
{
    [Authorize]
    public class YeuCauHoTrosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

        // Constructor now only needs DB context and UserManager
        public YeuCauHoTrosController(
            ApplicationDbContext context,
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================== PHẦN 1: DASHBOARD & THỐNG KÊ ==================
        public async Task<IActionResult> Index(string? statusFilter, string? searchString)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Staff");

            var query = _context.YeuCauHoTros.Include(x => x.User).AsQueryable();

            // 1. Phân quyền
            if (!isAdmin) query = query.Where(x => x.UserId == uid);

            // 2. Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x =>
                    x.MaYeuCauHoTro.Contains(searchString) ||
                    x.ChuDe.Contains(searchString) ||
                    (x.User.HoTen != null && x.User.HoTen.Contains(searchString))
                );
            }

            // 3. Lọc trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "ChoXuLy") query = query.Where(x => x.TrangThai == "Chờ xử lý");
                else if (statusFilter == "DangXuLy") query = query.Where(x => x.TrangThai == "Đang xử lý");
                else if (statusFilter == "DaPhanHoi") query = query.Where(x => x.TrangThai == "Đã phản hồi");
            }

            var data = await query.OrderByDescending(x => x.NgayGui).ToListAsync();

            // 4. Dữ liệu cho Biểu đồ (Chỉ Admin thấy)
            if (isAdmin)
            {
                ViewBag.ChartLabels = new[] { "Chờ xử lý", "Đang xử lý", "Hoàn thành" };
                ViewBag.ChartData = new[] {
                    data.Count(x => x.TrangThai == "Chờ xử lý"),
                    data.Count(x => x.TrangThai == "Đang xử lý"),
                    data.Count(x => x.TrangThai == "Đã phản hồi")
                };
            }
            ViewData["CurrentFilter"] = searchString;
            return View(data);
        }

        // The AskAi method is removed from here

        // ================== PHẦN 3: TẠO & XỬ LÝ PHIẾU (NHƯ CŨ) ==================
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(YeuCauHoTro model)
        {
            ModelState.Remove("MaYeuCauHoTro"); ModelState.Remove("UserId");
            ModelState.Remove("TrangThai"); ModelState.Remove("User"); ModelState.Remove("TinNhanHoTros");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                model.UserId = user.Id;
                model.MaYeuCauHoTro = MaSoHelper.TaoMa("YC");
                model.NgayGui = DateTime.Now;
                model.TrangThai = "Chờ xử lý";

                var firstMsg = new TinNhanHoTro
                {
                    MaYeuCauHoTro = model.MaYeuCauHoTro,
                    NoiDung = model.NoiDung,
                    NguoiGui = user.HoTen ?? "Khách",
                    LaNhanVien = false,
                    ThoiGian = DateTime.Now
                };

                _context.YeuCauHoTros.Add(model);
                _context.TinNhanHoTros.Add(firstMsg);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Chat(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Staff");

            var request = await _context.YeuCauHoTros.Include(x => x.User).FirstOrDefaultAsync(x => x.MaYeuCauHoTro == id);
            if (request == null) return NotFound();
            if (!isAdmin && request.UserId != uid) return Forbid();

            ViewBag.Messages = await _context.TinNhanHoTros.Where(x => x.MaYeuCauHoTro == id).OrderBy(x => x.ThoiGian).ToListAsync();
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string id, string messageContent)
        {
            if (string.IsNullOrWhiteSpace(messageContent)) return RedirectToAction("Chat", new { id });
            var request = await _context.YeuCauHoTros.FindAsync(id);
            if (request == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Staff");

            var msg = new TinNhanHoTro
            {
                MaYeuCauHoTro = id,
                NoiDung = messageContent,
                ThoiGian = DateTime.Now,
                LaNhanVien = isStaff,
                NguoiGui = isStaff ? $"Support ({user.HoTen})" : user.HoTen
            };

            request.TrangThai = isStaff ? "Đã phản hồi" : "Chờ xử lý";
            _context.TinNhanHoTros.Add(msg);
            await _context.SaveChangesAsync();
            return RedirectToAction("Chat", new { id });
        }
    }
}