using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tour_2040.Models;

namespace Tour_2040.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        // ... (Giữ nguyên Constructor và các biến private) ...
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IWebHostEnvironment _environment;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập Email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập họ tên")]
            [Display(Name = "Họ tên")]
            // Regex: Chỉ chấp nhận chữ cái và khoảng trắng (Tiếng Việt OK)
            [RegularExpression(@"^[^0-9]*$", ErrorMessage = "Họ tên không được chứa số.")]
            public string? HoTen { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
            [DataType(DataType.Date)]
            [Display(Name = "Ngày sinh")]
            // Custom Validation: Đủ 16 tuổi
            [MinAge(16, ErrorMessage = "Bạn phải đủ 16 tuổi để đăng ký.")]
            public DateTime? NgaySinh { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập SĐT")]
            [Display(Name = "Số điện thoại")]
            // Regex: 10 số, bắt đầu bằng 0
            [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.")]
            [StringLength(10, MinimumLength = 10, ErrorMessage = "SĐT phải đúng 10 số.")]
            public string? PhoneNumber { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập CCCD")]
            [Display(Name = "Số CCCD")]
            // Regex: 12 số
            [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD chỉ được chứa số.")]
            [StringLength(12, MinimumLength = 12, ErrorMessage = "CCCD phải đúng 12 chữ số.")]
            public string? CCCD { get; set; }

            [Display(Name = "Ảnh CCCD")]
            public IFormFile? AnhCCCD { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        // Custom Validation Attribute: Kiểm tra tuổi
        public class MinAgeAttribute : ValidationAttribute
        {
            private int _limit;
            public MinAgeAttribute(int limit) { _limit = limit; }
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value == null) return ValidationResult.Success; // Để Required check null
                DateTime dt = (DateTime)value;
                if (dt.AddYears(_limit) > DateTime.Now) return new ValidationResult(ErrorMessage ?? "Tuổi không hợp lệ.");
                return ValidationResult.Success;
            }
        }

        public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                // 1. Upload ảnh
                string? anhCCCDUrl = null;
                if (Input.AnhCCCD != null)
                {
                    string folder = Path.Combine(_environment.WebRootPath, "uploads/cccd");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.AnhCCCD.FileName);
                    using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                    {
                        await Input.AnhCCCD.CopyToAsync(stream);
                    }
                    anhCCCDUrl = "/uploads/cccd/" + fileName;
                }

                // 2. Tạo User
                var prefix = $"{DateTime.Now:yyMM}";
                var count = await _userManager.Users.CountAsync(u => u.MaNguoiDung != null && u.MaNguoiDung.StartsWith(prefix));

                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    HoTen = Input.HoTen,
                    CCCD = Input.CCCD,
                    NgaySinh = Input.NgaySinh,
                    AnhCCCDUrl = anhCCCDUrl,
                    MaNguoiDung = prefix + (count + 1).ToString("D4")
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("User")) await _roleManager.CreateAsync(new IdentityRole("User"));
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}