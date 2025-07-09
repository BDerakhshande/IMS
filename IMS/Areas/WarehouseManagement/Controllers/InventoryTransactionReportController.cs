using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IMS.Application.WarehouseManagement.DTOs;
using System.Globalization;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class InventoryTransactionReportController : Controller
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ICategoryService _categoryService;
        private readonly IInventoryTransactionReportService _reportService;
        private readonly IWarehouseDbContext _dbContext;

        public InventoryTransactionReportController(
            IInventoryTransactionReportService reportService,
            IWarehouseService warehouseService,
            ICategoryService categoryService,
            IWarehouseDbContext dbContext)
        {
            _reportService = reportService;
            _warehouseService = warehouseService;
            _categoryService = categoryService;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            await PopulateSelectLists();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] InventoryTransactionReportItemDto filter)
        {
            var fromDate = ParseDate(filter.FromDate);
            var toDate = ParseDate(filter.ToDate);
            var documentType = MapToEnglishDocumentType(filter.DocumentType);

            var reportData = await _reportService.GetReportAsync(
                filter.WarehouseName,
                filter.DepartmentName,
                filter.SectionName,
                filter.CategoryName,
                filter.GroupName,
                filter.StatusName,
                filter.ProductName,
                fromDate,
                toDate,
                documentType
            );

            var result = reportData.Select(d => new InventoryTransactionReportDto
            {
                Date = d.Date,
                DocumentNumber = d.DocumentNumber,
                DocumentType = MapToPersianDocumentType(d.DocumentType),
                CategoryName = d.CategoryName,
                GroupName = d.GroupName,
                StatusName = d.StatusName,
                ProductName = d.ProductName,
                SourceWarehouseName = d.SourceWarehouseName,
                SourceDepartmentName = d.SourceDepartmentName,
                SourceSectionName = d.SourceSectionName,
                DestinationWarehouseName = d.DestinationWarehouseName,
                DestinationDepartmentName = d.DestinationDepartmentName,
                DestinationSectionName = d.DestinationSectionName,
                Quantity = d.Quantity
            });

            return Json(result);
        }

        private async Task PopulateSelectLists(InventoryReportFilterDto filter = null)
        {
            ViewBag.Warehouses = new SelectList(await _warehouseService.GetAllWarehousesAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");
            ViewBag.Groups = new SelectList(await _reportService.GetAllGroupsAsync(), "Value", "Text");
            ViewBag.Statuses = new SelectList(await _reportService.GetAllStatusesAsync(), "Value", "Text");
            ViewBag.Products = new SelectList(await _reportService.GetAllProductsAsync(), "Value", "Text");

            var zones = new List<SelectListItem>();
            var sections = new List<SelectListItem>();

            if (filter?.Warehouses != null)
            {
                foreach (var warehouse in filter.Warehouses)
                {
                    if (warehouse.WarehouseId > 0)
                    {
                        var zoneList = await _reportService.GetZonesByWarehouseIdAsync(warehouse.WarehouseId);
                        zones.AddRange(zoneList.Select(z => new SelectListItem { Value = z.Value, Text = z.Text }));
                    }

                    if (warehouse.ZoneIds != null && warehouse.ZoneIds.Any())
                    {
                        var sectionList = await _reportService.GetSectionsByZoneIdsAsync(warehouse.ZoneIds);
                        sections.AddRange(sectionList.Select(s => new SelectListItem { Value = s.Value, Text = s.Text }));
                    }
                }
            }

            ViewBag.Zones = new SelectList(zones.DistinctBy(z => z.Value), "Value", "Text");
            ViewBag.Sections = new SelectList(sections.DistinctBy(s => s.Value), "Value", "Text");
        }

        // ======== Helper Methods ========
        private DateTime? ParseDate(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return null;

            return DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsedDate) ? parsedDate : null;
        }

        private string? MapToEnglishDocumentType(string? type) => type switch
        {
            "رسید" => "Receipt",
            "حواله" => "Issue",
            "انتقال" => "Transfer",
            _ => null
        };

        private string MapToPersianDocumentType(string type) => type switch
        {
            "Receipt" => "رسید",
            "Issue" => "حواله",
            "Transfer" => "انتقال",
            _ => type
        };

        // ======== Ajax Actions ========
        [HttpGet]
        public async Task<JsonResult> GetZonesByWarehouseId(int warehouseId)
        {
            var zones = await _reportService.GetZonesByWarehouseIdAsync(warehouseId);
            return Json(zones);
        }

        [HttpGet]
        public async Task<JsonResult> GetSectionsByZoneIds([FromQuery] List<int> zoneIds)
        {
            var sections = await _reportService.GetSectionsByZoneIdsAsync(zoneIds);
            return Json(sections);
        }

        [HttpGet]
        public async Task<JsonResult> GetGroupsByCategoryId(int categoryId)
        {
            var groups = await _reportService.GetGroupsByCategoryIdAsync(categoryId);
            return Json(groups);
        }

        [HttpGet]
        public async Task<JsonResult> GetStatusesByGroupId(int groupId)
        {
            var statuses = await _reportService.GetStatusesByGroupIdAsync(groupId);
            return Json(statuses);
        }

        [HttpGet]
        public async Task<JsonResult> GetProductsByStatusId(int statusId)
        {
            var products = await _reportService.GetProductsByStatusIdAsync(statusId);
            return Json(products);
        }
    }
}
