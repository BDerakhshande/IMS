using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productsService;
        private readonly IStatusService _statusService;
        private readonly IUnitService _unitService;
        private readonly ICategoryService _categoryService;

        public ProductsController(
            IProductService productsService,
            IStatusService statusService,
            IUnitService unitService,
            ICategoryService categoryService) 
        {
            _productsService = productsService;
            _statusService = statusService;
            _unitService = unitService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int statusId)
        {
            var allProducts = await _productsService.GetAllAsync(statusId);

            var status = await _statusService.GetStatusByIdAsync(statusId);
            if (status == null)
                return NotFound();

            ViewBag.StatusId = statusId;
            ViewBag.GroupId = status.GroupId;

            return View(allProducts);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int statusId)
        {
            var nextCode = await _categoryService.GenerateNextCodeAsync<Product>(
                x => x.Code,
                x => x.Id
            );

            var dto = new ProductDto
            {
                StatusId = statusId,
                Code = nextCode,
                UnitId = 1 // مقدار پیش‌فرض برای UnitId
            };

            ViewBag.Units = new SelectList(await _unitService.GetAllAsync(), "Id", "Name", dto.UnitId);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto dto)
        {
            // بررسی اعتبار UnitId قبل از بررسی ModelState
            var units = await _unitService.GetAllAsync();
            var unitExists = units.Any(u => u.Id == dto.UnitId);

            if (!unitExists)
            {
                ModelState.AddModelError("UnitId", "واحد انتخاب شده معتبر نیست");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Units = new SelectList(units, "Id", "Name", dto.UnitId);
                return View(dto);
            }

            try
            {
                await _productsService.CreateAsync(dto);
                return RedirectToAction(nameof(Index), new { statusId = dto.StatusId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Units = new SelectList(units, "Id", "Name", dto.UnitId);
                return View(dto);
            }
        }




        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productsService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.StatusId = product.StatusId;
            ViewBag.Units = new SelectList(await _unitService.GetAllAsync(), "Id", "Name", product.Unit?.Id);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductDto dto)
        {
            try
            {
                await _productsService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index), new { statusId = dto.StatusId });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("کدی که وارد کرده‌اید"))
                    ModelState.AddModelError(nameof(dto.Code), ex.Message);
                else
                    ModelState.AddModelError(string.Empty, ex.Message);

                ViewBag.StatusId = dto.StatusId;
                ViewBag.Units = new SelectList(await _unitService.GetAllAsync(), "Id", "Name", dto.Unit?.Id);

                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int statusId)
        {
            try
            {
                await _productsService.DeleteAsync(id);
                TempData["SuccessMessage"] = "محصول با موفقیت حذف شد.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "امکان حذف این کالا وجود ندارد زیرا با این کالا عملیات انجام شده است.";
            }
            return RedirectToAction(nameof(Index), new { statusId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productsService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var status = await _statusService.GetStatusByIdAsync(product.StatusId);
            ViewBag.StatusId = product.StatusId;
            ViewBag.StatusName = status?.Name ?? "نامشخص";

            return View(product);
        }
    }
}