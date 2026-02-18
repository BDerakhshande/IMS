using IMS.Areas.AccountManagement.Data;
using IMS.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.ProcurementManagement.Controllers
{
    [Area("ProcurementManagement")]
    public class ProcurementManagementHome : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProcurementManagementHome(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                ViewData["Username"] = user.UserName; 
            }

            return View();
        }

    }
}
