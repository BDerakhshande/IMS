using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class UnitsController : Controller
    {
        private readonly IUnitService _unitService;

        public UnitsController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        // GET: WarehouseManagement/Units
        public async Task<IActionResult> Index()
        {
            var units = await _unitService.GetAllAsync();
            return View(units);
        }

        // GET: WarehouseManagement/Units/Create
        public IActionResult Create()
        {
            return View(new UnitDto());
        }

        // POST: WarehouseManagement/Units/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _unitService.CreateAsync(dto);
                TempData["SuccessMessage"] = "واحد با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: WarehouseManagement/Units/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _unitService.GetByIdAsync(id);
            if (unit == null)
                return NotFound();

            return View(unit);
        }

        // POST: WarehouseManagement/Units/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UnitDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _unitService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "واحد با موفقیت به‌روزرسانی شد.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // POST: WarehouseManagement/Units/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _unitService.DeleteAsync(id);
                TempData["SuccessMessage"] = "واحد با موفقیت حذف شد.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"امکان حذف وجود ندارد: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
