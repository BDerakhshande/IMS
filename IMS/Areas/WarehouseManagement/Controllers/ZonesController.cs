using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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





        [HttpGet]
        public async Task<IActionResult> Create(int warehouseId)
        {
            var dto = new StorageZoneDto
            {
                WarehouseId = warehouseId,
                // تولید خودکار کد جدید
                ZoneCode = await _warehouseService.GenerateNextCodeAsync<StorageZone>(
                    z => z.ZoneCode,
                    z => z.Id
                )
            };

            return View(dto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StorageZoneDto dto)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(dto);
            //}

            try
            {
                int newZoneId = await _warehouseService.CreateZoneAsync(dto);
                return RedirectToAction(nameof(Index), new { id = dto.WarehouseId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("ZoneCode", ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                // بررسی خطای تکراری بودن کلید
                if (dbEx.InnerException?.Message.Contains("Cannot insert duplicate key row") == true)
                {
                    ModelState.AddModelError("ZoneCode", "کد وارد شده قبلاً ثبت شده است.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "خطایی در ثبت اطلاعات رخ داد.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "خطای غیرمنتظره‌ای رخ داده است.");
            }

            return View(dto);
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
 
            try
            {
                await _warehouseService.UpdateZoneAsync(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ZoneCode", ex.Message);
                return View(dto);
            }

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

            return RedirectToAction("Index", "Zones", new { area = "WarehouseManagement", id = warehouseId });
        }




    }
}
