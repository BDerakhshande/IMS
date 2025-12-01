using DocumentFormat.OpenXml.InkML;
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

            // جمع‌آوری شناسه‌ها
            var allWarehouseIds = consumedItems.Select(i => i.WarehouseId)
                .Concat(producedItems.Select(i => i.WarehouseId)).Distinct().ToList();
            var allProductIds = consumedItems.Select(i => i.ProductId)
                .Concat(producedItems.Select(i => i.ProductId)).Distinct().ToList();

            // بارگذاری موجودی‌ها و آیتم‌ها
            var inventories = await _dbContext.Inventories
                .Where(i => allWarehouseIds.Contains(i.WarehouseId) && allProductIds.Contains(i.ProductId))
                .ToListAsync(cancellationToken);

            var inventoryItems = await _dbContext.InventoryItems
                .Include(ii => ii.Inventory)
                .Where(ii => allWarehouseIds.Contains(ii.Inventory.WarehouseId) && allProductIds.Contains(ii.Inventory.ProductId))
                .ToListAsync(cancellationToken);

            var inventoryItemsById = inventoryItems.ToDictionary(ii => ii.Id);
            var inventoryItemsMap = inventoryItems
                .GroupBy(ii => new { ii.Inventory.WarehouseId, ii.Inventory.ZoneId, ii.Inventory.SectionId, ii.Inventory.ProductId })
                .ToDictionary(g => g.Key, g => g.ToList());

            var products = await _dbContext.Products.Where(p => allProductIds.Contains(p.Id)).ToListAsync(cancellationToken);
            var productNames = products.ToDictionary(p => p.Id, p => p.Name);

            var conversionDocument = new ConversionDocument
            {
                DocumentNumber = nextDocNumber,
                CreatedAt = DateTime.Now,
                ConsumedItems = new List<ConversionConsumedItem>(),
                ProducedItems = new List<ConversionProducedItem>()
            };

            using var transaction = await (_dbContext as DbContext).Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // ================================
                // پردازش اقلام مصرفی
                // ================================
                foreach (var consumed in consumedItems)
                {
                    var productName = productNames.GetValueOrDefault(consumed.ProductId, "نامشخص");

                    var sourceInventory = inventories.FirstOrDefault(i =>
                        i.WarehouseId == consumed.WarehouseId &&
                        i.ProductId == consumed.ProductId &&
                        i.ZoneId == consumed.ZoneId &&
                        i.SectionId == consumed.SectionId
                    );

                    if (sourceInventory == null)
                    {
                        errors.Add($"موجودی برای کالای مصرفی '{productName}' یافت نشد.");
                        continue;
                    }

                    var consumedItem = new ConversionConsumedItem
                    {
                        CategoryId = consumed.CategoryId,
                        GroupId = consumed.GroupId,
                        StatusId = consumed.StatusId,
                        ProductId = consumed.ProductId,
                        Quantity = consumed.Quantity,
                        ZoneId = consumed.ZoneId.Value,
                        SectionId = consumed.SectionId.Value,
                        WarehouseId = consumed.WarehouseId,
                        ProjectId = consumed.ProjectId,
                        UniqueCodes = new List<ConversionConsumedItemUniqueCode>()
                    };

                    // کاهش موجودی بر اساس UniqueCode
                    if (consumed.InventoryItemIds != null && consumed.InventoryItemIds.Any())
                    {
                        foreach (var invId in consumed.InventoryItemIds)
                        {
                            if (!inventoryItemsById.TryGetValue(invId, out var invItem)) continue;

                            consumedItem.UniqueCodes.Add(new ConversionConsumedItemUniqueCode
                            {
                                InventoryItemId = invId,
                                ConversionConsumedItem = consumedItem
                            });

                            _dbContext.InventoryItems.Remove(invItem);
                            sourceInventory.Quantity -= 1;
                        }
                    }
                    else
                    {
                        // محاسبه موجودی واقعی عمومی (غیر یکتا)
                        int totalNonUniqueQuantity = (int)(sourceInventory.Quantity - sourceInventory.InventoryItems.Count);

                        if (totalNonUniqueQuantity <= 0)
                        {
                            // فقط موجودی یکتا وجود دارد => خطا بده
                            errors.Add($"موجودی کالای {productName} فقط به صورت کد یکتا موجود است. برای کاهش موجودی باید کد یکتا مشخص شود.");
                            continue; // ادامه پردازش آیتم بعدی
                        }

                        if (totalNonUniqueQuantity < consumed.Quantity)
                        {
                            errors.Add($"موجودی کالای عمومی {productName} کافی نیست (موجودی عمومی: {totalNonUniqueQuantity}). بخشی از موجودی این کالا دارای کد یکتا است و باید انتخاب شود.");
                            continue;
                        }

                        // کاهش موجودی عمومی
                        sourceInventory.Quantity -= consumed.Quantity;
                    }


                    conversionDocument.ConsumedItems.Add(consumedItem);
                }

                // ================================
                // پردازش اقلام تولیدی
                // ================================
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

                    var producedItem = new ConversionProducedItem
                    {
                        CategoryId = produced.CategoryId,
                        GroupId = produced.GroupId,
                        StatusId = produced.StatusId,
                        ProductId = produced.ProductId,
                        Quantity = produced.Quantity,
                        ZoneId = produced.ZoneId.Value,
                        SectionId = produced.SectionId.Value,
                        WarehouseId = produced.WarehouseId,
                        ProjectId = produced.ProjectId,
                        UniqueCodes = new List<ConversionProducedItemUniqueCode>()
                    };

                    // ایجاد کدهای یکتا اگر نیاز باشد
                    var createdUniqueCodes = new List<string>();
                    if (produced.UniqueCodes != null && produced.UniqueCodes.Any())
                    {
                        createdUniqueCodes.AddRange(produced.UniqueCodes);
                    }
                    else if (produced.GenerateUniqueCodes)
                    {
                        // دریافت آخرین شماره کد یکتا از ProductItems
                        int lastNumber = await _dbContext.ProductItems
                            .Where(pi => pi.ProductId == produced.ProductId)
                            .Select(pi => (int?)Convert.ToInt32(pi.UniqueCode))
                            .MaxAsync(cancellationToken) ?? 0;

                        for (int i = 1; i <= produced.Quantity; i++)
                            createdUniqueCodes.Add((lastNumber + i).ToString());
                    }




                    foreach (var code in createdUniqueCodes)
                    {
                        producedItem.UniqueCodes.Add(new ConversionProducedItemUniqueCode
                        {
                            UniqueCode = code,
                            ConversionProducedItem = producedItem
                        });

                        var newItem = new InventoryItem
                        {
                            Inventory = destinationInventory,
                            UniqueCode = code
                        };
                        _dbContext.InventoryItems.Add(newItem);
                        destinationInventory.Quantity += 1;
                    }

                    // کالای عمومی
                    if (!createdUniqueCodes.Any())
                        destinationInventory.Quantity += produced.Quantity;

                    conversionDocument.ProducedItems.Add(producedItem);
                }

                if (errors.Any())
                    throw new InvalidOperationException(string.Join("; ", errors));

                _dbContext.conversionDocuments.Add(conversionDocument);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return (conversionDocument.Id, nextDocNumber);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }


        public async Task<(int Id, string DocumentNumber)> UpdateConversionDocumentAsync(
    int documentId,
    List<ConversionConsumedItemDto> consumedItems,
    List<ConversionProducedItemDto> producedItems,
    CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.conversionDocuments
                .Include(d => d.ConsumedItems)
                    .ThenInclude(ci => ci.UniqueCodes)
                .Include(d => d.ProducedItems)
                    .ThenInclude(pi => pi.UniqueCodes)
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

            if (document == null)
                throw new InvalidOperationException("سند مورد نظر یافت نشد.");

            // بازگرداندن موجودی‌های قبلی
            foreach (var consumed in document.ConsumedItems)
            {
                var inventory = await _dbContext.Inventories
                    .FirstOrDefaultAsync(i =>
                        i.WarehouseId == consumed.WarehouseId &&
                        i.ZoneId == consumed.ZoneId &&
                        i.SectionId == consumed.SectionId &&
                        i.ProductId == consumed.ProductId, cancellationToken);

                if (inventory != null)
                    inventory.Quantity += consumed.Quantity;
            }

            foreach (var produced in document.ProducedItems)
            {
                var inventory = await _dbContext.Inventories
                    .FirstOrDefaultAsync(i =>
                        i.WarehouseId == produced.WarehouseId &&
                        i.ZoneId == produced.ZoneId &&
                        i.SectionId == produced.SectionId &&
                        i.ProductId == produced.ProductId, cancellationToken);

                if (inventory != null)
                {
                    inventory.Quantity -= produced.Quantity;
                    if (inventory.Quantity < 0) inventory.Quantity = 0;
                }
            }

            _dbContext.conversionConsumedItems.RemoveRange(document.ConsumedItems);
            _dbContext.conversionProducedItems.RemoveRange(document.ProducedItems);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // حالا دوباره مثل ایجاد سند، اقلام جدید را ثبت کنیم
            return await ConvertAndRegisterDocumentAsync(consumedItems, producedItems, cancellationToken);
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
