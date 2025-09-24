using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.Services
{
    public class InventoryOperationService : IInventoryOperationService
    {
        private readonly IWarehouseDbContext _context;

        public InventoryOperationService(IWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(InventoryCreateDto dto)
        {
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.ProductId == dto.ProductId &&
                    i.WarehouseId == dto.WarehouseId &&
                    i.ZoneId == dto.ZoneId &&
                    i.SectionId == dto.SectionId);

            if (existingInventory == null)
            {
                // رکورد وجود نداره → چیزی اضافه نشه
                return false;
            }

            // فقط موجودی رو افزایش بده
            existingInventory.Quantity += dto.Quantity;

            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }


        public async Task<decimal> GetQuantityAsync(int productId, int warehouseId, int? zoneId, int? sectionId)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.ProductId == productId &&
                    i.WarehouseId == warehouseId &&
                    ((zoneId == null && i.ZoneId == null) || (zoneId != null && i.ZoneId == zoneId)) &&
                    ((sectionId == null && i.SectionId == null) || (sectionId != null && i.SectionId == sectionId))
                );

            return inventory?.Quantity ?? 0;
        }

        public async Task<InventoryCreateDto> LoadAsync(InventoryCreateDto inputDto)
        {
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.ProductId == inputDto.ProductId &&
                    i.WarehouseId == inputDto.WarehouseId &&
                    i.ZoneId == inputDto.ZoneId &&
                    i.SectionId == inputDto.SectionId);

            return new InventoryCreateDto
            {
                WarehouseId = inputDto.WarehouseId,
                ZoneId = inputDto.ZoneId,
                SectionId = inputDto.SectionId,
                CategoryId = inputDto.CategoryId,
                GroupId = inputDto.GroupId,
                StatusId = inputDto.StatusId,
                ProductId = inputDto.ProductId,
                Quantity = existingInventory?.Quantity ?? 0
            };
        }
    }
}
