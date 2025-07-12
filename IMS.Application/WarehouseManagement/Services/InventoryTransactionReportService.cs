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

        public async Task<List<InventoryTransactionReportDto>> GetReportAsync(InventoryTransactionReportItemDto filter)
        {
            var query = _dbContext.ReceiptOrIssueItems
                .Include(i => i.ReceiptOrIssue)
                .Include(i => i.Category)
                .Include(i => i.Group)
                .Include(i => i.Status)
                .Include(i => i.Product)
                .Include(i => i.SourceWarehouse)
                .Include(i => i.SourceZone)
                .Include(i => i.SourceSection)
                .Include(i => i.DestinationWarehouse)
                .Include(i => i.DestinationZone)
                .Include(i => i.DestinationSection)
                .AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(i => i.ReceiptOrIssue.Date >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(i => i.ReceiptOrIssue.Date <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.DocumentType))
            {
                if (Enum.TryParse<ReceiptOrIssueType>(filter.DocumentType, out var docTypeEnum))
                {
                    query = query.Where(i => i.ReceiptOrIssue.Type == docTypeEnum);
                }
                else
                {
                   
                    return new List<InventoryTransactionReportDto>();
                }
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filter.CategoryId);

            if (filter.GroupId.HasValue)
                query = query.Where(i => i.GroupId == filter.GroupId);

            if (filter.StatusId.HasValue)
                query = query.Where(i => i.StatusId == filter.StatusId);

            if (filter.ProductId.HasValue)
                query = query.Where(i => i.ProductId == filter.ProductId);

            if (filter.WarehouseId.HasValue)
                query = query.Where(i =>
                    i.SourceWarehouseId == filter.WarehouseId ||
                    i.DestinationWarehouseId == filter.WarehouseId);

            if (filter.ZoneId.HasValue)
                query = query.Where(i =>
                    i.SourceZoneId == filter.ZoneId ||
                    i.DestinationZoneId == filter.ZoneId);

            if (filter.SectionId.HasValue)
                query = query.Where(i =>
                    i.SourceSectionId == filter.SectionId ||
                    i.DestinationSectionId == filter.SectionId);

            var result = await query
                .OrderBy(i => i.ReceiptOrIssue.Date)
                .Select(i => new InventoryTransactionReportDto
                {
                    Date = i.ReceiptOrIssue.Date.ToString("yyyy/MM/dd"),
                    DocumentNumber = i.ReceiptOrIssue.DocumentNumber,
                    DocumentType = i.ReceiptOrIssue.Type.ToString(),

                    CategoryName = i.Category != null ? i.Category.Name : "",
                    GroupName = i.Group != null ? i.Group.Name : "",
                    StatusName = i.Status != null ? i.Status.Name : "",
                    ProductName = i.Product != null ? i.Product.Name : "",

                    SourceWarehouseName = i.SourceWarehouse != null ? i.SourceWarehouse.Name : "",
                    SourceDepartmentName = i.SourceZone != null ? i.SourceZone.Name : "",
                    SourceSectionName = i.SourceSection != null ? i.SourceSection.Name : "",

                    DestinationWarehouseName = i.DestinationWarehouse != null ? i.DestinationWarehouse.Name : "",
                    DestinationDepartmentName = i.DestinationZone != null ? i.DestinationZone.Name : "",
                    DestinationSectionName = i.DestinationSection != null ? i.DestinationSection.Name : "",

                    Quantity = i.Quantity
                })
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

        public async Task<List<SelectListItem>> GetSectionsByZoneIdAsync(int zoneId)
        {
            return await _dbContext.StorageSections
                .Where(s => s.ZoneId == zoneId)
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
