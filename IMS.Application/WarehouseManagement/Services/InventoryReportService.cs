using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class InventoryReportService: IInventoryReportService
    {
        private IWarehouseDbContext _dbContext;
        public InventoryReportService(IWarehouseDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<List<InventoryReportResultDto>> GetInventoryReportAsync(InventoryReportFilterDto filter)
        {
            var query = _dbContext.Inventories
                .Include(i => i.Warehouse)
                .Include(i => i.Zone)
                .Include(i => i.Section)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .AsQueryable();

            if (filter.Warehouses?.Any() == true)
            {
                var warehouseIds = filter.Warehouses.Select(w => w.WarehouseId).ToList();
                query = query.Where(i => warehouseIds.Contains(i.WarehouseId));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.Product.Status.Group.CategoryId == filter.CategoryId.Value);

            if (filter.GroupId.HasValue)
                query = query.Where(i => i.Product.Status.GroupId == filter.GroupId.Value);

            if (filter.StatusId.HasValue)
                query = query.Where(i => i.Product.StatusId == filter.StatusId.Value);

            if (filter.ProductId.HasValue)
                query = query.Where(i => i.ProductId == filter.ProductId.Value);

            if (filter.MinQuantity.HasValue)
                query = query.Where(i => i.Quantity >= filter.MinQuantity.Value);

            if (filter.MaxQuantity.HasValue)
                query = query.Where(i => i.Quantity <= filter.MaxQuantity.Value);

            if (!string.IsNullOrWhiteSpace(filter.ProductSearch))
            {
                query = query.Where(i => i.Product.Name.Contains(filter.ProductSearch));
            }

            var list = await query.ToListAsync();

            // فیلتر نهایی براساس Zone و Section در حافظه
            if (filter.Warehouses?.Any() == true)
            {
                list = list.Where(i =>
                    filter.Warehouses.Any(w =>
                        w.WarehouseId == i.WarehouseId &&
                        (w.ZoneIds == null || w.ZoneIds.Count == 0 || (i.ZoneId != null && w.ZoneIds.Contains(i.ZoneId.Value))) &&
                        (w.SectionIds == null || w.SectionIds.Count == 0 || (i.SectionId != null && w.SectionIds.Contains(i.SectionId.Value)))
                    )).ToList();
            }

            // حالا گروه‌بندی و جمع مقدار Quantity
            var groupedResults = list
                .GroupBy(i => new
                {
                    i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    ZoneId = i.ZoneId,
                    ZoneName = i.Zone?.Name,
                    SectionId = i.SectionId,
                    SectionName = i.Section?.Name,
                    CategoryId = i.Product.Status.Group.CategoryId,
                    CategoryName = i.Product.Status.Group.Category.Name,
                    GroupId = i.Product.Status.GroupId,
                    GroupName = i.Product.Status.Group.Name,
                    StatusId = i.Product.StatusId,
                    StatusName = i.Product.Status.Name,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name
                })
                .Select(g => new InventoryReportResultDto
                {
                    WarehouseName = g.Key.WarehouseName,
                    ZoneName = g.Key.ZoneName,
                    SectionName = g.Key.SectionName,
                    CategoryName = g.Key.CategoryName,
                    GroupName = g.Key.GroupName,
                    StatusName = g.Key.StatusName,
                    ProductName = g.Key.ProductName,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            return groupedResults;
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
