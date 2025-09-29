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
            // پیدا کردن موجودی موجود
            var existingInventory = await _context.Inventories
                .Include(i => i.InventoryItems) // بارگذاری آیتم‌های یکتا
                .FirstOrDefaultAsync(i =>
                    i.ProductId == dto.ProductId &&
                    i.WarehouseId == dto.WarehouseId &&
                    i.ZoneId == dto.ZoneId &&
                    i.SectionId == dto.SectionId);

            if (existingInventory == null)
            {
                // اگر موجودی هنوز وجود ندارد، یک رکورد جدید بسازیم
                existingInventory = new Inventory
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.WarehouseId,
                    ZoneId = dto.ZoneId,
                    SectionId = dto.SectionId,
                    Quantity = 0
                };
                _context.Inventories.Add(existingInventory);
            }

            if (dto.UniqueCodes != null && dto.UniqueCodes.Any())
            {
                // ===== حالت کالاهای یکتا =====
                int addedCount = 0;
                foreach (var code in dto.UniqueCodes)
                {
                    // بررسی اینکه کد یکتا تکراری نباشد
                    if (!existingInventory.InventoryItems.Any(x => x.UniqueCode == code))
                    {
                        var item = new InventoryItem
                        {
                            UniqueCode = code,
                            Inventory = existingInventory
                        };
                        existingInventory.InventoryItems.Add(item);
                        addedCount++;
                    }
                }

                if (addedCount == 0)
                    throw new InvalidOperationException("هیچ کد یکتای جدیدی برای اضافه کردن وجود ندارد.");

                // موجودی دقیقاً برابر تعداد آیتم‌های یکتا
                existingInventory.Quantity = existingInventory.InventoryItems.Count;
            }
            else
            {
                // ===== حالت کالاهای عادی =====
                if (dto.Quantity <= 0)
                    throw new InvalidOperationException("برای کالاهای عادی باید مقدار افزایشی معتبر وارد شود.");

                existingInventory.Quantity += dto.Quantity;
            }

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
