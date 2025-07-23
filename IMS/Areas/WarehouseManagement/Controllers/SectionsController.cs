using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{

    [Area("WarehouseManagement")]
    public class SectionsController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public SectionsController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public async Task<IActionResult> Index(int id) 
        {
            var allSections = await _warehouseService.GetAllSectionsAsync();

            // فقط بخش‌های مربوط به این قسمت (Zone)
            var filtered = allSections.Where(s => s.ZoneId == id).ToList();

            // گرفتن اطلاعات Zone (اگر متدش رو داری)
            var zone = await _warehouseService.GetZoneByIdAsync(id);

            ViewBag.ZoneId = id;
            ViewBag.ZoneName = zone?.ZoneCode ?? "؟";
            ViewBag.WarehouseId = zone?.WarehouseId ?? 0;

            return View(filtered);
        }




        [HttpGet]
        public IActionResult Create(int zoneId)
        {
            var dto = new StorageSectionDto
            {
                ZoneId = zoneId // Set ZoneId from the query parameter
            };
            return View(dto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StorageSectionDto dto)
        {
            if (ModelState.IsValid)
            {
                if (dto.ZoneId <= 0)
                {
                    ModelState.AddModelError("", "ZoneId نامعتبر است.");
                    return View(dto);
                }

                try
                {
                    await _warehouseService.CreateSectionAsync(dto);
                    return RedirectToAction(nameof(Index), new { id = dto.ZoneId });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(dto.SectionCode), ex.Message);
                }
            }

            return View(dto);
        }




        // GET: Sections/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var section = await _warehouseService.GetSectionByIdAsync(id);
            if (section == null)
                return NotFound();

            return View(section);
        }

        // POST: Sections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StorageSectionDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    await _warehouseService.UpdateSectionAsync(dto);
                    return RedirectToAction(nameof(Index), new { id = dto.ZoneId });
                }
                catch (DbUpdateException ex)
                {
                    // بررسی خطای تکراری بودن کلید یکتا از دیتابیس
                    if (ex.InnerException != null)
                    {
                        var message = ex.InnerException.Message;

                        if (message.Contains("IX_StorageSections_ZoneId_SectionCode"))
                        {
                            ModelState.AddModelError("SectionCode", "کد بخش در این قسمت قبلاً ثبت شده است.");
                        }
                        else if (message.Contains("IX_StorageSections_SectionCode"))
                        {
                            ModelState.AddModelError("SectionCode", "این کد بخش قبلاً ثبت شده است.");
                        }
                        else
                        {
                            ModelState.AddModelError("", $"خطا در بروزرسانی: {ex.Message}");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", $"خطا در بروزرسانی: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"خطا در بروزرسانی: {ex.Message}");
                }
            }

            return View(dto);
        }


        // GET: Sections/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var section = await _warehouseService.GetSectionByIdAsync(id);
            if (section == null)
                return NotFound();

            return View(section);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int zoneId)
        {
            try
            {
                await _warehouseService.DeleteSectionAsync(id);
            }
            catch (Exception)
            {
                TempData["DeleteError"] = "حذف این بخش امکان‌پذیر نیست. لطفاً مطمئن شوید که این بخش به آیتم‌های دیگری وابسته نباشد.";
                return RedirectToAction(nameof(Index), new { id = zoneId });
            }

            return RedirectToAction(nameof(Index), new { id = zoneId });
        }

    }
}
