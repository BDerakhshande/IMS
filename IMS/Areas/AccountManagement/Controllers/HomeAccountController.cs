using IMS.Areas.AccountManagement.Data;
using IMS.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.AccountManagement.Controllers
{
    [Area("AccountManagement")]
    public class HomeAccountController : Controller
    {
        private readonly AccountManagementDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeAccountController (AccountManagementDbContext context , UserManager<ApplicationUser> userManager)
        {
           
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // دریافت کاربر فعلی

            if (user != null)
            {
                ViewData["Username"] = user.UserName; // نام کاربری را به ViewData اضافه می‌کنیم
            }

            return View();
        }


    }
}
