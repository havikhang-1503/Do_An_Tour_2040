// Folder: Controllers
// File path: Controllers/ThanhToansController.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tour_2040.Models;
using Tour_2040.Data;
using Tour_2040.Utils;

namespace Tour_2040.Controllers
{
    [Authorize]
    public class ThanhToansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThanhToansController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminOrStaff =>
            User.Identity != null &&
            User.Identity.IsAuthenticated &&
            (User.IsInRole("Admin") || User.IsInRole("Staff"));

        // ================== HISTORY BY TRANSACTION CODE ==================
        public async Task<IActionResult> Index(string maGd)
        {
            if (string.IsNullOrWhiteSpace(maGd)) return NotFound();

            var giaoDich = await _context.GiaoDiches
                .Include(g => g.ThanhToans)
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.MaGiaoDich == maGd);

            if (giaoDich == null) return NotFound();

            if (!IsAdminOrStaff)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (giaoDich.UserId != userId) return Forbid();
            }

            var data = giaoDich.ThanhToans
                .OrderByDescending(t => t.NgayThanhToan)
                .ToList();

            return View(data);
        }

        // ================== START PAYMENT PAGE ==================
        [HttpGet]
        public async Task<IActionResult> BatDauThanhToan(
            string tourId,
            int slNguoiLon,
            int slTreEm,
            string hoTen,
            string soDienThoai,
            string email,
            string cccd,
            string diaChi,
            string? ghiChu = null,
            string? ngaySinh = null,
            string? anhCCCD = null)
        {
            if (string.IsNullOrEmpty(tourId)) return NotFound();

            // ✅ Fallback: nếu trang trước gửi SoNguoiLon/SoTreEm (khác tên slNguoiLon/slTreEm)
            // thì binder sẽ default 0. Ta đọc thêm từ Query để “cứu” dữ liệu.
            if (slNguoiLon == 0 && int.TryParse(Request.Query["SoNguoiLon"], out var qAdult1)) slNguoiLon = qAdult1;
            if (slTreEm == 0 && int.TryParse(Request.Query["SoTreEm"], out var qChild1)) slTreEm = qChild1;

            if (slNguoiLon == 0 && int.TryParse(Request.Query["soNguoiLon"], out var qAdult2)) slNguoiLon = qAdult2;
            if (slTreEm == 0 && int.TryParse(Request.Query["soTreEm"], out var qChild2)) slTreEm = qChild2;

            if (slNguoiLon < 0 || slTreEm < 0)
                return BadRequest("Số người không hợp lệ.");

            var totalPax = slNguoiLon + slTreEm;
            if (totalPax <= 0)
                return BadRequest("Vui lòng chọn ít nhất 1 người để đặt tour.");

            var tour = await _context.Tours
                .Include(t => t.TourDichVus).ThenInclude(td => td.DichVu)
                .FirstOrDefaultAsync(t => t.MaTour == tourId);

            if (tour == null) return NotFound();

            var dichVus = tour.TourDichVus?
                .Where(td => td.DichVu != null)
                .Select(td => td.DichVu!)
                .ToList() ?? new List<DichVu>();

            var tongTienDichVu = dichVus.Sum(d => d.DonGia);
            var tongTienDuKien = (tour.GiaTour * totalPax) + tongTienDichVu;

            // ✅ PARSE ngày sinh nếu có
            DateTime? parsedNgaySinh = null;
            if (!string.IsNullOrEmpty(ngaySinh))
            {
                if (DateTime.TryParse(ngaySinh, out var tempDate))
                {
                    parsedNgaySinh = tempDate;
                }
            }

            var vm = new StartThanhToanViewModel
            {
                MaTour = tour.MaTour,
                TenTour = tour.TenTour,
                DiaDiem = tour.DiaDiem,
                NgayDi = tour.NgayDi,
                NgayVe = tour.NgayVe,

                HoTenNguoiDaiDien = hoTen,
                SoDienThoai = soDienThoai,
                Email = email,
                CCCD = cccd,
                DiaChiLienHe = diaChi,

                AnhCCCDUrl = anhCCCD,
                NgaySinh = parsedNgaySinh,
                GhiChu = ghiChu,

                SoNguoiLon = slNguoiLon,
                SoTreEm = slTreEm,

                TongTienDichVu = tongTienDichVu,
                TongTienDuKien = tongTienDuKien
            };

            ViewBag.DichVus = dichVus;
            ViewBag.GhiChu = ghiChu;
            ViewBag.NgaySinh = parsedNgaySinh;

            // Load Vouchers
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var now = DateTime.Now;
                var myVouchers = await _context.UserVouchers
                    .Include(uv => uv.Voucher)
                    .Where(uv =>
                        uv.UserId == userId &&
                        !uv.IsUsed &&
                        uv.Voucher != null &&
                        uv.Voucher.IsActive &&
                        uv.Voucher.TrangThaiDuyet == "Đã duyệt" &&
                        (!uv.Voucher.NgayBatDau.HasValue || uv.Voucher.NgayBatDau <= now) &&
                        (!uv.Voucher.NgayKetThuc.HasValue || uv.Voucher.NgayKetThuc >= now))
                    .ToListAsync();

                ViewBag.MyVouchers = myVouchers;
            }

            return View("BatDauThanhToan", vm);
        }

        private async Task LoadVouchersToViewBagAsync(string userId)
        {
            var now = DateTime.Now;
            var myVouchers = await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .Where(uv =>
                    uv.UserId == userId &&
                    !uv.IsUsed &&
                    uv.Voucher != null &&
                    uv.Voucher.IsActive &&
                    uv.Voucher.TrangThaiDuyet == "Đã duyệt" &&
                    (!uv.Voucher.NgayBatDau.HasValue || uv.Voucher.NgayBatDau <= now) &&
                    (!uv.Voucher.NgayKetThuc.HasValue || uv.Voucher.NgayKetThuc >= now))
                .ToListAsync();

            ViewBag.MyVouchers = myVouchers;
        }

        // ================== PROCESS PAYMENT ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(StartThanhToanViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Forbid();

            // ✅ VALIDATION: chặn đơn 0 khách
            var totalPax = model.SoNguoiLon + model.SoTreEm;
            if (totalPax <= 0)
            {
                ModelState.AddModelError("", "Số khách phải lớn hơn 0 (ít nhất 1 người).");
            }

            // ✅ Load tour + dịch vụ để tính tiền lại (đừng tin hidden input)
            var tour = await _context.Tours
                .Include(t => t.TourDichVus).ThenInclude(td => td.DichVu)
                .FirstOrDefaultAsync(t => t.MaTour == model.MaTour);

            if (tour == null) return NotFound();

            var dichVus = tour.TourDichVus?
                .Where(td => td.DichVu != null)
                .Select(td => td.DichVu!)
                .ToList() ?? new List<DichVu>();

            var tongTienDichVu = dichVus.Sum(d => d.DonGia);
            var tongTienDuKien = (tour.GiaTour * totalPax) + tongTienDichVu;

            // Update lại model để view hiển thị đúng nếu có lỗi
            model.TongTienDichVu = tongTienDichVu;
            model.TongTienDuKien = tongTienDuKien;

            if (!ModelState.IsValid)
            {
                ViewBag.DichVus = dichVus;
                await LoadVouchersToViewBagAsync(userId);
                return View("BatDauThanhToan", model);
            }

            // 1. Tính toán giảm giá voucher (dựa trên tongTienDuKien server tính)
            decimal giam = 0m;
            UserVoucher? usedUserVoucher = null;

            if (!string.IsNullOrEmpty(model.SelectedUserVoucherId))
            {
                usedUserVoucher = await _context.UserVouchers
                    .Include(x => x.Voucher)
                    .FirstOrDefaultAsync(x =>
                        x.MaUserVoucher == model.SelectedUserVoucherId &&
                        x.UserId == userId &&
                        !x.IsUsed);

                if (usedUserVoucher?.Voucher != null)
                {
                    var v = usedUserVoucher.Voucher;
                    giam = (v.LoaiGiam == "PhanTram")
                        ? (tongTienDuKien * v.GiaTriGiam / 100m)
                        : v.GiaTriGiam;

                    if (v.SoTienGiamToiDa.HasValue && giam > v.SoTienGiamToiDa.Value)
                        giam = v.SoTienGiamToiDa.Value;
                }
            }

            var finalTotal = Math.Max(tongTienDuKien - giam, 0);

            // 2. Lưu Giao dịch
            var maGd = MaSoHelper.TaoMa("MGD");
            var giaoDich = new GiaoDich
            {
                MaGiaoDich = maGd,
                UserId = userId,
                TourId = tour.MaTour,
                NgayTao = DateTime.Now,
                TongTien = finalTotal,
                TrangThai = "Hoàn thành",

                // ✅ pax
                SoNguoiLon = model.SoNguoiLon,
                SoTreEm = model.SoTreEm,

                // Snapshot khách
                HoTenNguoiDaiDien = model.HoTenNguoiDaiDien,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                CCCD = model.CCCD,
                DiaChiLienHe = model.DiaChiLienHe,
                NgaySinh = model.NgaySinh,
                AnhCCCDUrl = model.AnhCCCDUrl,
                GhiChu = model.GhiChu
            };
            _context.GiaoDiches.Add(giaoDich);

            // 3. Lưu Thanh toán
            var thanhToan = new ThanhToan
            {
                MaThanhToan = MaSoHelper.TaoMa("MTT"),
                GiaoDichId = maGd,
                SoTien = finalTotal,
                PhuongThuc = model.PhuongThucThanhToan,
                NgayThanhToan = DateTime.Now,
                TrangThai = "Thành công"
            };
            _context.ThanhToans.Add(thanhToan);

            // 4. Lưu Hợp đồng
            _context.HopDongs.Add(new HopDong
            {
                MaHopDong = MaSoHelper.TaoMa("MHD"),
                GiaoDichId = maGd,
                TourId = tour.MaTour,
                NgayTao = DateTime.Now,
                TrangThai = "Đã ký"
            });

            // ✅ 5. Lưu Lịch trình (FIX CHÍNH: gán SoNguoiLon/SoTreEm + link GiaoDichId)
            _context.LichTrinhs.Add(new LichTrinh
            {
                MaLichTrinh = MaSoHelper.TaoMa("MLT"),
                TourId = tour.MaTour,
                UserId = userId,
                NgayDat = DateTime.Now,
                TongTien = finalTotal,

                // ✅ FIX: Số người không còn = 0
                SoNguoiLon = model.SoNguoiLon,
                SoTreEm = model.SoTreEm,

                // ✅ Link giao dịch để sau này truy vết/đối soát
                GiaoDichId = maGd,

                // Gợi ý flow: đặt xong thường là chờ admin xác nhận
                TrangThai = "Chờ xác nhận"
            });

            // 6. Cập nhật Tour & Voucher
            tour.SoNguoiHienTai = (tour.SoNguoiHienTai ?? 0) + totalPax;

            if (usedUserVoucher != null)
            {
                usedUserVoucher.IsUsed = true;
                usedUserVoucher.Voucher!.DaSuDung++;
            }

            await _context.SaveChangesAsync();

            // CHUYỂN HƯỚNG SANG TRANG ĐANG THANH TOÁN
            return RedirectToAction("DangThanhToan", new { maGd = maGd });
        }

        [HttpGet]
        public IActionResult DangThanhToan(string maGd)
        {
            if (string.IsNullOrEmpty(maGd))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.MaGd = maGd;
            return View();
        }

        // ================== EXTERNAL PAYMENT CALLBACK (DEMO) ==================
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ThanhToanSuccess(string maGiaoDich, decimal soTien, string phuongThuc, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(maGiaoDich)) return NotFound();

            var gd = await _context.GiaoDiches
                .Include(x => x.ThanhToans)
                .Include(x => x.Tour)
                .Include(x => x.HopDongs)
                .FirstOrDefaultAsync(x => x.MaGiaoDich == maGiaoDich);

            if (gd == null) return NotFound();

            var tt = new ThanhToan
            {
                MaThanhToan = MaSoHelper.TaoMa("MTT"),
                GiaoDichId = gd.MaGiaoDich,
                SoTien = soTien,
                PhuongThuc = phuongThuc,
                GhiChu = noiDung,
                TrangThai = "Thành công",
                NgayThanhToan = DateTime.Now,
                TenTaiKhoan = "Khách Demo",
                SoTaiKhoan = "Unknown"
            };
            _context.ThanhToans.Add(tt);

            var daThanhToan = gd.ThanhToans.Sum(x => x.SoTien) + soTien;

            if (daThanhToan >= gd.TongTien && gd.TrangThai != "Hoàn thành")
            {
                gd.TrangThai = "Hoàn thành";
                if (!gd.HopDongs.Any())
                {
                    var hopDong = new HopDong
                    {
                        MaHopDong = MaSoHelper.TaoMa("MHD"),
                        GiaoDichId = gd.MaGiaoDich,
                        TourId = gd.TourId,
                        NgayTao = DateTime.Now,
                        TrangThai = "Đã ký",
                        NoiDung = "Hợp đồng tự động"
                    };
                    _context.HopDongs.Add(hopDong);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "GiaoDiches", new { maGd = maGiaoDich });
        }
    }
}
