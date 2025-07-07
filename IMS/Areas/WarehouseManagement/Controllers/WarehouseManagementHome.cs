using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    public class WarehouseManagementHome : Controller
    {
        [Area("WarehouseManagement")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
