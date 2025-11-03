using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Text.Json;

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

        [HttpPost]  // بدون [FromBody] – از Form data bind می‌شود
        public async Task<IActionResult> ExportInventoryToExcel(InventoryReportFilterDto filter)
        {
            // لاگ برای دیباگ (حذف کنید بعد از تست)
            Console.WriteLine($"UniqueCodes in filter for Excel: {string.Join(", ", filter.UniqueCodes ?? new List<string>())}");
            Console.WriteLine($"Warehouses count: {filter.Warehouses?.Count ?? 0}");
            Console.WriteLine($"ProductId: {filter.ProductId}");

            // گرفتن داده‌ها از سرویس
            var data = await _inventoryReportService.GetInventoryReportAsync(filter);

            if (data == null || !data.Any())
                return BadRequest("داده‌ای برای گزارش وجود ندارد");

            // فیلتر اضافی بر اساس کد یکتا اگر وجود داشته باشد (مشابه PDF برای consistency)
            if (filter.UniqueCodes != null && filter.UniqueCodes.Any())
            {
                var codes = filter.UniqueCodes
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                // لاگ برای دیباگ
                Console.WriteLine($"Filtered data count after UniqueCodes in Excel: {data.Count(i => i.UniqueCodes.Any(uc => codes.Contains(uc)))}");

                data = data.Where(i => i.UniqueCodes.Any(uc => codes.Contains(uc))).ToList();

                if (!data.Any())
                    return BadRequest("هیچ داده‌ای با کد یکتاهای انتخاب شده یافت نشد");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventory Report");

            // ستون‌ها
            worksheet.Cell(1, 1).Value = "نام انبار";
            worksheet.Cell(1, 2).Value = "قسمت";
            worksheet.Cell(1, 3).Value = "بخش";
            worksheet.Cell(1, 4).Value = "دسته‌بندی";
            worksheet.Cell(1, 5).Value = "گروه";
            worksheet.Cell(1, 6).Value = "طبقه";
            worksheet.Cell(1, 7).Value = "کالا";
            worksheet.Cell(1, 8).Value = "موجودی";
            worksheet.Cell(1, 9).Value = "کدهای یکتا";

            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.WarehouseName ?? "-";
                worksheet.Cell(row, 2).Value = item.ZoneName ?? "-";
                worksheet.Cell(row, 3).Value = item.SectionName ?? "-";
                worksheet.Cell(row, 4).Value = item.CategoryName ?? "-";
                worksheet.Cell(row, 5).Value = item.GroupName ?? "-";
                worksheet.Cell(row, 6).Value = item.StatusName ?? "-";
                worksheet.Cell(row, 7).Value = item.ProductName ?? "-";
                worksheet.Cell(row, 8).Value = item.Quantity;
                worksheet.Cell(row, 9).Value = string.Join(", ", item.UniqueCodes ?? new List<string>());
                row++;
            }

            var total = data.Sum(x => x.Quantity);
            worksheet.Cell(row, 7).Value = "جمع کل:";
            worksheet.Cell(row, 8).Value = total;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileBytes = stream.ToArray();

            var fileName = $"InventoryReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        [HttpPost]
        public async Task<IActionResult> ExportToPdf(InventoryReportFilterDto filter)
        {
            // لاگ برای دیباگ (حذف کنید بعد از تست)
            Console.WriteLine($"UniqueCodes in filter: {string.Join(", ", filter.UniqueCodes ?? new List<string>())}");

            // گرفتن داده‌ها از سرویس
            var items = await _inventoryReportService.GetInventoryReportAsync(filter);

            if (items == null || !items.Any())
                return Content("داده‌ای برای گزارش وجود ندارد");

            // فیلتر بر اساس کد یکتا اگر وجود داشته باشد
            if (filter.UniqueCodes != null && filter.UniqueCodes.Any())
            {
                var codes = filter.UniqueCodes
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                // لاگ برای دیباگ
                Console.WriteLine($"Filtered items count after UniqueCodes: {items.Count(i => i.UniqueCodes.Any(uc => codes.Contains(uc)))}");

                // فقط آیتم‌هایی که حداقل یکی از کدهایشان با کدهای فیلتر شده مطابقت دارد
                items = items.Where(i => i.UniqueCodes.Any(uc => codes.Contains(uc))).ToList();

                if (!items.Any())
                    return Content("هیچ داده‌ای با کد یکتاهای انتخاب شده یافت نشد");
            }

            // دیکشنری‌ها فقط بر اساس آیتم‌های فیلتر شده ساخته می‌شوند
            var warehouseNames = items
                .Where(i => i.WarehouseId > 0)
                .GroupBy(i => i.WarehouseId)
                .ToDictionary(g => g.Key, g => g.First().WarehouseName);

            var categoryNames = items
                .Where(i => i.CategoryId > 0)
                .GroupBy(i => i.CategoryId)
                .ToDictionary(g => g.Key, g => g.First().CategoryName);

            var groupNames = items
                .Where(i => i.GroupId > 0)
                .GroupBy(i => i.GroupId)
                .ToDictionary(g => g.Key, g => g.First().GroupName);

            var statusNames = items
                .Where(i => i.StatusId > 0)
                .GroupBy(i => i.StatusId)
                .ToDictionary(g => g.Key, g => g.First().StatusName);

            var productNames = items
                .Where(i => i.ProductId > 0)
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.First().ProductName);

            var zoneNames = items
                .Where(i => i.ZoneId.HasValue)
                .GroupBy(i => i.ZoneId.Value)
                .ToDictionary(g => g.Key, g => g.First().ZoneName);

            var sectionNames = items
                .Where(i => i.SectionId.HasValue)
                .GroupBy(i => i.SectionId.Value)
                .ToDictionary(g => g.Key, g => g.First().SectionName);

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
                SectionNames = sectionNames,
                UniqueCodesFilter = filter.UniqueCodes?.Where(c => !string.IsNullOrWhiteSpace(c)).ToList() ?? new List<string>()
            };

            return await Task.Run(() =>
                new ViewAsPdf("InventoryPdfView", vm)
                {
                    FileName = $"InventoryReport_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10),
                    CustomSwitches = "--disable-smart-shrinking --print-media-type --background"
                });
        }
        [HttpGet]
        public async Task<IActionResult> GetUniqueCodes(int productId, int? sourceWarehouseId = null)
        {
            var inventoryQuery = _dbContext.Inventories
                .AsNoTracking()
                .Include(i => i.InventoryItems)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .Where(i => i.ProductId == productId);

            if (sourceWarehouseId.HasValue)
                inventoryQuery = inventoryQuery.Where(i => i.WarehouseId == sourceWarehouseId.Value);

            var inventoryCodes = await inventoryQuery
                .SelectMany(i => i.InventoryItems
                    .Where(ii => !string.IsNullOrWhiteSpace(ii.UniqueCode))
                .Select(ii => new
                {
                    id = ii.UniqueCode.ToString(),
                    text =
        "C" + (i.Product.Status.Group.Category.Code ?? "NA") +
        "G" + (i.Product.Status.Group.Code ?? "NA") +
        "S" + (i.Product.Status.Code ?? "NA") +
        "P" + (i.Product.Code ?? "NA") + "_" + (ii.UniqueCode ?? "NA"),
                    hierarchy =
        "C" + (i.Product.Status.Group.Category.Code ?? "NA") +
        "G" + (i.Product.Status.Group.Code ?? "NA") +
        "S" + (i.Product.Status.Code ?? "NA") +
        "P" + (i.Product.Code ?? "NA") +
        " (" + (ii.UniqueCode ?? "NA") + ")"
                })
)
                .ToListAsync();

            // اگر موجودی در انبار نداشت، از ProductItems بگیر
            if (!inventoryCodes.Any())
            {
                var productItems = await _dbContext.ProductItems
                    .AsNoTracking()
                    .Include(pi => pi.Product)
                        .ThenInclude(p => p.Status)
                            .ThenInclude(s => s.Group)
                                .ThenInclude(g => g.Category)
                    .Where(pi => pi.ProductId == productId && !string.IsNullOrWhiteSpace(pi.UniqueCode))
                   .Select(pi => new
                   {
                       id = pi.UniqueCode.ToString(),
                       text =
        "C" + (pi.Product.Status.Group.Category.Code ?? "NA") +
        "G" + (pi.Product.Status.Group.Code ?? "NA") +
        "S" + (pi.Product.Status.Code ?? "NA") +
        "P" + (pi.Product.Code ?? "NA") + "_" + (pi.UniqueCode ?? "NA"),
                       hierarchy =
        "C" + (pi.Product.Status.Group.Category.Code ?? "NA") +
        "G" + (pi.Product.Status.Group.Code ?? "NA") +
        "S" + (pi.Product.Status.Code ?? "NA") +
        "P" + (pi.Product.Code ?? "NA") +
        " (" + (pi.UniqueCode ?? "NA") + ")"
                   })

                    .ToListAsync();

                return Json(productItems);
            }

            return Json(inventoryCodes);
        }

        [HttpGet]
        public async Task<IActionResult> GetUniqueCodeDetails(string uniqueCodeId)
        {
            if (string.IsNullOrWhiteSpace(uniqueCodeId))
                return BadRequest("کد یکتا نامعتبر است.");

            var inventoryItem = await _dbContext.InventoryItems
                .AsNoTracking()
                .Include(ii => ii.Inventory)
                .ThenInclude(i => i.Warehouse)
                .Include(ii => ii.Inventory)
                .ThenInclude(i => i.Zone)  // تصحیح: Zone به جای StorageZone
                .Include(ii => ii.Inventory)
                .ThenInclude(i => i.Section)  // تصحیح: Section به جای StorageSection
                .Include(ii => ii.Inventory)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Status)
                .ThenInclude(s => s.Group)
                .ThenInclude(g => g.Category)
                .Where(ii => ii.UniqueCode == uniqueCodeId)
                .Select(ii => new
                {
                    UniqueCode = ii.UniqueCode,
                    WarehouseId = ii.Inventory.WarehouseId,
                    WarehouseName = ii.Inventory.Warehouse.Name,
                    ZoneId = ii.Inventory.ZoneId,
                    ZoneName = ii.Inventory.Zone != null ? ii.Inventory.Zone.Name : null,  // تصحیح: Zone به جای StorageZone
                    SectionId = ii.Inventory.SectionId,
                    SectionName = ii.Inventory.Section != null ? ii.Inventory.Section.Name : null,  // تصحیح: Section به جای StorageSection
                                                                                                    // اضافه کردن IDهای سلسله‌مراتب
                    CategoryId = ii.Inventory.Product.Status.Group.CategoryId,
                    GroupId = ii.Inventory.Product.Status.GroupId,
                    StatusId = ii.Inventory.Product.StatusId,
                    ProductId = ii.Inventory.ProductId
                })
                .FirstOrDefaultAsync();

            if (inventoryItem == null)
            {
                // اگر در موجودی نبود، از ProductItems بگیر (بدون انبار)
                var productItem = await _dbContext.ProductItems
                    .AsNoTracking()
                    .Include(pi => pi.Product)
                    .ThenInclude(p => p.Status)
                    .ThenInclude(s => s.Group)
                    .ThenInclude(g => g.Category)
                    .Where(pi => pi.UniqueCode == uniqueCodeId)
                    .Select(pi => new
                    {
                        UniqueCode = pi.UniqueCode,
                        WarehouseId = (int?)null,
                        WarehouseName = (string?)null,
                        ZoneId = (int?)null,
                        ZoneName = (string?)null,
                        SectionId = (int?)null,
                        SectionName = (string?)null,
                        // IDهای سلسله‌مراتب
                        CategoryId = pi.Product.Status.Group.CategoryId,
                        GroupId = pi.Product.Status.GroupId,
                        StatusId = pi.Product.StatusId,
                        ProductId = pi.ProductId
                    })
                    .FirstOrDefaultAsync();

                if (productItem == null) return NotFound();

                return Json(new
                {
                    uniqueCode = productItem.UniqueCode,
                    warehouseId = productItem.WarehouseId,
                    warehouseName = productItem.WarehouseName,
                    zoneId = productItem.ZoneId,
                    zoneName = productItem.ZoneName,
                    sectionId = productItem.SectionId,
                    sectionName = productItem.SectionName,
                    categoryId = productItem.CategoryId,
                    groupId = productItem.GroupId,
                    statusId = productItem.StatusId,
                    productId = productItem.ProductId
                });
            }

            return Json(new
            {
                uniqueCode = inventoryItem.UniqueCode,
                warehouseId = inventoryItem.WarehouseId,
                warehouseName = inventoryItem.WarehouseName,
                zoneId = inventoryItem.ZoneId,
                zoneName = inventoryItem.ZoneName,
                sectionId = inventoryItem.SectionId,
                sectionName = inventoryItem.SectionName,
                categoryId = inventoryItem.CategoryId,
                groupId = inventoryItem.GroupId,
                statusId = inventoryItem.StatusId,
                productId = inventoryItem.ProductId
            });
        }

    }
}