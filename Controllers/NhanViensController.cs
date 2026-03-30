using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System; // Thêm System namespace cho DateTime
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Utils; // Import MaSoHelper

namespace Tour_2040.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được quản lý nhân viên
    public class NhanViensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public NhanViensController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ================== DANH SÁCH NHÂN VIÊN ==================
        public async Task<IActionResult> Index()
        {
            var nhanViens = await _context.NhanViens
                .Include(n => n.User)
                .OrderBy(n => n.HoTen)
                .ToListAsync();

            return View(nhanViens);
        }

        // ================== CHỌN USER ĐỂ DUYỆT (LỌC ỨNG VIÊN) ==================
        public async Task<IActionResult> ChonUser()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var candidates = new List<CandidateNhanVienVM>();

            // Lấy danh sách user ID đã là nhân viên rồi để loại trừ
            var existingStaffUserIds = await _context.NhanViens
                .Select(n => n.ApplicationUserId)
                .ToListAsync();

            foreach (var u in allUsers)
            {
                // Bỏ qua nếu đã có trong bảng NhanVien
                if (existingStaffUserIds.Contains(u.Id)) continue;

                var roles = await _userManager.GetRolesAsync(u);

                // Điều kiện ứng viên: Phải có role User, KHÔNG phải Staff, KHÔNG phải Admin
                var isUserOnly = roles.Contains("User") &&
                                 !roles.Contains("Staff") &&
                                 !roles.Contains("Admin");

                if (isUserOnly)
                {
                    candidates.Add(new CandidateNhanVienVM
                    {
                        UserId = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber
                    });
                }
            }

            return View(candidates);
        }

        // ================== XỬ LÝ DUYỆT USER -> NHÂN VIÊN (LOGIC QUAN TRỌNG) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFromUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // 1. CẤP ROLE STAFF TRƯỚC (Để ẩn khỏi danh sách khách hàng ngay lập tức)
            if (!await _roleManager.RoleExistsAsync("Staff"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Staff"));
            }

            if (!await _userManager.IsInRoleAsync(user, "Staff"))
            {
                await _userManager.AddToRoleAsync(user, "Staff");
            }

            // 2. TẠO HỒ SƠ NHÂN VIÊN (Chuyển sang mã NV)
            var existingNv = await _context.NhanViens.FirstOrDefaultAsync(n => n.ApplicationUserId == user.Id);
            if (existingNv == null)
            {
                var nv = new NhanVien
                {
                    MaNhanVien = MaSoHelper.TaoMa("NV"), // Sinh mã NV tự động (NVxxxx)
                    ApplicationUserId = user.Id,
                    HoTen = user.HoTen ?? user.UserName ?? "Nhân viên mới",
                    Email = user.Email,
                    SoDienThoai = user.PhoneNumber,
                    NgayVaoLam = DateTime.Now,
                    ConLamViec = true,
                    ChucVu = "Nhân viên" // Chức vụ mặc định
                };

                _context.NhanViens.Add(nv);
            }

            // 3. XỬ LÝ HỒ SƠ KHÁCH HÀNG CŨ (KH...)
            var oldKh = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == user.Email);

            if (oldKh != null)
            {
                // Kiểm tra xem khách này đã từng mua tour chưa
                var hasHistory = await _context.GiaoDiches.AnyAsync(g => g.KhachHangId == oldKh.MaTenKhachHang);

                if (!hasHistory)
                {
                    // TRƯỜNG HỢP 1: Chưa mua gì -> Xóa vĩnh viễn hồ sơ KH (Chuyển hẳn sang NV)
                    _context.KhachHangs.Remove(oldKh);
                }
                else
                {
                    // TRƯỜNG HỢP 2: Đã mua tour -> Giữ lại để lưu lịch sử, nhưng đánh dấu nội bộ
                    // (Hồ sơ này sẽ tự ẩn bên ds Khách hàng do code lọc Role ở Controller kia)
                    oldKh.NhomKhach = "Nội bộ (Staff)";
                    _context.Update(oldKh);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Đã duyệt {user.HoTen} lên làm Nhân viên. Mã NV mới đã được tạo.";

            // Quay lại trang chọn user để duyệt tiếp người khác nếu cần
            return RedirectToAction(nameof(ChonUser));
        }

        // ================== CẬP NHẬT THÔNG TIN NHÂN VIÊN ==================
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var nv = await _context.NhanViens
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            if (nv == null) return NotFound();
            return View(nv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NhanVien model)
        {
            if (id != model.MaNhanVien) return NotFound();

            // Bỏ qua validate User object
            ModelState.Remove(nameof(model.User));

            if (!ModelState.IsValid) return View(model);

            try
            {
                _context.NhanViens.Update(model);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Cập nhật thông tin nhân viên thành công.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.NhanViens.Any(e => e.MaNhanVien == id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ================== XÓA NHÂN VIÊN ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv != null)
            {
                // Logic mở rộng: Có thể xóa Role "Staff" ở đây nếu muốn họ quay về làm dân thường
                // var user = await _userManager.FindByIdAsync(nv.ApplicationUserId);
                // if (user != null) await _userManager.RemoveFromRoleAsync(user, "Staff");

                _context.NhanViens.Remove(nv);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã xóa nhân viên khỏi hệ thống.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}   