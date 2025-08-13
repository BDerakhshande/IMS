using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IMS.Application.WarehouseManagement.DTOs;
using System.Globalization;
using IMS.Domain.WarehouseManagement.Enums;
using Rotativa.AspNetCore;
using IMS.Models.ProMan;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class InventoryTransactionReportController : Controller
    {
        private readonly IInventoryTransactionReportService _reportService;
        private readonly IWarehouseService _warehouseService;
        private readonly ICategoryService _categoryService;

        public InventoryTransactionReportController(
            IInventoryTransactionReportService reportService,
            IWarehouseService warehouseService,
            ICategoryService categoryService)
        {
            _reportService = reportService;
            _warehouseService = warehouseService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            await PopulateSelectLists();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] InventoryTransactionReportItemDto filter)
        {
            try
            {
                filter.DocumentType = MapToEnglishDocumentType(filter.DocumentType);

                // اگر نوع سند داده شده بود اعتبارسنجی شود، در غیر اینصورت فیلتر نوع سند اعمال نشود
                if (!string.IsNullOrWhiteSpace(filter.DocumentType))
                {
                    if (filter.DocumentType != "Conversion" && !Enum.TryParse<ReceiptOrIssueType>(filter.DocumentType, out _))
                    {
                        return BadRequest("نوع سند نامعتبر است.");
                    }
                }

                else
                {
                    // اگر DocumentType خالی بود مقدارش را null بگذاریم تا در سرویس فیلتر نشود
                    filter.DocumentType = null;
                }

                filter.FromDate = ParsePersianDate(filter.FromDateString);
                filter.ToDate = ParsePersianDate(filter.ToDateString);

                var reportData = await _reportService.GetReportAsync(filter);

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
            catch (Exception ex)
            {
                Console.WriteLine("خطا در دریافت گزارش: " + ex.ToString());
                return StatusCode(500, $"خطای سرور: {ex.Message}");
            }
        }







        private async Task PopulateSelectLists()
        {
            ViewBag.Warehouses = new SelectList(await _warehouseService.GetAllWarehousesAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");
            ViewBag.Groups = new SelectList(await _reportService.GetAllGroupsAsync(), "Value", "Text");
            ViewBag.Statuses = new SelectList(await _reportService.GetAllStatusesAsync(), "Value", "Text");
            ViewBag.Products = new SelectList(await _reportService.GetAllProductsAsync(), "Value", "Text");

            ViewBag.Zones = new SelectList(await _reportService.GetAllZonesAsync(), "Value", "Text");
            ViewBag.Sections = new SelectList(await _reportService.GetAllSectionsAsync(), "Value", "Text");
        }

        // ======== Helper Methods ========
        private DateTime? ParsePersianDate(string? persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
            {
                Console.WriteLine("Persian date is null or empty");
                return null;
            }

            try
            {
                var parts = persianDate.Split('/');
                if (parts.Length != 3)
                {
                    Console.WriteLine($"Invalid date format: {persianDate}");
                    return null;
                }

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                var pc = new PersianCalendar();
                var result = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                Console.WriteLine($"Parsed date: {persianDate} -> {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing date {persianDate}: {ex.Message}");
                return null;
            }
        }



        private string? MapToEnglishDocumentType(string? type) => type switch
        {
            "رسید" => "Receipt",
            "حواله" => "Issue",
            "انتقال" => "Transfer",
            "تبدیل" => "Conversion",   // اضافه شده
            _ => null
        };

        private string MapToPersianDocumentType(string type) => type switch
        {
            "Receipt" => "رسید",
            "Issue" => "حواله",
            "Transfer" => "انتقال",
            "Conversion" => "تبدیل",   // اضافه شده
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
        public async Task<JsonResult> GetSectionsByZoneId(int zoneId)
        {
            var sections = await _reportService.GetSectionsByZoneIdAsync(zoneId);
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



        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromForm] InventoryTransactionReportItemDto filter)
        {
            try
            {
                filter.DocumentType = MapToEnglishDocumentType(filter.DocumentType);

                if (!string.IsNullOrWhiteSpace(filter.DocumentType))
                {
                    if (filter.DocumentType != "Conversion" && !Enum.TryParse<ReceiptOrIssueType>(filter.DocumentType, out _))
                        return BadRequest("نوع سند نامعتبر است.");
                }
                else
                {
                    filter.DocumentType = null;
                }

                filter.FromDate = ParsePersianDate(filter.FromDateString);
                filter.ToDate = ParsePersianDate(filter.ToDateString);

                var fileContent = await _reportService.ExportReportToExcelAsync(filter);

                var fileName = $"InventoryTransactionReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(fileContent,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("خطا در خروجی اکسل: " + ex);
                return StatusCode(500, $"خطای سرور: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportToPdf([FromForm] InventoryTransactionReportItemDto filter)
        {
            try
            {
                // تبدیل نوع سند به انگلیسی
                filter.DocumentType = MapToEnglishDocumentType(filter.DocumentType);

                if (!string.IsNullOrWhiteSpace(filter.DocumentType))
                {
                    if (filter.DocumentType != "Conversion" && !Enum.TryParse<ReceiptOrIssueType>(filter.DocumentType, out _))
                        return BadRequest("نوع سند نامعتبر است.");
                }
                else
                {
                    filter.DocumentType = null;
                }

                // تبدیل تاریخ‌های فارسی به DateTime
                filter.FromDate = ParsePersianDate(filter.FromDateString);
                filter.ToDate = ParsePersianDate(filter.ToDateString);

                // دریافت داده‌ها
                var items = await _reportService.GetReportAsync(filter);

                // آماده‌سازی ViewModel
                var vm = new InventoryTransactionReportPdfViewModel
                {
                    Items = items,
                    Filter = filter,

                    WarehouseName = filter.WarehouseName,
                    ZoneName = filter.ZoneName,
                    SectionName = filter.SectionName,

                    CategoryName = filter.CategoryName,
                    GroupName = filter.GroupName,
                    StatusName = filter.StatusName,
                    ProductName = filter.ProductName
                };


                // بازگشت PDF با Rotativa
                return new ViewAsPdf("InventoryTransactionPdfView", vm)
                {
                    FileName = $"InventoryTransactionReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--disable-smart-shrinking --print-media-type --background"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("خطا در خروجی PDF: " + ex);
                return StatusCode(500, $"خطای سرور: {ex.Message}");
            }
        }


    }
}
