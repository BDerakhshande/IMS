using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
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
            var query = from i in _dbContext.Inventories
                        join p in _dbContext.Products on i.ProductId equals p.Id
                        join s in _dbContext.Statuses on p.StatusId equals s.Id
                        join g in _dbContext.Groups on s.GroupId equals g.Id
                        join c in _dbContext.Categories on g.CategoryId equals c.Id
                        join w in _dbContext.Warehouses on i.WarehouseId equals w.Id
                        join z in _dbContext.StorageZones on i.ZoneId equals z.Id into zoneJoin
                        from z in zoneJoin.DefaultIfEmpty()
                        join sec in _dbContext.StorageSections on i.SectionId equals sec.Id into sectionJoin
                        from sec in sectionJoin.DefaultIfEmpty()
                        select new
                        {
                            Inventory = i,
                            Product = p,
                            Status = s,
                            Group = g,
                            Category = c,
                            Warehouse = w,
                            Zone = z,
                            Section = sec
                        };

            if (filter.Warehouses?.Any(w => w.WarehouseId > 0) == true)
            {
                var warehouseIds = filter.Warehouses
                    .Where(w => w.WarehouseId > 0)
                    .Select(w => w.WarehouseId)
                    .ToList();

                query = query.Where(x => warehouseIds.Contains(x.Inventory.WarehouseId));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(x => x.Category.Id == filter.CategoryId.Value);

            if (filter.GroupId.HasValue)
                query = query.Where(x => x.Group.Id == filter.GroupId.Value);

            if (filter.StatusId.HasValue)
                query = query.Where(x => x.Status.Id == filter.StatusId.Value);

            if (filter.ProductId.HasValue)
                query = query.Where(x => x.Product.Id == filter.ProductId.Value);

            if (!string.IsNullOrWhiteSpace(filter.ProductSearch))
            {
                var search = filter.ProductSearch.Trim();
                query = query.Where(x => x.Product.Name.Contains(search));
            }

            var list = await query.ToListAsync();

            if (filter.Warehouses?.Any(w => w.WarehouseId > 0) == true)
            {
                list = list.Where(i =>
                    filter.Warehouses.Any(w =>
                        w.WarehouseId == i.Inventory.WarehouseId &&
                        (w.ZoneIds == null || w.ZoneIds.Count == 0 || (i.Inventory.ZoneId != null && w.ZoneIds.Contains(i.Inventory.ZoneId.Value))) &&
                        (w.SectionIds == null || w.SectionIds.Count == 0 || (i.Inventory.SectionId != null && w.SectionIds.Contains(i.Inventory.SectionId.Value)))
                    )
                ).ToList();
            }

            var groupedResults = list
      .GroupBy(i => new
      {
          i.Inventory.WarehouseId,
          WarehouseName = i.Warehouse.Name,
          ZoneId = i.Inventory.ZoneId,
          ZoneName = i.Zone?.Name,
          SectionId = i.Inventory.SectionId,
          SectionName = i.Section?.Name,
          CategoryId = i.Category.Id,
          CategoryName = i.Category.Name,
          GroupId = i.Group.Id,
          GroupName = i.Group.Name,
          StatusId = i.Status.Id,
          StatusName = i.Status.Name,
          ProductId = i.Product.Id,
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
          Quantity = g.Sum(x => x.Inventory.Quantity)
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

        public async Task<byte[]> ExportReportToExcelAsync(InventoryReportFilterDto filter)
        {
            var data = await GetInventoryReportAsync(filter);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventory Report");

            worksheet.Cell(1, 1).Value = "نام انبار";
            worksheet.Cell(1, 2).Value = "قسمت";
            worksheet.Cell(1, 3).Value = "بخش";
            worksheet.Cell(1, 4).Value = "دسته‌بندی";
            worksheet.Cell(1, 5).Value = "گروه";
            worksheet.Cell(1, 6).Value = "طبقه";
            worksheet.Cell(1, 7).Value = "کالا";
            worksheet.Cell(1, 8).Value = "موجودی";

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
                row++;
            }

            var total = data.Sum(x => x.Quantity);

            worksheet.Cell(row, 7).Value = "جمع کل:";
            worksheet.Cell(row, 8).Value = total;  // به جای استفاده از فرمول


            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }



    }
}
