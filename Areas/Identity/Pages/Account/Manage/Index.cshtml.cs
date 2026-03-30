using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tour_2040.Models;

namespace Tour_2040.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment; // Dùng để lấy đường dẫn wwwroot

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        public string Username { get; set; } = string.Empty;
        public string MaNguoiDung { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string? AnhCCCDUrl { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Họ tên không được để trống")]
            [Display(Name = "Họ và tên")]
            public string? HoTen { get; set; }

            [Phone]
            [Display(Name = "Số điện thoại")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Số CCCD/Định danh")]
            public string? CCCD { get; set; }

            [Display(Name = "Ngày sinh")]
            [DataType(DataType.Date)]
            public DateTime? NgaySinh { get; set; }

            // Nhận file từ Form
            [Display(Name = "Ảnh đại diện")]
            public IFormFile? AvatarFile { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Username = user.UserName ?? string.Empty;
            MaNguoiDung = user.MaNguoiDung ?? "N/A";
            ProfilePictureUrl = user.ProfilePictureUrl;
            AnhCCCDUrl = user.AnhCCCDUrl;

            Input = new InputModel
            {
                HoTen = user.HoTen,
                PhoneNumber = user.PhoneNumber,
                CCCD = user.CCCD,
                NgaySinh = user.NgaySinh
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Không tìm thấy người dùng.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Không tìm thấy người dùng.");

            // 1. XỬ LÝ UPLOAD ẢNH (Nếu người dùng có chọn file)
            if (Input.AvatarFile != null)
            {
                // Tạo đường dẫn đến wwwroot/uploads/avatars
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

                // Nếu thư mục chưa có thì tạo mới
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file duy nhất (Guid) để tránh trùng lặp
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.AvatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vào server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.AvatarFile.CopyToAsync(fileStream);
                }

                // Cập nhật đường dẫn vào Database
                user.ProfilePictureUrl = "/uploads/avatars/" + uniqueFileName;
            }

            // 2. CẬP NHẬT THÔNG TIN CÁ NHÂN
            user.HoTen = Input.HoTen;
            user.PhoneNumber = Input.PhoneNumber;
            user.CCCD = Input.CCCD;
            user.NgaySinh = Input.NgaySinh;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                StatusMessage = "Lỗi: Không thể cập nhật hồ sơ.";
                return RedirectToPage();
            }

            // Refresh lại phiên đăng nhập để cập nhật thông tin mới lên Cookie (Header)
            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Cập nhật hồ sơ thành công!";
            return RedirectToPage();
        }
    }
}