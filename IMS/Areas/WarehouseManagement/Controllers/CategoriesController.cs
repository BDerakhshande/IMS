using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: WarehouseManagement/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

       

        // GET: WarehouseManagement/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.CreateAsync(dto);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Code", ex.Message);
                }
            }
            return View(dto);
        }



        // GET: WarehouseManagement/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var updated = await _categoryService.UpdateAsync(id, dto);
                    if (!updated)
                        return NotFound();

                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    // نمایش پیام خطای "کد تکراری"
                    ModelState.AddModelError(nameof(dto.Code), ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"خطا در بروزرسانی: {ex.Message}");
                }
            }

            return View(dto);
        }








        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                TempData["SuccessMessage"] = "دسته‌بندی با موفقیت حذف شد.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }



    }
}
