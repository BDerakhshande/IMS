using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
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

        public async Task<(bool success, string uniqueCode)> AddAsync(InventoryCreateDto dto)
        {
            string generatedUniqueCode = null;

            // بررسی اجباری بودن Zone و Section
          
            // بررسی موجودی در انبار موردنظر
            var existingInventory = await _context.Inventories
                .Include(i => i.InventoryItems)
                .FirstOrDefaultAsync(i =>
                    i.ProductId == dto.ProductId &&
                    i.WarehouseId == dto.WarehouseId &&
                    i.ZoneId == dto.ZoneId &&
                    i.SectionId == dto.SectionId);

            if (existingInventory == null)
            {
                existingInventory = new Inventory
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.WarehouseId,
                    ZoneId = dto.ZoneId,
                    SectionId = dto.SectionId,
                    Quantity = 0,
                    InventoryItems = new List<InventoryItem>()
                };
                _context.Inventories.Add(existingInventory);
            }
            else if (existingInventory.InventoryItems == null)
            {
                existingInventory.InventoryItems = new List<InventoryItem>();
            }

            if (dto.IsUnique)
            {
                // پیدا کردن آخرین شماره کد یکتا
                int? lastCode = await _context.ProductItems
                    .Where(pi => pi.ProductId == dto.ProductId)
                    .MaxAsync(pi => (int?)Convert.ToInt32(pi.UniqueCode));

                int newCode = (lastCode ?? 0) + 1;
                generatedUniqueCode = newCode.ToString();

                // تعیین Sequence
                int? lastSequence = await _context.ProductItems
                    .Where(pi => pi.ProductId == dto.ProductId)
                    .MaxAsync(pi => (int?)pi.Sequence);

                int newSequence = (lastSequence ?? 0) + 1;

                // افزودن آیتم به InventoryItem
                var item = new InventoryItem
                {
                    UniqueCode = generatedUniqueCode,
                    Inventory = existingInventory
                };
                existingInventory.InventoryItems.Add(item);

                // افزودن به ProductItem
                var productItem = new ProductItem
                {
                    ProductId = dto.ProductId,
                    UniqueCode = generatedUniqueCode,
                    Sequence = newSequence,
                    ProductItemStatus = ProductItemStatus.Ready,
                    ProjectId = null
                };
                _context.ProductItems.Add(productItem);

                existingInventory.Quantity += 1;
            }
            else
            {
                if (dto.Quantity <= 0)
                    throw new InvalidOperationException("برای کالاهای عادی باید مقدار افزایشی معتبر وارد شود.");

                existingInventory.Quantity += dto.Quantity;
            }

            // ثبت لاگ
            var log = new InventoryReceiptLog
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                ZoneId = dto.ZoneId,
                SectionId = dto.SectionId,
                Quantity = dto.IsUnique ? 1 : dto.Quantity,
                IsUnique = dto.IsUnique,
                UniqueCode = generatedUniqueCode,
                CreatedAt = DateTime.Now,
                DocumentType = "AddInventory"
            };
            _context.InventoryReceiptLogs.Add(log);

            // ذخیره تغییرات
            await _context.SaveChangesAsync(CancellationToken.None);

            return (true, generatedUniqueCode);
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
                Quantity = existingInventory?.Quantity ?? 0,
                IsUnique = inputDto.IsUnique
            };
        }
    }
}
