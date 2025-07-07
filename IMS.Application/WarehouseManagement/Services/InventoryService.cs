using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class InventoryService: IInventoryService
    {
        private readonly IWarehouseDbContext _context;

        public InventoryService(IWarehouseDbContext context)
        {
            _context = context;
        }


        public async Task CreateAsync(InventoryCreateDto dto)
        {
            var inventory = new Inventory
            {
                WarehouseId = dto.WarehouseId,
                ZoneId = dto.ZoneId,
                SectionId = dto.SectionId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync(CancellationToken.None);
        }



        public async Task<InventoryCreateDto> LoadOrCreateAsync(InventoryCreateDto inputDto)
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

        public async Task<bool> UpdateQuantityAsync(int productId, int warehouseId, int? zoneId, int? sectionId, int newQuantity)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.ProductId == productId &&
                    i.WarehouseId == warehouseId &&
                    i.ZoneId == zoneId &&
                    i.SectionId == sectionId);

            if (inventory == null)
                return false;

            inventory.Quantity = newQuantity;

            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }



        public async Task<int> GetQuantityAsync(int productId, int warehouseId, int? zoneId, int? sectionId)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.ProductId == productId &&
                    i.WarehouseId == warehouseId &&
                    ((zoneId == null && i.ZoneId == null) || (zoneId != null && i.ZoneId == zoneId)) &&
                    ((sectionId == null && i.SectionId == null) || (sectionId != null && i.SectionId == sectionId))
                );

            if (inventory == null)
                return 0;

            return (int)inventory.Quantity;
        }



    }
}
