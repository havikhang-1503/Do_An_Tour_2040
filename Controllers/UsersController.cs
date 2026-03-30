// Folder: Controllers
// File path: Controllers/UsersController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Tour_2040.Models;

namespace Tour_2040.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // Lock/Unlock Account
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent locking self or other Admins (optional but recommended)
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "Không thể khóa tài khoản Admin.";
                return RedirectToAction(nameof(Index));
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            {
                // Unlock
                user.LockoutEnd = null;
                TempData["Success"] = $"Đã mở khóa cho {user.Email}";
            }
            else
            {
                // Lock indefinitely
                user.LockoutEnd = DateTimeOffset.Now.AddYears(100);
                TempData["Message"] = $"Đã khóa tài khoản {user.Email}";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}