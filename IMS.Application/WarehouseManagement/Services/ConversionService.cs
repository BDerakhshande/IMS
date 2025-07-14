using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class ConversionService : IConversionService
    {
        private readonly IWarehouseDbContext _dbContext;

        public ConversionService(IWarehouseDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<ConversionDocumentDto>> GetConversionDocumentsAsync()
        {
            var documents = await _dbContext.conversionDocuments
                .Include(d => d.ConsumedItems)
                    .ThenInclude(ci => ci.Product)
                .Include(d => d.ProducedItems)
                    .ThenInclude(pi => pi.Product)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new ConversionDocumentDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    DocumentNumber = d.DocumentNumber, // ✅ این خط درست است

                    ConsumedProducts = d.ConsumedItems.Select(ci => new ProductInfoDto
                    {
                        ProductName = ci.Product.Name,
                        Quantity = ci.Quantity
                    }).ToList(),

                    ProducedProducts = d.ProducedItems.Select(pi => new ProductInfoDto
                    {
                        ProductName = pi.Product.Name,
                        Quantity = pi.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return documents;
        }


        public async Task<string> GetNextConversionDocumentNumberAsync()
        {
            var existingNumbers = await _dbContext.conversionDocuments
                .Select(r => r.DocumentNumber)
                .ToListAsync();

            var existingInts = existingNumbers
                .Select(s => int.TryParse(s, out int n) ? n : 0)
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();

            int nextNumber = 1;
            foreach (var number in existingInts)
            {
                if (number == nextNumber)
                    nextNumber++;
                else if (number > nextNumber)
                    break;
            }

            return nextNumber.ToString(); // ← مثلاً "1"، "2"، "3"
        }


        public async Task<(int Id, string DocumentNumber)> ConvertAndRegisterDocumentAsync(
    List<ConversionConsumedItemDto> consumedItems,
    List<ConversionProducedItemDto> producedItems)
        {
           

            if (consumedItems == null || producedItems == null)
                throw new ArgumentException("اقلام مصرفی یا تولیدی نمی‌تواند خالی باشد.");

            string nextDocNumber = await GetNextConversionDocumentNumberAsync();

            var conversionDocument = new ConversionDocument
            {
                DocumentNumber = nextDocNumber,
                CreatedAt = DateTime.Now,
                ConsumedItems = new List<ConversionConsumedItem>(),
                ProducedItems = new List<ConversionProducedItem>()
            };

            foreach (var consumed in consumedItems)
            {
                var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == consumed.WarehouseId &&
                    i.ZoneId == consumed.ZoneId &&
                    i.SectionId == consumed.SectionId &&
                    i.ProductId == consumed.ProductId);
                if (inventory == null)
                {
                    var productName = await _dbContext.Products
                        .Where(p => p.Id == consumed.ProductId)
                        .Select(p => p.Name)
                        .FirstOrDefaultAsync() ?? "نامشخص";

                    throw new InvalidOperationException($"موجودی برای کالای مصرفی '{productName}' یافت نشد.");
                }

                if (inventory.Quantity < consumed.Quantity)
                {
                    var productName = await _dbContext.Products
                        .Where(p => p.Id == consumed.ProductId)
                        .Select(p => p.Name)
                        .FirstOrDefaultAsync() ?? "نامشخص";

                    throw new InvalidOperationException($"مقدار کافی از کالا '{productName}' در انبار موجود نیست.");
                }

                inventory.Quantity -= consumed.Quantity;

                ((DbContext)_dbContext).Entry(inventory).Property(i => i.Quantity).IsModified = true;


                conversionDocument.ConsumedItems.Add(new ConversionConsumedItem
                {
                    CategoryId = consumed.CategoryId,
                    GroupId = consumed.GroupId,
                    StatusId = consumed.StatusId,
                    ProductId = consumed.ProductId,
                    Quantity = consumed.Quantity,
                    ZoneId = consumed.ZoneId,
                    SectionId = consumed.SectionId,
                    WarehouseId = consumed.WarehouseId
                });
            }

            // گرفتن نام همه کالاهای مرتبط در یک کوئری قبل از حلقه
            var productIds = producedItems.Select(p => p.ProductId).Distinct().ToList();

            var productNames = await _dbContext.Products
    .Where(p => productIds.Contains(p.Id))
    .ToDictionaryAsync(p => p.Id, p => p.Name);
            foreach (var produced in producedItems)
            {
                var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
        i.WarehouseId == produced.WarehouseId &&
        i.ZoneId == produced.ZoneId &&
        i.SectionId == produced.SectionId &&
        i.ProductId == produced.ProductId);

                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        WarehouseId = produced.WarehouseId,
                        ZoneId = produced.ZoneId,
                        SectionId = produced.SectionId,
                        ProductId = produced.ProductId,
                        Quantity = 0
                    };
                    _dbContext.Inventories.Add(inventory);
                }
                inventory.Quantity += produced.Quantity;

                conversionDocument.ProducedItems.Add(new ConversionProducedItem
                {
                    CategoryId = produced.CategoryId,
                    GroupId = produced.GroupId,
                    StatusId = produced.StatusId,
                    ProductId = produced.ProductId,
                    Quantity = produced.Quantity,
                    ZoneId = produced.ZoneId,
                    SectionId = produced.SectionId,
                    WarehouseId = produced.WarehouseId
                });
            }

            _dbContext.conversionDocuments.Add(conversionDocument);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            return (conversionDocument.Id, nextDocNumber);
        }

        public async Task<bool> DeleteConversionDocumentAsync(int documentId)
        {
            var document = await _dbContext.conversionDocuments
                .Include(d => d.ConsumedItems)
                .Include(d => d.ProducedItems)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return false;

            // بازگرداندن مواد مصرفی به انبار
            foreach (var consumed in document.ConsumedItems)
            {
                var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == consumed.WarehouseId &&
                    i.ZoneId == consumed.ZoneId &&
                    i.SectionId == consumed.SectionId &&
                    i.ProductId == consumed.ProductId);

                if (inventory != null)
                {
                    inventory.Quantity += consumed.Quantity;
                    ((DbContext)_dbContext).Entry(inventory).Property(i => i.Quantity).IsModified = true;
                }
            }

            // کم کردن محصولات تولیدی از انبار
            foreach (var produced in document.ProducedItems)
            {
                var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == produced.WarehouseId &&
                    i.ZoneId == produced.ZoneId &&
                    i.SectionId == produced.SectionId &&
                    i.ProductId == produced.ProductId);

                if (inventory != null)
                {
                    inventory.Quantity -= produced.Quantity;
                    if (inventory.Quantity < 0)
                        inventory.Quantity = 0;

                    ((DbContext)_dbContext).Entry(inventory).Property(i => i.Quantity).IsModified = true;
                }
            }

            // حذف اقلام و سند
            _dbContext.conversionConsumedItems.RemoveRange(document.ConsumedItems);
            _dbContext.conversionProducedItems.RemoveRange(document.ProducedItems);
            _dbContext.conversionDocuments.Remove(document);

            await _dbContext.SaveChangesAsync(CancellationToken.None);

            return true;
        }



        public async Task<(int Id, string DocumentNumber)> EditConversionDocumentAsync(
      int documentId,
      List<ConversionConsumedItemDto> consumedItems,
      List<ConversionProducedItemDto> producedItems,
      CancellationToken cancellationToken = default)
        {
            if (consumedItems == null || producedItems == null)
                throw new ArgumentException("اقلام مصرفی یا تولیدی نمی‌تواند خالی باشد.");

            // Cast _dbContext to WarehouseDbContext برای دسترسی به Database و تراکنش
            var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);


            using var transaction = await efDbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var document = await efDbContext.conversionDocuments
                    .Include(d => d.ConsumedItems)
                    .Include(d => d.ProducedItems)
                    .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

                if (document == null)
                    throw new InvalidOperationException("سند تبدیل یافت نشد.");

                // ۱. جمع آوری همه ProductId ها برای مصرفی و تولیدی
                var allProductIds = document.ConsumedItems.Select(c => c.ProductId)
                    .Concat(document.ProducedItems.Select(p => p.ProductId))
                    .Concat(consumedItems.Select(c => c.ProductId))
                    .Concat(producedItems.Select(p => p.ProductId))
                    .Distinct()
                    .ToList();

                // ۲. واکشی موجودی‌های مرتبط با همه کالاها (برای مصرفی و تولیدی)
                var inventories = await efDbContext.Inventories
                    .Where(i => allProductIds.Contains(i.ProductId))
                    .ToListAsync(cancellationToken);

                // ۳. واکشی نام کالاها برای خطاها
                var productNames = await efDbContext.Products
                    .Where(p => allProductIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

                // ۴. بازگرداندن موجودی اقلام قبلی (مصرفی)
                foreach (var consumed in document.ConsumedItems)
                {
                    var inventory = inventories.FirstOrDefault(i =>
                        i.WarehouseId == consumed.WarehouseId &&
                        i.ZoneId == consumed.ZoneId &&
                        i.SectionId == consumed.SectionId &&
                        i.ProductId == consumed.ProductId);

                    if (inventory != null)
                    {
                        inventory.Quantity += consumed.Quantity;
                        efDbContext.Entry(inventory).Property(i => i.Quantity).IsModified = true;
                    }
                }

                // ۵. بازگرداندن موجودی اقلام قبلی (تولیدی)
                foreach (var produced in document.ProducedItems)
                {
                    var inventory = inventories.FirstOrDefault(i =>
                        i.WarehouseId == produced.WarehouseId &&
                        i.ZoneId == produced.ZoneId &&
                        i.SectionId == produced.SectionId &&
                        i.ProductId == produced.ProductId);

                    if (inventory != null)
                    {
                        inventory.Quantity -= produced.Quantity;
                        if (inventory.Quantity < 0) inventory.Quantity = 0;
                        efDbContext.Entry(inventory).Property(i => i.Quantity).IsModified = true;
                    }
                }

                // ۶. حذف اقلام قبلی
                efDbContext.conversionConsumedItems.RemoveRange(document.ConsumedItems);
                efDbContext.conversionProducedItems.RemoveRange(document.ProducedItems);
                document.ConsumedItems.Clear();
                document.ProducedItems.Clear();

                // ۷. افزودن اقلام جدید مصرفی و کاهش موجودی
                foreach (var consumed in consumedItems)
                {
                    var inventory = inventories.FirstOrDefault(i =>
                        i.WarehouseId == consumed.WarehouseId &&
                        i.ZoneId == consumed.ZoneId &&
                        i.SectionId == consumed.SectionId &&
                        i.ProductId == consumed.ProductId);

                    if (inventory == null)
                    {
                        var productName = productNames.GetValueOrDefault(consumed.ProductId) ?? "نامشخص";
                        throw new InvalidOperationException($"موجودی برای کالای مصرفی '{productName}' یافت نشد.");
                    }

                    if (inventory.Quantity < consumed.Quantity)
                    {
                        var productName = productNames.GetValueOrDefault(consumed.ProductId) ?? "نامشخص";
                        throw new InvalidOperationException($"مقدار کافی از کالا '{productName}' در انبار موجود نیست.");
                    }

                    inventory.Quantity -= consumed.Quantity;
                    efDbContext.Entry(inventory).Property(i => i.Quantity).IsModified = true;

                    document.ConsumedItems.Add(new ConversionConsumedItem
                    {
                        CategoryId = consumed.CategoryId,
                        GroupId = consumed.GroupId,
                        StatusId = consumed.StatusId,
                        ProductId = consumed.ProductId,
                        Quantity = consumed.Quantity,
                        ZoneId = consumed.ZoneId,
                        SectionId = consumed.SectionId,
                        WarehouseId = consumed.WarehouseId
                    });
                }

                // ۸. افزودن اقلام جدید تولیدی و افزایش موجودی (در صورت نبودن موجودی، ایجادش کن)
                foreach (var produced in producedItems)
                {
                    var inventory = inventories.FirstOrDefault(i =>
                        i.WarehouseId == produced.WarehouseId &&
                        i.ZoneId == produced.ZoneId &&
                        i.SectionId == produced.SectionId &&
                        i.ProductId == produced.ProductId);

                    if (inventory == null)
                    {
                        inventory = new Inventory
                        {
                            WarehouseId = produced.WarehouseId,
                            ZoneId = produced.ZoneId,
                            SectionId = produced.SectionId,
                            ProductId = produced.ProductId,
                            Quantity = 0
                        };
                        efDbContext.Inventories.Add(inventory);
                        inventories.Add(inventory); // اضافه کردن به لیست برای پیگیری
                    }
                    inventory.Quantity += produced.Quantity;

                    document.ProducedItems.Add(new ConversionProducedItem
                    {
                        CategoryId = produced.CategoryId,
                        GroupId = produced.GroupId,
                        StatusId = produced.StatusId,
                        ProductId = produced.ProductId,
                        Quantity = produced.Quantity,
                        ZoneId = produced.ZoneId,
                        SectionId = produced.SectionId,
                        WarehouseId = produced.WarehouseId
                    });
                }

                // ۹. ذخیره تغییرات و commit تراکنش
                await efDbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return (document.Id, document.DocumentNumber);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }





    }
}
