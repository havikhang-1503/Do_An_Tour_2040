// Folder: Areas/Identity/Pages/Account/Manage
// File path: Areas/Identity/Pages/Account/Manage/TwoFactorAuthentication.cshtml.cs
// File name: TwoFactorAuthentication.cshtml.cs
// Class: TwoFactorAuthenticationModel
// Label: Identity-Manage-2FA-Model
// Mô tả: Quản lý 2FA: đọc trạng thái, quên trình duyệt, tạo mã recovery, tắt 2FA.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Tour_2040.Models;

namespace Tour_2040.Areas.Identity.Pages.Account.Manage
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;

        public TwoFactorAuthenticationModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TwoFactorAuthenticationModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public bool Is2faEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
        public int RecoveryCodesLeft { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy user hiện tại.");
            }

            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            return Page();
        }

        // POST: ForgetBrowser
        public async Task<IActionResult> OnPostForgetBrowserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy user hiện tại.");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            StatusMessage = "Trình duyệt hiện tại đã được quên. Khi đăng nhập lần sau, hệ thống sẽ hỏi mã 2FA.";
            return RedirectToPage();
        }

        // POST: GenerateRecoveryCodes
        public async Task<IActionResult> OnPostGenerateRecoveryCodesAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy user hiện tại.");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                StatusMessage = "Cần bật xác thực 2 yếu tố trước khi tạo mã khôi phục.";
                return RedirectToPage();
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodesLeft = recoveryCodes.Count();
            StatusMessage = $"Đã tạo {RecoveryCodesLeft} mã khôi phục mới. Hãy lưu trữ ở nơi an toàn.";

            return RedirectToPage();
        }

        // POST: Disable2fa
        public async Task<IActionResult> OnPostDisable2faAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy user hiện tại.");
            }

            var disableResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disableResult.Succeeded)
            {
                StatusMessage = "Không thể tắt xác thực 2 yếu tố.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Đã tắt xác thực 2 yếu tố cho tài khoản.";
            _logger.LogInformation("User đã tắt 2FA.");

            return RedirectToPage();
        }
    }
}
