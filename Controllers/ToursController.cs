using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// ================== (NEW) using cho timeline JSON ==================
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Models.ViewModels;
using Tour_2040.Utils;
using Tour_2040.Services; // Added

namespace Tour_2040.Controllers
{
    [Authorize]
    public class ToursController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ToursController> _logger;
        private readonly ISupportAiService _aiService; // New Field

        public ToursController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            ILogger<ToursController> logger,
            ISupportAiService aiService) // New Parameter
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _logger = logger;
            _aiService = aiService;
        }

        #region ================== 1. HELPER METHODS (XỬ LÝ FILE & LOGIC PHỤ) ==================
        // ✅ [1.1] Kiểm tra Admin hoặc Staff
        private bool IsAdminOrStaff =>
            User.Identity != null && User.Identity.IsAuthenticated &&
            (User.IsInRole("Admin") || User.IsInRole("Staff"));

        // ✅ [1.2] Kiểm tra User đã xác thực
        private bool isAuthenticated() => User.Identity?.IsAuthenticated ?? false;

        // ✅ [1.3] Tạo mã tour
        private string GenerateMaTour(string prefix)
        {
            return MaSoHelper.TaoMa(prefix);
        }

        // ✅ [1.4] Upload File (CCCD, Ảnh Tour)
        private async Task<string?> UploadFile(IFormFile? file, string folderName)
        {
            if (file == null || file.Length == 0) return null;
            try
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", folderName);
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{folderName}-{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/{folderName}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload file: {ex.Message}");
                return null;
            }
        }

        // ✅ [1.5] Xóa Ảnh Cũ
        private void DeleteOldImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            try
            {
                var localPath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(localPath)) System.IO.File.Delete(localPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Không thể xóa ảnh: {ex.Message}");
            }
        }

        // ✅ [1.6] Load Dữ Liệu cho Create/Edit (Tỉnh, Xã, Địa điểm, Dịch vụ)
        private async Task LoadTourCreateDataAsync(TourCreateViewModel vm)
        {
            var tinhThanhList = await _context.TinhThanhs
                .OrderBy(t => t.TenTinh)
                .ToListAsync();
            ViewData["TinhThanhList"] = new SelectList(tinhThanhList, "MaTinhThanh", "TenTinh");

            var diaDiems = await _context.DiaDiems
                .Include(d => d.XaPhuong)
                    .ThenInclude(xp => xp!.TinhThanh)
                .OrderBy(d => d.TenDiaDiem)
                .ToListAsync();
            ViewData["DiaDiemList"] = new SelectList(diaDiems, "TenDiaDiem", "TenDiaDiem");

            vm.AvailableDichVus = await _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => d.TrangThai == "Active")
                .OrderBy(d => d.LoaiDichVu)
                .ThenBy(d => d.TenDichVu)
                .ToListAsync();

            ViewBag.DichVuList = vm.AvailableDichVus;
        }

        // ✅ [1.7] Lọc Dịch Vụ theo Điểm Đến (FIX: Không sử dụng ?? trong LINQ)
        private async Task<List<string>> FilterSelectedDichVuIdsByDestinationAsync(string? tenDiaDiem, List<string> selectedIds)
        {
            // Fix: Không dùng ?? trong expression tree
            if (selectedIds == null)
                selectedIds = new List<string>();

            selectedIds = selectedIds.Where(x => !string.IsNullOrWhiteSpace(x))
                                     .Select(x => x.Trim())
                                     .Distinct()
                                     .ToList();

            if (!selectedIds.Any()) return selectedIds;
            if (string.IsNullOrWhiteSpace(tenDiaDiem)) return selectedIds;

            var allowed = await _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => d.TrangThai == "Active" &&
                            (d.DiaDiem != null && d.DiaDiem.TenDiaDiem == tenDiaDiem ||
                             d.MaDiaDiem == null))
                .Select(d => d.MaTenDichVu)
                .ToListAsync();

            return selectedIds.Intersect(allowed).Distinct().ToList();
        }

        // ✅ [1.8] Hợp nhất các ID dịch vụ được chọn
        private List<string> GetSelectedDichVuIdsUnified(TourCreateViewModel model)
        {
            var a = model.SelectedDichVuIds ?? new List<string>();
            var b = model.SelectedServiceIds ?? new List<string>();
            return a.Concat(b).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList();
        }

        // ✅ [1.9] Kiểm tra tour có tồn tại
        private bool TourExists(string id)
        {
            return _context.Tours.Any(e => e.MaTour == id);
        }

        // ================== (NEW) 1.10 - COMBINE DATE + HH:mm ==================
        private static DateTime? CombineDateAndTime(DateTime? date, string? hhmm)
        {
            if (!date.HasValue) return null;
            if (string.IsNullOrWhiteSpace(hhmm)) return date.Value;

            if (TimeSpan.TryParse(hhmm.Trim(), out var ts))
                return date.Value.Date.Add(ts);

            return date.Value;
        }

        // ================== (NEW) 1.11 - ITINERARY JSON -> TEXT (ĐỔ VÀO MoTa) ==================
        private static string BuildMoTaFromItineraryJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;

            try
            {
                var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonSerializer.Deserialize<ItineraryDoc>(json, opt);
                if (doc?.Days == null || doc.Days.Count == 0) return string.Empty;

                var sb = new StringBuilder();
                sb.AppendLine("LỊCH TRÌNH CHI TIẾT");
                sb.AppendLine("----------------------------------------");

                foreach (var day in doc.Days.OrderBy(d => d.DayIndex))
                {
                    var dateText = day.Date;
                    if (DateTime.TryParse(day.Date, out var dd))
                        dateText = dd.ToString("dd/MM/yyyy");

                    sb.AppendLine($"Ngày {day.DayIndex} ({dateText})");

                    if (day.Items != null && day.Items.Count > 0)
                    {
                        foreach (var it in day.Items)
                        {
                            var start = string.IsNullOrWhiteSpace(it.Start) ? "--:--" : it.Start;
                            var end = string.IsNullOrWhiteSpace(it.End) ? "--:--" : it.End;

                            var title = string.IsNullOrWhiteSpace(it.Title) ? "Hoạt động" : it.Title.Trim();
                            var type = string.IsNullOrWhiteSpace(it.Type) ? "" : $" • {it.Type.Trim()}";
                            var loc = string.IsNullOrWhiteSpace(it.Location) ? "" : $" • {it.Location.Trim()}";

                            sb.AppendLine($"{start} - {end}: {title}{type}{loc}");

                            if (!string.IsNullOrWhiteSpace(it.Note))
                                sb.AppendLine($"   - Ghi chú: {it.Note.Trim()}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("Chưa có hoạt động.");
                    }

                    sb.AppendLine();
                }

                return sb.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        // ================== (NEW) 1.12 - DTO PARSE ITINERARY JSON ==================
        private sealed class ItineraryDoc
        {
            public string? TemplateKey { get; set; }
            public List<ItineraryDay> Days { get; set; } = new();
        }

        private sealed class ItineraryDay
        {
            public int DayIndex { get; set; }
            public string? Date { get; set; }
            public List<ItineraryItem> Items { get; set; } = new();
        }

        private sealed class ItineraryItem
        {
            public string? Start { get; set; }
            public string? End { get; set; }
            public string? Title { get; set; }
            public string? Location { get; set; }
            public string? Type { get; set; }
            public string? Note { get; set; }
        }
        #endregion

        #region ================== 2. API CASCADING DROPDOWN (LOAD TỈNH/XÃ/ĐỊA ĐIỂM & LỌC DỊCH VỤ) ==================
        // ✅ [2.1] API: Lấy Xã/Phường theo Tỉnh/TP
        [HttpGet]
        public async Task<IActionResult> GetXaPhuongs(int tinhThanhId)
        {
            var xaList = await _context.XaPhuongs
                .Where(x => x.TinhThanhId == tinhThanhId)
                .OrderBy(x => x.TenXaPhuong)
                .Select(x => new { id = x.MaXaPhuong, name = x.TenXaPhuong })
                .ToListAsync();

            return Json(xaList);
        }

        // ✅ [2.2] API: Lấy Địa Điểm theo Xã/Phường
        [HttpGet]
        public async Task<IActionResult> GetDiaDiems(int xaPhuongId)
        {
            var diaDiemList = await _context.DiaDiems
                .Where(d => d.XaPhuongId == xaPhuongId)
                .OrderBy(d => d.TenDiaDiem)
                .Select(d => new { id = d.MaDiaDiem, name = d.TenDiaDiem })
                .ToListAsync();

            return Json(diaDiemList);
        }

        // ✅ [2.3] API: Lấy danh sách Địa điểm theo tên (autocomplete/suggestion)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetDiaDiemsByName(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Json(new List<object>());

            var result = await _context.DiaDiems
                .Where(d => d.TenDiaDiem.Contains(term))
                .Select(d => new { id = d.MaDiaDiem, name = d.TenDiaDiem })
                .Take(10)
                .ToListAsync();

            return Json(result);
        }

        // ✅ [2.4] API: Lấy Dịch Vụ theo Xã/Phường + Loại dịch vụ
        // - Trả về cả dịch vụ theo địa điểm thuộc Xã/Phường + dịch vụ gợi ý (MaDiaDiem == null)
        [HttpGet]
        public async Task<IActionResult> GetDichVus(int xaPhuongId, string? loaiDichVu)
        {
            var query = _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => d.TrangThai == "Active")
                .AsQueryable();

            if (xaPhuongId > 0)
            {
                query = query.Where(d =>
                    (d.DiaDiem != null && d.DiaDiem.XaPhuongId == xaPhuongId) ||
                    d.MaDiaDiem == null
                );
            }

            if (!string.IsNullOrWhiteSpace(loaiDichVu))
            {
                loaiDichVu = loaiDichVu.Trim();
                query = query.Where(d => d.LoaiDichVu == loaiDichVu);
            }

            var list = await query
                .OrderBy(d => d.TenDichVu)
                .Select(d => new
                {
                    id = d.MaTenDichVu,
                    name = d.TenDichVu,
                    loai = d.LoaiDichVu,
                    donGia = d.DonGia,
                    diaDiem = d.DiaDiem != null ? d.DiaDiem.TenDiaDiem : null,
                    isSuggested = d.MaDiaDiem == null
                })
                .ToListAsync();

            return Json(list);
        }

        // ✅ [2.5] API: Tìm dịch vụ theo từ khóa (dùng cho ô nhập tìm kiếm dịch vụ)
        [HttpGet]
        public async Task<IActionResult> SearchDichVus(string term, int? xaPhuongId, string? loaiDichVu)
        {
            if (string.IsNullOrWhiteSpace(term)) return Json(new List<object>());
            term = term.Trim();

            var query = _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => d.TrangThai == "Active" && d.TenDichVu.Contains(term))
                .AsQueryable();

            if (xaPhuongId.HasValue && xaPhuongId.Value > 0)
            {
                var xaId = xaPhuongId.Value;
                query = query.Where(d =>
                    (d.DiaDiem != null && d.DiaDiem.XaPhuongId == xaId) ||
                    d.MaDiaDiem == null
                );
            }

            if (!string.IsNullOrWhiteSpace(loaiDichVu))
            {
                loaiDichVu = loaiDichVu.Trim();
                query = query.Where(d => d.LoaiDichVu == loaiDichVu);
            }

            var result = await query
                .OrderBy(d => d.TenDichVu)
                .Select(d => new
                {
                    id = d.MaTenDichVu,
                    name = d.TenDichVu,
                    loai = d.LoaiDichVu,
                    donGia = d.DonGia
                })
                .Take(20)
                .ToListAsync();

            return Json(result);
        }

        // ✅ [2.6] API: AI Gợi ý địa điểm
        [HttpGet]
        public async Task<IActionResult> SuggestLocations(string area)
        {
            if (string.IsNullOrWhiteSpace(area)) return Json(new List<string>());
            var suggestions = await _aiService.SuggestLocationsAsync(area);
            return Json(suggestions);
        }

        // ✅ [2.7] API: AI Tạo nội dung Tour (Magic Fill)
        [HttpGet]
        public async Task<IActionResult> SuggestTourContent(string tourName)
        {
            if (string.IsNullOrWhiteSpace(tourName)) return Json(new { });
            var json = await _aiService.SuggestTourInfoAsync(tourName);
            return Content(json, "application/json");
        }

        #endregion

        #region ================== 3. PUBLIC SEARCH & DISPLAY (DANH SÁCH TÌM KIẾM & CHI TIẾT) ==================
        // ✅ [3.1] GET: Gợi ý tìm kiếm
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Json(new List<object>());

            term = term.Trim().ToLower();

            var tours = await _context.Tours
                .Where(t => !t.IsHidden && t.TrangThai == "DaDuyet" &&
                            (t.TenTour != null && t.TenTour.ToLower().Contains(term) ||
                             t.DiaDiem != null && t.DiaDiem.ToLower().Contains(term)))
                .Select(t => new
                {
                    id = t.MaTour,
                    label = t.TenTour != null ? t.TenTour : "Chưa có tên",
                    price = t.GiaTour.ToString("N0") + " đ",
                    image = t.HinhAnhTourUrl
                })
                .Take(5)
                .ToListAsync();

            return Json(tours);
        }

        // ✅ [3.2] GET: Danh sách Tour (với filter & sort)
        [AllowAnonymous]
        public async Task<IActionResult> Index(
            string? keyword, string? sort, string? diaDiem, string? noiKhoiHanh,
            decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Tours.AsQueryable();

            if (!IsAdminOrStaff)
            {
                query = query.Where(t => !t.IsHidden && t.TrangThai == "DaDuyet");
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                ViewBag.CurrentKeyword = keyword;
                query = query.Where(t => (t.TenTour != null && t.TenTour.Contains(keyword)) ||
                                         (t.DiaDiem != null && t.DiaDiem.Contains(keyword)) ||
                                         (t.MoTa != null && t.MoTa.Contains(keyword)));
            }

            if (!string.IsNullOrEmpty(diaDiem))
                query = query.Where(t => t.DiaDiem == diaDiem);
            if (!string.IsNullOrEmpty(noiKhoiHanh))
                query = query.Where(t => t.NoiKhoiHanh == noiKhoiHanh);
            if (minPrice.HasValue)
                query = query.Where(t => t.GiaTour >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(t => t.GiaTour <= maxPrice.Value);

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(t => t.GiaTour);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(t => t.GiaTour);
                    break;
                case "date":
                    query = query.OrderBy(t => t.NgayDi);
                    break;
                case "popular":
                    query = query.OrderByDescending(t => t.SoNguoiHienTai);
                    break;
                default:
                    query = query.OrderByDescending(t => t.NgayTao);
                    break;
            }

            ViewBag.DiaDiemList = await _context.Tours
                .Where(t => !t.IsHidden && t.DiaDiem != null)
                .Select(t => t.DiaDiem)
                .Distinct()
                .ToListAsync();

            ViewBag.SortOrder = sort;

            return View(await query.ToListAsync());
        }

        // ✅ [3.3] GET: Chi tiết Tour (hiển thị đầy đủ + dịch vụ + review)
        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var tour = await _context.Tours
                .Include(t => t.TourDichVus)
                    .ThenInclude(td => td.DichVu)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.MaTour == id);

            if (tour == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            bool isOwner = isAuthenticated() && tour.UserId == currentUserId;


            // ✅ BLOCK: Tour cá nhân chưa duyệt => không cho xem trang Details công khai
            if (tour.IsPersonal && tour.TrangThai != "DaDuyet" && !IsAdminOrStaff)
            {
                // Chủ tour xem bằng trang PersonalDetails (chỉ xem, không đặt tour)
                if (isOwner)
                {
                    TempData["Error"] = "Tour cá nhân của em đang ở trạng thái chờ duyệt/từ chối nên chưa xem được trang chi tiết công khai. Em xem ở trang xem tour cá nhân nhé.";
                    return RedirectToAction(nameof(PersonalDetails), new { id = tour.MaTour });
                }

                // Người khác: quay về danh sách
                return RedirectToAction(nameof(Index));
            }

            if (tour.IsHidden && !IsAdminOrStaff && !isOwner)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Reviews = await _context.DanhGiaTours
                .Include(r => r.User)
                .Where(r => r.TourId == id)
                .OrderByDescending(r => r.NgayDanhGia)
                .Take(5)
                .ToListAsync();

            // Load thông tin Tỉnh/Xã của Điểm đến
            if (!string.IsNullOrWhiteSpace(tour.DiaDiem))
            {
                var diaDiemInfo = await _context.DiaDiems
                    .Include(d => d.XaPhuong)
                        .ThenInclude(xp => xp!.TinhThanh)
                    .FirstOrDefaultAsync(d => d.TenDiaDiem == tour.DiaDiem);
                ViewBag.DiaDiemInfo = diaDiemInfo;
            }

            // Load thông tin Tỉnh/Xã của Điểm khởi hành
            if (!string.IsNullOrWhiteSpace(tour.NoiKhoiHanh))
            {
                var noiKhoiHanhInfo = await _context.DiaDiems
                    .Include(d => d.XaPhuong)
                        .ThenInclude(xp => xp!.TinhThanh)
                    .FirstOrDefaultAsync(d => d.TenDiaDiem == tour.NoiKhoiHanh);
                ViewBag.NoiKhoiHanhInfo = noiKhoiHanhInfo;
            }

            return View(tour);
        }

        // ✅ [3.4] GET: Xem tour cá nhân (chỉ xem, không đặt tour)
        [Authorize]
        public async Task<IActionResult> PersonalDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var tour = await _context.Tours
                .Include(t => t.TourDichVus)
                    .ThenInclude(td => td.DichVu)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.MaTour == id);

            if (tour == null) return NotFound();

            // Không phải tour cá nhân => dùng Details công khai
            if (!tour.IsPersonal)
                return RedirectToAction(nameof(Details), new { id = tour.MaTour });

            var currentUserId = _userManager.GetUserId(User);
            bool isOwner = isAuthenticated() && tour.UserId == currentUserId;

            // Tour cá nhân: chỉ chủ tour hoặc Admin/Staff được xem
            if (!IsAdminOrStaff && !isOwner) return Forbid();

            // Load thông tin Tỉnh/Xã của Điểm đến
            if (!string.IsNullOrWhiteSpace(tour.DiaDiem))
            {
                var diaDiemInfo = await _context.DiaDiems
                    .Include(d => d.XaPhuong)
                        .ThenInclude(xp => xp!.TinhThanh)
                    .FirstOrDefaultAsync(d => d.TenDiaDiem == tour.DiaDiem);
                ViewBag.DiaDiemInfo = diaDiemInfo;
            }

            // Load thông tin Tỉnh/Xã của Điểm khởi hành
            if (!string.IsNullOrWhiteSpace(tour.NoiKhoiHanh))
            {
                var noiKhoiHanhInfo = await _context.DiaDiems
                    .Include(d => d.XaPhuong)
                        .ThenInclude(xp => xp!.TinhThanh)
                    .FirstOrDefaultAsync(d => d.TenDiaDiem == tour.NoiKhoiHanh);
                ViewBag.NoiKhoiHanhInfo = noiKhoiHanhInfo;
            }

            return View(tour);
        }

        #endregion

        #region ================== 4. BOOKING FLOW (QUY TRÌNH ĐẶT TOUR) ==================
        // ✅ [4.1] GET: Hiển thị form đăng ký tour
        public async Task<IActionResult> Register(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var tour = await _context.Tours
                .Include(t => t.TourDichVus)
                    .ThenInclude(td => td.DichVu)
                .FirstOrDefaultAsync(t => t.MaTour == id);

            if (tour == null || (tour.IsHidden && !IsAdminOrStaff)) return NotFound();

            // Kiểm tra full chỗ
            if (tour.SoNguoiToiDa.HasValue && tour.SoNguoiHienTai >= tour.SoNguoiToiDa)
            {
                TempData["Error"] = "Rất tiếc, Tour này đã hết chỗ!";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var user = await _userManager.GetUserAsync(User);

            // FIX: Dùng variable thay vì ??
            string hoTen = user != null ? user.HoTen : "";
            string email = user != null ? user.Email : "";
            string soDienThoai = user != null ? user.PhoneNumber : "";
            string cccd = user != null ? user.CCCD : "";
            DateTime? ngaySinh = user != null ? user.NgaySinh : null;

            var model = new TourRegisterViewModel
            {
                TourId = tour.MaTour,
                MaTour = tour.MaTour,
                TenTour = tour.TenTour != null ? tour.TenTour : "",
                GiaTour = tour.GiaTour,
                HinhAnhTourUrl = tour.HinhAnhTourUrl,
                NgayDi = tour.NgayDi,
                NoiKhoiHanh = tour.NoiKhoiHanh,
                DiaDiem = tour.DiaDiem,
                HoTenDaiDien = hoTen,
                Email = email,
                SoDienThoai = soDienThoai,
                CCCD = cccd,
                NgaySinh = ngaySinh,
                SoNguoiLon = 1,
                SoTreEm = 0,
                TourDichVus = tour.TourDichVus != null ? tour.TourDichVus.ToList() : new List<TourDichVu>()
            };

            model.TongTienDuKien = tour.GiaTour;
            return View(model);
        }

        // ✅ [4.2] POST: Xử lý Đăng Ký Tour (submit form Register)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(TourRegisterViewModel model)
        {
            var tour = await _context.Tours.FindAsync(model.TourId);
            if (tour == null) return NotFound();

            // Xử lý Upload ảnh CCCD
            string? duongDanAnhCCCD = null;
            if (model.AnhCCCDFile != null)
            {
                duongDanAnhCCCD = await UploadFile(model.AnhCCCDFile, "cccd");
                if (duongDanAnhCCCD == null)
                {
                    ModelState.AddModelError("AnhCCCDFile", "Lỗi khi lưu ảnh. Vui lòng thử lại.");
                }
            }
            else
            {
                ModelState.AddModelError("AnhCCCDFile", "Vui lòng chọn ảnh CCCD.");
            }

            if (ModelState.IsValid)
            {
                decimal giaTour = tour.GiaTour;
                decimal tongTien = (model.SoNguoiLon * giaTour) + (model.SoTreEm * giaTour * 0.7m);

                return RedirectToAction("BatDauThanhToan", "ThanhToans", new
                {
                    tourId = model.TourId,
                    slNguoiLon = model.SoNguoiLon,
                    slTreEm = model.SoTreEm,
                    hoTen = model.HoTenDaiDien,
                    soDienThoai = model.SoDienThoai,
                    email = model.Email,
                    cccd = model.CCCD,
                    diaChi = model.DiaChiLienHe,
                    ghiChu = model.GhiChu,
                    ngaySinh = model.NgaySinh != null ? model.NgaySinh.Value.ToString("yyyy-MM-dd") : null,
                    anhCCCD = duongDanAnhCCCD
                });
            }

            // Reload dữ liệu nếu lỗi
            var tourReload = await _context.Tours
                .Include(t => t.TourDichVus)
                .FirstOrDefaultAsync(t => t.MaTour == model.TourId);

            model.TenTour = tour.TenTour != null ? tour.TenTour : "";
            model.MaTour = tour.MaTour;
            model.HinhAnhTourUrl = tour.HinhAnhTourUrl;
            model.DiaDiem = tour.DiaDiem;
            model.NoiKhoiHanh = tour.NoiKhoiHanh;
            model.GiaTour = tour.GiaTour;
            model.NgayDi = tour.NgayDi;
            model.TongTienDuKien = (model.SoNguoiLon * tour.GiaTour) + (model.SoTreEm * tour.GiaTour * 0.7m);
            model.TourDichVus = tourReload != null ? tourReload.TourDichVus.ToList() : new List<TourDichVu>();

            return View(model);
        }
        #endregion

        #region ================== 5. USER PERSONAL TOUR (TOUR CÁ NHÂN) ==================
        // ✅ [5.1] GET: Tạo tour cá nhân mới
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreatePersonal()
        {

            // ✅ NEW: Load Tỉnh/TP cho dropdown (CreatePersonal cũng cần như Admin Create)
            var tinhThanhList = await _context.TinhThanhs
                .OrderBy(t => t.TenTinh)
                .ToListAsync();
            ViewData["TinhThanhList"] = new SelectList(tinhThanhList, "MaTinhThanh", "TenTinh");

            ViewBag.Keywords = await _context.DiaDiems
                .Select(d => d.TenDiaDiem)
                .Distinct()
                .ToListAsync() ?? new List<string>();

            ViewBag.DefaultTours = await _context.Tours
                .Where(t => t.IsDefault && !t.IsHidden)
                .ToListAsync() ?? new List<Tour>();

            ViewBag.DichVuList = await _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => d.TrangThai == "Active")
                .OrderBy(d => d.TenDichVu)
                .ToListAsync() ?? new List<DichVu>();

            return View(new Tour
            {
                IsPersonal = true,
                TrangThai = "ChoDuyet",
                SoNguoiToiDa = 2,
                NgayDi = DateTime.Now.AddDays(7),
                NgayVe = DateTime.Now.AddDays(10)
            });
        }

        // ✅ [5.2] POST: Lưu tour cá nhân mới
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePersonal(Tour tour, string[] SelectedServices)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            ModelState.Remove(nameof(tour.MaTour));
            ModelState.Remove(nameof(tour.UserId));
            ModelState.Remove(nameof(tour.TrangThai));
            ModelState.Remove(nameof(tour.User));

            if (ModelState.IsValid)
            {
                try
                {
                    tour.MaTour = MaSoHelper.TaoMa("P");
                    tour.UserId = user.Id;
                    tour.TrangThai = "ChoDuyet";
                    tour.IsPersonal = true;
                    tour.NgayTao = DateTime.Now;

                    _context.Add(tour);
                    await _context.SaveChangesAsync();

                    if (SelectedServices != null && SelectedServices.Any())
                    {
                        // ✅ Chuẩn hóa + lọc theo điểm đến (an toàn dữ liệu, đỡ bị chọn “lạc quẻ”)
                        var selectedIds = SelectedServices
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(x => x.Trim())
                            .Distinct()
                            .ToList();

                        selectedIds = await FilterSelectedDichVuIdsByDestinationAsync(tour.DiaDiem, selectedIds);

                        foreach (var sId in selectedIds)
                        {
                            _context.TourDichVus.Add(new TourDichVu { TourId = tour.MaTour, DichVuId = sId });
                        }
                        await _context.SaveChangesAsync();
                    }
                    TempData["Success"] = "Yêu cầu đã được gửi!";
                    return RedirectToAction(nameof(Custom));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            ViewBag.Keywords = await _context.DiaDiems
                .Select(d => d.TenDiaDiem)
                .Distinct()
                .ToListAsync();

            ViewBag.DefaultTours = await _context.Tours
                .Where(t => t.IsDefault && !t.IsHidden)
                .ToListAsync();

            ViewBag.DichVuList = await _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => d.TrangThai == "Active")
                .OrderBy(d => d.TenDichVu)
                .ToListAsync();

            var tinhThanhList = await _context.TinhThanhs
                .OrderBy(t => t.TenTinh)
                .ToListAsync();
            ViewData["TinhThanhList"] = new SelectList(tinhThanhList, "MaTinhThanh", "TenTinh");

            return View(tour);
        }

        // ✅ [5.3] POST: Clone tour thành tour cá nhân
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloneToPersonal(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var originalTour = await _context.Tours
                .Include(t => t.TourDichVus)
                .FirstOrDefaultAsync(t => t.MaTour == id);

            if (originalTour == null) return NotFound();

            var personalTour = new Tour
            {
                MaTour = MaSoHelper.TaoMa("P"),
                TenTour = "Tùy chỉnh: " + originalTour.TenTour,
                DiaDiem = originalTour.DiaDiem,
                NoiKhoiHanh = originalTour.NoiKhoiHanh,
                MoTa = originalTour.MoTa,
                GiaTour = originalTour.GiaTour,
                NgayDi = originalTour.NgayDi,
                NgayVe = originalTour.NgayVe,
                UserId = user != null ? user.Id : null,
                IsPersonal = true,
                TrangThai = "ChoDuyet",
                NgayTao = DateTime.Now,
                IsHidden = true
            };

            _context.Tours.Add(personalTour);
            await _context.SaveChangesAsync();

            foreach (var dv in originalTour.TourDichVus)
            {
                _context.TourDichVus.Add(new TourDichVu { TourId = personalTour.MaTour, DichVuId = dv.DichVuId });
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Custom));
        }

        // ✅ [5.4] GET: Danh sách tour cá nhân của user
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Custom()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var myTours = await _context.Tours
                .Where(t => t.UserId == user.Id && t.IsPersonal)
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            return View(myTours);
        }


        // ✅ [5.5] GET: Chỉnh sửa tour cá nhân (form edit)
        [Authorize(Roles = "User")]
        public async Task<IActionResult> EditPersonal(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var tour = await _context.Tours
                .Include(t => t.TourDichVus)
                .FirstOrDefaultAsync(t => t.MaTour == id && t.UserId == user.Id);

            if (tour == null) return NotFound();

            if (tour.TrangThai != "ChoDuyet")
            {
                TempData["Error"] = "Tour này đã được xử lý hoặc hủy, không thể chỉnh sửa.";
                return RedirectToAction(nameof(Custom));
            }

            // ✅ Load data giống CreatePersonal để UI dropdown + dịch vụ hoạt động
            var tinhThanhList = await _context.TinhThanhs
                .OrderBy(t => t.TenTinh)
                .ToListAsync();
            ViewData["TinhThanhList"] = new SelectList(tinhThanhList, "MaTinhThanh", "TenTinh");

            ViewBag.Keywords = await _context.DiaDiems
                .Select(d => d.TenDiaDiem)
                .Distinct()
                .ToListAsync() ?? new List<string>();

            ViewBag.DichVuList = await _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => d.TrangThai == "Active")
                .OrderBy(d => d.TenDichVu)
                .ToListAsync() ?? new List<DichVu>();

            // ✅ Preload dịch vụ đã chọn (để render ra các pill)
            var selectedIds = tour.TourDichVus?.Select(x => x.DichVuId).Distinct().ToList() ?? new List<string>();
            ViewBag.SelectedDichVus = await _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => selectedIds.Contains(d.MaTenDichVu))
                .ToListAsync();

            // ✅ Preselect dropdown theo tên địa điểm đang lưu (NoiKhoiHanh/DiaDiem)
            var startDd = await _context.DiaDiems
                .Include(d => d.XaPhuong)
                    .ThenInclude(x => x!.TinhThanh)
                .FirstOrDefaultAsync(d => d.TenDiaDiem == tour.NoiKhoiHanh);

            var destDd = await _context.DiaDiems
                .Include(d => d.XaPhuong)
                    .ThenInclude(x => x!.TinhThanh)
                .FirstOrDefaultAsync(d => d.TenDiaDiem == tour.DiaDiem);

            ViewBag.StartTinhId = startDd?.XaPhuong?.TinhThanhId;
            ViewBag.StartXaId = startDd?.XaPhuongId;
            ViewBag.StartDiaDiem = startDd?.TenDiaDiem ?? tour.NoiKhoiHanh;

            ViewBag.DestTinhId = destDd?.XaPhuong?.TinhThanhId;
            ViewBag.DestXaId = destDd?.XaPhuongId;
            ViewBag.DestDiaDiem = destDd?.TenDiaDiem ?? tour.DiaDiem;

            return View(tour);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePersonalStatus(string id, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.MaTour == id);
            if (tour == null) return NotFound();

            if (!tour.IsPersonal)
            {
                TempData["Error"] = "Chỉ hỗ trợ toggle cho tour cá nhân.";
                return RedirectToAction(nameof(Index));
            }

            if (tour.TrangThai == "TuChoi")
            {
                tour.TrangThai = "DaDuyet";   // => Hoạt động
                tour.IsHidden = false;
                TempData["Success"] = $"Đã chuyển {tour.MaTour} sang Hoạt động.";
            }
            else if (tour.TrangThai == "DaDuyet")
            {
                tour.TrangThai = "TuChoi";    // => Từ chối
                tour.IsHidden = true;
                TempData["Success"] = $"Đã chuyển {tour.MaTour} sang Từ chối.";
            }
            else
            {
                TempData["Error"] = "Chỉ toggle giữa Hoạt động và Từ chối.";
                return RedirectToAction(nameof(Index));
            }

            _context.Update(tour);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
        // ✅ [5.6] POST: Lưu chỉnh sửa tour cá nhân
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPersonal(string id, Tour tour, string[] SelectedServices)
        {
            if (string.IsNullOrWhiteSpace(id) || id != tour.MaTour) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // ✅ Bỏ validate các field hệ thống (thường không có trong form)
            ModelState.Remove(nameof(tour.UserId));
            ModelState.Remove(nameof(tour.User));
            ModelState.Remove(nameof(tour.TrangThai));
            ModelState.Remove(nameof(tour.IsPersonal));
            ModelState.Remove(nameof(tour.IsDefault));
            ModelState.Remove(nameof(tour.IsHidden));
            ModelState.Remove(nameof(tour.NgayTao));
            ModelState.Remove(nameof(tour.SoNguoiHienTai));
            ModelState.Remove(nameof(tour.SoNguoiToiDa));

            var existingTour = await _context.Tours
                .Include(t => t.TourDichVus)
                .FirstOrDefaultAsync(t => t.MaTour == id && t.UserId == user.Id);

            if (existingTour == null) return NotFound();

            if (existingTour.TrangThai != "ChoDuyet")
            {
                TempData["Error"] = "Tour này đã được xử lý hoặc hủy, không thể chỉnh sửa.";
                return RedirectToAction(nameof(Custom));
            }

            if (ModelState.IsValid)
            {
                existingTour.TenTour = tour.TenTour;
                existingTour.NoiKhoiHanh = tour.NoiKhoiHanh;
                existingTour.DiaDiem = tour.DiaDiem;
                existingTour.NgayDi = tour.NgayDi;
                existingTour.NgayVe = tour.NgayVe;

                existingTour.MoTa = tour.MoTa;
                existingTour.GiaTour = tour.GiaTour;

                existingTour.YeuCauKhachSan = tour.YeuCauKhachSan;
                existingTour.YeuCauNhaHang = tour.YeuCauNhaHang;
                existingTour.YeuCauPhuongTien = tour.YeuCauPhuongTien;
                existingTour.YeuCauVuiChoi = tour.YeuCauVuiChoi;
                existingTour.YeuCauKhac = tour.YeuCauKhac;

                existingTour.NgayTao = DateTime.Now;

                // ✅ Update dịch vụ (xóa cũ, thêm mới)
                if (existingTour.TourDichVus != null && existingTour.TourDichVus.Any())
                {
                    _context.TourDichVus.RemoveRange(existingTour.TourDichVus);
                }

                var selectedIds = (SelectedServices ?? Array.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                selectedIds = await FilterSelectedDichVuIdsByDestinationAsync(existingTour.DiaDiem, selectedIds);

                foreach (var sid in selectedIds)
                {
                    _context.TourDichVus.Add(new TourDichVu
                    {
                        TourId = existingTour.MaTour,
                        DichVuId = sid
                    });
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật tour cá nhân thành công!";
                return RedirectToAction(nameof(Custom));
            }

            // ✅ Reload dữ liệu dropdown/service nếu lỗi validate
            var tinhThanhList = await _context.TinhThanhs
                .OrderBy(t => t.TenTinh)
                .ToListAsync();
            ViewData["TinhThanhList"] = new SelectList(tinhThanhList, "MaTinhThanh", "TenTinh");

            ViewBag.Keywords = await _context.DiaDiems
                .Select(d => d.TenDiaDiem)
                .Distinct()
                .ToListAsync() ?? new List<string>();

            ViewBag.DichVuList = await _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => d.TrangThai == "Active")
                .OrderBy(d => d.TenDichVu)
                .ToListAsync() ?? new List<DichVu>();

            var selectedIdsBack = (SelectedServices ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();

            ViewBag.SelectedDichVus = await _context.DichVus
                .Include(d => d.DiaDiem)
                .Where(d => selectedIdsBack.Contains(d.MaTenDichVu))
                .ToListAsync();

            return View(tour);
        }
        #endregion

        #region ================== 6. ADMIN SYSTEM MANAGEMENT (TẠO/CHỈNH SỬA/XÓA TOUR) ==================
        // ✅ [6.1] GET: Tạo tour mới (Admin/Staff) - form tạo
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create()
        {
            var tinhThanhList = await _context.TinhThanhs
                .OrderBy(t => t.TenTinh)
                .ToListAsync();
            ViewData["TinhThanhList"] = new SelectList(tinhThanhList, "MaTinhThanh", "TenTinh");

            var vm = new TourCreateViewModel
            {
                MaTour = MaSoHelper.TaoMa("T"),
                NgayDi = DateTime.Now.AddDays(10),
                NgayVe = DateTime.Now.AddDays(15),
                SoNguoiToiDa = 20,

                // ================== (NEW) default time ==================
                GioKhoiHanh = "06:30",
                GioKetThuc = "18:00"
            };

            await LoadTourCreateDataAsync(vm);
            return View(vm);
        }

        // ✅ [6.2] POST: Lưu tour mới (Admin/Staff)
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TourCreateViewModel model, IFormFile? AnhTourFile)
        {
            if (string.IsNullOrEmpty(model.MaTour))
                model.MaTour = MaSoHelper.TaoMa("T");

            if (!ModelState.IsValid)
            {
                await LoadTourCreateDataAsync(model);
                return View(model);
            }

            // ================== (NEW) Build MoTa từ JSON nếu MoTa đang rỗng ==================
            if (!string.IsNullOrWhiteSpace(model.LichTrinhJson) && string.IsNullOrWhiteSpace(model.MoTa))
            {
                model.MoTa = BuildMoTaFromItineraryJson(model.LichTrinhJson);
            }

            // ================== (NEW) Combine date + time ==================
            var ngayDiFull = CombineDateAndTime(model.NgayDi, model.GioKhoiHanh);
            var ngayVeFull = CombineDateAndTime(model.NgayVe, model.GioKetThuc);

            var tour = new Tour
            {
                MaTour = model.MaTour,
                TenTour = model.TenTour,
                LoaiTour = model.LoaiTour,
                NoiKhoiHanh = model.NoiKhoiHanh,
                DiaDiem = model.DiaDiem,
                MoTa = model.MoTa,
                GiaTour = model.GiaTour,
                SoNguoiToiDa = model.SoNguoiToiDa,
                SoNguoiHienTai = 0,
                NgayDi = model.NgayDi,
                NgayVe = model.NgayVe,
                IsDefault = true,
                IsPersonal = false,
                IsHidden = false,
                TrangThai = "DaDuyet",
                NgayTao = DateTime.Now
            };

            // ================== (NEW) Override ngày giờ đầy đủ + lưu JSON ==================
            tour.NgayDi = ngayDiFull;
            tour.NgayVe = ngayVeFull;
            tour.LichTrinhJson = model.LichTrinhJson;

            tour.HinhAnhTourUrl = await UploadFile(AnhTourFile, "tours");

            _context.Add(tour);
            await _context.SaveChangesAsync();

            var selectedIds = GetSelectedDichVuIdsUnified(model);
            selectedIds = await FilterSelectedDichVuIdsByDestinationAsync(model.DiaDiem, selectedIds);

            if (selectedIds.Any())
            {
                foreach (var sid in selectedIds)
                {
                    _context.TourDichVus.Add(new TourDichVu
                    {
                        TourId = tour.MaTour,
                        DichVuId = sid
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Đã tạo Tour hệ thống thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ [6.3] GET: Chỉnh sửa tour (Admin/Staff) - hiển thị form với dữ liệu cũ
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var tour = await _context.Tours
                .Include(t => t.TourDichVus)
                .FirstOrDefaultAsync(t => t.MaTour == id);

            if (tour == null) return NotFound();

            var vm = new TourCreateViewModel
            {
                MaTour = tour.MaTour,
                TenTour = tour.TenTour != null ? tour.TenTour : "",
                LoaiTour = tour.LoaiTour != null ? tour.LoaiTour : "",
                NoiKhoiHanh = tour.NoiKhoiHanh != null ? tour.NoiKhoiHanh : "",
                DiaDiem = tour.DiaDiem != null ? tour.DiaDiem : "",
                MoTa = tour.MoTa != null ? tour.MoTa : "",
                GiaTour = tour.GiaTour,
                SoNguoiToiDa = tour.SoNguoiToiDa.HasValue ? tour.SoNguoiToiDa.Value : 0,
                SoNguoiHienTai = tour.SoNguoiHienTai.HasValue ? tour.SoNguoiHienTai.Value : 0,
                NgayDi = tour.NgayDi.HasValue ? tour.NgayDi.Value : DateTime.Now,
                NgayVe = tour.NgayVe.HasValue ? tour.NgayVe.Value : DateTime.Now,
                HinhAnhTourUrl = tour.HinhAnhTourUrl,

                // ================== (NEW) Đổ giờ HH:mm và JSON vào form Edit ==================
                GioKhoiHanh = tour.NgayDi.HasValue ? tour.NgayDi.Value.ToString("HH:mm") : "06:30",
                GioKetThuc = tour.NgayVe.HasValue ? tour.NgayVe.Value.ToString("HH:mm") : "18:00",
                LichTrinhJson = tour.LichTrinhJson
            };

            var existed = tour.TourDichVus.Select(td => td.DichVuId).ToList();
            vm.SelectedDichVuIds = existed;
            vm.SelectedServiceIds = existed;

            // ================== [EDIT GET] ĐỔ DATA TỈNH/XÃ CỦA ĐIỂM KHỞI HÀNH ==================
            if (!string.IsNullOrWhiteSpace(vm.NoiKhoiHanh))
            {
                var tenDD = vm.NoiKhoiHanh.Trim();

                var ddKhoiHanh = await _context.DiaDiems
                    .Include(d => d.XaPhuong)
                        .ThenInclude(xp => xp!.TinhThanh)
                    .FirstOrDefaultAsync(d => d.TenDiaDiem == tenDD);

                if (ddKhoiHanh != null && ddKhoiHanh.XaPhuong != null)
                {
                    vm.NoiKhoiHanhXa = ddKhoiHanh.XaPhuongId.ToString();
                    vm.NoiKhoiHanhTinh = ddKhoiHanh.XaPhuong.TinhThanhId.ToString();
                }
            }

            // ================== [EDIT GET] ĐỔ DATA TỈNH/XÃ CỦA ĐIỂM ĐẾN ==================
            if (!string.IsNullOrWhiteSpace(vm.DiaDiem))
            {
                var tenDD = vm.DiaDiem.Trim();

                var ddDiemDen = await _context.DiaDiems
                    .Include(d => d.XaPhuong)
                        .ThenInclude(xp => xp!.TinhThanh)
                    .FirstOrDefaultAsync(d => d.TenDiaDiem == tenDD);

                if (ddDiemDen != null && ddDiemDen.XaPhuong != null)
                {
                    vm.DiaDiemXa = ddDiemDen.XaPhuongId.ToString();
                    vm.DiaDiemTinh = ddDiemDen.XaPhuong.TinhThanhId.ToString();
                }
            }

            ViewBag.CurrentImg = tour.HinhAnhTourUrl;

            await LoadTourCreateDataAsync(vm);
            return View(vm);
        }

        // ✅ [6.4] POST: Lưu chỉnh sửa tour (Admin/Staff)
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TourCreateViewModel model, IFormFile? AnhTourFile)
        {
            if (id != model.MaTour) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.CurrentImg = model.HinhAnhTourUrl;
                await LoadTourCreateDataAsync(model);
                return View(model);
            }

            try
            {
                var tourDb = await _context.Tours
                    .Include(t => t.TourDichVus)
                    .FirstOrDefaultAsync(t => t.MaTour == id);

                if (tourDb == null) return NotFound();

                // ================== (NEW) Build MoTa từ JSON nếu MoTa rỗng ==================
                if (!string.IsNullOrWhiteSpace(model.LichTrinhJson) && string.IsNullOrWhiteSpace(model.MoTa))
                {
                    model.MoTa = BuildMoTaFromItineraryJson(model.LichTrinhJson);
                }

                // ================== (NEW) Combine date + time ==================
                var ngayDiFull = CombineDateAndTime(model.NgayDi, model.GioKhoiHanh);
                var ngayVeFull = CombineDateAndTime(model.NgayVe, model.GioKetThuc);

                // Cập nhật thông tin cơ bản
                tourDb.TenTour = model.TenTour;
                tourDb.LoaiTour = model.LoaiTour;
                tourDb.NoiKhoiHanh = model.NoiKhoiHanh;
                tourDb.DiaDiem = model.DiaDiem;
                tourDb.MoTa = model.MoTa;
                tourDb.GiaTour = model.GiaTour;
                tourDb.SoNguoiToiDa = model.SoNguoiToiDa;

                // giữ dòng cũ (không xóa), nhưng override bằng full datetime
                tourDb.NgayDi = model.NgayDi;
                tourDb.NgayVe = model.NgayVe;

                // ================== (NEW) Override ngày giờ đầy đủ + lưu JSON ==================
                tourDb.NgayDi = ngayDiFull;
                tourDb.NgayVe = ngayVeFull;
                tourDb.LichTrinhJson = model.LichTrinhJson;

                // Xử lý ảnh
                if (AnhTourFile != null)
                {
                    DeleteOldImage(tourDb.HinhAnhTourUrl);
                    tourDb.HinhAnhTourUrl = await UploadFile(AnhTourFile, "tours");
                }

                // Xóa dịch vụ cũ
                if (tourDb.TourDichVus != null && tourDb.TourDichVus.Any())
                {
                    _context.TourDichVus.RemoveRange(tourDb.TourDichVus);
                }

                // Thêm dịch vụ mới
                var selectedIds = GetSelectedDichVuIdsUnified(model);

                // ================== (NEW) Lọc dịch vụ theo điểm đến (đúng chuẩn Create) ==================
                selectedIds = await FilterSelectedDichVuIdsByDestinationAsync(model.DiaDiem, selectedIds);

                if (selectedIds.Any())
                {
                    foreach (var sid in selectedIds)
                    {
                        _context.TourDichVus.Add(new TourDichVu
                        {
                            TourId = id,
                            DichVuId = sid
                        });
                    }
                }

                _context.Update(tourDb);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật Tour thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TourExists(model.MaTour != null ? model.MaTour : "")) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        #region ================== 6.X. DELETE TOUR (MERGED + FIX AMBIGUOUS) ==================

        // ✅ GET: (optional) Trang xác nhận xóa (nếu em có dùng Delete.cshtml)
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> Delete(string id, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.MaTour == id);
            if (tour == null) return NotFound();

            ViewData["ReturnUrl"] = returnUrl;
            return View(tour);
        }

        // ✅ POST: Xóa tour (Index.cshtml dùng action này để tránh trùng endpoint)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteHard(string id, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction(nameof(Index));

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
                return RedirectToAction(nameof(Index));

            try
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
                TempData["ToastSuccess"] = $"Đã xóa tour {id}.";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                TempData["ToastError"] =
                    "Không xóa được vì tour đang có dữ liệu liên quan (đơn đặt/đánh giá...). " +
                    "Hãy dùng Ẩn/Hiện thay vì xóa hẳn.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // ✅ POST: Nếu em có Delete.cshtml submit về asp-action="Delete" thì vẫn chạy,
        // nhưng sẽ gom về DeleteHard để code chỉ có 1 nơi xử lý.
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> DeleteConfirmed(string id, string? returnUrl = null)
            => DeleteHard(id, returnUrl);

        // ✅ [6.7] POST: Ẩn/Hiện tour (thêm AntiForgery + returnUrl cho UI)
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHide(string id, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            tour.IsHidden = !tour.IsHidden;
            _context.Update(tour);
            await _context.SaveChangesAsync();

            return SafeRedirect(returnUrl);
        }

        // ✅ Safe redirect: chỉ cho redirect nội bộ
        private IActionResult SafeRedirect(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        #endregion


        #endregion

        #region ================== 7. ADMIN APPROVAL (DUYỆT/TỪ CHỐI TOUR CÁ NHÂN) ==================
        // ✅ [7.1] GET: Danh sách tour chờ duyệt
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> PendingPersonalList()
        {
            var pendingTours = await _context.Tours
                .Include(t => t.User)
                .Where(t => t.IsPersonal && t.TrangThai == "ChoDuyet")
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            return View(pendingTours);
        }

        // ✅ [7.2] POST: Duyệt tour cá nhân
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePersonal(string id, decimal giaTour, string? adminNote)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            if (giaTour <= 0)
            {
                TempData["Error"] = "Vui lòng nhập giá tour hợp lệ.";
                return RedirectToAction(nameof(PendingPersonalList));
            }

            tour.TrangThai = "DaDuyet";
            tour.GiaTour = giaTour;
            tour.IsHidden = false;

            if (!string.IsNullOrEmpty(adminNote))
            {
                tour.MoTa += $"\n[Admin Note]: {adminNote}";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã duyệt tour {tour.MaTour} và báo giá {giaTour:N0}đ thành công!";
            return RedirectToAction(nameof(PendingPersonalList));
        }

        // ✅ [7.3] POST: Từ chối tour cá nhân
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPersonal(string id, string lyDo)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            tour.TrangThai = "TuChoi";
            tour.MoTa += $"\n[Lý do từ chối]: {lyDo}";
            tour.IsHidden = true;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã từ chối yêu cầu tour.";
            return RedirectToAction(nameof(PendingPersonalList));
        }
        #endregion
    }
}
