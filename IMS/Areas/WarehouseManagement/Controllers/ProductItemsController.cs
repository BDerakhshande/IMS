using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{

    [Area("WarehouseManagement")]
    public class ProductItemsController : Controller
    {
        private readonly IProductItemService _productItemService;
        private readonly IProductService _productService;

        public ProductItemsController(IProductItemService productItemService, IProductService productService)
        {
            _productItemService = productItemService;
            _productService = productService;
        }

        // GET: ProductItems
        public async Task<IActionResult> Index(int productId)
        {
            if (productId <= 0)
                return BadRequest("شناسه محصول معتبر نیست.");

            var items = await _productItemService.GetByProductIdAsync(productId);
            ViewBag.ProductId = productId;

            var product = await _productService.GetByIdAsync(productId);
            ViewBag.StatusId = product?.StatusId ?? 0;

            return View(items);
        }

        // GET: ProductItems/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _productItemService.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        // GET: ProductItems/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _productItemService.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            // پروژه‌ها
            var projects = await _productItemService.GetProjectsAsync();
            ViewBag.Projects = projects.Select(p => new SelectListItem
            {
                Value = p.Value,
                Text = p.Text,
                Selected = (item.ProjectId.HasValue && item.ProjectId.Value.ToString() == p.Value)
            }).ToList();

            // وضعیت‌ها
            ViewBag.Statuses = Enum.GetValues(typeof(ProductItemStatus))
                .Cast<ProductItemStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString(),
                    Selected = (item.ItemStatus == s)
                }).ToList();

            return View(item);
        }

        // POST: ProductItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductItemDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var updatedItem = await _productItemService.UpdateAsync(dto);
                if (updatedItem == null)
                {
                    TempData["ErrorMessage"] = "آیتم مورد نظر یافت نشد.";
                    return RedirectToAction(nameof(Index), new { productId = dto.ProductId });
                }

                TempData["SuccessMessage"] = "ویرایش آیتم محصول با موفقیت انجام شد.";
                return RedirectToAction(nameof(Index), new { productId = dto.ProductId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(dto);
            }
        }

        // POST: ProductItems/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int productId)
        {
            try
            {
                var result = await _productItemService.DeleteAsync(id);
                if (result)
                    TempData["SuccessMessage"] = "آیتم محصول با موفقیت حذف شد.";
                else
                    TempData["ErrorMessage"] = "آیتم پیدا نشد.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { productId });
        }
    }

}
