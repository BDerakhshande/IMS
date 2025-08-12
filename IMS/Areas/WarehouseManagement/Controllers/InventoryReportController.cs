using System.Text.Json;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class InventoryReportController : Controller
    {
        

        private readonly IInventoryReportService _inventoryReportService;
        private readonly IWarehouseService _warehouseService;
        private readonly ICategoryService _categoryService;
        private readonly IWarehouseDbContext _dbContext;

        public InventoryReportController(IInventoryReportService inventoryReportService , IWarehouseService warehouseService , ICategoryService categoryService
            ,IWarehouseDbContext dbContext)
        {
            _inventoryReportService = inventoryReportService;
            _warehouseService = warehouseService;
            _categoryService = categoryService;
            _dbContext = dbContext;
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> Index(InventoryReportFilterDto filter)
        {
            if (filter.Warehouses == null || filter.Warehouses.Count == 0)
            {
                filter.Warehouses = new List<WarehouseFilter> { new WarehouseFilter() };
            }

            bool searchPerformed = false;

            // تشخیص اینکه فرم ارسال شده یا نه:
            if (Request.Method == "POST")
            {
                searchPerformed = true;

                filter.Items = await _inventoryReportService.GetInventoryReportAsync(filter);
                ViewBag.TotalQuantity = filter.Items?.Sum(i => i.Quantity) ?? 0;
            }

            await PopulateSelectLists(filter);

            ViewBag.SearchPerformed = searchPerformed;

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

        [HttpPost]
        public async Task<IActionResult> ExportInventoryToExcel([FromBody] InventoryReportFilterDto filter)
        {
            var fileBytes = await _inventoryReportService.ExportReportToExcelAsync(filter);
            var fileName = $"InventoryReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }



        [HttpPost]
        public async Task<IActionResult> ExportToPdf(InventoryReportFilterDto filter)
        {

            // Log the filter object to verify its contents
           
            Console.WriteLine(JsonSerializer.Serialize(filter));
            var items = await _inventoryReportService.GetInventoryReportAsync(filter);

            var warehouseIds = filter.Warehouses?.Select(w => w.WarehouseId).ToList() ?? new List<int>();
            var warehouseNames = await _dbContext.Warehouses
                .Where(w => warehouseIds.Contains(w.Id))
                .ToDictionaryAsync(w => w.Id, w => w.Name);

            var categoryNames = new Dictionary<int, string>();
            if (filter.CategoryId.HasValue)
            {
                var cat = await _dbContext.Categories.FindAsync(filter.CategoryId.Value);
                if (cat != null)
                    categoryNames[cat.Id] = cat.Name;
            }

            var groupNames = new Dictionary<int, string>();
            if (filter.GroupId.HasValue)
            {
                var group = await _dbContext.Groups.FindAsync(filter.GroupId.Value);
                if (group != null)
                    groupNames[group.Id] = group.Name;
            }

            var statusNames = new Dictionary<int, string>();
            if (filter.StatusId.HasValue)
            {
                var status = await _dbContext.Statuses.FindAsync(filter.StatusId.Value);
                if (status != null)
                    statusNames[status.Id] = status.Name;
            }

            var productNames = new Dictionary<int, string>();
            if (filter.ProductId.HasValue)
            {
                var product = await _dbContext.Products.FindAsync(filter.ProductId.Value);
                if (product != null)
                    productNames[product.Id] = product.Name;
            }

            var zoneNames = items
     .Where(i => i.ZoneName != null)
     .GroupBy(i => new { i.ZoneId, i.ZoneName })
     .ToDictionary(g => g.Key.ZoneId ?? 0, g => g.Key.ZoneName);

            var sectionNames = items
                .Where(i => i.SectionName != null)
                .GroupBy(i => new { i.SectionId, i.SectionName })
                .ToDictionary(g => g.Key.SectionId ?? 0, g => g.Key.SectionName);




            var vm = new InventoryReportPdfViewModel
            {
                Items = items,
                Filter = filter,
                WarehouseNames = warehouseNames,
                CategoryNames = categoryNames,
                GroupNames = groupNames,
                StatusNames = statusNames,
                ProductNames = productNames,
                ZoneNames =  zoneNames,
                SectionNames = sectionNames
            };

            return new ViewAsPdf("InventoryPdfView", vm)
            {
                FileName = $"InventoryReport_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                CustomSwitches = "--disable-smart-shrinking --print-media-type --background"
            };

        }


    }
}