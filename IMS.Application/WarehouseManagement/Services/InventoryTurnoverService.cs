using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace IMS.Application.WarehouseManagement.Services
{
    public class InventoryTurnoverService:IInventoryTurnoverService
    {
        private readonly IWarehouseDbContext _context;

        public InventoryTurnoverService(IWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryTurnoverDto>> GetInventoryTurnoverAsync(InventoryTurnoverFilterDto filter)
        {
            if (filter == null || filter.FromDate > filter.ToDate || filter.WarehouseId <= 0)
                throw new ArgumentException("پارامترهای ورودی فیلتر نامعتبر هستند.");

            try
            {
                var query = _context.ReceiptOrIssueItems
                    .Where(i => i.ReceiptOrIssue.Date <= filter.ToDate &&
                               ((i.DestinationWarehouseId == filter.WarehouseId &&
                                 (filter.ZoneId == 0 || i.DestinationZoneId == filter.ZoneId) &&
                                 (filter.SectionId == 0 || i.DestinationSectionId == filter.SectionId)) ||
                                (i.SourceWarehouseId == filter.WarehouseId &&
                                 (filter.ZoneId == 0 || i.SourceZoneId == filter.ZoneId) &&
                                 (filter.SectionId == 0 || i.SourceSectionId == filter.SectionId))))
                    .GroupBy(i => new
                    {
                        i.ProductId,
                        CategoryId = i.CategoryId ?? 0,
                        GroupId = i.GroupId ?? 0,
                        StatusId = i.StatusId ?? 0,
                        WarehouseId = filter.WarehouseId,
                        ZoneId = i.DestinationWarehouseId == filter.WarehouseId ? i.DestinationZoneId ?? 0 : i.SourceZoneId ?? 0,
                        SectionId = i.DestinationWarehouseId == filter.WarehouseId ? i.DestinationSectionId ?? 0 : i.SourceSectionId ?? 0
                    })
                    .Select(g => new InventoryTurnoverDto
                    {
                        WarehouseId = g.Key.WarehouseId,
                        ZoneId = g.Key.ZoneId,
                        SectionId = g.Key.SectionId,
                        ProductId = g.Key.ProductId,
                        CategoryId = g.Key.CategoryId,
                        GroupId = g.Key.GroupId,
                        StatusId = g.Key.StatusId,
                        OpeningQuantity = g.Where(i => i.ReceiptOrIssue.Date < filter.FromDate &&
                                                     i.DestinationWarehouseId == filter.WarehouseId)
                                         .Sum(i => (decimal?)i.Quantity) ?? 0,
                        TotalIn = g.Where(i => i.ReceiptOrIssue.Date >= filter.FromDate &&
                                              i.ReceiptOrIssue.Date <= filter.ToDate &&
                                              i.DestinationWarehouseId == filter.WarehouseId &&
                                              (i.ReceiptOrIssue.Type == ReceiptOrIssueType.Receipt ||
                                               i.ReceiptOrIssue.Type == ReceiptOrIssueType.Transfer))
                                   .Sum(i => (decimal?)i.Quantity) ?? 0,
                        TotalOut = g.Where(i => i.ReceiptOrIssue.Date >= filter.FromDate &&
                                              i.ReceiptOrIssue.Date <= filter.ToDate &&
                                              i.SourceWarehouseId == filter.WarehouseId &&
                                              (i.ReceiptOrIssue.Type == ReceiptOrIssueType.Issue ||
                                               i.ReceiptOrIssue.Type == ReceiptOrIssueType.Transfer))
                                   .Sum(i => (decimal?)i.Quantity) ?? 0
                    })
                    .Where(dto => dto.OpeningQuantity != 0 || dto.TotalIn != 0 || dto.TotalOut != 0);

                var result = await query.ToListAsync();

                if (!result.Any())
                    return new List<InventoryTurnoverDto>();

                await LoadRelatedNames(result);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("خطا در دریافت گردش موجودی انبار.", ex);
            }
        }

        private async Task LoadRelatedNames(List<InventoryTurnoverDto> items)
        {
            var ids = new
            {
                WarehouseIds = items.Select(r => r.WarehouseId).Distinct().ToList(),
                ZoneIds = items.Select(r => r.ZoneId).Distinct().Where(id => id > 0).ToList(),
                SectionIds = items.Select(r => r.SectionId).Distinct().Where(id => id > 0).ToList(),
                ProductIds = items.Select(r => r.ProductId).Distinct().ToList(),
                CategoryIds = items.Select(r => r.CategoryId).Distinct().Where(id => id > 0).ToList(),
                GroupIds = items.Select(r => r.GroupId).Distinct().Where(id => id > 0).ToList(),
                StatusIds = items.Select(r => r.StatusId).Distinct().Where(id => id > 0).ToList()
            };

            var warehouses = await _context.Warehouses.Where(w => ids.WarehouseIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => w.Name ?? "نامشخص");
            var zones = await _context.StorageZones.Where(z => ids.ZoneIds.Contains(z.Id)).ToDictionaryAsync(z => z.Id, z => z.Name ?? "نامشخص");
            var sections = await _context.StorageSections.Where(s => ids.SectionIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name ?? "نامشخص");
            var products = await _context.Products.Where(p => ids.ProductIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name ?? "نامشخص");
            var categories = await _context.Categories.Where(c => ids.CategoryIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name ?? "نامشخص");
            var groups = await _context.Groups.Where(g => ids.GroupIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Name ?? "نامشخص");
            var statuses = await _context.Statuses.Where(s => ids.StatusIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name ?? "نامشخص");

            foreach (var item in items)
            {
                item.WarehouseName = warehouses.GetValueOrDefault(item.WarehouseId, "نامشخص");
                item.ZoneName = zones.GetValueOrDefault(item.ZoneId, "نامشخص");
                item.SectionName = sections.GetValueOrDefault(item.SectionId, "نامشخص");
                item.ProductName = products.GetValueOrDefault(item.ProductId, "نامشخص");
                item.CategoryName = categories.GetValueOrDefault(item.CategoryId, "نامشخص");
                item.GroupName = groups.GetValueOrDefault(item.GroupId, "نامشخص");
                item.StatusName = statuses.GetValueOrDefault(item.StatusId, "نامشخص");
            }
        }



        public async Task<List<SelectListItem>> GetZonesByWarehouseIdAsync(int warehouseId)
        {
            return await _context.StorageZones
                .Where(z => z.WarehouseId == warehouseId)
                .OrderBy(z => z.Name)
                .Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetSectionsByZoneIdsAsync(List<int> zoneIds)
        {
            if (zoneIds == null || zoneIds.Count == 0)
                return new List<SelectListItem>();

            return await _context.StorageSections
                .Where(s => zoneIds.Contains(s.ZoneId))
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetWarehousesAsync()
        {
            return await _context.Warehouses
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.Name
                }).ToListAsync();
        }


    }
}
