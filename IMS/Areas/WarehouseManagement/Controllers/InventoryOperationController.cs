using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.WarehouseManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class InventoryOperationController : Controller
    {
        private readonly IInventoryOperationService _inventoryOperationService;
        private readonly IWarehouseService _warehouseService;
        private readonly ICategoryService _categoryService;
        private readonly IGroupService _groupService;
        private readonly IStatusService _statusService;
        private readonly IProductService _productService;
        private readonly IWarehouseDbContext _context;

        public InventoryOperationController(
            IInventoryOperationService inventoryOperationService,
            IWarehouseService warehouseService,
            ICategoryService categoryService,
            IGroupService groupService,
            IStatusService statusService,
            IProductService productService,
            IWarehouseDbContext context)
        {
            _inventoryOperationService = inventoryOperationService;
            _warehouseService = warehouseService;
            _categoryService = categoryService;
            _groupService = groupService;
            _statusService = statusService;
            _productService = productService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Load()
        {
            var dto = new InventoryCreateDto
            {
                Warehouses = await _warehouseService.GetSelectListAsync(),
                Categories = await _categoryService.GetSelectListAsync(),
                Groups = Enumerable.Empty<SelectListItem>(),
                Statuses = Enumerable.Empty<SelectListItem>(),
                Products = Enumerable.Empty<SelectListItem>(),
                Zones = Enumerable.Empty<SelectListItem>(),
                Sections = Enumerable.Empty<SelectListItem>()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Load(InventoryCreateDto inputDto)
        {
            var dto = await _inventoryOperationService.LoadAsync(inputDto);

            if (dto == null)
                return Json(new { success = false });

            return Json(new { success = true, quantity = dto.Quantity });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] InventoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست." });

            try
            {
                var result = await _inventoryOperationService.AddAsync(dto);

                if (result.success)
                {
                    // گرفتن مقدار جدید موجودی
                    var newQuantity = await GetCurrentQuantity(dto);

                    // اگر کد یکتا ایجاد شده، آن را به پاسخ اضافه کن
                    if (dto.IsUnique && !string.IsNullOrEmpty(result.uniqueCode))
                    {
                        return Json(new
                        {
                            success = true,
                            newQuantity = newQuantity,
                            uniqueCode = result.uniqueCode,
                            message = "کالای یکتا با موفقیت ایجاد شد"
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        newQuantity = newQuantity,
                        message = "موجودی با موفقیت افزایش یافت"
                    });
                }

                return Json(new { success = false, message = "خطا در افزایش موجودی" });
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا برای دیباگ
                Console.WriteLine($"Error in Add action: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<decimal> GetCurrentQuantity(InventoryCreateDto dto)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.ProductId == dto.ProductId &&
                    i.WarehouseId == dto.WarehouseId &&
                    i.ZoneId == dto.ZoneId &&
                    i.SectionId == dto.SectionId);

            return inventory?.Quantity ?? 0;
        }

    

        // --- Ajax Methods ---
        [HttpGet]
        public async Task<IActionResult> GetGroups(int categoryId)
        {
            var groups = await _groupService.GetSelectListByCategoryIdAsync(categoryId);
            return Json(groups);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatuses(int groupId)
        {
            var statuses = await _statusService.GetSelectListByGroupIdAsync(groupId);
            return Json(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int statusId)
        {
            var products = await _productService.GetSelectListByStatusIdAsync(statusId);
            return Json(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetZones(int warehouseId)
        {
            var zones = await _warehouseService.GetZonesByWarehouseIdAsync(warehouseId);
            var list = zones.Select(z => new SelectListItem
            {
                Value = z.Id.ToString(),
                Text = z.Name
            });
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetSections(int zoneId)
        {
            var sections = await _warehouseService.GetSectionsByZoneAsync(zoneId);
            var list = sections.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            });
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetUniqueCodes(int productId)
        {
            var codes = await _context.ProductItems
                .Where(pi => pi.ProductId == productId)
                .Select(pi => new
                {
                    pi.Id,
                    pi.UniqueCode
                })
                .ToListAsync();

            return Json(codes);
        }



    }
}