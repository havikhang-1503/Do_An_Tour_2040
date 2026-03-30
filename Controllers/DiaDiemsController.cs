// Folder: Controllers
// File path: Controllers/DiaDiemsController.cs

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Utils;
using System.Text.Json;

namespace Tour_2040.Controllers
{
    public class DiaDiemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.ISupportAiService _aiService;

        public DiaDiemsController(ApplicationDbContext context, Services.ISupportAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        private void LoadTinhDropdown(int? selectedTinh = null)
        {
            ViewData["TinhThanhId"] = new SelectList(
                _context.TinhThanhs.OrderBy(t => t.TenTinh),
                "MaTinhThanh",
                "TenTinh",
                selectedTinh
            );
        }
        // API lấy địa điểm theo Xã/Phường (cascade step 2)
        [HttpGet]
        public async Task<IActionResult> GetDiaDiems(int xaPhuongId)
        {
            var ddList = await _context.DiaDiems
                .Where(d => d.XaPhuongId == xaPhuongId)
                .OrderBy(d => d.TenDiaDiem)
                .Select(d => new { name = d.TenDiaDiem })
                .ToListAsync();

            return Json(ddList);
        }

        private async Task LoadXaDropdownAsync(int? tinhThanhId, int? selectedXa = null)
        {   // ✅ NEW: Tỉnh/TP dropdown
            var tinhs = await _context.TinhThanhs
                .OrderBy(t => t.TenTinh)
                .ToListAsync();

            ViewBag.TinhThanhList = new SelectList(tinhs, "MaTinhThanh", "TenTinh");
            if (!tinhThanhId.HasValue)
            {
                ViewData["XaPhuongId"] = new SelectList(Enumerable.Empty<SelectListItem>());
                return;
            }

                var xaList = await _context.XaPhuongs
                    .Where(x => x.TinhThanhId == tinhThanhId.Value)
                    .OrderBy(x => x.TenXaPhuong)
                    .ToListAsync();

                ViewData["XaPhuongId"] = new SelectList(xaList, "MaXaPhuong", "TenXaPhuong", selectedXa);
            }

        // API cho dropdown cascade
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

        public async Task<IActionResult> Index(string? searchString)
        {
            var query = _context.DiaDiems
                .Include(d => d.XaPhuong)
                    .ThenInclude(x => x!.TinhThanh)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(d =>
                    d.MaDiaDiem.Contains(searchString) ||
                    d.TenDiaDiem.Contains(searchString) ||
                    (d.XaPhuong != null && d.XaPhuong.TenXaPhuong.Contains(searchString)) ||
                    (d.XaPhuong != null && d.XaPhuong.TinhThanh != null && d.XaPhuong.TinhThanh.TenTinh.Contains(searchString)));
            }

            var list = await query
                .OrderByDescending(d => d.ConSuDung)
                .ThenBy(d => d.TenDiaDiem)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var diaDiem = await _context.DiaDiems
                .Include(d => d.XaPhuong)
                    .ThenInclude(x => x!.TinhThanh)
                .FirstOrDefaultAsync(m => m.MaDiaDiem == id);

            if (diaDiem == null) return NotFound();
            return View(diaDiem);
        }

        public async Task<IActionResult> Create()
        {
            var model = new DiaDiem { MaDiaDiem = MaSoHelper.TaoMa("MDD"), ConSuDung = true };

            LoadTinhDropdown();
            await LoadXaDropdownAsync(null);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiaDiem diaDiem, int? TinhThanhId)
        {
            if (string.IsNullOrWhiteSpace(diaDiem.MaDiaDiem))
                diaDiem.MaDiaDiem = MaSoHelper.TaoMa("MDD");

            if (!TinhThanhId.HasValue)
                ModelState.AddModelError("TinhThanhId", "Vui lòng chọn Tỉnh/Thành.");

            if (TinhThanhId.HasValue)
            {
                var xa = await _context.XaPhuongs.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MaXaPhuong == diaDiem.XaPhuongId);

                if (xa == null)
                    ModelState.AddModelError(nameof(DiaDiem.XaPhuongId), "Xã/Phường không hợp lệ.");
                else if (xa.TinhThanhId != TinhThanhId.Value)
                    ModelState.AddModelError(nameof(DiaDiem.XaPhuongId), "Xã/Phường không thuộc Tỉnh/Thành đã chọn.");
            }

            if (ModelState.IsValid)
            {
                if (_context.DiaDiems.Any(e => e.MaDiaDiem == diaDiem.MaDiaDiem))
                {
                    ModelState.AddModelError(nameof(DiaDiem.MaDiaDiem), "Mã địa điểm này đã tồn tại.");
                }
                else
                {
                    _context.Add(diaDiem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            LoadTinhDropdown(TinhThanhId);
            await LoadXaDropdownAsync(TinhThanhId, diaDiem.XaPhuongId);
            ViewBag.SelectedTinhThanhId = TinhThanhId;

            return View(diaDiem);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var diaDiem = await _context.DiaDiems
                .Include(d => d.XaPhuong)
                    .ThenInclude(x => x!.TinhThanh)
                .FirstOrDefaultAsync(d => d.MaDiaDiem == id);

            if (diaDiem == null) return NotFound();

            var tinhId = diaDiem.XaPhuong?.TinhThanhId;

            LoadTinhDropdown(tinhId);
            await LoadXaDropdownAsync(tinhId, diaDiem.XaPhuongId);

            ViewBag.SelectedTinhThanhId = tinhId;

            return View(diaDiem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DiaDiem diaDiem, int? TinhThanhId)
        {
            if (id != diaDiem.MaDiaDiem) return NotFound();

            if (!TinhThanhId.HasValue)
                ModelState.AddModelError("TinhThanhId", "Vui lòng chọn Tỉnh/Thành.");

            if (TinhThanhId.HasValue)
            {
                var xa = await _context.XaPhuongs.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MaXaPhuong == diaDiem.XaPhuongId);

                if (xa == null)
                    ModelState.AddModelError(nameof(DiaDiem.XaPhuongId), "Xã/Phường không hợp lệ.");
                else if (xa.TinhThanhId != TinhThanhId.Value)
                    ModelState.AddModelError(nameof(DiaDiem.XaPhuongId), "Xã/Phường không thuộc Tỉnh/Thành đã chọn.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(diaDiem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DiaDiems.Any(e => e.MaDiaDiem == diaDiem.MaDiaDiem)) return NotFound();
                    throw;
                }
            }

            LoadTinhDropdown(TinhThanhId);
            await LoadXaDropdownAsync(TinhThanhId, diaDiem.XaPhuongId);
            ViewBag.SelectedTinhThanhId = TinhThanhId;

            return View(diaDiem);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var diaDiem = await _context.DiaDiems
                .Include(d => d.XaPhuong)
                    .ThenInclude(x => x!.TinhThanh)
                .FirstOrDefaultAsync(m => m.MaDiaDiem == id);

            if (diaDiem == null) return NotFound();
            return View(diaDiem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var diaDiem = await _context.DiaDiems.FindAsync(id);
            if (diaDiem != null)
            {
                _context.DiaDiems.Remove(diaDiem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // ✅ API: AI Gợi ý thông tin địa điểm
        [HttpGet]
        public async Task<IActionResult> SuggestLocationInfo(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName)) return Json(new { });
            var json = await _aiService.SuggestLocationDetailAsync(locationName);
            return Content(json, "application/json");
        }
        // ✅ API: Smart Auto Create Location from Index
        [HttpPost]
        public async Task<IActionResult> AutoCreateLocation(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName)) return Json(new { success = false, message = "Vui lòng nhập tên địa điểm" });

            // 1. Ask AI
            var jsonStr = await _aiService.SuggestLocationDetailAsync(locationName);
            // Parse using Case Insensitive
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            AI_LocationDto? info = null;
            try { info = JsonSerializer.Deserialize<AI_LocationDto>(jsonStr, options); } catch { }

            if (info == null) return Json(new { success = false, message = "AI không trả về kết quả hợp lệ." });
            
            // Default names if AI fails
            var pName = !string.IsNullOrWhiteSpace(info.TinhThanh) ? info.TinhThanh : "Chưa xác định";
            var xName = !string.IsNullOrWhiteSpace(info.XaPhuong) ? info.XaPhuong : "Trung tâm";

            try 
            {
                // 2. Handle Province (TinhThanh)
                // Tim gan dung
                var tinh = await _context.TinhThanhs
                    .FirstOrDefaultAsync(t => t.TenTinh.ToLower().Contains(pName.ToLower()) || pName.ToLower().Contains(t.TenTinh.ToLower()));

                if (tinh == null)
                {
                    // Create new Province
                    int maxId = await _context.TinhThanhs.AnyAsync() ? await _context.TinhThanhs.MaxAsync(t => t.MaTinhThanh) : 0;
                    tinh = new TinhThanh { MaTinhThanh = maxId + 1, TenTinh = pName };
                    _context.TinhThanhs.Add(tinh);
                    await _context.SaveChangesAsync();
                }

                // 3. Handle Ward (XaPhuong)
                var xa = await _context.XaPhuongs
                    .FirstOrDefaultAsync(x => x.TinhThanhId == tinh.MaTinhThanh && 
                        (x.TenXaPhuong.ToLower().Contains(xName.ToLower()) || xName.ToLower().Contains(x.TenXaPhuong.ToLower())));

                if (xa == null)
                {
                    // Create new Ward
                    // ID Strategy: TinhID * 100 + Counter? Or simple Max?
                    // Safe approach: Get max ID of all XaPhuongs? No, usually ID is global or composite.
                    // Let's assume global Unique ID for simplicity or Max ID + 1 to avoid collision.
                    // Checking existing seed strategy: (MaTinh * 100) + counter. This suggests ID might heavily depend on Tinh.
                    // But standard int ID suggests just unique is fine.
                    // Let's safe pick: Max(MaXaPhuong) + 1.
                    int maxXaId = await _context.XaPhuongs.AnyAsync() ? await _context.XaPhuongs.MaxAsync(x => x.MaXaPhuong) : 0;
                    // Ensure it is at least TinhID * 100 (?) No, let's just use Max + 1 to be safe database-wise.
                    int newXaId = maxXaId + 1;
                    
                    xa = new XaPhuong 
                    { 
                        MaXaPhuong = newXaId, 
                        TenXaPhuong = xName, 
                        TinhThanhId = tinh.MaTinhThanh 
                    };
                    _context.XaPhuongs.Add(xa);
                    await _context.SaveChangesAsync();
                }

                // 4. Create Location
                var newLoc = new DiaDiem
                {
                    MaDiaDiem = MaSoHelper.TaoMa("MDD"),
                    TenDiaDiem = locationName, // Use user input name or AI? User input is safer intent.
                    XaPhuongId = xa.MaXaPhuong,
                    DiaChiChiTiet = info.DiaChi ?? "Đang cập nhật",
                    MoTa = info.MoTa ?? $"Địa điểm {locationName} tại {pName}",
                    ConSuDung = true
                };

                _context.DiaDiems.Add(newLoc);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã tạo thành công: {locationName} ({pName})", id = newLoc.MaDiaDiem });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // Helper Class
        private class AI_LocationDto {
            public string? MoTa { get; set; }
            public string? DiaChi { get; set; }
            public string? TinhThanh { get; set; }
            public string? XaPhuong { get; set; }
        }
    }
}
