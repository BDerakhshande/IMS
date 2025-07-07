using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IWarehouseService _warehouseService;
        private readonly ICategoryService _categoryService;
        private readonly IGroupService _groupService;
        private readonly IStatusService _statusService;
        private readonly IProductService _productService;

        public InventoryController(
              IInventoryService inventoryService,
              IWarehouseService warehouseService,
              ICategoryService categoryService,
              IGroupService groupService,
              IStatusService statusService,
              IProductService productService)
        {
            _inventoryService = inventoryService;
            _warehouseService = warehouseService;
            _categoryService = categoryService;
            _groupService = groupService;
            _statusService = statusService;
            _productService = productService;
        }


        
        //[HttpGet]
        //public async Task<IActionResult> Index()
        //{
        //    var dto = new InventoryCreateDto
        //    {
        //        Warehouses = await _warehouseService.GetSelectListAsync(),
        //        Categories = await _categoryService.GetSelectListAsync()
        //    };

        //    return View(dto);
        //}

        
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index(InventoryCreateDto dto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        await FillDropdownsAsync(dto);
        //        return View(dto);
        //    }

        //    await _inventoryService.CreateAsync(dto);
        //    return RedirectToAction("Index");
        //}



        [HttpGet]
        public async Task<IActionResult> LoadInventory()
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
        public async Task<IActionResult> LoadInventory(InventoryCreateDto inputDto)
        {
            var dto = await _inventoryService.LoadOrCreateAsync(inputDto);

            if (dto == null)
                return Json(new { success = false });

            return Json(new { success = true, quantity = dto.Quantity });
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInventory([FromForm] InventoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات ارسالی معتبر نیست." });

            // فراخوانی سرویس برای به‌روزرسانی موجودی
            var result = await _inventoryService.UpdateQuantityAsync(
                dto.ProductId,
                dto.WarehouseId,
                dto.ZoneId,
                dto.SectionId,
                dto.NewQuantity);

            if (!result)
                return Json(new { success = false, message = "خطا در به‌روزرسانی موجودی." });

            // دریافت مقدار جدید موجودی پس از به‌روزرسانی
            var updatedQuantity = await _inventoryService.GetQuantityAsync(
                dto.ProductId,
                dto.WarehouseId,
                dto.ZoneId,
                dto.SectionId);

            return Json(new { success = true, newQuantity = updatedQuantity });
        }



        // متد Ajax برای گرفتن گروه‌ها بر اساس دسته‌بندی (CategoryId)
        [HttpGet]
        public async Task<IActionResult> GetGroups(int categoryId)
        {
            var groups = await _groupService.GetSelectListByCategoryIdAsync(categoryId);
            return Json(groups);
        }

        // متد Ajax برای گرفتن وضعیت‌ها بر اساس گروه (GroupId)
        [HttpGet]
        public async Task<IActionResult> GetStatuses(int groupId)
        {
            var statuses = await _statusService.GetSelectListByGroupIdAsync(groupId);
            return Json(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int statusId)
        {
            Console.WriteLine("StatusId received: " + statusId); // یا لاگ‌نویسی مشابه
            var products = await _productService.GetSelectListByStatusIdAsync(statusId);

            if (!products.Any())
                Console.WriteLine("No products found for status " + statusId);

            return Json(products);
        }


        // متد Ajax برای گرفتن نواحی ذخیره‌سازی (Zones) بر اساس انبار (WarehouseId)
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

        // متد Ajax برای گرفتن بخش‌ها (Sections) بر اساس ناحیه ذخیره‌سازی (ZoneId)
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


        private async Task FillDropdownsAsync(InventoryCreateDto dto)
        {
            dto.Warehouses = await _warehouseService.GetSelectListAsync();
            dto.Categories = await _categoryService.GetSelectListAsync();

            dto.Groups = dto.CategoryId > 0
                ? await _groupService.GetSelectListByCategoryIdAsync(dto.CategoryId)
                : Enumerable.Empty<SelectListItem>();

            dto.Statuses = dto.GroupId > 0
                ? await _statusService.GetSelectListByGroupIdAsync(dto.GroupId)
                : Enumerable.Empty<SelectListItem>();

            dto.Products = dto.StatusId > 0
                ? await _productService.GetSelectListByStatusIdAsync(dto.StatusId)
                : Enumerable.Empty<SelectListItem>();

            dto.Zones = dto.WarehouseId > 0
                ? (await _warehouseService.GetZonesByWarehouseIdAsync(dto.WarehouseId))
                    .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
                : Enumerable.Empty<SelectListItem>();

            dto.Sections = dto.ZoneId.HasValue
                ? (await _warehouseService.GetSectionsByZoneAsync(dto.ZoneId.Value))
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                : Enumerable.Empty<SelectListItem>();
        }


    }
}
