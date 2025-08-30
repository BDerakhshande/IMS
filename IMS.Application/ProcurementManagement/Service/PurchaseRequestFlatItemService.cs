using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.ProcurementManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProcurementManagement.Service
{
    public class PurchaseRequestFlatItemService : IPurchaseRequestFlatItemService
    {
        private readonly IProcurementManagementDbContext _procurementContext;
        private readonly IApplicationDbContext _projectContext;
        private readonly IWarehouseDbContext _warehouseContext;

        public PurchaseRequestFlatItemService(
            IProcurementManagementDbContext procurementContext,
            IApplicationDbContext projectContext,
            IWarehouseDbContext warehouseContext)
        {
            _procurementContext = procurementContext;
            _projectContext = projectContext;
            _warehouseContext = warehouseContext;
        }

        public async Task<List<SelectListItem>> GetAllCategoriesAsync()
        {
            return await _warehouseContext.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId)
        {
            return await _warehouseContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId)
        {
            return await _warehouseContext.Statuses
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId)
        {
            return await _warehouseContext.Products
                .Where(p => p.StatusId == statusId)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }



        #region Get Flat Items
        public async Task<List<PurchaseRequestFlatItemDto>> GetFlatItemsAsync(
            string? requestNumber = null,
            string? requestTitle = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? requestTypeId = null,
            int? projectId = null,
            List<ProductFilterDto>? products = null,
            CancellationToken cancellationToken = default)
        {
            // مرحله 1: اعمال فیلترهای ساده روی دیتابیس
            var query = _procurementContext.PurchaseRequestFlatItems.AsQueryable();

            if (!string.IsNullOrEmpty(requestNumber))
                query = query.Where(x => x.RequestNumber.Contains(requestNumber));

            if (!string.IsNullOrEmpty(requestTitle))
                query = query.Where(x => x.RequestTitle.Contains(requestTitle));

            if (fromDate.HasValue)
                query = query.Where(x => x.RequestDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.RequestDate <= toDate.Value);

            if (requestTypeId.HasValue)
                query = query.Where(x => x.RequestTypeId == requestTypeId.Value);

            if (projectId.HasValue)
                query = query.Where(x => x.ProjectId == projectId.Value);

            // دریافت داده‌ها از دیتابیس
            var flatItemsList = await query
                .OrderByDescending(x => x.RequestDate)
                .ToListAsync(cancellationToken);

            // اعمال فیلتر سلسله‌مراتبی محصولات در حافظه
            if (products != null && products.Any())
            {
                var conditions = products
                    .Where(p => p.CategoryId.HasValue || p.GroupId.HasValue || p.StatusId.HasValue || p.ProductId.HasValue)
                    .ToList();

                if (conditions.Any())
                {
                    flatItemsList = flatItemsList
                        .Where(x => conditions.Any(p =>
                            (!p.CategoryId.HasValue || x.CategoryId == p.CategoryId) &&
                            (!p.GroupId.HasValue || x.GroupId == p.GroupId) &&
                            (!p.StatusId.HasValue || x.StatusId == p.StatusId) &&
                            (!p.ProductId.HasValue || x.ProductId == p.ProductId)
                        ))
                        .ToList();
                }
            }

            // جمع‌آوری productIds برای محاسبه موجودی و درخواست‌های معلق
            var productIds = flatItemsList.Select(x => x.ProductId).Distinct().ToList();

            // موجودی کل محصولات در انبار مرکزی
            var stocksDict = await _warehouseContext.Inventories
                .Where(inv => productIds.Contains(inv.ProductId) && inv.Warehouse.Name.Contains("مرکزی"))
                .GroupBy(inv => inv.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalQuantity, cancellationToken);

            // درخواست‌های معلق برای محصولات
            var pendingDict = await _procurementContext.PurchaseRequestItems
                .Where(i => productIds.Contains(i.ProductId) && i.PurchaseRequest.Status != Status.Completed && !i.IsSupplyStopped)
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, TotalPendingQuantity = g.Sum(x => x.RemainingQuantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalPendingQuantity, cancellationToken);

            var result = new List<PurchaseRequestFlatItemDto>();

            // حلقه امن روی کپی لیست برای حذف آیتم‌ها در صورت نیاز
            foreach (var x in flatItemsList.ToList())
            {
                stocksDict.TryGetValue(x.ProductId, out decimal totalStock);
                pendingDict.TryGetValue(x.ProductId, out decimal totalPending);

                var needToSupply = Math.Max(0, x.Quantity - totalStock);

                // حذف آیتم‌هایی که نیاز به تامین صفر دارند و تامین متوقف نشده
                if (needToSupply == 0 && !x.IsSupplyStopped)
                {
                    _procurementContext.PurchaseRequestFlatItems.Remove(x);
                    continue; // از افزودن به خروجی جلوگیری شود
                }

                result.Add(new PurchaseRequestFlatItemDto
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    RequestTitle = x.RequestTitle,
                    RequestDate = x.RequestDate,
                    RequestTypeId = x.RequestTypeId,
                    RequestTypeName = x.RequestTypeName,
                    ProjectId = x.ProjectId,
                    ProjectName = x.ProjectName,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    GroupId = x.GroupId,
                    GroupName = x.GroupName,
                    StatusId = x.StatusId,
                    StatusName = x.StatusName,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    TotalStock = totalStock,
                    PendingRequests = totalPending,
                    NeedToSupply = needToSupply,
                    IsSupplyStopped = x.IsSupplyStopped
                });
            }

            // ذخیره تغییرات حذف شده‌ها
            await _procurementContext.SaveChangesAsync(cancellationToken);

            return result;
        }
        #endregion


    }
}


