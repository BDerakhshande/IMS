using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.ProcurementManagement.Controllers
{
    [Area("ProcurementManagement")]
    public class ProcurementManagementHome : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
