using IMS.Areas.AccountManagement.Data;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.AccountManagement.Controllers
{
    [Area("AccountManagement")]
    public class HomeAccountController : Controller
    {
        private readonly AccountManagementDbContext _context;
        public HomeAccountController (AccountManagementDbContext context)
        {
           
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


    }
}
