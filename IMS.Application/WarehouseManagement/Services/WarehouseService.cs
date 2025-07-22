using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseDbContext _context;

        public WarehouseService(IWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsCodeDuplicateAsync(string code, int? excludeId = null)
        {
            return await _context.Warehouses
                .AnyAsync(w => w.Code == code && (!excludeId.HasValue || w.Id != excludeId.Value));
        }


        public async Task<int?> CreateWarehouseAsync(WarehouseDto dto)
        {
            var existing = await _context.Warehouses
                .AnyAsync(w => w.Code == dto.Code);

            if (existing)
                return null;

            var warehouse = new Warehouse
            {
                Name = dto.Name,
                Code = dto.Code,
                Location = dto.Location,
                Description = dto.Description,
                Manager = dto.Manager,
                StorageConditions = dto.StorageConditions,
                IsActive = true
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync(CancellationToken.None);
            return warehouse.Id;
        }

        public async Task<int> CreateZoneAsync(StorageZoneDto dto)
        {
            var zone = new StorageZone
            {
                Name = dto.Name,
                ZoneCode = dto.ZoneCode,
                WarehouseId = dto.WarehouseId
            };

            _context.StorageZones.Add(zone);
            await _context.SaveChangesAsync(CancellationToken.None);
            return zone.Id;
        }

        public async Task<int> CreateSectionAsync(StorageSectionDto dto)
        {
           
            var section = new StorageSection
            {
                Name = dto.Name,
                SectionCode = dto.SectionCode,
                ZoneId = dto.ZoneId,
                Capacity = dto.Capacity,
                Dimensions = dto.Dimensions
            };

            _context.StorageSections.Add(section);
            await _context.SaveChangesAsync(CancellationToken.None);
            return section.Id;
        }



        public async Task<List<WarehouseDto>> GetAllWarehousesAsync()
        {
            var warehouses = await _context.Warehouses
                .AsNoTracking()
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Location = w.Location,
                    Description = w.Description,
                    Manager = w.Manager,
                    StorageConditions = w.StorageConditions,
                    IsActive = w.IsActive
                }).ToListAsync();

            return warehouses;
        }

        public async Task<List<WarehouseDto>> GetAllWarehousesWithHierarchyAsync()
        {
            var warehouses = await _context.Warehouses
                .Include(w => w.Zones)
                    .ThenInclude(z => z.Sections)
                .AsNoTracking()
                .ToListAsync();

            return warehouses.Select(w => new WarehouseDto
            {
                Id = w.Id,
                Name = w.Name,
                Code = w.Code,
                Location = w.Location,
                Description = w.Description,
                Manager = w.Manager,
                StorageConditions = w.StorageConditions,
                IsActive = w.IsActive,
                Zones = w.Zones.Select(z => new StorageZoneDto
                {
                    Id = z.Id,
                    Name = z.Name,
                    ZoneCode = z.ZoneCode,
                    WarehouseId = z.WarehouseId,
                    Sections = z.Sections.Select(s => new StorageSectionDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        SectionCode = s.SectionCode,
                        ZoneId = s.ZoneId,
                        Capacity = s.Capacity,
                        Dimensions = s.Dimensions,
                        ZoneCode = z.ZoneCode,           // اضافه شده
                        WarehouseCode = w.Code           // اضافه شده
                    }).ToList()
                }).ToList()
            }).ToList();
        }

        public async Task<List<StorageZoneDto>> GetZonesByWarehouseIdAsync(int warehouseId)
        {
            var zones = await _context.StorageZones
                .Where(z => z.WarehouseId == warehouseId)
                .Include(z => z.Sections)
                .Include(z => z.Warehouse)
                .AsNoTracking()
                .ToListAsync();

            return zones.Select(z => new StorageZoneDto
            {
                Id = z.Id,
                Name = z.Name,
                ZoneCode = z.ZoneCode,
                WarehouseId = z.WarehouseId,
                WarehouseCode = z.Warehouse?.Code,
                Sections = z.Sections.Select(s => new StorageSectionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    SectionCode = s.SectionCode,
                    ZoneId = s.ZoneId,
                    Capacity = s.Capacity,
                    Dimensions = s.Dimensions,
                    ZoneCode = z.ZoneCode,            // اضافه شده
                    WarehouseCode = z.Warehouse?.Code // اضافه شده
                }).ToList()
            }).ToList();
        }

        public async Task UpdateWarehouseAsync(WarehouseDto dto)
        {
            var warehouse = await _context.Warehouses.FindAsync(dto.Id);
            if (warehouse == null)
                throw new Exception("انبار یافت نشد.");

            warehouse.Code = dto.Code;
            warehouse.Name = dto.Name;
            warehouse.Location = dto.Location;
            warehouse.Manager = dto.Manager;
            warehouse.IsActive = dto.IsActive;

            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task UpdateZoneAsync(StorageZoneDto dto)
        {
            var zone = await _context.StorageZones.FindAsync(dto.Id);
            if (zone == null)
                throw new Exception("ناحیه ذخیره‌سازی یافت نشد.");

            zone.Name = dto.Name;
            zone.ZoneCode = dto.ZoneCode;

            _context.StorageZones.Update(zone);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteWarehouseAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
                throw new Exception("انبار یافت نشد.");

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        public async Task DeleteZoneAsync(int id)
        {
            var zone = await _context.StorageZones
                .Include(z => z.Sections)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zone == null)
                throw new Exception("ناحیه یافت نشد.");

            if (zone.Sections != null && zone.Sections.Any())
                throw new Exception("این ناحیه دارای بخش‌هایی است و قابل حذف نیست.");

            _context.StorageZones.Remove(zone);
            await _context.SaveChangesAsync(CancellationToken.None);
        }


        public async Task<StorageZoneDto> GetZoneByIdAsync(int id)
        {
            var zone = await _context.StorageZones.FindAsync(id);
            if (zone == null) return null;

            return new StorageZoneDto
            {
                Id = zone.Id,
                Name = zone.Name,
                ZoneCode = zone.ZoneCode,
                WarehouseId = zone.WarehouseId
            };
        }

        public async Task<WarehouseDto?> GetWarehouseHierarchyAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.Zones)
                    .ThenInclude(z => z.Sections)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == warehouseId);

            if (warehouse == null) return null;

            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Code = warehouse.Code,
                Location = warehouse.Location,
                Description = warehouse.Description,
                Manager = warehouse.Manager,
                StorageConditions = warehouse.StorageConditions,
                IsActive = warehouse.IsActive,
                Zones = warehouse.Zones.Select(z => new StorageZoneDto
                {
                    Id = z.Id,
                    Name = z.Name,
                    ZoneCode = z.ZoneCode,
                    WarehouseId = z.WarehouseId,
                    Sections = z.Sections.Select(s => new StorageSectionDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        SectionCode = s.SectionCode,
                        ZoneId = s.ZoneId,
                        Capacity = s.Capacity,
                        Dimensions = s.Dimensions,
                        ZoneCode = z.ZoneCode,            // اضافه شده
                        WarehouseCode = warehouse.Code    // اضافه شده
                    }).ToList()
                }).ToList()
            };
        }


        public async Task<List<StorageSectionDto>> GetSectionsByZoneAsync(int zoneId)
        {
            var sections = await _context.StorageSections
                .Where(s => s.ZoneId == zoneId)
                .Include(s => s.Zone)
                    .ThenInclude(z => z.Warehouse)
                .AsNoTracking()
                .ToListAsync();

            return sections.Select(s => new StorageSectionDto
            {
                Id = s.Id,
                Name = s.Name,
                SectionCode = s.SectionCode,
                ZoneId = s.ZoneId,
                Capacity = s.Capacity,
                Dimensions = s.Dimensions,
                ZoneCode = s.Zone?.ZoneCode,
                WarehouseCode = s.Zone?.Warehouse?.Code
            }).ToList();
        }


        public async Task UpdateSectionAsync(StorageSectionDto dto)
        {
            var section = await _context.StorageSections.FindAsync(dto.Id);
            if (section == null)
                throw new Exception("بخش مورد نظر یافت نشد.");

            section.Name = dto.Name;
            section.SectionCode = dto.SectionCode;
            section.ZoneId = dto.ZoneId;
            section.Capacity = dto.Capacity;
            section.Dimensions = dto.Dimensions;

            _context.StorageSections.Update(section);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // سرویس حذف
        public async Task DeleteSectionAsync(int id)
        {
            var section = await _context.StorageSections.FindAsync(id);
            if (section == null)
                throw new Exception("بخش مورد نظر برای حذف یافت نشد.");

            _context.StorageSections.Remove(section);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task<StorageSectionDto> GetSectionByIdAsync(int id)
        {
            var section = await _context.StorageSections
                .Include(s => s.Zone)
                .ThenInclude(z => z.Warehouse)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (section == null) return null;

            return new StorageSectionDto
            {
                Id = section.Id,
                Name = section.Name,
                SectionCode = section.SectionCode,
                ZoneId = section.ZoneId,
                Capacity = section.Capacity,
                Dimensions = section.Dimensions,
                ZoneCode = section.Zone?.ZoneCode,
                WarehouseCode = section.Zone?.Warehouse?.Code
            };
        }



        public async Task<List<StorageSectionDto>> GetAllSectionsAsync()
        {
            var sections = await _context.StorageSections
                .Include(s => s.Zone)
                    .ThenInclude(z => z.Warehouse)
                .AsNoTracking()
                .ToListAsync();

            return sections.Select(s => new StorageSectionDto
            {
                Id = s.Id,
                Name = s.Name,
                SectionCode = s.SectionCode,
                ZoneId = s.ZoneId,
                Capacity = s.Capacity,
                Dimensions = s.Dimensions,
                ZoneCode = s.Zone?.ZoneCode,
                WarehouseCode = s.Zone?.Warehouse?.Code
            }).ToList();
        }



        public async Task<List<StorageSectionDto>> GetSectionsByWarehouseIdAsync(int warehouseId)
        {
            var sections = await _context.StorageSections
                .Include(s => s.Zone)
                    .ThenInclude(z => z.Warehouse)
                .Where(s => s.Zone.WarehouseId == warehouseId)
                .AsNoTracking()
                .ToListAsync();

            return sections.Select(s => new StorageSectionDto
            {
                Id = s.Id,
                Name = s.Name,
                SectionCode = s.SectionCode,
                ZoneId = s.ZoneId,
                Capacity = s.Capacity,
                Dimensions = s.Dimensions,
                ZoneCode = s.Zone?.ZoneCode,
                WarehouseCode = s.Zone?.Warehouse?.Code
            }).ToList();
        }



        public async Task<List<StorageZoneDto>> GetAllZonesAsync()
        {
            var zones = await _context.StorageZones
                .AsNoTracking()
                .ToListAsync();

            return zones.Select(z => new StorageZoneDto
            {
                Id = z.Id,
                Name = z.Name,
                ZoneCode = z.ZoneCode,
                WarehouseId = z.WarehouseId
            }).ToList();
        }


        public async Task<IEnumerable<SelectListItem>> GetSelectListAsync()
        {
            return await _context.Warehouses
                .Where(w => w.IsActive)
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.Name} ({w.Code})"
                })
                .ToListAsync();
        }





    }
}
