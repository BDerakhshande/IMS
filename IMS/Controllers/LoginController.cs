using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using IMS.Infrastructure.Persistence.Identity;

namespace IMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index() => View();

        public class LoginVm
        {
            [Required] public string UserName { get; set; } = null!;
            [Required] public string Password { get; set; } = null!;
            public bool RememberMe { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                vm.UserName, vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "نام کاربری یا رمز عبور اشتباه است.");
                return View(vm);
            }

            // پس از ورود موفق، کاربر به صفحه Home/Index هدایت می‌شود
            return RedirectToAction("Index", "Home"); // هدایت به صفحه زیر سیستم‌ها
        }

    }
}
