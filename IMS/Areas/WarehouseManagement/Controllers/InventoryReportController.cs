using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class InventoryReportController : Controller
    {
        

        private readonly IInventoryReportService _inventoryReportService;
        private readonly IWarehouseService _warehouseService;
        private readonly ICategoryService _categoryService;

        public InventoryReportController(IInventoryReportService inventoryReportService , IWarehouseService warehouseService , ICategoryService categoryService)
        {
            _inventoryReportService = inventoryReportService;
            _warehouseService = warehouseService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(InventoryReportFilterDto filter)
        {
            var items = await _inventoryReportService.GetInventoryReportAsync(filter);
            filter.Items = items;

            await PopulateSelectLists(filter); 

            return View(filter);
        }

        private async Task PopulateSelectLists(InventoryReportFilterDto filter)
        {
            // دریافت همه انبارها و دسته‌بندی‌ها
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name", filter.WarehouseId);
            ViewBag.Categories = new SelectList(categories, "Id", "Name", filter.CategoryId);

            // دریافت همه مناطق بدون توجه به انبار
            var zones = await _inventoryReportService.GetAllZonesAsync();
            ViewBag.Zones = new SelectList(zones, "Value", "Text", filter.ZoneId);

            // دریافت همه بخش‌ها بدون توجه به منطقه
            var sections = await _inventoryReportService.GetAllSectionsAsync();
            ViewBag.Sections = new SelectList(sections, "Value", "Text", filter.SectionId);

            // دریافت همه گروه‌ها
            var groups = await _inventoryReportService.GetAllGroupsAsync();
            ViewBag.Groups = new SelectList(groups, "Value", "Text", filter.GroupId);

            // دریافت همه وضعیت‌ها
            var statuses = await _inventoryReportService.GetAllStatusesAsync();
            ViewBag.Statuses = new SelectList(statuses, "Value", "Text", filter.StatusId);

            // دریافت همه کالاها
            var products = await _inventoryReportService.GetAllProductsAsync();
            ViewBag.Products = new SelectList(products, "Value", "Text", filter.ProductId);
        }



        [HttpGet]
        public async Task<JsonResult> GetAllZones()
        {
            var zones = await _inventoryReportService.GetAllZonesAsync();
            return Json(zones);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllSections()
        {
            var sections = await _inventoryReportService.GetAllSectionsAsync();
            return Json(sections);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllGroups()
        {
            var groups = await _inventoryReportService.GetAllGroupsAsync();
            return Json(groups);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllStatuses()
        {
            var statuses = await _inventoryReportService.GetAllStatusesAsync();
            return Json(statuses);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllProducts()
        {
            var products = await _inventoryReportService.GetAllProductsAsync();
            return Json(products);
        }



    }
}