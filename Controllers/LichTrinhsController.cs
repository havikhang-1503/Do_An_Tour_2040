// Folder: Controllers
// File path: Controllers/LichTrinhsController.cs

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Models;

// ⚠️ Nếu DbContext của em tên khác (vd: TourDbContext), đổi lại ở đây:
using Tour_2040.Data;

namespace Tour_2040.Controllers
{
    [Authorize]
    public class LichTrinhsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] AllowedStatuses = new[]
        {
            "Chờ xác nhận",
            "Đã xác nhận",
            "Đang khởi hành",
            "Hoàn thành",
            "Đã hủy"
        };

        public LichTrinhsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsAdminOrStaff() => User.IsInRole("Admin") || User.IsInRole("Staff");

        private static bool CanUserCancel(LichTrinh lt)
        {
            var s = (lt.TrangThai ?? "").Trim();
            return s == "Chờ xác nhận" || s == "Đã xác nhận";
        }

        private static bool CanUserRate(LichTrinh lt)
        {
            var s = (lt.TrangThai ?? "").Trim();
            return s == "Hoàn thành";
        }

        // =========================
        // USER: Danh sách lịch trình của tôi
        // =========================
        public async Task<IActionResult> Index(string? keyword)
        {
            if (IsAdminOrStaff())
                return RedirectToAction(nameof(AdminIndex));

            var userId = _userManager.GetUserId(User);

            var q = _context.LichTrinhs
                .Include(x => x.Tour)
                .Where(x => x.UserId == userId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(x =>
                    x.MaLichTrinh.Contains(keyword) ||
                    (x.Tour != null && EF.Functions.Like(x.Tour.TenTour!, $"%{keyword}%"))
                );
            }

            ViewData["Keyword"] = keyword ?? "";
            var data = await q.AsNoTracking()
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();

            return View(data);
        }

        // =========================
        // USER: Chi tiết lịch trình (chỉ của mình)
        // =========================
        public async Task<IActionResult> Details(string id)
        {
            if (IsAdminOrStaff())
                return RedirectToAction(nameof(AdminDetails), new { id });

            var userId = _userManager.GetUserId(User);

            var lt = await _context.LichTrinhs
                .Include(x => x.Tour)
                .Include(x => x.LichTrinhChiTiets)
                    .ThenInclude(ct => ct.DiaDiem)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLichTrinh == id && x.UserId == userId);

            if (lt == null) return NotFound();

            ViewData["CanCancel"] = CanUserCancel(lt);

            // Check đã đánh giá chưa (dùng EF.Property để đỡ phụ thuộc tên property cụ thể của DanhGiaTour)
            var hasRated = await _context.Set<DanhGiaTour>().AnyAsync(d =>
                EF.Property<string>(d, "TourId") == lt.TourId &&
                EF.Property<string>(d, "UserId") == userId
            );

            ViewData["CanRate"] = CanUserRate(lt) && !hasRated;
            ViewData["HasRated"] = hasRated;

            return View(lt);
        }

        // =========================
        // USER: Hủy lịch trình
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id, string? reason)
        {
            var userId = _userManager.GetUserId(User);

            var lt = await _context.LichTrinhs
                .FirstOrDefaultAsync(x => x.MaLichTrinh == id && x.UserId == userId);

            if (lt == null) return NotFound();

            if (!CanUserCancel(lt))
            {
                TempData["Error"] = "Không thể hủy lịch trình ở trạng thái hiện tại.";
                return RedirectToAction(nameof(Details), new { id });
            }

            lt.TrangThai = "Đã hủy";
            lt.NgayHuy = DateTime.Now;
            lt.LyDoHuy = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            lt.GhiChuTrangThai = string.IsNullOrWhiteSpace(lt.GhiChuTrangThai)
                ? "Khách hủy lịch"
                : lt.GhiChuTrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã hủy lịch trình.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // =========================
        // USER: Đánh giá tour (sau khi hoàn thành)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(string id, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating phải từ 1 đến 5 sao.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var userId = _userManager.GetUserId(User);

            var lt = await _context.LichTrinhs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLichTrinh == id && x.UserId == userId);

            if (lt == null) return NotFound();

            if (!CanUserRate(lt))
            {
                TempData["Error"] = "Chỉ được đánh giá sau khi tour hoàn thành.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Chặn đánh giá trùng
            var existed = await _context.Set<DanhGiaTour>().AnyAsync(d =>
                EF.Property<string>(d, "TourId") == lt.TourId &&
                EF.Property<string>(d, "UserId") == userId
            );

            if (existed)
            {
                TempData["Error"] = "Bạn đã đánh giá tour này rồi.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Tạo mới DanhGiaTour bằng reflection để giảm phụ thuộc vào tên property cụ thể
            var dg = Activator.CreateInstance<DanhGiaTour>();
            if (dg == null)
            {
                TempData["Error"] = "Không tạo được đối tượng đánh giá (DanhGiaTour).";
                return RedirectToAction(nameof(Details), new { id });
            }

            SetIfExists(dg, "TourId", lt.TourId);
            SetIfExists(dg, "UserId", userId);
            SetIfExists(dg, "SoSao", rating);
            SetIfExists(dg, "NhanXet", string.IsNullOrWhiteSpace(comment) ? null : comment.Trim());
            SetIfExists(dg, "NgayDanhGia", DateTime.Now);
            SetIfExists(dg, "LichTrinhId", lt.MaLichTrinh);

            _context.Add(dg);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction(nameof(Details), new { id });
        }

        private static void SetIfExists(object obj, string prop, object? value)
        {
            var p = obj.GetType().GetProperty(prop);
            if (p == null || !p.CanWrite) return;

            // Convert nếu cần
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            object? v = value;

            if (value != null && !t.IsAssignableFrom(value.GetType()))
            {
                v = Convert.ChangeType(value, t);
            }

            p.SetValue(obj, v);
        }

        // =========================
        // ADMIN/STAFF: Danh sách tất cả lịch trình + filter
        // =========================
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AdminIndex(
            string? keyword,
            string? status,
            string? tourId,
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            var q = _context.LichTrinhs
                .Include(x => x.Tour)
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(x =>
                    x.MaLichTrinh.Contains(keyword) ||
                    (x.UserId != null && x.UserId.Contains(keyword)) ||
                    (x.User != null && (x.User.Email!.Contains(keyword) || x.User.UserName!.Contains(keyword))) ||
                    (x.Tour != null && EF.Functions.Like(x.Tour.TenTour!, $"%{keyword}%"))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();
                q = q.Where(x => x.TrangThai == status);
            }

            if (!string.IsNullOrWhiteSpace(tourId))
            {
                tourId = tourId.Trim();
                q = q.Where(x => x.TourId == tourId);
            }

            if (fromDate.HasValue)
                q = q.Where(x => x.NgayDat.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                q = q.Where(x => x.NgayDat.Date <= toDate.Value.Date);

            ViewData["Keyword"] = keyword ?? "";
            ViewData["Status"] = status ?? "";
            ViewData["TourId"] = tourId ?? "";
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd") ?? "";
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd") ?? "";
            ViewData["AllowedStatuses"] = AllowedStatuses;

            var data = await q.AsNoTracking()
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();

            return View(data);
        }

        // =========================
        // ADMIN/STAFF: Chi tiết lịch trình (tất cả)
        // =========================
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AdminDetails(string id)
        {
            var lt = await _context.LichTrinhs
                .Include(x => x.Tour)
                .Include(x => x.User)
                .Include(x => x.LichTrinhChiTiets)
                    .ThenInclude(ct => ct.DiaDiem)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLichTrinh == id);

            if (lt == null) return NotFound();

            ViewData["AllowedStatuses"] = AllowedStatuses;
            return View(lt);
        }

        // =========================
        // ADMIN/STAFF: Cập nhật trạng thái + ghi chú
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, string status, string? note)
        {
            status = (status ?? "").Trim();

            if (!AllowedStatuses.Contains(status))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(AdminDetails), new { id });
            }

            var lt = await _context.LichTrinhs.FirstOrDefaultAsync(x => x.MaLichTrinh == id);
            if (lt == null) return NotFound();

            lt.TrangThai = status;
            lt.GhiChuTrangThai = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

            // Nếu set hủy từ admin
            if (status == "Đã hủy" && lt.NgayHuy == null)
                lt.NgayHuy = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật trạng thái.";
            return RedirectToAction(nameof(AdminDetails), new { id });
        }

        // =========================
        // ADMIN/STAFF: Export CSV danh sách khách/lịch trình
        // =========================
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ExportCsv(
            string? keyword,
            string? status,
            string? tourId,
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            var q = _context.LichTrinhs
                .Include(x => x.Tour)
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(x =>
                    x.MaLichTrinh.Contains(keyword) ||
                    (x.User != null && (x.User.Email!.Contains(keyword) || x.User.UserName!.Contains(keyword))) ||
                    (x.Tour != null && EF.Functions.Like(x.Tour.TenTour!, $"%{keyword}%"))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();
                q = q.Where(x => x.TrangThai == status);
            }

            if (!string.IsNullOrWhiteSpace(tourId))
            {
                tourId = tourId.Trim();
                q = q.Where(x => x.TourId == tourId);
            }

            if (fromDate.HasValue)
                q = q.Where(x => x.NgayDat.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                q = q.Where(x => x.NgayDat.Date <= toDate.Value.Date);

            var data = await q.AsNoTracking()
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();

            static string Esc(string? s)
            {
                s ??= "";
                if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                    return $"\"{s.Replace("\"", "\"\"")}\"";
                return s;
            }

            var sb = new StringBuilder();
            sb.AppendLine("MaLichTrinh,TourId,TenTour,UserEmail,NgayDat,SoNguoiLon,SoTreEm,TongTien,TrangThai,GhiChuTrangThai,NgayHuy,LyDoHuy");

            foreach (var x in data)
            {
                sb.AppendLine(string.Join(",",
                    Esc(x.MaLichTrinh),
                    Esc(x.TourId),
                    Esc(x.Tour?.TenTour),
                    Esc(x.User?.Email ?? x.User?.UserName ?? x.UserId),
                    Esc(x.NgayDat.ToString("yyyy-MM-dd HH:mm")),
                    Esc(x.SoNguoiLon.ToString()),
                    Esc(x.SoTreEm.ToString()),
                    Esc(x.TongTien.ToString("0.00")),
                    Esc(x.TrangThai),
                    Esc(x.GhiChuTrangThai),
                    Esc(x.NgayHuy?.ToString("yyyy-MM-dd HH:mm")),
                    Esc(x.LyDoHuy)
                ));
            }

            var bytes = Encoding.UTF8.GetBytes("\uFEFF" + sb.ToString());
            var fileName = $"lich-trinh_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // =========================
        // ADMIN/STAFF: Dashboard KPI (8 chỉ số)
        // =========================
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var total = await _context.LichTrinhs.CountAsync();
            var pending = await _context.LichTrinhs.CountAsync(x => x.TrangThai == "Chờ xác nhận");
            var confirmed = await _context.LichTrinhs.CountAsync(x => x.TrangThai == "Đã xác nhận");
            var inProgress = await _context.LichTrinhs.CountAsync(x => x.TrangThai == "Đang khởi hành");
            var completed = await _context.LichTrinhs.CountAsync(x => x.TrangThai == "Hoàn thành");
            var canceled = await _context.LichTrinhs.CountAsync(x => x.TrangThai == "Đã hủy");

            var revenueMonth = await _context.LichTrinhs
                .Where(x => x.NgayDat >= monthStart && x.TrangThai != "Đã hủy")
                .SumAsync(x => (decimal?)x.TongTien) ?? 0m;

            // Avg rating (nếu DanhGiaTour có property SoSao)
            double avgRating = 0;
            try
            {
                avgRating = await _context.Set<DanhGiaTour>()
                    .Select(d => (double?)EF.Property<int>(d, "SoSao"))
                    .AverageAsync() ?? 0;
            }
            catch
            {
                avgRating = 0; // nếu model khác tên cột thì thôi, không phá dashboard
            }

            ViewData["Total"] = total;
            ViewData["Pending"] = pending;
            ViewData["Confirmed"] = confirmed;
            ViewData["InProgress"] = inProgress;
            ViewData["Completed"] = completed;
            ViewData["Canceled"] = canceled;
            ViewData["RevenueMonth"] = revenueMonth;
            ViewData["AvgRating"] = avgRating;

            var recent = await _context.LichTrinhs
                .Include(x => x.Tour)
                .Include(x => x.User)
                .OrderByDescending(x => x.NgayDat)
                .Take(10)
                .AsNoTracking()
                .ToListAsync();

            return View(recent);
        }
    }
}
