using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class InventoryTurnoverController : Controller
    {
        private readonly IInventoryTurnoverService _inventoryTurnoverService;

        public InventoryTurnoverController(IInventoryTurnoverService inventoryTurnoverService)
        {
            _inventoryTurnoverService = inventoryTurnoverService;
        }

        public IActionResult Index()
        {
            return View();
        }




        [HttpPost]
        public async Task<IActionResult> GetInventoryTurnover([FromBody] InventoryTurnoverFilterDto filter)
        {
            if (filter == null)
                return BadRequest("Filter cannot be null.");

            if (filter.FromDate > filter.ToDate)
                return BadRequest("FromDate cannot be after ToDate.");

            var data = await _inventoryTurnoverService.GetInventoryTurnoverAsync(filter);
            return Json(data);
        }




        [HttpGet]
        public async Task<IActionResult> GetWarehouses()
        {
            var warehouses = await _inventoryTurnoverService.GetWarehousesAsync(); // متد مربوطه در سرویس باید باشه
            return Json(warehouses);
        }

        // اکشن دریافت زون‌ها بر اساس انبار (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetZones(int warehouseId)
        {
            var zones = await _inventoryTurnoverService.GetZonesByWarehouseIdAsync(warehouseId);
            return Json(zones);
        }

        // اکشن دریافت بخش‌ها بر اساس لیست زون‌ها (AJAX)
        [HttpPost]
        public async Task<IActionResult> GetSections([FromBody] List<int> zoneIds)
        {
            var sections = await _inventoryTurnoverService.GetSectionsByZoneIdsAsync(zoneIds);
            return Json(sections);
        }
    }
}
