// Folder: Controllers
// File path: Controllers/TinhThanhsController.cs

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Data;
using Tour_2040.Models;

namespace Tour_2040.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class TinhThanhsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TinhThanhsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var q = _context.TinhThanhs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.TenTinh.Contains(search) || x.MaTinhThanh.ToString().Contains(search));

            var list = await q.OrderBy(x => x.TenTinh).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.TinhThanhs.FirstOrDefaultAsync(x => x.MaTinhThanh == id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create() => View(new TinhThanh());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TinhThanh model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.TinhThanhs.AnyAsync(x => x.MaTinhThanh == model.MaTinhThanh))
                    ModelState.AddModelError(nameof(TinhThanh.MaTinhThanh), "Mã tỉnh đã tồn tại.");

                if (ModelState.IsValid)
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.TinhThanhs.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TinhThanh model)
        {
            if (id != model.MaTinhThanh) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TinhThanhs.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.TinhThanhs.FindAsync(id);
            if (item != null)
            {
                _context.TinhThanhs.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
