using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tour_2040.Models;

namespace Tour_2040.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public string ReturnUrl { get; set; } = "/";

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập Email")]
            [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Xóa cookie bên ngoài để đảm bảo quy trình đăng nhập sạch sẽ
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        // Dùng để hứng thông báo lỗi từ các trang khác chuyển về
        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Nạp lại danh sách đăng nhập ngoài (để nếu lỗi thì form vẫn hiển thị đúng)
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                try
                {
                    // Thử đăng nhập
                    // Lưu ý: Input.Email được dùng làm UserName. Đảm bảo lúc Seed Data bạn set UserName = Email.
                    var result = await _signInManager.PasswordSignInAsync(
                        Input.Email,
                        Input.Password,
                        Input.RememberMe,
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("Tài khoản bị khóa.");
                        return RedirectToPage("./Lockout");
                    }

                    // Nếu thất bại
                    ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Kiểm tra lại Email hoặc Mật khẩu.");
                }
                catch (Exception ex)
                {
                    // Bắt lỗi hệ thống (Ví dụ: Lỗi FormatException Base64 hash trong DB)
                    _logger.LogError(ex, "Lỗi xảy ra trong quá trình đăng nhập.");
                    ModelState.AddModelError(string.Empty, "Lỗi dữ liệu hệ thống. Vui lòng liên hệ Admin.");
                }
            }

            // Nếu code chạy đến đây nghĩa là có lỗi, trả về trang cũ
            return Page();
        }
    }
}