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

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> Index(InventoryReportFilterDto filter)
        {
            if (filter.Warehouses == null || filter.Warehouses.Count == 0)
            {
                filter.Warehouses = new List<WarehouseFilter> { new WarehouseFilter() };
            }

            filter.Items = await _inventoryReportService.GetInventoryReportAsync(filter);

            // اگر می‌خواهی مجموع کل را محاسبه و ارسال کنی:
            ViewBag.TotalQuantity = filter.Items?.Sum(i => i.Quantity) ?? 0;

            await PopulateSelectLists(filter);

            return View(filter);
        }






        private async Task PopulateSelectLists(InventoryReportFilterDto filter = null)
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            var categories = await _categoryService.GetAllAsync();
            var groups = await _inventoryReportService.GetAllGroupsAsync();
            var statuses = await _inventoryReportService.GetAllStatusesAsync();
            var products = await _inventoryReportService.GetAllProductsAsync();

            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Groups = new SelectList(groups, "Value", "Text");
            ViewBag.Statuses = new SelectList(statuses, "Value", "Text");
            ViewBag.Products = new SelectList(products, "Value", "Text");

            var zones = new List<SelectListItem>();
            var sections = new List<SelectListItem>();

            if (filter?.Warehouses != null)
            {
                foreach (var warehouse in filter.Warehouses)
                {
                    if (warehouse.WarehouseId > 0)
                    {
                        var zoneList = await _inventoryReportService.GetZonesByWarehouseIdAsync(warehouse.WarehouseId);
                        zones.AddRange(zoneList.Select(z => new SelectListItem { Value = z.Value, Text = z.Text }));
                    }

                    if (warehouse.ZoneIds != null && warehouse.ZoneIds.Any())
                    {
                        var sectionList = await _inventoryReportService.GetSectionsByZoneIdsAsync(warehouse.ZoneIds);
                        sections.AddRange(sectionList.Select(s => new SelectListItem { Value = s.Value, Text = s.Text }));
                    }
                }
            }

            ViewBag.Zones = new SelectList(zones.DistinctBy(z => z.Value), "Value", "Text");
            ViewBag.Sections = new SelectList(sections.DistinctBy(s => s.Value), "Value", "Text");
        }



        // اکشن‌های API برای بارگذاری مناطق و بخش‌ها
        [HttpGet]
        public async Task<JsonResult> GetZonesByWarehouseId(int warehouseId)
        {
            var zones = await _inventoryReportService.GetZonesByWarehouseIdAsync(warehouseId);
            return Json(zones);
        }

        [HttpGet]
        public async Task<JsonResult> GetSectionsByZoneIds([FromQuery] List<int> zoneIds)
        {
            var sections = await _inventoryReportService.GetSectionsByZoneIdsAsync(zoneIds);
            return Json(sections);
        }
        [HttpGet]
        public async Task<JsonResult> GetGroupsByCategoryId(int categoryId)
        {
            var groups = await _inventoryReportService.GetGroupsByCategoryIdAsync(categoryId);
            return Json(groups);
        }

        [HttpGet]
        public async Task<JsonResult> GetStatusesByGroupId(int groupId)
        {
            var statuses = await _inventoryReportService.GetStatusesByGroupIdAsync(groupId);
            return Json(statuses);
        }

        [HttpGet]
        public async Task<JsonResult> GetProductsByStatusId(int statusId)
        {
            var products = await _inventoryReportService.GetProductsByStatusIdAsync(statusId);
            return Json(products);
        }



    }
}