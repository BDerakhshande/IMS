using System.Globalization;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProcurementManagement.Service;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace IMS.Areas.ProcurementManagement.Controllers
{
    [Area("ProcurementManagement")]
    public class PurchaseRequestFlatItemController : Controller
    {
        private readonly IPurchaseRequestFlatItemService _flatItemService;
        private readonly IApplicationDbContext _projectContext;
        private readonly IWarehouseDbContext _warehouseContext;
        private readonly IProcurementManagementDbContext _procurementContext;

        public PurchaseRequestFlatItemController(
            IPurchaseRequestFlatItemService flatItemService,
            IApplicationDbContext projectContext,
            IWarehouseDbContext warehouseContext,
            IProcurementManagementDbContext procurementContext)
        {
            _flatItemService = flatItemService;
            _projectContext = projectContext;
            _warehouseContext = warehouseContext;
            _procurementContext = procurementContext;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(FlatItems));
        }




        [HttpGet]
        public async Task<IActionResult> FlatItems(
            List<ProductFilterDto>? products = null,
            string? requestNumber = null,
            string? requestTitle = null,   
            string? fromDateString = null,
            string? toDateString = null,
            int? requestTypeId = null,
            int? projectId = null,
            CancellationToken cancellationToken = default)
        {
            // تنظیم تاریخ پیش‌فرض
            var today = DateTime.Today;
            var pc = new PersianCalendar();
            string defaultDateString = $"{pc.GetYear(today)}/{pc.GetMonth(today):D2}/{pc.GetDayOfMonth(today):D2}";

            fromDateString ??= defaultDateString;
            toDateString ??= defaultDateString;

            var fromDate = ParsePersianDate(fromDateString);
            var toDate = ParsePersianDate(toDateString);

            var flatItems = await _flatItemService.GetFlatItemsAsync(
                requestNumber,
                requestTitle, 
                fromDate,
                toDate,
                requestTypeId,
                projectId,
                products,
                cancellationToken);

            var requestTypes = await _procurementContext.RequestTypes
                .Select(rt => new RequestTypeDto { Id = rt.Id, Name = rt.Name })
                .ToListAsync(cancellationToken);

            var model = new PurchaseRequestFlatItemsViewModel
            {
                FlatItems = flatItems,
                Projects = await _projectContext.Projects.ToListAsync(cancellationToken),
                Categories = await _warehouseContext.Categories.ToListAsync(cancellationToken),
                Groups = await _warehouseContext.Groups.ToListAsync(cancellationToken),
                Statuses = await _warehouseContext.Statuses.ToListAsync(cancellationToken),
                Products = products ?? new List<ProductFilterDto> { new ProductFilterDto() },
                RequestTypes = requestTypes,
                SelectedRequestNumber = requestNumber,
                SelectedRequestTitle = requestTitle, // ← اگر در ViewModel دارید
                SelectedRequestTypeId = requestTypeId,
                FromDateString = fromDateString,
                ToDateString = toDateString
            };

            return View(model);
        }




        [HttpGet]
        public async Task<JsonResult> GetGroupsByCategoryId(int categoryId, CancellationToken cancellationToken)
        {
            var groups = await _warehouseContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .Select(g => new { g.Id, g.Name })
                .ToListAsync(cancellationToken);

            return Json(groups);
        }

        [HttpGet]
        public async Task<JsonResult> GetStatusesByGroupId(int groupId, CancellationToken cancellationToken)
        {
            var statuses = await _warehouseContext.Statuses
                .Where(s => s.GroupId == groupId)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync(cancellationToken);

            return Json(statuses);
        }

        [HttpGet]
        public async Task<JsonResult> GetProductsByStatusId(int statusId, CancellationToken cancellationToken)
        {
            var products = await _warehouseContext.Products
                .Where(p => p.StatusId == statusId)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync(cancellationToken);

            return Json(products);
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



     

        [HttpGet]
        public async Task<IActionResult> ExportPdf(
            List<ProductFilterDto>? products = null,
            string? requestNumber = null,
            string? requestTitle = null,
            string? fromDateString = null,
            string? toDateString = null,
            int? requestTypeId = null,
            int? projectId = null,
            CancellationToken cancellationToken = default)
        {
            // تبدیل تاریخ‌ها
            var fromDate = ParsePersianDate(fromDateString);
            var toDate = ParsePersianDate(toDateString);

            // دریافت داده‌ها
            var flatItems = await _flatItemService.GetFlatItemsAsync(
                requestNumber,
                requestTitle,
                fromDate,
                toDate,
                requestTypeId,
                projectId,
                products,
                cancellationToken);

            // آماده‌سازی مدل برای پرینت
            var model = new PurchaseRequestPrintViewModel
            {
                FlatItems = flatItems,
                RequestNumber = requestNumber,
                RequestTitle = requestTitle,
                FromDate = fromDateString,
                ToDate = toDateString,
                PrintDate = DateTime.Now
            };

            // اگر نوع درخواست مشخص شده، نام آن را بیابید
            if (requestTypeId.HasValue)
            {
                var requestType = await _procurementContext.RequestTypes
                    .FirstOrDefaultAsync(rt => rt.Id == requestTypeId.Value, cancellationToken);
                model.RequestTypeName = requestType?.Name;
            }

            // اگر پروژه مشخص شده، نام آن را بیابید
            if (projectId.HasValue)
            {
                var project = await _projectContext.Projects
                    .FirstOrDefaultAsync(p => p.Id == projectId.Value, cancellationToken);
                model.ProjectName = project?.ProjectName;
            }

            // استفاده از Rotativa برای تولید PDF
            return new ViewAsPdf("Print", model)
            {
                FileName = $"PurchaseRequests_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Landscape,
                PageMargins = { Left = 5, Right = 5, Top = 10, Bottom = 10 },
                CustomSwitches = "--print-media-type --header-center \"گزارش درخواست‌های خرید\""
            };
        }




        [HttpGet]
        public async Task<IActionResult> ExportToExcel(
       string? requestNumber,
       string? requestTitle,
       DateTime? fromDate,
       DateTime? toDate,
       int? requestTypeId,
       int? projectId,
       [FromQuery] List<ProductFilterDto>? products)
        {
            // دریافت بایت‌های Excel از سرویس
            var fileBytes = await _flatItemService.ExportFlatItemsToExcelAsync(
                requestNumber,
                requestTitle,
                fromDate,
                toDate,
                requestTypeId,
                projectId,
                products);

            // نام فایل
            var fileName = $"PurchaseRequests_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            // برگرداندن فایل
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
