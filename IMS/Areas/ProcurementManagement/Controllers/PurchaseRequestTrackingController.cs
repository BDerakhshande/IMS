using IMS.Application.ProcurementManagement.Service;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.ProcurementManagement.Controllers
{
    [Area("ProcurementManagement")]
    public class PurchaseRequestTrackingController : Controller
    {
        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly IPurchaseRequestTrackingService _purchaseRequestTrackingService;
        private readonly IProcurementManagementDbContext _procurementManagementDbContext;

        public PurchaseRequestTrackingController(IPurchaseRequestService purchaseRequestService, IPurchaseRequestTrackingService purchaseRequestTrackingService, IProcurementManagementDbContext procurementManagementDbContext)
        {
            _purchaseRequestService = purchaseRequestService;
            _purchaseRequestTrackingService = purchaseRequestTrackingService;
            _procurementManagementDbContext = procurementManagementDbContext;
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


    }
}
