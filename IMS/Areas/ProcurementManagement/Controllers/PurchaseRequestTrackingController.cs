using DocumentFormat.OpenXml.InkML;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProcurementManagement.Service;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.ProcurementManagement.Entities;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.ProcurementManagement.Controllers
{
    [Area("ProcurementManagement")]
    public class PurchaseRequestTrackingController : Controller
    {
        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly IPurchaseRequestTrackingService _purchaseRequestTrackingService;
        private readonly IProcurementManagementDbContext _procurementManagementDbContext;
        private readonly IApplicationDbContext _applicationDbContext;
        private readonly IWarehouseDbContext _warehouseContext;

        public PurchaseRequestTrackingController(IPurchaseRequestService purchaseRequestService,
            IPurchaseRequestTrackingService purchaseRequestTrackingService,
            IProcurementManagementDbContext procurementManagementDbContext,
            IApplicationDbContext applicationDbContext, IWarehouseDbContext warehouseContext)
        {
            _purchaseRequestService = purchaseRequestService;
            _purchaseRequestTrackingService = purchaseRequestTrackingService;
            _procurementManagementDbContext = procurementManagementDbContext;
            _applicationDbContext = applicationDbContext;
            _warehouseContext = warehouseContext;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _purchaseRequestService.GetAllAsync();
            return View(list);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int purchaseRequestId)
        {
            var items = await _purchaseRequestTrackingService.GetItemsWithStockAndNeedAsync(purchaseRequestId);

            // اگر لازم باشه اطلاعات هدر درخواست رو هم ارسال کنی:
            var header = await _procurementManagementDbContext.PurchaseRequests
                .AsNoTracking()
                .Where(pr => pr.Id == purchaseRequestId)
                .Select(pr => new {
                    pr.Id,
                    pr.RequestNumber,
                    pr.RequestDate,
                    pr.Title,
                    pr.Status
                })
                .FirstOrDefaultAsync();

            if (header == null)
                return NotFound();

            var model = new PurchaseRequestDetailsViewModel
            {
                PurchaseRequestId = header.Id,
                RequestNumber = header.RequestNumber,
                RequestDate = header.RequestDate,
                Title = header.Title,
                Status = header.Status,
                Items = items
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSupplyStop(int id, bool stopSupply)
        {
            var item = await _procurementManagementDbContext.PurchaseRequestItems.FindAsync(id);
            if (item == null)
                return Json(new { success = false, message = "آیتم پیدا نشد." });

            item.IsSupplyStopped = stopSupply;
            await _procurementManagementDbContext.SaveChangesAsync(CancellationToken.None);

            // گرفتن داده‌های به‌روز شده آیتم‌ها برای بازگرداندن به کلاینت
            var updatedItems = await _purchaseRequestTrackingService.GetItemsWithStockAndNeedAsync(item.PurchaseRequestId);

            // بازگرداندن وضعیت و داده‌های به‌روز شده به کلاینت
            return Json(new
            {
                success = true,
                isSupplyStopped = item.IsSupplyStopped,
                updatedItems = updatedItems
            });
        }



        [HttpPost]
        public async Task<IActionResult> AddPurchaseRequestItem([FromBody] PurchaseRequestItemDto dto)
        {
            try
            {
                // گرفتن نوع درخواست از دیتابیس
                var requestType = await _procurementManagementDbContext.RequestTypes
                    .FirstOrDefaultAsync(rt => rt.Name == "درخواست خرید کالا");

                if (requestType == null)
                    return Json(new { success = false, message = "نوع درخواست یافت نشد!" });

                // ایجاد ریکوست
                var request = new PurchaseRequest
                {
                    
                    RequestDate = DateTime.Now, // یا dto.RequestDate
                    Title = "درخواست خرید کالا",
                    RequestTypeId = requestType.Id,
                };

                // ایجاد آیتم
                var item = new PurchaseRequestItem
                {
                    CategoryId = dto.CategoryId,
                    GroupId = dto.GroupId,
                    StatusId = dto.StatusId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    Unit = dto.Unit,
                    ProjectId = dto.ProjectId,
                    Description = dto.Description
                };

                request.Items.Add(item);

                _procurementManagementDbContext.PurchaseRequests.Add(request);
                await _procurementManagementDbContext.SaveChangesAsync(CancellationToken.None);

                // بازگرداندن DTO برای جدول
                var resultDto = new
                {
                    Id = item.Id,
                    RequestNumber = request.RequestNumber,
                    RequestDate = request.RequestDate.ToString("yyyy/MM/dd"),
                    CategoryName = dto.CategoryName,
                    GroupName = dto.GroupName,
                    Status = dto.Status,
                    ProductName = dto.ProductName,
                    Quantity = dto.Quantity,
                    Unit = dto.Unit,
                    ProjectName = dto.ProjectName,
                    Description = dto.Description,
                    TotalStock = dto.TotalStock,
                    PendingRequests = dto.PendingRequests,
                    NeedToSupply = dto.NeedToSupply,
                    IsSupplyStopped = false
                };

                return Json(new { success = true, item = resultDto });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToFlatItemsAndShow(
      int id,
      int? projectId = null,
      int? categoryId = null,
      int? groupId = null,
      int? statusId = null,
      int? productId = null,
      int? requestTypeId = null,
      DateTime? fromDate = null,
      DateTime? toDate = null)
        {
            try
            {
                // بارگذاری آیتم همراه با درخواست خرید و نوع درخواست
                var item = await _procurementManagementDbContext.PurchaseRequestItems
                    .Include(i => i.PurchaseRequest)
                        .ThenInclude(r => r.RequestType)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                    return NotFound();

                // بررسی وجود آیتم در لیست تخت
                var exists = await _procurementManagementDbContext.PurchaseRequestFlatItems
                    .AnyAsync(f => f.ProductId == item.ProductId && f.RequestNumber == item.PurchaseRequest.RequestNumber);

                if (exists)
                {
                    TempData["ErrorMessage"] = "محصول از قبل در درخواست خرید اضافه شده است.";
                    return RedirectToAction(
                        "Details",
                        "PurchaseRequestTracking",
                        new { area = "ProcurementManagement", purchaseRequestId = item.PurchaseRequestId });
                }

                // گرفتن همه نام‌های مرتبط در یک مرحله
                var projectNameTask = item.ProjectId.HasValue
                    ? _applicationDbContext.Projects
                        .Where(p => p.Id == item.ProjectId.Value)
                        .Select(p => p.ProjectName)
                        .FirstOrDefaultAsync()
                    : Task.FromResult<string?>(null);

                var categoryNameTask = _warehouseContext.Categories
                    .Where(c => c.Id == item.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();

                var groupNameTask = _warehouseContext.Groups
                    .Where(g => g.Id == item.GroupId)
                    .Select(g => g.Name)
                    .FirstOrDefaultAsync();

                var statusNameTask = _warehouseContext.Statuses
                    .Where(s => s.Id == item.StatusId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();

                var productNameTask = _warehouseContext.Products
                    .Where(p => p.Id == item.ProductId)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync();

                await Task.WhenAll(projectNameTask, categoryNameTask, groupNameTask, statusNameTask, productNameTask);

                // ایجاد موجودیت تخت
                var flatEntity = new PurchaseRequestFlatItem
                {
                    RequestNumber = item.PurchaseRequest.RequestNumber,
                    RequestTitle = item.PurchaseRequest.Title,
                    RequestTypeId = item.PurchaseRequest.RequestTypeId,
                    RequestTypeName = item.PurchaseRequest.RequestType?.Name,
                    ProjectId = item.ProjectId ?? 0,
                    ProjectName = projectNameTask.Result,
                    CategoryId = item.CategoryId,
                    CategoryName = categoryNameTask.Result,
                    GroupId = item.GroupId,
                    GroupName = groupNameTask.Result,
                    StatusId = item.StatusId,
                    StatusName = statusNameTask.Result,
                    ProductId = item.ProductId,
                    ProductName = productNameTask.Result,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    TotalStock = 0,
                    PendingRequests = 0,
                    NeedToSupply = item.Quantity,
                    IsSupplyStopped = item.IsSupplyStopped,
                    RequestDate = item.PurchaseRequest.RequestDate
                };

                _procurementManagementDbContext.PurchaseRequestFlatItems.Add(flatEntity);
                await _procurementManagementDbContext.SaveChangesAsync(CancellationToken.None);

                TempData["SuccessMessage"] = "محصول با موفقیت به درخواست خرید اضافه شد.";

                return RedirectToAction(
                    "FlatItems",
                    "PurchaseRequestFlatItem",
                    new
                    {
                        area = "ProcurementManagement",
                        projectId,
                        categoryId,
                        groupId,
                        statusId,
                        productId,
                        requestTypeId,
                        fromDate,
                        toDate
                    });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطایی رخ داد: {ex.Message}";
                return RedirectToAction("Index");
            }
        }



    }
}
