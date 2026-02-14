using IMS.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IMS.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RegisterController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpGet]
        public IActionResult Index() => View(new RegisterVm());


        public class RegisterVm
        {
            [Required] public string FirstName { get; set; } = null!;
            [Required] public string LastName { get; set; } = null!;
            [Required, MinLength(4)] public string UserName { get; set; } = null!;
            public string? PhoneNumber { get; set; }
            [Required, MinLength(6)] public string Password { get; set; } = null!;
            [Required, Compare(nameof(Password))] public string ConfirmPassword { get; set; } = null!;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                PhoneNumber = vm.PhoneNumber,
                FirstName = vm.FirstName,
                LastName = vm.LastName
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(vm);
            }

            // در این قسمت به جای ورود خودکار به سیستم، کاربر به صفحه لاگین هدایت می‌شود
            return RedirectToAction("Index", "Login"); // هدایت به صفحه لاگین
        }

    }
}
