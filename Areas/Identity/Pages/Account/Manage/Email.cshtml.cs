// Folder: Areas/Identity/Pages/Account/Manage
// File path: Areas/Identity/Pages/Account/Manage/Email.cshtml.cs
// File name: Email.cshtml.cs
// Class: EmailModel
// Label: Identity-Manage-EmailModel
// Mô tả: Quản lý email đăng nhập, đơn giản: đổi email trực tiếp, có StatusMessage.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Tour_2040.Models;

namespace Tour_2040.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<EmailModel> _logger;

        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<EmailModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [Display(Name = "Email hiện tại")]
        public string Email { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email mới")]
            public string NewEmail { get; set; } = string.Empty;
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email ?? string.Empty;
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy user hiện tại.");
            }

            await LoadAsync(user);
            // Mặc định: điền sẵn email hiện tại vào field NewEmail
            Input = new InputModel { NewEmail = Email };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy user hiện tại.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var currentEmail = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail == currentEmail)
            {
                StatusMessage = "Email mới trùng với email hiện tại.";
                return RedirectToPage();
            }

            // Cập nhật email + username = email mới
            var setEmailResult = await _userManager.SetEmailAsync(user, Input.NewEmail);
            if (!setEmailResult.Succeeded)
            {
                StatusMessage = "Có lỗi khi cập nhật email.";
                return RedirectToPage();
            }

            user.UserName = Input.NewEmail;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User đã cập nhật email.");

            StatusMessage = "Đã cập nhật email đăng nhập.";
            return RedirectToPage();
        }
    }
}
