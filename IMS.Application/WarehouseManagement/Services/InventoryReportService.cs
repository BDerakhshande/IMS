using ClosedXML.Excel;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.Services
{
    public class InventoryReportService : IInventoryReportService
    {
        private readonly IWarehouseDbContext _dbContext;

        public InventoryReportService(IWarehouseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<InventoryReportResultDto>> GetInventoryReportAsync(InventoryReportFilterDto filter)
        {
            var baseQuery = _dbContext.Inventories
                .Include(i => i.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .Include(i => i.Warehouse)
                .Include(i => i.Zone)
                .Include(i => i.Section)
                .AsQueryable();

            // فیلترهای ساده
            if (filter.CategoryId.HasValue)
                baseQuery = baseQuery.Where(i => i.Product.Status.Group.CategoryId == filter.CategoryId.Value);

            if (filter.GroupId.HasValue)
                baseQuery = baseQuery.Where(i => i.Product.Status.GroupId == filter.GroupId.Value);

            if (filter.StatusId.HasValue)
                baseQuery = baseQuery.Where(i => i.Product.StatusId == filter.StatusId.Value);

            if (filter.ProductId.HasValue)
                baseQuery = baseQuery.Where(i => i.ProductId == filter.ProductId.Value);

            if (!string.IsNullOrWhiteSpace(filter.ProductSearch))
            {
                var s = filter.ProductSearch.Trim();
                baseQuery = baseQuery.Where(i => i.Product.Name.Contains(s));
            }

            // فیلتر UniqueCodes
            List<int> inventoryIdsFromUniqueCodes = null;
            if (filter.UniqueCodes != null && filter.UniqueCodes.Any())
            {
                var codes = filter.UniqueCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
                if (codes.Count > 0)
                {
                    inventoryIdsFromUniqueCodes = await _dbContext.InventoryItems
                        .Where(ii => codes.Contains(ii.UniqueCode))
                        .Select(ii => ii.InventoryId)
                        .Distinct()
                        .ToListAsync();

                    if (!inventoryIdsFromUniqueCodes.Any())
                        return new List<InventoryReportResultDto>();
                }
            }
            if (inventoryIdsFromUniqueCodes != null)
                baseQuery = baseQuery.Where(i => inventoryIdsFromUniqueCodes.Contains(i.Id));

            // فیلتر Warehouses / Zone / Section
            if (filter.Warehouses != null && filter.Warehouses.Any(w => w.WarehouseId > 0))
            {
                IQueryable<Inventory> combined = baseQuery.Where(i => false);
                foreach (var w in filter.Warehouses.Where(x => x.WarehouseId > 0))
                {
                    var warehouseId = w.WarehouseId;
                    var zoneIds = (w.ZoneIds ?? new List<int>()).Where(z => z > 0).ToList();
                    var sectionIds = (w.SectionIds ?? new List<int>()).Where(s => s > 0).ToList();

                    var temp = baseQuery.Where(i => i.WarehouseId == warehouseId);
                    if (zoneIds.Any())
                        temp = temp.Where(i => i.ZoneId.HasValue && zoneIds.Contains(i.ZoneId.Value));
                    if (sectionIds.Any())
                        temp = temp.Where(i => i.SectionId.HasValue && sectionIds.Contains(i.SectionId.Value));

                    combined = combined.Concat(temp);
                }
                baseQuery = combined.Distinct();
            }

            // دریافت Inventory
            var inventories = await baseQuery
                .Include(i => i.InventoryItems)
                .ToListAsync();

            if (!inventories.Any())
                return new List<InventoryReportResultDto>();

            // UniqueCodes lookup
            var inventoryIds = inventories.Select(i => i.Id).ToList();
            var inventoryItems = await _dbContext.InventoryItems
                .Where(ii => inventoryIds.Contains(ii.InventoryId) && !string.IsNullOrWhiteSpace(ii.UniqueCode))
                .Select(ii => new { ii.InventoryId, ii.UniqueCode })
                .ToListAsync();

            var itemsLookup = inventoryItems
                .GroupBy(ii => ii.InventoryId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.UniqueCode).Distinct().ToList());

            // گروه‌بندی نهایی
            var groupedResults = inventories
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
                    WarehouseId = g.Key.WarehouseId,
                    WarehouseName = g.Key.WarehouseName,
                    ZoneId = g.Key.ZoneId,
                    ZoneName = g.Key.ZoneName,
                    SectionId = g.Key.SectionId,
                    SectionName = g.Key.SectionName,
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    GroupId = g.Key.GroupId,
                    GroupName = g.Key.GroupName,
                    StatusId = g.Key.StatusId,
                    StatusName = g.Key.StatusName,
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    Quantity = g.Sum(x => x.Quantity),
                    UniqueCodes = g.Select(x => x.Id)
                                   .Where(id => itemsLookup.ContainsKey(id))
                                   .SelectMany(id => itemsLookup[id])
                                   .Distinct()
                                   .ToList()
                })
                .Where(r => r.Quantity > 0)
                .ToList();

            return groupedResults;
        }

        #region SelectListItem Methods

        public async Task<List<SelectListItem>> GetZonesByWarehouseIdAsync(int warehouseId)
        {
            return await _dbContext.StorageZones
                .Where(z => z.WarehouseId == warehouseId)
                .OrderBy(z => z.Name)
                .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllZonesAsync()
        {
            return await _dbContext.StorageZones
                .OrderBy(z => z.Name)
                .Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllSectionsAsync()
        {
            return await _dbContext.StorageSections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllGroupsAsync()
        {
            return await _dbContext.Groups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllStatusesAsync()
        {
            return await _dbContext.Statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllProductsAsync()
        {
            return await _dbContext.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetSectionsByZoneIdsAsync(List<int> zoneIds)
        {
            if (zoneIds == null || zoneIds.Count == 0) return new List<SelectListItem>();

            return await _dbContext.StorageSections
                .Where(s => zoneIds.Contains(s.ZoneId))
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId)
        {
            return await _dbContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId)
        {
            return await _dbContext.Statuses
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId)
        {
            return await _dbContext.Products
                .Where(p => p.StatusId == statusId)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();
        }

        #endregion

        public async Task<byte[]> ExportReportToExcelAsync(InventoryReportFilterDto filter)
        {
            var data = await GetInventoryReportAsync(filter);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventory Report");

            // ستون‌ها
            worksheet.Cell(1, 1).Value = "نام انبار";
            worksheet.Cell(1, 2).Value = "قسمت";
            worksheet.Cell(1, 3).Value = "بخش";
            worksheet.Cell(1, 4).Value = "دسته‌بندی";
            worksheet.Cell(1, 5).Value = "گروه";
            worksheet.Cell(1, 6).Value = "طبقه";
            worksheet.Cell(1, 7).Value = "کالا";
            worksheet.Cell(1, 8).Value = "موجودی";
            worksheet.Cell(1, 9).Value = "کدهای یکتا"; // ستون جدید

            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.WarehouseName;
                worksheet.Cell(row, 2).Value = item.ZoneName;
                worksheet.Cell(row, 3).Value = item.SectionName;
                worksheet.Cell(row, 4).Value = item.CategoryName;
                worksheet.Cell(row, 5).Value = item.GroupName;
                worksheet.Cell(row, 6).Value = item.StatusName;
                worksheet.Cell(row, 7).Value = item.ProductName;
                worksheet.Cell(row, 8).Value = item.Quantity;
                worksheet.Cell(row, 9).Value = string.Join(", ", item.UniqueCodes);
                row++;
            }

            var total = data.Sum(x => x.Quantity);
            worksheet.Cell(row, 7).Value = "جمع کل:";
            worksheet.Cell(row, 8).Value = total;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }



        public async Task<List<SelectListItem>> GetUniqueCodesByProductIdAsync(int productId)
        {
            return await _dbContext.InventoryItems
                .Where(ii => ii.Inventory.ProductId == productId && !string.IsNullOrWhiteSpace(ii.UniqueCode))
                .OrderBy(ii => ii.UniqueCode)
                .Select(ii => new SelectListItem
                {
                    Value = ii.UniqueCode,
                    Text = ii.UniqueCode
                })
                .Distinct()
                .ToListAsync();
        }


    }
}
