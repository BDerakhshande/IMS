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
            // گرفتن داده‌های اصلی بر اساس فیلتر
            var items = await _inventoryReportService.GetInventoryReportAsync(filter);

            // پر کردن SelectListها و نام‌ها مثل Index
            await PopulateSelectLists(filter);

            // محاسبه نام‌ها برای فیلتر انتخاب‌شده (با استفاده از ViewBag که PopulateSelectLists پر کرده)
            var warehouseNames = (await _warehouseService.GetAllWarehousesAsync())
                .ToDictionary(w => w.Id, w => w.Name);

            var categoryNames = (await _categoryService.GetAllAsync())
                .ToDictionary(c => c.Id, c => c.Name);

            var groupNames = (await _inventoryReportService.GetAllGroupsAsync())
                .ToDictionary(g => int.Parse(g.Value), g => g.Text);

            var statusNames = (await _inventoryReportService.GetAllStatusesAsync())
                .ToDictionary(s => int.Parse(s.Value), s => s.Text);

            var productNames = (await _inventoryReportService.GetAllProductsAsync())
                .ToDictionary(p => int.Parse(p.Value), p => p.Text);

            // گرفتن نام زون‌ها و سکشن‌ها از آیتم‌هایی که فیلتر شد
            var zoneNames = items
                .Where(i => i.ZoneId.HasValue && !string.IsNullOrEmpty(i.ZoneName))
                .GroupBy(i => i.ZoneId.Value)
                .ToDictionary(g => g.Key, g => g.First().ZoneName);

            var sectionNames = items
                .Where(i => i.SectionId.HasValue && !string.IsNullOrEmpty(i.SectionName))
                .GroupBy(i => i.SectionId.Value)
                .ToDictionary(g => g.Key, g => g.First().SectionName);

            // ساخت ViewModel برای PDF
            var vm = new InventoryReportPdfViewModel
            {
                Items = items,
                Filter = filter,
                WarehouseNames = warehouseNames,
                CategoryNames = categoryNames,
                GroupNames = groupNames,
                StatusNames = statusNames,
                ProductNames = productNames,
                ZoneNames = zoneNames,
                SectionNames = sectionNames
            };

            // خروجی PDF
            return new ViewAsPdf("InventoryPdfView", vm)
            {
                FileName = $"InventoryReport_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10),
                CustomSwitches = "--disable-smart-shrinking --print-media-type --background"
            };
        }


    }
}