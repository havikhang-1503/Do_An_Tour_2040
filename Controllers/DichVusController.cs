// Folder: Controllers
// File path: Controllers/DichVusController.cs
// File name: DichVusController.cs
// Label: C-DichVu
// Mô tả: CRUD Dịch vụ + API cascade Tỉnh -> Xã/Phường -> Địa điểm (đúng theo schema của project).
//
// FIX(A): Không dùng XaPhuong.MaTinhThanh / DiaDiem.MaXaPhuong (project em không có).
//         Dùng XaPhuong.TinhThanhId và DiaDiem.XaPhuongId (giống DiaDiemsController).
// FIX(B): API nhận linh hoạt query param: maTinh/tinhThanhId và maXa/xaPhuongId.
// FIX(C): Thêm API Trace để Edit auto set 3 dropdown nhanh: GetDiaDiemTrace(maDiaDiem).
// NOTE: Hot reload báo “Changing visibility…” thì restart app là hết (không phải lỗi).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Utils;

namespace Tour_2040.Controllers
{
    [Authorize]
    public class DichVusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DichVusController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Loại dịch vụ bắt buộc phải gắn Địa điểm
        private static readonly string[] LOAI_CAN_DIA_DIEM =
        {
            "Phương tiện",
            "Nhà hàng",
            "Khách sạn",
            "Khu vui chơi"
        };

        private bool LoaiCanDiaDiem(string? loai)
            => !string.IsNullOrWhiteSpace(loai) && LOAI_CAN_DIA_DIEM.Contains(loai);

        // ===================== API CASCADE =====================

        // GET: /DichVus/GetTinhThanhs
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTinhThanhs()
        {
            var list = await _context.TinhThanhs
                .OrderBy(t => t.TenTinh)
                .Select(t => new { id = t.MaTinhThanh, name = t.TenTinh })
                .ToListAsync();

            return Json(list);
        }

        // GET: /DichVus/GetXaPhuongsByTinh?maTinh=1  (hoặc ?tinhThanhId=1)
        // Alias endpoint giống DiaDiemsController: /DichVus/GetXaPhuongs?tinhThanhId=1
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetXaPhuongsByTinh(int? maTinh, int? tinhThanhId)
        {
            var id = maTinh ?? tinhThanhId;
            if (!id.HasValue || id.Value <= 0) return Json(Array.Empty<object>());

            // ✅ FIX: dùng XaPhuong.TinhThanhId (đúng schema)
            var list = await _context.XaPhuongs
                .Where(x => x.TinhThanhId == id.Value)
                .OrderBy(x => x.TenXaPhuong)
                .Select(x => new { id = x.MaXaPhuong, name = x.TenXaPhuong })
                .ToListAsync();

            return Json(list);
        }

        // Alias cho tiện: /DichVus/GetXaPhuongs?tinhThanhId=...
        [HttpGet]
        [AllowAnonymous]
        public Task<IActionResult> GetXaPhuongs(int tinhThanhId)
            => GetXaPhuongsByTinh(null, tinhThanhId);

        // GET: /DichVus/GetDiaDiemsByXaPhuong?maXa=2 (hoặc ?xaPhuongId=2)
        // Alias endpoint giống DiaDiemsController: /DichVus/GetDiaDiems?xaPhuongId=2
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetDiaDiemsByXaPhuong(int? maXa, int? xaPhuongId)
        {
            var id = maXa ?? xaPhuongId;
            if (!id.HasValue || id.Value <= 0) return Json(Array.Empty<object>());

            // ✅ FIX: dùng DiaDiem.XaPhuongId (đúng schema)
            var list = await _context.DiaDiems
                .Where(d => d.XaPhuongId == id.Value)
                .OrderBy(d => d.TenDiaDiem)
                .Select(d => new { id = d.MaDiaDiem, name = d.TenDiaDiem }) // id là string (MDDxxx)
                .ToListAsync();

            return Json(list);
        }

        // Alias: /DichVus/GetDiaDiems?xaPhuongId=...
        [HttpGet]
        [AllowAnonymous]
        public Task<IActionResult> GetDiaDiems(int xaPhuongId)
            => GetDiaDiemsByXaPhuong(null, xaPhuongId);

        // ✅ NEW: Trace ngược từ MaDiaDiem => trả về tinhThanhId + xaPhuongId (để Edit auto set nhanh)
        // GET: /DichVus/GetDiaDiemTrace?maDiaDiem=MDD000001
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetDiaDiemTrace(string maDiaDiem)
        {
            if (string.IsNullOrWhiteSpace(maDiaDiem))
                return Json(new { ok = false });

            var dd = await _context.DiaDiems
                .Include(d => d.XaPhuong)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDiaDiem == maDiaDiem);

            if (dd == null)
                return Json(new { ok = false });

            var xaId = dd.XaPhuongId;
            var tinhId = dd.XaPhuong?.TinhThanhId ?? 0;

            return Json(new
            {
                ok = true,
                tinhThanhId = tinhId,
                xaPhuongId = xaId,
                maDiaDiem = dd.MaDiaDiem
            });
        }

        // ===================== CRUD =====================

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchString, string? loai, string? trangThai)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["LoaiFilter"] = loai;
            ViewData["TrangThaiFilter"] = trangThai;

            var query = _context.DichVus
                .Include(d => d.DiaDiem)
                    .ThenInclude(dd => dd!.XaPhuong)
                        .ThenInclude(xp => xp!.TinhThanh)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(s =>
                    s.TenDichVu.Contains(searchString) ||
                    s.MaDichVu.Contains(searchString) ||
                    (s.DiaDiem != null && s.DiaDiem.TenDiaDiem.Contains(searchString)));
            }

            if (!string.IsNullOrWhiteSpace(loai))
                query = query.Where(d => d.LoaiDichVu == loai);

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            var list = await query.OrderBy(d => d.MaTenDichVu).ToListAsync();
            return View(list);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var dichVu = await _context.DichVus
                .Include(d => d.DiaDiem)
                    .ThenInclude(dd => dd!.XaPhuong)
                        .ThenInclude(xp => xp!.TinhThanh)
                .FirstOrDefaultAsync(d => d.MaTenDichVu == id);

            if (dichVu == null) return NotFound();
            return View(dichVu);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new DichVu
            {
                MaDichVu = MaSoHelper.TaoMa("MDV"),
                TrangThai = "Active",
                HieuLucTu = DateTime.Now
            };
            model.MaTenDichVu = model.MaDichVu;

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DichVu model, IFormFile? HinhAnhFile)
        {
            if (string.IsNullOrWhiteSpace(model.MaDichVu))
                model.MaDichVu = MaSoHelper.TaoMa("MDV");

            model.MaTenDichVu = model.MaDichVu;

            if (LoaiCanDiaDiem(model.LoaiDichVu) && string.IsNullOrWhiteSpace(model.MaDiaDiem))
                ModelState.AddModelError(nameof(DichVu.MaDiaDiem), "Loại dịch vụ này bắt buộc phải chọn Địa điểm.");

            if (await _context.DichVus.AnyAsync(d => d.MaTenDichVu == model.MaTenDichVu))
                ModelState.AddModelError(nameof(DichVu.MaDichVu), "Mã dịch vụ này đã tồn tại.");

            if (ModelState.IsValid)
            {
                if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(HinhAnhFile.FileName)}";
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "dichvu");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await HinhAnhFile.CopyToAsync(stream);

                    model.HinhAnhUrl = $"/images/dichvu/{fileName}";
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var dichVu = await _context.DichVus.FindAsync(id);
            if (dichVu == null) return NotFound();

            return View(dichVu);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DichVu model, IFormFile? HinhAnhFile)
        {
            if (id != model.MaTenDichVu) return NotFound();

            if (LoaiCanDiaDiem(model.LoaiDichVu) && string.IsNullOrWhiteSpace(model.MaDiaDiem))
                ModelState.AddModelError(nameof(DichVu.MaDiaDiem), "Loại dịch vụ này bắt buộc phải chọn Địa điểm.");

            if (ModelState.IsValid)
            {
                if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(HinhAnhFile.FileName)}";
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "dichvu");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await HinhAnhFile.CopyToAsync(stream);

                    model.HinhAnhUrl = $"/images/dichvu/{fileName}";
                }
                else
                {
                    _context.Entry(model).Property(x => x.HinhAnhUrl).IsModified = false;
                }

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var dichVu = await _context.DichVus
                .Include(d => d.DiaDiem)
                .FirstOrDefaultAsync(m => m.MaTenDichVu == id);

            if (dichVu == null) return NotFound();
            return View(dichVu);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var dichVu = await _context.DichVus.FindAsync(id);
            if (dichVu != null)
            {
                _context.DichVus.Remove(dichVu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
