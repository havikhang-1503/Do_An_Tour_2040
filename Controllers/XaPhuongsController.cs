// Folder: Controllers
// File path: Controllers/XaPhuongsController.cs

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Data;
using Tour_2040.Models;

namespace Tour_2040.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class XaPhuongsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public XaPhuongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void LoadTinh(int? selected = null)
        {
            ViewData["TinhThanhId"] = new SelectList(
                _context.TinhThanhs.OrderBy(x => x.TenTinh),
                "MaTinhThanh", "TenTinh", selected
            );
        }

        public async Task<IActionResult> Index(int? tinhThanhId, string? search)
        {
            LoadTinh(tinhThanhId);

            var q = _context.XaPhuongs
                .Include(x => x.TinhThanh)
                .AsQueryable();

            if (tinhThanhId.HasValue)
                q = q.Where(x => x.TinhThanhId == tinhThanhId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.TenXaPhuong.Contains(search) || x.MaXaPhuong.ToString().Contains(search));

            var list = await q.OrderBy(x => x.TinhThanh!.TenTinh).ThenBy(x => x.TenXaPhuong).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.XaPhuongs
                .Include(x => x.TinhThanh)
                .FirstOrDefaultAsync(x => x.MaXaPhuong == id);

            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            LoadTinh(null);
            return View(new XaPhuong());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(XaPhuong model)
        {
            LoadTinh(model.TinhThanhId);

            if (ModelState.IsValid)
            {
                if (!await _context.TinhThanhs.AnyAsync(t => t.MaTinhThanh == model.TinhThanhId))
                    ModelState.AddModelError(nameof(XaPhuong.TinhThanhId), "Tỉnh/Thành không hợp lệ.");

                if (await _context.XaPhuongs.AnyAsync(x => x.MaXaPhuong == model.MaXaPhuong))
                    ModelState.AddModelError(nameof(XaPhuong.MaXaPhuong), "Mã xã/phường đã tồn tại.");

                if (ModelState.IsValid)
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { tinhThanhId = model.TinhThanhId });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.XaPhuongs.FindAsync(id);
            if (item == null) return NotFound();

            LoadTinh(item.TinhThanhId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, XaPhuong model)
        {
            if (id != model.MaXaPhuong) return NotFound();

            LoadTinh(model.TinhThanhId);

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { tinhThanhId = model.TinhThanhId });
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.XaPhuongs
                .Include(x => x.TinhThanh)
                .FirstOrDefaultAsync(x => x.MaXaPhuong == id);

            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.XaPhuongs.FindAsync(id);
            if (item != null)
            {
                var tinhId = item.TinhThanhId;
                _context.XaPhuongs.Remove(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { tinhThanhId = tinhId });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
