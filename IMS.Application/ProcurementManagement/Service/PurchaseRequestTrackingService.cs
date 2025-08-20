using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.ProcurementManagement.Entities;
using IMS.Domain.ProcurementManagement.Enums;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProcurementManagement.Service
{
    public class PurchaseRequestTrackingService : IPurchaseRequestTrackingService
    {
    
        private readonly IProcurementManagementDbContext _procurementDb;
        private readonly IWarehouseDbContext _warehouseDb;
        private readonly IApplicationDbContext _projectContext;
    
        public PurchaseRequestTrackingService(IProcurementManagementDbContext procurementDb, IWarehouseDbContext warehouseDb ,
            IApplicationDbContext projectContext)
        {
            _procurementDb = procurementDb;
            _warehouseDb = warehouseDb;
            _projectContext = projectContext;
        }
        public async Task<List<PurchaseRequest>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // فقط داده‌های هدر درخواست خرید را بارگذاری می‌کنیم (بدون Include آیتم‌ها)
            return await _procurementDb.PurchaseRequests
                .AsNoTracking() // چون صرفا خواندنی است
                .OrderByDescending(pr => pr.RequestDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<PurchaseRequestItemDto>> GetItemsWithStockAndNeedAsync(int purchaseRequestId, CancellationToken cancellationToken = default)
        {
            // گرفتن همه آیتم‌های درخواست خرید (شامل وضعیت توقف تامین)
            var items = await _procurementDb.PurchaseRequestItems
                .AsNoTracking()
                .Where(i => i.PurchaseRequestId == purchaseRequestId)
                .ToListAsync(cancellationToken);

            var productIds = items.Select(i => i.ProductId).Distinct().ToList();

            var products = await _warehouseDb.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Status)
                    .ThenInclude(s => s.Group)
                        .ThenInclude(g => g.Category)
                .ToListAsync(cancellationToken);

            // موجودی کل محصولات
            var stocks = await _warehouseDb.Inventories
    .AsNoTracking()
    .Where(inv => productIds.Contains(inv.ProductId)
                  && inv.Warehouse.Name.Contains("مرکزی"))
    .GroupBy(inv => inv.ProductId)
    .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
    .ToDictionaryAsync(x => x.ProductId, x => x.TotalQuantity, cancellationToken);


            // مجموع درخواست‌های باز و **غیرفعال نشده**
            var pendingRequests = await _procurementDb.PurchaseRequestItems
                .AsNoTracking()
                .Where(i => productIds.Contains(i.ProductId)
                            && i.PurchaseRequest.Status != Status.Completed
                            && !i.IsSupplyStopped          // فیلتر توقف تامین شده‌ها
                            && i.PurchaseRequestId != purchaseRequestId) // حذف درخواست جاری
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, TotalPendingQuantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalPendingQuantity, cancellationToken);

            var result = new List<PurchaseRequestItemDto>();

            foreach (var item in items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null) continue;

                stocks.TryGetValue(item.ProductId, out decimal totalStock);
                pendingRequests.TryGetValue(item.ProductId, out decimal totalPending);

                var needToSupply = Math.Max(0, (totalPending + item.Quantity) - totalStock);

                result.Add(new PurchaseRequestItemDto
                {
                    Id = item.Id,
                    PurchaseRequestId = item.PurchaseRequestId,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    TotalStock = totalStock,
                    PendingRequests = totalPending,
                    NeedToSupply = needToSupply,
                    StatusId = product.Status.Id,
                    Status = product.Status.Name,
                    GroupId = product.Status.Group.Id,
                    GroupName = product.Status.Group.Name,
                    CategoryId = product.Status.Group.Category.Id,
                    CategoryName = product.Status.Group.Category.Name,
                    IsSupplyStopped = item.IsSupplyStopped  
                });

            }

            return result;
        }



     //   public async Task<List<PurchaseRequestFlatItemDto>> GetFlatItemsAsync(
     //DateTime? fromDate = null,
     //DateTime? toDate = null,
     //int? projectId = null,
     //int? categoryId = null,
     //int? groupId = null,
     //int? productId = null,
     //int? requestTypeId = null,
     //CancellationToken cancellationToken = default)
     //   {
     //       // ساخت کوئری پایه روی PurchaseRequests
     //       var query = _procurementDb.PurchaseRequests
     //           .Include(pr => pr.RequestType)
     //           .Include(pr => pr.Items)
     //           .AsQueryable();

     //       // اعمال فیلترهای مربوط به PurchaseRequest روی دیتابیس
     //       if (fromDate.HasValue)
     //           query = query.Where(pr => pr.RequestDate >= fromDate.Value);
     //       if (toDate.HasValue)
     //           query = query.Where(pr => pr.RequestDate <= toDate.Value);
     //       if (requestTypeId.HasValue)
     //           query = query.Where(pr => pr.RequestTypeId == requestTypeId.Value);

     //       // گرفتن داده‌ها از دیتابیس
     //       var requests = await query.ToListAsync(cancellationToken);

     //       // گرفتن داده‌های جانبی از دیتابیس (Categories, Groups, Products, Projects, Status)
     //       var categories = await _warehouseDb.Categories.ToListAsync(cancellationToken);
     //       var groups = await _warehouseDb.Groups.ToListAsync(cancellationToken);
     //       var products = await _warehouseDb.Products
     //                       .Include(p => p.Status)  // وضعیت محصول
     //                       .ToListAsync(cancellationToken);
     //       var projects = await _projectContext.Projects.ToListAsync(cancellationToken);

     //       // ساخت flatItems
     //       var flatItems = requests
     //           .SelectMany(pr => pr.Items, (pr, item) => {
     //               var product = products.FirstOrDefault(p => p.Id == item.ProductId);

     //               return new PurchaseRequestFlatItemDto
     //               {
     //                   Id = item.Id,
     //                   RequestNumber = pr.RequestNumber,
     //                   RequestTypeId = pr.RequestTypeId,
     //                   RequestTypeName = pr.RequestType?.Name,
     //                   ProjectId = item.ProjectId ?? 0,
     //                   ProjectName = item.ProjectId.HasValue ? projects.FirstOrDefault(p => p.Id == item.ProjectId.Value)?.ProjectName : null,
     //                   CategoryId = item.CategoryId,
     //                   CategoryName = categories.FirstOrDefault(c => c.Id == item.CategoryId)?.Name,
     //                   GroupId = item.GroupId,
     //                   GroupName = groups.FirstOrDefault(g => g.Id == item.GroupId)?.Name,
     //                   ProductId = item.ProductId,
     //                   ProductName = product?.Name,
     //                   Quantity = item.Quantity,
     //                   Unit = item.Unit,
     //                   TotalStock = 0,
     //                   PendingRequests = 0,
     //                   NeedToSupply = item.Quantity,
     //                   IsSupplyStopped = item.IsSupplyStopped,
     //                   RequestDate = pr.RequestDate,
     //                   StatusId = product?.Status?.Id ?? 0,        // اضافه شده
     //                   StatusName = product?.Status?.Name          // اضافه شده
     //               };
     //           })
     //           .ToList();

     //       return flatItems;
     //   }

    }
}
