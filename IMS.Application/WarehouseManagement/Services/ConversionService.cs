using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.Services
{
    public class ConversionService : IConversionService
    {
        private readonly IWarehouseDbContext _dbContext;
        private readonly IApplicationDbContext _projectContext;
        private readonly IInventoryOperationService _inventoryOperationService;

        public ConversionService(
            IWarehouseDbContext dbContext,
            IApplicationDbContext projectContext,
            IInventoryOperationService inventoryOperationService)
        {
            _dbContext = dbContext;
            _projectContext = projectContext;
            _inventoryOperationService = inventoryOperationService;
        }

        public async Task<List<ConversionDocumentDto>> GetConversionDocumentsAsync()
        {
            var projects = await _projectContext.Projects
                .Select(p => new { p.Id, p.ProjectName })
                .ToListAsync(CancellationToken.None);
            var projectDict = projects.ToDictionary(p => p.Id, p => p.ProjectName);

            var documents = await _dbContext.conversionDocuments
                .Include(d => d.ConsumedItems)
                    .ThenInclude(ci => ci.Product)
                .Include(d => d.ProducedItems)
                    .ThenInclude(pi => pi.Product)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(CancellationToken.None);

            return documents.Select(d => new ConversionDocumentDto
            {
                Id = d.Id,
                CreatedAt = d.CreatedAt,
                DocumentNumber = d.DocumentNumber,
                ConsumedProducts = d.ConsumedItems.Select(ci => new ProductInfoDto
                {
                    ProductName = ci.Product?.Name,
                    Quantity = ci.Quantity,
                    ProjectId = ci.ProjectId,
                    ProjectTitle = ci.ProjectId.HasValue && projectDict.ContainsKey(ci.ProjectId.Value)
                        ? projectDict[ci.ProjectId.Value]
                        : null
                }).ToList(),
                ProducedProducts = d.ProducedItems.Select(pi => new ProductInfoDto
                {
                    ProductName = pi.Product?.Name,
                    Quantity = pi.Quantity,
                    ProjectId = pi.ProjectId,
                    ProjectTitle = pi.ProjectId.HasValue && projectDict.ContainsKey(pi.ProjectId.Value)
                        ? projectDict[pi.ProjectId.Value]
                        : null
                }).ToList(),
            }).ToList();
        }

        public async Task<string> GetNextConversionDocumentNumberAsync()
        {
            var existingNumbers = await _dbContext.conversionDocuments
                .Select(r => r.DocumentNumber)
                .ToListAsync(CancellationToken.None);
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
            return nextNumber.ToString();
        }

        public async Task<(int Id, string DocumentNumber)> ConvertAndRegisterDocumentAsync(
            List<ConversionConsumedItemDto> consumedItems,
            List<ConversionProducedItemDto> producedItems,
            CancellationToken cancellationToken = default)
        {
            if (consumedItems == null || !consumedItems.Any())
                throw new ArgumentException("اقلام مصرفی نمی‌تواند خالی باشد.");
            if (producedItems == null || !producedItems.Any())
                throw new ArgumentException("اقلام تولیدی نمی‌تواند خالی باشد.");

            var errors = new List<string>();
            string nextDocNumber = await GetNextConversionDocumentNumberAsync();

            var allWarehouseIds = consumedItems.Select(i => i.WarehouseId)
                .Concat(producedItems.Select(i => i.WarehouseId))
                .Distinct()
                .ToList();
            var allProductIds = consumedItems.Select(i => i.ProductId)
                .Concat(producedItems.Select(i => i.ProductId))
                .Distinct()
                .ToList();
            var allZoneIds = consumedItems.Select(i => i.ZoneId)
                .Concat(producedItems.Select(i => i.ZoneId))
                .Distinct()
                .ToList();
            var allSectionIds = consumedItems.Select(i => i.SectionId)
                .Concat(producedItems.Select(i => i.SectionId))
                .Distinct()
                .ToList();

            // تبدیل لیست‌های int به int? برای هماهنگی با ستون‌های nullable
            List<int?> allZoneIdsNullable = allZoneIds.Select(z => (int?)z).ToList();
            List<int?> allSectionIdsNullable = allSectionIds.Select(s => (int?)s).ToList();
            List<int?> allWarehouseIdsNullable = allWarehouseIds.Select(w => (int?)w).ToList();
            List<int?> allProductIdsNullable = allProductIds.Select(p => (int?)p).ToList();

            // گرفتن Inventoryها با فیلترهای null-safe
            var inventories = await _dbContext.Inventories
                .Where(inv =>
                    allWarehouseIds.Contains(inv.WarehouseId) &&
                    allProductIds.Contains(inv.ProductId) &&
                    allZoneIdsNullable.Contains(inv.ZoneId) &&
                    allSectionIdsNullable.Contains(inv.SectionId))
                .ToListAsync(cancellationToken);

            // گرفتن InventoryItemها با Include و فیلترهای null-safe
            var inventoryItems = await _dbContext.InventoryItems
                .Include(ii => ii.Inventory)
                .Where(ii =>
                    allWarehouseIds.Contains(ii.Inventory.WarehouseId) &&
                    allProductIds.Contains(ii.Inventory.ProductId) &&
                    allZoneIdsNullable.Contains(ii.Inventory.ZoneId) &&
                    allSectionIdsNullable.Contains(ii.Inventory.SectionId))
                .ToListAsync(cancellationToken);

            var inventoryItemsMap = inventoryItems
                .GroupBy(ii => new
                {
                    ii.Inventory.WarehouseId,
                    ii.Inventory.ZoneId,
                    ii.Inventory.SectionId,
                    ii.Inventory.ProductId
                })
                .ToDictionary(g => g.Key, g => g.ToList());

            var productNames = await _dbContext.Products
                .Where(p => allProductIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            var conversionDocument = new ConversionDocument
            {
                DocumentNumber = nextDocNumber,
                CreatedAt = DateTime.Now,
                ConsumedItems = new List<ConversionConsumedItem>(),
                ProducedItems = new List<ConversionProducedItem>()
            };

            // --- پردازش consumed items ---
            foreach (var consumed in consumedItems)
            {
                var productName = productNames.GetValueOrDefault(consumed.ProductId, "نامشخص");
                var sourceInventory = inventories.FirstOrDefault(i =>
                    i.WarehouseId == consumed.WarehouseId &&
                    i.ZoneId == consumed.ZoneId &&
                    i.SectionId == consumed.SectionId &&
                    i.ProductId == consumed.ProductId);

                if (sourceInventory == null)
                {
                    errors.Add($"موجودی برای کالای مصرفی '{productName}' یافت نشد.");
                    continue;
                }

                var key = new
                {
                    consumed.WarehouseId,
                    ZoneId = (int?)consumed.ZoneId,
                    SectionId = (int?)consumed.SectionId,
                    consumed.ProductId
                };


                var uniqueCount = inventoryItemsMap.ContainsKey(key) ? inventoryItemsMap[key].Count : 0;
                decimal totalNonUniqueQuantity = sourceInventory.Quantity - uniqueCount;
                if (totalNonUniqueQuantity <= 0)
                {
                    errors.Add($"کالای {productName} فقط به‌صورت کد یکتا موجود است. مصرف عمومی ممکن نیست.");
                    continue;
                }
                if (totalNonUniqueQuantity < consumed.Quantity)
                {
                    errors.Add($"موجودی عمومی کالای {productName} کافی نیست (موجودی عمومی: {totalNonUniqueQuantity}).");
                    continue;
                }

                sourceInventory.Quantity -= consumed.Quantity;
                _dbContext.Inventories.Update(sourceInventory);

                conversionDocument.ConsumedItems.Add(new ConversionConsumedItem
                {
                    CategoryId = consumed.CategoryId,
                    GroupId = consumed.GroupId,
                    StatusId = consumed.StatusId,
                    ProductId = consumed.ProductId,
                    Quantity = consumed.Quantity,
                    ZoneId = consumed.ZoneId,
                    SectionId = consumed.SectionId,
                    WarehouseId = consumed.WarehouseId,
                    ProjectId = consumed.ProjectId
                });
            }

            if (errors.Any())
                throw new InvalidOperationException(string.Join("; ", errors));

            // --- پردازش produced items ---
            foreach (var produced in producedItems)
            {
                var productName = productNames.GetValueOrDefault(produced.ProductId, "نامشخص");
                var destinationInventory = inventories.FirstOrDefault(i =>
                    i.WarehouseId == produced.WarehouseId &&
                    i.ZoneId == produced.ZoneId &&
                    i.SectionId == produced.SectionId &&
                    i.ProductId == produced.ProductId);

                if (destinationInventory == null)
                {
                    destinationInventory = new Inventory
                    {
                        WarehouseId = produced.WarehouseId,
                        ZoneId = produced.ZoneId,
                        SectionId = produced.SectionId,
                        ProductId = produced.ProductId,
                        Quantity = 0
                    };
                    _dbContext.Inventories.Add(destinationInventory);
                    inventories.Add(destinationInventory);
                }

                destinationInventory.Quantity += produced.Quantity;
                _dbContext.Inventories.Update(destinationInventory);

                conversionDocument.ProducedItems.Add(new ConversionProducedItem
                {
                    CategoryId = produced.CategoryId,
                    GroupId = produced.GroupId,
                    StatusId = produced.StatusId,
                    ProductId = produced.ProductId,
                    Quantity = produced.Quantity,
                    ZoneId = produced.ZoneId,
                    SectionId = produced.SectionId,
                    WarehouseId = produced.WarehouseId,
                    ProjectId = produced.ProjectId
                });
            }

            _dbContext.conversionDocuments.Add(conversionDocument);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return (conversionDocument.Id, nextDocNumber);
        }

        public async Task<bool> DeleteConversionDocumentAsync(int documentId)
        {
            var document = await _dbContext.conversionDocuments
                .Include(d => d.ConsumedItems)
                .Include(d => d.ProducedItems)
                .FirstOrDefaultAsync(d => d.Id == documentId, CancellationToken.None);

            if (document == null) return false;

            var dbContext = _dbContext as DbContext;
            using var transaction = await dbContext.Database.BeginTransactionAsync(CancellationToken.None);
            try
            {
                foreach (var consumed in document.ConsumedItems)
                {
                    var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                        i.WarehouseId == consumed.WarehouseId &&
                        i.ZoneId == consumed.ZoneId &&
                        i.SectionId == consumed.SectionId &&
                        i.ProductId == consumed.ProductId, CancellationToken.None);
                    if (inventory != null)
                    {
                        inventory.Quantity += consumed.Quantity;
                        _dbContext.Inventories.Update(inventory);
                    }
                }

                foreach (var produced in document.ProducedItems)
                {
                    var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                        i.WarehouseId == produced.WarehouseId &&
                        i.ZoneId == produced.ZoneId &&
                        i.SectionId == produced.SectionId &&
                        i.ProductId == produced.ProductId, CancellationToken.None);
                    if (inventory != null)
                    {
                        inventory.Quantity -= produced.Quantity;
                        if (inventory.Quantity < 0) inventory.Quantity = 0;
                        _dbContext.Inventories.Update(inventory);
                    }
                }

                _dbContext.conversionConsumedItems.RemoveRange(document.ConsumedItems);
                _dbContext.conversionProducedItems.RemoveRange(document.ProducedItems);
                _dbContext.conversionDocuments.Remove(document);

                await _dbContext.SaveChangesAsync(CancellationToken.None);
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}