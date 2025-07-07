using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    public class ProjectManagementHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
