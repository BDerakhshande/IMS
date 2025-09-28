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
        private readonly IWarehouseDbContext _warehouseDbContext;

        public ProductItemsController(IProductItemService productItemService , IProductService productService ,
            IWarehouseDbContext warehouseDbContext)
        {
            _productItemService = productItemService;
            _productService = productService;
            _warehouseDbContext = warehouseDbContext;
        }

        // GET: ProductItems
        public async Task<IActionResult> Index(int productId)
        {
            if (productId <= 0)
                return BadRequest("شناسه محصول معتبر نیست.");

            var items = await _productItemService.GetByProductIdAsync(productId);
            ViewBag.ProductId = productId;

            // گرفتن محصول اصلی از IProductService
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

        // GET: ProductItems/Create
        public async Task<IActionResult> Create(int productId)
        {
            if (productId <= 0)
                return BadRequest("شناسه محصول معتبر نیست.");

            // ===== محاسبه شماره ترتیبی بعدی =====
            int nextSequence = 1;
            var lastItem = await _warehouseDbContext.ProductItems
                .Where(pi => pi.ProductId == productId)
                .OrderByDescending(pi => pi.Sequence)
                .FirstOrDefaultAsync();

            if (lastItem != null)
                nextSequence = lastItem.Sequence + 1;

            var dto = new ProductItemDto
            {
                ProductId = productId,
                Sequence = nextSequence  // شماره بعدی به جای 1
            };

            // پروژه‌ها
            var projects = await _productItemService.GetProjectsAsync();
            ViewBag.Projects = projects.Select(p => new SelectListItem
            {
                Value = p.Value.ToString(),
                Text = p.Text,
            }).ToList();

            // وضعیت‌ها
            ViewBag.Statuses = Enum.GetValues(typeof(ProductItemStatus))
                .Cast<ProductItemStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString()
                }).ToList();

            return View(dto);
        }




        // POST: ProductItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductItemDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var createdItem = await _productItemService.CreateAsync(dto);
                TempData["SuccessMessage"] = "آیتم محصول با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(Index), new { productId = dto.ProductId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(dto);
            }
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
                Value = p.Value.ToString(),
                Text = p.Text,
                Selected = (item.ProjectId.HasValue && item.ProjectId.Value == int.Parse(p.Value))
            }).ToList();

            // وضعیت‌ها
            ViewBag.Statuses = Enum.GetValues(typeof(ProductItemStatus))
                .Cast<ProductItemStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString(),
                    Selected = (item.Status == s)
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
