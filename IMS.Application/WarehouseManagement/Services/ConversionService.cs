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



        public async Task<(int Id, string DocumentNumber)> UpdateConversionDocumentAsync(
        int documentId,
        List<ConversionConsumedItemDto> consumedItems,
        List<ConversionProducedItemDto> producedItems,
        CancellationToken cancellationToken = default)
        {
            if (consumedItems == null || !consumedItems.Any())
                throw new ArgumentException("اقلام مصرفی نمی‌تواند خالی باشد.");
            if (producedItems == null || !producedItems.Any())
                throw new ArgumentException("اقلام تولیدی نمی‌تواند خالی باشد.");

            var document = await _dbContext.conversionDocuments
                .Include(d => d.ConsumedItems)
                .Include(d => d.ProducedItems)
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

            if (document == null)
                throw new InvalidOperationException("سند تبدیل یافت نشد.");

            var allProductIds = consumedItems.Select(x => x.ProductId)
                .Concat(producedItems.Select(x => x.ProductId))
                .Concat(document.ConsumedItems.Select(x => x.ProductId))
                .Concat(document.ProducedItems.Select(x => x.ProductId))
                .Distinct()
                .ToList();

            var inventories = await _dbContext.Inventories
                .Where(i => allProductIds.Contains(i.ProductId))
                .ToListAsync(cancellationToken);

            var productNames = await _dbContext.Products
                .Where(p => allProductIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            // بازگرداندن موجودی‌ها
            foreach (var consumed in document.ConsumedItems)
            {
                var inv = inventories.FirstOrDefault(i =>
                    i.WarehouseId == consumed.WarehouseId &&
                    i.ZoneId == consumed.ZoneId &&
                    i.SectionId == consumed.SectionId &&
                    i.ProductId == consumed.ProductId);
                if (inv != null)
                    inv.Quantity += consumed.Quantity;
            }

            foreach (var produced in document.ProducedItems)
            {
                var inv = inventories.FirstOrDefault(i =>
                    i.WarehouseId == produced.WarehouseId &&
                    i.ZoneId == produced.ZoneId &&
                    i.SectionId == produced.SectionId &&
                    i.ProductId == produced.ProductId);
                if (inv != null)
                {
                    inv.Quantity -= produced.Quantity;
                    if (inv.Quantity < 0) inv.Quantity = 0;
                }
            }

            _dbContext.conversionConsumedItems.RemoveRange(document.ConsumedItems);
            _dbContext.conversionProducedItems.RemoveRange(document.ProducedItems);
            document.ConsumedItems.Clear();
            document.ProducedItems.Clear();

            foreach (var item in consumedItems)
            {
                var inv = inventories.FirstOrDefault(i =>
                    i.WarehouseId == item.WarehouseId &&
                    i.ZoneId == item.ZoneId &&
                    i.SectionId == item.SectionId &&
                    i.ProductId == item.ProductId);

                if (inv == null)
                    throw new InvalidOperationException($"موجودی برای کالای مصرفی '{productNames.GetValueOrDefault(item.ProductId) ?? "نامشخص"}' یافت نشد.");

                if (inv.Quantity < item.Quantity)
                    throw new InvalidOperationException($"مقدار کافی از کالای '{productNames.GetValueOrDefault(item.ProductId) ?? "نامشخص"}' موجود نیست.");

                inv.Quantity -= item.Quantity;

                document.ConsumedItems.Add(new ConversionConsumedItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    CategoryId = item.CategoryId,
                    GroupId = item.GroupId,
                    StatusId = item.StatusId,
                    WarehouseId = item.WarehouseId,
                    ZoneId = item.ZoneId,
                    SectionId = item.SectionId
                });
            }

            foreach (var item in producedItems)
            {
                var inv = inventories.FirstOrDefault(i =>
                    i.WarehouseId == item.WarehouseId &&
                    i.ZoneId == item.ZoneId &&
                    i.SectionId == item.SectionId &&
                    i.ProductId == item.ProductId);

                if (inv == null)
                {
                    inv = new Inventory
                    {
                        ProductId = item.ProductId,
                        WarehouseId = item.WarehouseId,
                        ZoneId = item.ZoneId,
                        SectionId = item.SectionId,
                        Quantity = 0
                    };
                    _dbContext.Inventories.Add(inv);
                    inventories.Add(inv);
                }

                inv.Quantity += item.Quantity;

                document.ProducedItems.Add(new ConversionProducedItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    CategoryId = item.CategoryId,
                    GroupId = item.GroupId,
                    StatusId = item.StatusId,
                    WarehouseId = item.WarehouseId,
                    ZoneId = item.ZoneId,
                    SectionId = item.SectionId
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (document.Id, document.DocumentNumber);
        }






    }
}
