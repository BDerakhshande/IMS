using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class InventoryTransactionReportService: IInventoryTransactionReportService
    {
        private readonly IWarehouseDbContext _dbContext;

        public InventoryTransactionReportService(IWarehouseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<InventoryTransactionReportDto>> GetReportAsync(
            string? warehouseName = null,
            string? departmentName = null,
            string? sectionName = null,
            string? categoryName = null,
            string? groupName = null,
            string? statusName = null,
            string? productName = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? documentType = null)
        {
            var query = _dbContext.ReceiptOrIssues.
                Include(r => r.Items)
                    .ThenInclude(i => i.Category)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Group)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Status)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceWarehouse)
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceZone)
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceSection)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationWarehouse)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationZone)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationSection)
                .AsQueryable();

            // فیلتر تاریخ
            if (fromDate.HasValue)
                query = query.Where(r => r.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.Date <= toDate.Value);

            // فیلتر نوع سند
            if (!string.IsNullOrEmpty(documentType))
            {
                // تبدیل رشته به Enum (فرض می‌کنیم نام‌ها مطابق Enum است)
                if (Enum.TryParse<ReceiptOrIssueType>(documentType, out var docTypeEnum))
                    query = query.Where(r => r.Type == docTypeEnum);
            }

            // فیلترهای آیتم‌ها
            if (!string.IsNullOrEmpty(categoryName))
                query = query.Where(r => r.Items.Any(i => i.Category != null && i.Category.Name == categoryName));

            if (!string.IsNullOrEmpty(groupName))
                query = query.Where(r => r.Items.Any(i => i.Group != null && i.Group.Name == groupName));

            if (!string.IsNullOrEmpty(statusName))
                query = query.Where(r => r.Items.Any(i => i.Status != null && i.Status.Name == statusName));

            if (!string.IsNullOrEmpty(productName))
                query = query.Where(r => r.Items.Any(i => i.Product != null && i.Product.Name == productName));

            if (!string.IsNullOrEmpty(warehouseName))
                query = query.Where(r => r.Items.Any(i =>
                    (i.SourceWarehouse != null && i.SourceWarehouse.Name == warehouseName) ||
                    (i.DestinationWarehouse != null && i.DestinationWarehouse.Name == warehouseName)));

            if (!string.IsNullOrEmpty(departmentName))
                query = query.Where(r => r.Items.Any(i =>
                    (i.SourceZone != null && i.SourceZone.Name == departmentName) ||
                    (i.DestinationZone != null && i.DestinationZone.Name == departmentName)));

            if (!string.IsNullOrEmpty(sectionName))
                query = query.Where(r => r.Items.Any(i =>
                    (i.SourceSection != null && i.SourceSection.Name == sectionName) ||
                    (i.DestinationSection != null && i.DestinationSection.Name == sectionName)));

            var result = await query
                .SelectMany(r => r.Items.Select(i => new InventoryTransactionReportDto
                {
                    Date = r.Date.ToString("yyyy/MM/dd"),
                    DocumentNumber = r.DocumentNumber,
                    DocumentType = r.Type.ToString(),

                    CategoryName = i.Category != null ? i.Category.Name : "",
                    GroupName = i.Group != null ? i.Group.Name : "",
                    StatusName = i.Status != null ? i.Status.Name : "",
                    ProductName = i.Product.Name,

                    SourceWarehouseName = i.SourceWarehouse != null ? i.SourceWarehouse.Name : "",
                    SourceDepartmentName = i.SourceZone != null ? i.SourceZone.Name : "",
                    SourceSectionName = i.SourceSection != null ? i.SourceSection.Name : "",

                    DestinationWarehouseName = i.DestinationWarehouse != null ? i.DestinationWarehouse.Name : "",
                    DestinationDepartmentName = i.DestinationZone != null ? i.DestinationZone.Name : "",
                    DestinationSectionName = i.DestinationSection != null ? i.DestinationSection.Name : "",

                    Quantity = i.Quantity
                }))
                .OrderBy(r => r.Date)
                .ToListAsync();

            return result;
        }





        public async Task<List<SelectListItem>> GetZonesByWarehouseIdAsync(int warehouseId)
        {
            return await _dbContext.StorageZones
                .Where(z => z.WarehouseId == warehouseId)
                .Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetAllZonesAsync()
        {
            return await _dbContext.StorageZones
                .OrderBy(z => z.Name)
                .Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetAllSectionsAsync()
        {
            return await _dbContext.StorageSections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetAllGroupsAsync()
        {
            return await _dbContext.Groups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllStatusesAsync()
        {
            return await _dbContext.Statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllProductsAsync()
        {
            return await _dbContext.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetSectionsByZoneIdsAsync(List<int> zoneIds)
        {
            if (zoneIds == null || zoneIds.Count == 0)
                return new List<SelectListItem>();

            return await _dbContext.StorageSections
                .Where(s => zoneIds.Contains(s.ZoneId))
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId)
        {
            return await _dbContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId)
        {
            return await _dbContext.Statuses
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId)
        {
            return await _dbContext.Products
                .Where(p => p.StatusId == statusId)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToListAsync();
        }
    }
}
