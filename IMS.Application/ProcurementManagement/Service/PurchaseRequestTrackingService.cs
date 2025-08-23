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

        public async Task<List<PurchaseRequestItemDto>> GetItemsWithStockAndNeedAsync(
       int purchaseRequestId,
       CancellationToken cancellationToken = default)
        {
    
            var purchaseRequest = await _procurementDb.PurchaseRequests
                .Include(pr => pr.Items)
                .FirstOrDefaultAsync(pr => pr.Id == purchaseRequestId, cancellationToken);

            if (purchaseRequest == null)
                return new List<PurchaseRequestItemDto>();

            var items = purchaseRequest.Items.ToList();
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();

            // محصولات و وضعیت‌ها
            var products = await _warehouseDb.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Status)
                    .ThenInclude(s => s.Group)
                        .ThenInclude(g => g.Category)
                .ToListAsync(cancellationToken);

            // موجودی کل محصولات در انبار مرکزی
            var stocks = await _warehouseDb.Inventories
                .AsNoTracking()
                .Where(inv => productIds.Contains(inv.ProductId) && inv.Warehouse.Name.Contains("مرکزی"))
                .GroupBy(inv => inv.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalQuantity, cancellationToken);

  
            var pendingRequests = await _procurementDb.PurchaseRequestItems
                .AsNoTracking()
                .Where(i => productIds.Contains(i.ProductId)
                            && i.PurchaseRequest.Status != Status.Completed
                            && !i.IsSupplyStopped
                            && i.PurchaseRequestId != purchaseRequestId)
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

                // بروزرسانی وضعیت آیتم
                if (needToSupply == 0 && !item.IsFullySupplied)
                {
                    item.IsFullySupplied = true;
                }

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
                    IsSupplyStopped = item.IsSupplyStopped,
                    IsFullySupplied = item.IsFullySupplied
                });
            }

            // بروزرسانی وضعیت هدر درخواست
            if (purchaseRequest.Items.All(i => i.IsFullySupplied))
            {
                purchaseRequest.Status = Status.Completed;
            }

            await _procurementDb.SaveChangesAsync(cancellationToken);

            return result;
        }

    }
}
