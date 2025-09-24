using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public InventoryOperationController(
            IInventoryOperationService inventoryOperationService,
            IWarehouseService warehouseService,
            ICategoryService categoryService,
            IGroupService groupService,
            IStatusService statusService,
            IProductService productService)
        {
            _inventoryOperationService = inventoryOperationService;
            _warehouseService = warehouseService;
            _categoryService = categoryService;
            _groupService = groupService;
            _statusService = statusService;
            _productService = productService;
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

            var result = await _inventoryOperationService.AddAsync(dto);

            if (!result)
                return Json(new { success = false, message = "رکورد موجودی یافت نشد." });

            var updatedQuantity = await _inventoryOperationService.GetQuantityAsync(
                dto.ProductId, dto.WarehouseId, dto.ZoneId, dto.SectionId);

            return Json(new { success = true, newQuantity = updatedQuantity });
        }

        [HttpGet]
        public async Task<IActionResult> GetQuantity(int productId, int warehouseId, int? zoneId, int? sectionId)
        {
            var quantity = await _inventoryOperationService.GetQuantityAsync(productId, warehouseId, zoneId, sectionId);
            return Json(new { success = true, quantity });
        }

        // --- Ajax Methods (مشابه InventoryController) ---
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
    }
}
