// Folder: Controllers
// File path: Controllers/ThongKeController.cs

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Tour_2040.Data;
using Tour_2040.Models;

namespace Tour_2040.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ThongKeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================== SHARED: BUILD VIEWMODEL ==================
        private async Task<ThongKeViewModel> BuildThongKeViewModel(
            DateTime? fromDate,
            DateTime? toDate,
            string? kieuThongKe)
        {
            var today = DateTime.Today;

            var from = fromDate.HasValue ? fromDate.Value.Date : today.AddMonths(-1);
            var to = toDate.HasValue ? toDate.Value.Date.AddDays(1).AddTicks(-1) : today.AddDays(1).AddTicks(-1);
            var mode = string.IsNullOrWhiteSpace(kieuThongKe) ? "Ngay" : kieuThongKe;

            // ===== TRANSACTIONS IN PERIOD =====
            var giaoDiches = await _context.GiaoDiches
                .Include(gd => gd.Tour)
                .Where(gd => gd.NgayTao >= from && gd.NgayTao <= to)
                .ToListAsync();

            // ===== TOUR OVERVIEW =====
            var tongTour = await _context.Tours.CountAsync();
            var tongTourMacDinh = await _context.Tours.CountAsync(t => t.IsDefault);
            var tongTourCaNhan = await _context.Tours.CountAsync(t => t.IsPersonal);

            // ===== USERS / CUSTOMERS / STAFF =====
            var tongUser = await _userManager.Users.CountAsync();
            var tongKhachHang = await _context.KhachHangs.CountAsync();
            var tongNhanVien = await _context.NhanViens.CountAsync();

            // ===== CONTRACTS & SUPPORT =====
            var hopDongs = await _context.HopDongs
                .Where(hd => hd.NgayTao >= from && hd.NgayTao <= to)
                .ToListAsync();

            var tongHopDong = hopDongs.Count;

            // FIX: GiaoDichId is string
            var hopDongGiaoDichIds = hopDongs
                .Where(hd => !string.IsNullOrEmpty(hd.GiaoDichId))
                .Select(hd => hd.GiaoDichId)
                .Distinct()
                .ToList();

            var soHopDongCoThanhToan = hopDongGiaoDichIds.Count;

            // FIX: Compare string MaGiaoDich
            var doanhThuTuHopDong = giaoDiches
                .Where(gd => hopDongGiaoDichIds.Contains(gd.MaGiaoDich)) // Assuming GiaoDichId in HopDong matches MaGiaoDich
                .Sum(gd => gd.TongTien);

            var tongYeuCauHoTro = await _context.YeuCauHoTros.CountAsync();
            var tongDiaDiem = await _context.DiaDiems.CountAsync();
            var tongDichVu = await _context.DichVus.CountAsync();

            // ===== TOUR REVENUE BY TYPE =====
            // FIX: Use MaTour (string) for comparison
            var macDinhIds = await _context.Tours
                .Where(t => t.IsDefault)
                .Select(t => t.MaTour)
                .ToListAsync();

            var caNhanIds = await _context.Tours
                .Where(t => t.IsPersonal)
                .Select(t => t.MaTour)
                .ToListAsync();

            // Note: GiaoDich.TourId stores MaTour string
            decimal doanhThuMacDinh = giaoDiches
                .Where(gd => !string.IsNullOrEmpty(gd.TourId) && macDinhIds.Contains(gd.TourId))
                .Sum(gd => gd.TongTien);

            decimal doanhThuCaNhan = giaoDiches
                .Where(gd => !string.IsNullOrEmpty(gd.TourId) && caNhanIds.Contains(gd.TourId))
                .Sum(gd => gd.TongTien);

            // ===== SERVICE STATS =====
            var dichVuStats = await _context.DichVus
                .GroupBy(d => d.LoaiDichVu)
                .Select(g => new ThongKeDichVuItem
                {
                    LoaiDichVu = g.Key,
                    SoLuong = g.Count(),
                    TongDonGia = g.Sum(x => x.DonGia)
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            // ===== REVENUE BY TIME =====
            var doanhThuTheoMoc = mode switch
            {
                "Nam" => giaoDiches.GroupBy(gd => gd.NgayTao.Year).OrderBy(g => g.Key)
                            .Select(g => new ThongKeDoanhThuItem { Nhan = g.Key.ToString(), TongTien = g.Sum(x => x.TongTien) }).ToList(),
                "Thang" => giaoDiches.GroupBy(gd => new { gd.NgayTao.Year, gd.NgayTao.Month }).OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                            .Select(g => new ThongKeDoanhThuItem { Nhan = $"{g.Key.Year:D4}-{g.Key.Month:D2}", TongTien = g.Sum(x => x.TongTien) }).ToList(),
                _ => giaoDiches.GroupBy(gd => gd.NgayTao.Date).OrderBy(g => g.Key)
                            .Select(g => new ThongKeDoanhThuItem { Nhan = g.Key.ToString("dd/MM/yyyy"), TongTien = g.Sum(x => x.TongTien) }).ToList(),
            };

            return new ThongKeViewModel
            {
                FromDate = from,
                ToDate = to,
                KieuThongKe = mode,
                TongSoTour = tongTour,
                TongSoTourMacDinh = tongTourMacDinh,
                TongSoTourCaNhan = tongTourCaNhan,
                TongSoKhachHang = tongKhachHang,
                TongSoNhanVien = tongNhanVien,
                TongSoUser = tongUser,
                TongSoGiaoDich = giaoDiches.Count,
                TongDoanhThu = giaoDiches.Sum(gd => gd.TongTien),
                TongSoHopDong = tongHopDong,
                SoHopDongCoThanhToan = soHopDongCoThanhToan,
                DoanhThuTuHopDong = doanhThuTuHopDong,
                TongSoYeuCauHoTro = tongYeuCauHoTro,
                TongSoDiaDiem = tongDiaDiem,
                TongSoDichVu = tongDichVu,
                DoanhThuTourMacDinh = doanhThuMacDinh,
                DoanhThuTourCaNhan = doanhThuCaNhan,
                ThongKeDichVus = dichVuStats,
                DoanhThuTheoMoc = doanhThuTheoMoc
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? kieuThongKe)
        {
            var vm = await BuildThongKeViewModel(fromDate, toDate, kieuThongKe);
            return View(vm);
        }

        // ================== SERVICE DETAILS (BY TYPE) ==================
        [HttpGet]
        public async Task<IActionResult> ChiTietDichVu(string loaiDichVu, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(loaiDichVu)) return RedirectToAction(nameof(Index));

            var from = fromDate == default ? DateTime.Today.AddMonths(-1) : fromDate.Date;
            var to = toDate == default ? DateTime.Today.AddDays(1).AddTicks(-1) : toDate.Date.AddDays(1).AddTicks(-1);

            // FIX: Join logic for string keys (TourId, DichVuId)
            var query = _context.GiaoDiches
                .Include(g => g.Tour)
                .Include(g => g.User)
                .Where(g => g.NgayTao >= from && g.NgayTao <= to)
                .Where(g => !string.IsNullOrEmpty(g.TourId))
                .Join(
                    _context.TourDichVus,
                    gd => gd.TourId,
                    td => td.TourId,
                    (gd, td) => new { gd, td }
                )
                .Join(
                    _context.DichVus,
                    temp => temp.td.DichVuId,
                    dv => dv.MaDichVu, // Using MaDichVu (string)
                    (temp, dv) => new { temp.gd, dv }
                )
                .Where(x => x.dv.LoaiDichVu == loaiDichVu);

            var raw = await query.ToListAsync();

            var items = raw
                .GroupBy(x => x.gd.MaGiaoDich) // Group by string transaction code
                .Select(g => {
                    var gd = g.First().gd;
                    return new ThongKeDichVuChiTietItem
                    {
                        MaGiaoDich = gd.MaGiaoDich,
                        NgayTao = gd.NgayTao,
                        TenTour = gd.Tour?.TenTour ?? "(Không rõ tour)",
                        TenKhach = gd.User?.HoTen ?? gd.User?.UserName ?? "(Không rõ khách)",
                        SoTien = gd.TongTien
                    };
                })
                .OrderByDescending(x => x.NgayTao)
                .ToList();

            return View(new ThongKeDichVuChiTietViewModel
            {
                LoaiDichVu = loaiDichVu,
                FromDate = from,
                ToDate = to,
                TongSoGiaoDich = items.Count,
                TongTien = items.Sum(x => x.SoTien),
                Items = items
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(DateTime? fromDate, DateTime? toDate, string? kieuThongKe)
        {
            var vm = await BuildThongKeViewModel(fromDate, toDate, kieuThongKe);
            var sb = new StringBuilder();
            sb.AppendLine("Label,Revenue");
            foreach (var item in vm.DoanhThuTheoMoc)
            {
                sb.AppendLine($"{item.Nhan},{item.TongTien}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"thongke_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
    }
}