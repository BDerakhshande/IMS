using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
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


        public async Task<List<PurchaseRequestFlatItemDto>> GetFlatItemsAsync(
     string? requestNumber = null,
     DateTime? fromDate = null,
     DateTime? toDate = null,
     int? requestTypeId = null,
     int? projectId = null,
     List<ProductFilterDto>? products = null,  // ← لیست محصولات
     CancellationToken cancellationToken = default)
        {
            // مرحله 1: اعمال فیلترهای ساده در دیتابیس
            var query = _procurementContext.PurchaseRequestFlatItems.AsQueryable();

            if (!string.IsNullOrEmpty(requestNumber))
                query = query.Where(x => x.RequestNumber.Contains(requestNumber));

            if (fromDate.HasValue)
                query = query.Where(x => x.RequestDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.RequestDate <= toDate.Value);

            if (requestTypeId.HasValue)
                query = query.Where(x => x.RequestTypeId == requestTypeId.Value);

            if (projectId.HasValue)
                query = query.Where(x => x.ProjectId == projectId.Value);

            // مرحله 2: دریافت داده‌ها از دیتابیس قبل از اعمال فیلتر سلسله‌مراتبی
            var flatItemsList = await query
                .OrderByDescending(x => x.RequestDate)
                .ToListAsync(cancellationToken);

            // مرحله 3: اعمال فیلتر سلسله‌مراتبی محصولات در حافظه
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

            // مرحله 4: تبدیل به DTO
            var result = flatItemsList.Select(x => new PurchaseRequestFlatItemDto
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
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
                TotalStock = x.TotalStock,
                PendingRequests = x.PendingRequests,
                NeedToSupply = x.NeedToSupply,
                IsSupplyStopped = x.IsSupplyStopped
            }).ToList();

            return result;
        }


    }
}


