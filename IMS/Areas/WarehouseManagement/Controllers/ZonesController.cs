using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class ZonesController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public ZonesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var zones = await _warehouseService.GetZonesByWarehouseIdAsync(id);
            ViewBag.WarehouseId = id;
            return View(zones);
        }


       


        // اکشن GET برای نمایش فرم ایجاد منطقه جدید
        [HttpGet]
        public IActionResult Create(int warehouseId)
        {
            var dto = new StorageZoneDto
            {
                WarehouseId = warehouseId
            };
            return View(dto);
        }

        // اکشن POST برای دریافت داده‌ها و ایجاد منطقه جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StorageZoneDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            int newZoneId = await _warehouseService.CreateZoneAsync(dto);

            // بعد از ایجاد منطقه جدید، معمولا به صفحه لیست مناطق همان انبار برمی‌گردیم
            return RedirectToAction(nameof(Index), new { id = dto.WarehouseId });
        }




        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _warehouseService.GetZoneByIdAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }



        [HttpPost]
        public async Task<IActionResult> Edit(StorageZoneDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _warehouseService.UpdateZoneAsync(dto);
            return RedirectToAction(nameof(Index), new { id = dto.WarehouseId });
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id, int warehouseId)
        {
            try
            {
                await _warehouseService.DeleteZoneAsync(id);
                TempData["SuccessMessage"] = "ناحیه با موفقیت حذف شد.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index", new { id = warehouseId });
        }



    }
}
