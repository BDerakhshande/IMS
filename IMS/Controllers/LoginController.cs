using Microsoft.AspNetCore.Mvc;

namespace IMS.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
