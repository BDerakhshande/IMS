using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class WarehousesController : Controller
    {
        private readonly IWarehouseService _warehouseService;
        
        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseService.GetAllWarehousesWithHierarchyAsync();
            return View(warehouses);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(WarehouseDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _warehouseService.CreateWarehouseAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _warehouseService.GetWarehouseHierarchyAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(WarehouseDto dto)
        {
           
            await _warehouseService.UpdateWarehouseAsync(dto);
            return RedirectToAction(nameof(Index)); // به صفحه لیست انبارها برگرد
        }



        public async Task<IActionResult> Details(int id)
        {
            var warehouse = await _warehouseService.GetWarehouseHierarchyAsync(id);
            if (warehouse == null) return NotFound();
            return View(warehouse);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _warehouseService.GetWarehouseHierarchyAsync(id);
            if (warehouse == null)
            {
                return NotFound();
            }

            // بررسی وجود Zone در انبار
            bool hasZones = warehouse.Zones != null && warehouse.Zones.Any();
            if (hasZones)
            {
                TempData["ErrorMessage"] = "این انبار شامل بخش‌ها یا قسمت‌هایی است و نمی‌توان آن را حذف کرد.";
                return RedirectToAction(nameof(Index));
            }

            await _warehouseService.DeleteWarehouseAsync(id);
            TempData["SuccessMessage"] = "انبار با موفقیت حذف شد.";
            return RedirectToAction(nameof(Index));
        }


    }
}
