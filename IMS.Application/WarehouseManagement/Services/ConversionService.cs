using System;
using System.Collections.Generic;
using System.Linq;
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



        public async Task<int> ConvertAndRegisterDocumentAsync(
       List<ConversionConsumedItemDto> consumedItems,
       List<ConversionProducedItemDto> producedItems)
        {
            if (consumedItems == null || producedItems == null)
                throw new ArgumentException("اقلام مصرفی یا تولیدی نمی‌تواند خالی باشد.");

            var conversionDocument = new ConversionDocument
            {
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
                    throw new InvalidOperationException($"موجودی برای کالای مصرفی با شناسه {consumed.ProductId} یافت نشد.");

                if (inventory.Quantity < consumed.Quantity)
                    throw new InvalidOperationException($"مقدار کافی از کالا ({consumed.ProductId}) در انبار موجود نیست.");

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

            return conversionDocument.Id;
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


    }
}
