using DocumentFormat.OpenXml.Bibliography;
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
         
            if (stopSupply)
                item.IsAddedToFlatItems = false;
            await _procurementManagementDbContext.SaveChangesAsync(CancellationToken.None);



            return Json(new
            {
                success = true,
                isSupplyStopped = item.IsSupplyStopped
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
                    RemainingQuantity = dto.RemainingQuantity,
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
                    Quantity = dto.RemainingQuantity,
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
        public async Task<IActionResult> AddToFlatItemsAndShow(int id)
        {
            try
            {
                var item = await _procurementManagementDbContext.PurchaseRequestItems
                    .Include(i => i.PurchaseRequest)
                        .ThenInclude(r => r.RequestType)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                    return Json(new { success = false, message = "آیتم پیدا نشد." });

                var exists = await _procurementManagementDbContext.PurchaseRequestFlatItems
                    .AnyAsync(f =>
                        f.ProductId == item.ProductId &&
                        f.RequestNumber == item.PurchaseRequest.RequestNumber &&
                        f.ProjectId == item.ProjectId &&
                        f.CategoryId == item.CategoryId &&
                        f.GroupId == item.GroupId &&
                        f.RequestTitle == item.PurchaseRequest.Title
                    );

                if (exists)
                    return Json(new { success = false, message = "محصول از قبل در درخواست خرید اضافه شده است." });
            
                         string? projectName = item.ProjectId.HasValue
                             ? await _applicationDbContext.Projects
                                 .Where(p => p.Id == item.ProjectId.Value)
                                 .Select(p => p.ProjectName)
                                 .FirstOrDefaultAsync()
                             : null;

                string? categoryName = await _warehouseContext.Categories
                    .Where(c => c.Id == item.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();

                string? groupName = await _warehouseContext.Groups
                    .Where(g => g.Id == item.GroupId)
                    .Select(g => g.Name)
                    .FirstOrDefaultAsync();

                string? statusName = await _warehouseContext.Statuses
                    .Where(s => s.Id == item.StatusId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();

                string? productName = await _warehouseContext.Products
                    .Where(p => p.Id == item.ProductId)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync();
                // محاسبه مقادیر به‌روز: موجودی انبار مرکزی
                var totalStock = await _warehouseContext.Inventories
                    .Where(inv => inv.ProductId == item.ProductId && inv.Warehouse.Name.Contains("مرکزی"))
                    .SumAsync(inv => (decimal?)inv.Quantity) ?? 0;

                // محاسبه درخواست‌های معلق برای محصول
                var totalPending = await _procurementManagementDbContext.PurchaseRequestItems
                    .Where(i => i.ProductId == item.ProductId
                                && i.PurchaseRequest.Status != Domain.ProcurementManagement.Enums.Status.Completed
                                && !i.IsSupplyStopped
                                && i.PurchaseRequestId != item.PurchaseRequestId)
                    .SumAsync(i => (decimal?)i.RemainingQuantity) ?? 0;

                // محاسبه نیاز به تامین واقعی
                var needToSupply = Math.Max(0, item.RemainingQuantity - totalStock);

                var flatEntity = new PurchaseRequestFlatItem
                        {
                            RequestNumber = item.PurchaseRequest.RequestNumber,
                            RequestTitle = item.PurchaseRequest.Title,
                            RequestTypeId = item.PurchaseRequest.RequestTypeId,
                            RequestTypeName = item.PurchaseRequest.RequestType?.Name,
                            ProjectId = item.ProjectId ?? 0,
                            ProjectName = projectName,
                            CategoryId = item.CategoryId,
                            CategoryName = categoryName,
                            GroupId = item.GroupId,
                            GroupName = groupName,
                            StatusId = item.StatusId,
                            StatusName = statusName,
                            ProductId = item.ProductId,
                            ProductName = productName,
                            Quantity = item.RemainingQuantity,
                            Unit = item.Unit,
                            TotalStock = totalStock,
                            PendingRequests = totalPending,
                            NeedToSupply = needToSupply,
                            IsSupplyStopped = item.IsSupplyStopped,
                            RequestDate = item.PurchaseRequest.RequestDate
                        };

                _procurementManagementDbContext.PurchaseRequestFlatItems.Add(flatEntity);
                item.IsAddedToFlatItems = true;
                await _procurementManagementDbContext.SaveChangesAsync(CancellationToken.None);

                return Json(new { success = true, message = "درخواست خرید با موفقیت ارسال شد." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ خطا: {ex.Message}" });
            }
        }








    }
}
