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

        public async Task<List<InventoryReportItemDto>> GetInventoryReportAsync(
    InventoryReportFilterDto? filter = null,
    CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Inventories
                .Where(i => i.Quantity > 0)
                .AsQueryable();

            if (filter != null)
            {
                if (filter.CategoryId.HasValue)
                    query = query.Where(i =>
                        i.Product != null &&
                        i.Product.Status != null &&
                        i.Product.Status.Group != null &&
                        i.Product.Status.Group.CategoryId == filter.CategoryId.Value);

                if (filter.GroupId.HasValue)
                    query = query.Where(i =>
                        i.Product != null &&
                        i.Product.Status != null &&
                        i.Product.Status.GroupId == filter.GroupId.Value);

                if (filter.StatusId.HasValue)
                    query = query.Where(i =>
                        i.Product != null &&
                        i.Product.StatusId == filter.StatusId.Value);

                if (filter.ProductId.HasValue)
                    query = query.Where(i =>
                        i.ProductId == filter.ProductId.Value);

                if (filter.WarehouseId.HasValue)
                    query = query.Where(i =>
                        i.WarehouseId == filter.WarehouseId.Value);

                if (filter.ZoneId.HasValue)
                    query = query.Where(i =>
                        i.ZoneId == filter.ZoneId.Value);

                if (filter.SectionId.HasValue)
                    query = query.Where(i =>
                        i.SectionId == filter.SectionId.Value);

                if (filter.MinQuantity.HasValue)
                    query = query.Where(i => i.Quantity >= filter.MinQuantity.Value);

                if (filter.MaxQuantity.HasValue)
                    query = query.Where(i => i.Quantity <= filter.MaxQuantity.Value);
            }

            var reportItems = await query
                .Select(i => new InventoryReportItemDto
                {
                    CategoryName = i.Product.Status.Group.Category.Name,
                    GroupName = i.Product.Status.Group.Name,
                    StatusName = i.Product.Status.Name,
                    ProductName = i.Product.Name,

                    WarehouseName = i.Warehouse.Name,
                    ZoneName = i.Zone != null ? i.Zone.Name : null,
                    SectionName = i.Section != null ? i.Section.Name : null,

                    Quantity = i.Quantity
                })
                .OrderBy(x => x.CategoryName)
                .ThenBy(x => x.GroupName)
                .ThenBy(x => x.StatusName)
                .ThenBy(x => x.ProductName)
                .ToListAsync(cancellationToken);

            return reportItems;
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




    }
}
