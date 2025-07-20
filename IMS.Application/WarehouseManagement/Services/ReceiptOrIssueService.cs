using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class ReceiptOrIssueService: IReceiptOrIssueService
    {
        private IWarehouseDbContext _dbContext;
        private readonly IApplicationDbContext _projectContext;
    

        public ReceiptOrIssueService(IWarehouseDbContext warehouseDbContext, IApplicationDbContext projectContext)
        {
            _dbContext = warehouseDbContext;
            _projectContext = projectContext;
        }

        public async Task<ReceiptOrIssueDto?> GetByIdAsync(int id)
        {
            var entity = await _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceSection)
                        .ThenInclude(s => s.Zone)
                            .ThenInclude(z => z.Warehouse)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationSection)
                        .ThenInclude(s => s.Zone)
                            .ThenInclude(z => z.Warehouse)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Category)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Group)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Status)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null)
                return null;

            // بارگذاری پروژه جداگانه
            string? projectTitle = null;
            if (entity.ProjectId != null)
            {
                var project = await _projectContext.Projects
                    .Where(p => p.Id == entity.ProjectId)
                    .Select(p => p.ProjectName)
                    .FirstOrDefaultAsync();

                projectTitle = project;
            }

            var dto = new ReceiptOrIssueDto
            {
                Id = entity.Id,
                Type = entity.Type,
                Date = entity.Date,
                DocumentNumber = entity.DocumentNumber,
                Description = entity.Description,
                ProjectId = entity.ProjectId,
                ProjectTitle = projectTitle,

                Items = (entity.Items ?? new List<ReceiptOrIssueItem>())
                    .Select(i => new ReceiptOrIssueItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,

                        SourceSectionId = i.SourceSectionId,
                        SourceSectionName = i.SourceSection?.Name,
                        SourceZoneName = i.SourceSection?.Zone?.Name,
                        SourceWarehouseName = i.SourceSection?.Zone?.Warehouse?.Name,

                        DestinationSectionId = i.DestinationSectionId,
                        DestinationSectionName = i.DestinationSection?.Name,
                        DestinationZoneName = i.DestinationSection?.Zone?.Name,
                        DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name,

                        CategoryId = i.CategoryId,
                        CategoryName = i.Category?.Name,

                        GroupId = i.GroupId,
                        GroupName = i.Group?.Name,

                        StatusId = i.StatusId,
                        StatusName = i.Status?.Name,

                        ProductName = i.Product?.Name
                    }).ToList()
            };

            return dto;
        }





        public async Task<List<ReceiptOrIssueDto>> GetAllAsync(int? warehouseId = null)
        {
            var query = _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceSection)
                        .ThenInclude(s => s.Zone)
                            .ThenInclude(z => z.Warehouse)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationSection)
                        .ThenInclude(s => s.Zone)
                            .ThenInclude(z => z.Warehouse)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(r => r.Items.Any(i =>
                    (i.SourceSection != null && i.SourceSection.Zone.WarehouseId == warehouseId.Value)
                    || (i.DestinationSection != null && i.DestinationSection.Zone.WarehouseId == warehouseId.Value)
                ));
            }

            var list = await query.ToListAsync();

            // واکشی پروژه‌ها به صورت جداگانه
            var projectIds = list
                .Where(r => r.ProjectId.HasValue)
                .Select(r => r.ProjectId!.Value)
                .Distinct()
                .ToList();

            var projects = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

            var result = list.Select(entity => new ReceiptOrIssueDto
            {
                Id = entity.Id,
                Type = entity.Type,
                Date = entity.Date,
                DocumentNumber = entity.DocumentNumber,
                Description = entity.Description,

                ProjectId = entity.ProjectId,
                ProjectTitle = entity.ProjectId.HasValue && projects.ContainsKey(entity.ProjectId.Value)
                    ? projects[entity.ProjectId.Value]
                    : null,

                Items = entity.Items.Select(i => new ReceiptOrIssueItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,

                    SourceSectionId = i.SourceSectionId,
                    SourceSectionName = i.SourceSection?.Name,
                    SourceZoneName = i.SourceSection?.Zone?.Name,
                    SourceWarehouseName = i.SourceSection?.Zone?.Warehouse?.Name,

                    DestinationSectionId = i.DestinationSectionId,
                    DestinationSectionName = i.DestinationSection?.Name,
                    DestinationZoneName = i.DestinationSection?.Zone?.Name,
                    DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name
                }).ToList()
            }).ToList();

            return result;
        }



        public async Task<ReceiptOrIssueDto> CreateAsync(ReceiptOrIssueDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Items collection cannot be empty.");

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

            // بارگذاری نام محصولات از دیتابیس بر اساس ProductId ها
            var productNames = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            foreach (var item in dto.Items)
            {
                if (item.ProductId <= 0)
                    throw new ArgumentException("ProductId must be greater than zero.", nameof(item.ProductId));
                if (item.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.", nameof(item.Quantity));
            }

            var sourceSectionIds = dto.Items
                .Where(i => i.SourceSectionId.HasValue)
                .Select(i => i.SourceSectionId!.Value)
                .Distinct()
                .ToList();

            var sourceZoneIds = dto.Items
                .Where(i => !i.SourceSectionId.HasValue && i.SourceZoneId.HasValue)
                .Select(i => i.SourceZoneId!.Value)
                .Distinct()
                .ToList();

            var destinationSectionIds = dto.Items
                .Where(i => i.DestinationSectionId.HasValue)
                .Select(i => i.DestinationSectionId!.Value)
                .Distinct()
                .ToList();

            var destinationZoneIds = dto.Items
                .Where(i => !i.DestinationSectionId.HasValue && i.DestinationZoneId.HasValue)
                .Select(i => i.DestinationZoneId!.Value)
                .Distinct()
                .ToList();

            var sourceSections = await _dbContext.StorageSections
                .Include(s => s.Zone)
                .ThenInclude(z => z!.Warehouse)
                .Where(s => sourceSectionIds.Contains(s.Id) || sourceZoneIds.Contains(s.ZoneId))
                .ToListAsync(cancellationToken);

            var destinationSections = await _dbContext.StorageSections
                .Include(s => s.Zone)
                .ThenInclude(z => z!.Warehouse)
                .Where(s => destinationSectionIds.Contains(s.Id) || destinationZoneIds.Contains(s.ZoneId))
                .ToListAsync(cancellationToken);

            var entity = new ReceiptOrIssue
            {
                Date = dto.Date,
                DocumentNumber = dto.DocumentNumber,
                Description = dto.Description,
                Type = dto.Type,
                ProjectId = dto.ProjectId,
                Items = new List<ReceiptOrIssueItem>()
            };

            foreach (var itemDto in dto.Items)
            {
                var sourceSection = itemDto.SourceSectionId.HasValue
                    ? sourceSections.FirstOrDefault(s => s.Id == itemDto.SourceSectionId.Value)
                    : sourceSections.FirstOrDefault(s => s.ZoneId == itemDto.SourceZoneId);

                var destinationSection = itemDto.DestinationSectionId.HasValue
                    ? destinationSections.FirstOrDefault(s => s.Id == itemDto.DestinationSectionId.Value)
                    : destinationSections.FirstOrDefault(s => s.ZoneId == itemDto.DestinationZoneId);

                var newItem = new ReceiptOrIssueItem
                {
                    CategoryId = itemDto.CategoryId,
                    GroupId = itemDto.GroupId,
                    StatusId = itemDto.StatusId,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    SourceWarehouseId = itemDto.SourceWarehouseId,
                    SourceZoneId = itemDto.SourceZoneId,
                    SourceSectionId = sourceSection?.Id,
                    DestinationWarehouseId = itemDto.DestinationWarehouseId,
                    DestinationZoneId = itemDto.DestinationZoneId,
                    DestinationSectionId = destinationSection?.Id,
                };

                entity.Items.Add(newItem);
            }

            var allWarehouseIds = dto.Items
                .SelectMany(i => new[] { i.SourceWarehouseId, i.DestinationWarehouseId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var allProductIds = dto.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var allZoneIds = dto.Items
                .SelectMany(i => new[] { i.SourceZoneId, i.DestinationZoneId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var allSectionIds = dto.Items
                .SelectMany(i => new[] { i.SourceSectionId, i.DestinationSectionId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var inventories = await _dbContext.Inventories
                .Where(inv => allWarehouseIds.Contains(inv.WarehouseId) &&
                              allProductIds.Contains(inv.ProductId) &&
                              (inv.ZoneId == null || allZoneIds.Contains(inv.ZoneId.Value)) &&
                              (inv.SectionId == null || allSectionIds.Contains(inv.SectionId.Value)))
                .ToListAsync(cancellationToken);

            foreach (var item in entity.Items)
            {
                var sourceInventory = inventories.FirstOrDefault(i =>
                    i.WarehouseId == item.SourceWarehouseId &&
                    i.ZoneId == item.SourceZoneId &&
                    i.SectionId == item.SourceSectionId &&
                    i.ProductId == item.ProductId);

                var destinationInventory = inventories.FirstOrDefault(i =>
                    i.WarehouseId == item.DestinationWarehouseId &&
                    i.ZoneId == item.DestinationZoneId &&
                    i.SectionId == item.DestinationSectionId &&
                    i.ProductId == item.ProductId);

                var productName = productNames.ContainsKey(item.ProductId) ? productNames[item.ProductId] : "نامشخص";

                switch (dto.Type)
                {
                    case ReceiptOrIssueType.Receipt:
                    case ReceiptOrIssueType.Issue:
                    case ReceiptOrIssueType.Transfer:
                        if (sourceInventory == null || destinationInventory == null)
                            throw new InvalidOperationException($"موجودی مبدأ یا مقصد برای کالای {productName} یافت نشد.");
                        sourceInventory.Quantity -= item.Quantity;
                        if (sourceInventory.Quantity <= 0)
                            throw new InvalidOperationException($"موجودی کالا {productName} در انبار مبدأ به صفر یا کمتر رسید.");
                        destinationInventory.Quantity += item.Quantity;
                        if (destinationInventory.Quantity <= 0)
                            throw new InvalidOperationException($"موجودی کالا {productName} در انبار مقصد به صفر یا کمتر رسید.");
                        break;
                }
            }

            _dbContext.ReceiptOrIssues.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // بارگذاری مجدد بدون Include پروژه (چون در DbContext جداست)
            var createdEntity = await _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceSection)
                    .ThenInclude(s => s!.Zone)
                    .ThenInclude(z => z!.Warehouse)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationSection)
                    .ThenInclude(s => s!.Zone)
                    .ThenInclude(z => z!.Warehouse)
                .FirstOrDefaultAsync(r => r.Id == entity.Id, cancellationToken);

            if (createdEntity == null)
                throw new InvalidOperationException("Failed to retrieve the created entity.");

            // بارگذاری نام پروژه جداگانه
            string? projectTitle = null;
            if (createdEntity.ProjectId.HasValue)
            {
                projectTitle = await _projectContext.Projects
                    .Where(p => p.Id == createdEntity.ProjectId.Value)
                    .Select(p => p.ProjectName)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var resultDto = new ReceiptOrIssueDto
            {
                Id = createdEntity.Id,
                Date = createdEntity.Date,
                DocumentNumber = createdEntity.DocumentNumber,
                Description = createdEntity.Description,
                Type = createdEntity.Type,
                ProjectId = createdEntity.ProjectId,
                ProjectTitle = projectTitle,
                Items = createdEntity.Items.Select(i => new ReceiptOrIssueItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    SourceWarehouseId = i.SourceWarehouseId,
                    SourceZoneId = i.SourceZoneId,
                    SourceSectionId = i.SourceSectionId,
                    DestinationWarehouseId = i.DestinationWarehouseId,
                    DestinationZoneId = i.DestinationZoneId,
                    DestinationSectionId = i.DestinationSectionId,
                    SourceSectionName = i.SourceSection?.Name,
                    SourceZoneName = i.SourceSection?.Zone?.Name,
                    SourceWarehouseName = i.SourceSection?.Zone?.Warehouse?.Name,
                    DestinationSectionName = i.DestinationSection?.Name,
                    DestinationZoneName = i.DestinationSection?.Zone?.Name,
                    DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name,
                    CategoryName = null,
                    GroupName = null,
                    StatusName = null,
                    ProductName = productNames.ContainsKey(i.ProductId) ? productNames[i.ProductId] : "نامشخص"
                }).ToList()
            };

            return resultDto;
        }




        public async Task<ReceiptOrIssueDto?> UpdateAsync(int id, ReceiptOrIssueDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Items collection cannot be empty.");

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

            var productNames = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            foreach (var item in dto.Items)
            {
                if (item.ProductId <= 0)
                    throw new ArgumentException("ProductId must be greater than zero.");
                if (item.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.");
            }

            var entity = await _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null)
                return null;

            // Rollback old inventory effect
            foreach (var oldItem in entity.Items)
            {
                var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == oldItem.SourceWarehouseId &&
                    i.ZoneId == oldItem.SourceZoneId &&
                    i.SectionId == oldItem.SourceSectionId &&
                    i.ProductId == oldItem.ProductId,
                    cancellationToken);

                if (inventory != null && (entity.Type == ReceiptOrIssueType.Issue || entity.Type == ReceiptOrIssueType.Transfer))
                    inventory.Quantity += oldItem.Quantity;

                inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == oldItem.DestinationWarehouseId &&
                    i.ZoneId == oldItem.DestinationZoneId &&
                    i.SectionId == oldItem.DestinationSectionId &&
                    i.ProductId == oldItem.ProductId,
                    cancellationToken);

                if (inventory != null && (entity.Type == ReceiptOrIssueType.Receipt || entity.Type == ReceiptOrIssueType.Transfer))
                    inventory.Quantity -= oldItem.Quantity;
            }

            var existingItems = entity.Items.ToList();
            var finalItems = new List<ReceiptOrIssueItem>();

            foreach (var dtoItem in dto.Items)
            {
                var existingItem = dtoItem.Id > 0 ? existingItems.FirstOrDefault(x => x.Id == dtoItem.Id) : null;

                if (existingItem != null)
                {
                    existingItem.ProductId = dtoItem.ProductId;
                    existingItem.Quantity = dtoItem.Quantity;
                    existingItem.CategoryId = dtoItem.CategoryId;
                    existingItem.GroupId = dtoItem.GroupId;
                    existingItem.StatusId = dtoItem.StatusId;
                    existingItem.SourceWarehouseId = dtoItem.SourceWarehouseId;
                    existingItem.SourceZoneId = dtoItem.SourceZoneId;
                    existingItem.SourceSectionId = dtoItem.SourceSectionId;
                    existingItem.DestinationWarehouseId = dtoItem.DestinationWarehouseId;
                    existingItem.DestinationZoneId = dtoItem.DestinationZoneId;
                    existingItem.DestinationSectionId = dtoItem.DestinationSectionId;

                    finalItems.Add(existingItem);
                }
                else
                {
                    finalItems.Add(new ReceiptOrIssueItem
                    {
                        ProductId = dtoItem.ProductId,
                        Quantity = dtoItem.Quantity,
                        CategoryId = dtoItem.CategoryId,
                        GroupId = dtoItem.GroupId,
                        StatusId = dtoItem.StatusId,
                        SourceWarehouseId = dtoItem.SourceWarehouseId,
                        SourceZoneId = dtoItem.SourceZoneId,
                        SourceSectionId = dtoItem.SourceSectionId,
                        DestinationWarehouseId = dtoItem.DestinationWarehouseId,
                        DestinationZoneId = dtoItem.DestinationZoneId,
                        DestinationSectionId = dtoItem.DestinationSectionId
                    });
                }
            }

            var toBeRemoved = existingItems.Where(ei => dto.Items.All(ni => ni.Id != ei.Id)).ToList();
            _dbContext.ReceiptOrIssueItems.RemoveRange(toBeRemoved);

            // به‌روزرسانی فیلدهای اصلی
            entity.Date = dto.Date;
            entity.DocumentNumber = dto.DocumentNumber;
            entity.Description = dto.Description;
            entity.Type = dto.Type;
            entity.ProjectId = dto.ProjectId; // ✅ اضافه شد
            entity.Items = finalItems;

            // محاسبه جدید موجودی‌ها
            var allWarehouseIds = dto.Items.SelectMany(i => new[] { i.SourceWarehouseId, i.DestinationWarehouseId }).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var allProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var allZoneIds = dto.Items.SelectMany(i => new[] { i.SourceZoneId, i.DestinationZoneId }).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var allSectionIds = dto.Items.SelectMany(i => new[] { i.SourceSectionId, i.DestinationSectionId }).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

            var inventories = await _dbContext.Inventories
                .Where(inv => allWarehouseIds.Contains(inv.WarehouseId)
                              && allProductIds.Contains(inv.ProductId)
                              && (inv.ZoneId == null || allZoneIds.Contains(inv.ZoneId.Value))
                              && (inv.SectionId == null || allSectionIds.Contains(inv.SectionId.Value)))
                .ToListAsync(cancellationToken);

            foreach (var item in finalItems)
            {
                var sourceInventory = inventories.FirstOrDefault(i =>
                    i.WarehouseId == item.SourceWarehouseId &&
                    i.ZoneId == item.SourceZoneId &&
                    i.SectionId == item.SourceSectionId &&
                    i.ProductId == item.ProductId);

                var destinationInventory = inventories.FirstOrDefault(i =>
                    i.WarehouseId == item.DestinationWarehouseId &&
                    i.ZoneId == item.DestinationZoneId &&
                    i.SectionId == item.DestinationSectionId &&
                    i.ProductId == item.ProductId);

                var productName = productNames.ContainsKey(item.ProductId) ? productNames[item.ProductId] : "نامشخص";

                switch (dto.Type)
                {
                    case ReceiptOrIssueType.Receipt:
                        if (destinationInventory == null)
                            throw new InvalidOperationException($"موجودی مقصد برای کالای {productName} یافت نشد.");
                        destinationInventory.Quantity += item.Quantity;
                        break;
                    case ReceiptOrIssueType.Issue:
                        if (sourceInventory == null)
                            throw new InvalidOperationException("موجودی مبدأ یافت نشد.");
                        if (sourceInventory.Quantity < item.Quantity)
                            throw new InvalidOperationException($"موجودی کالا {productName} در انبار مبدأ کافی نیست.");
                        sourceInventory.Quantity -= item.Quantity;
                        break;
                    case ReceiptOrIssueType.Transfer:
                        if (sourceInventory == null || destinationInventory == null)
                            throw new InvalidOperationException("موجودی مبدأ یا مقصد یافت نشد.");
                        if (sourceInventory.Quantity < item.Quantity)
                            throw new InvalidOperationException($"موجودی کالا {productName} در انبار مبدأ کافی نیست.");
                        sourceInventory.Quantity -= item.Quantity;
                        destinationInventory.Quantity += item.Quantity;
                        break;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // بارگذاری عنوان پروژه از DbContext دیگر
            string? projectTitle = null;
            if (entity.ProjectId.HasValue)
            {
                projectTitle = await _projectContext.Projects
                    .Where(p => p.Id == entity.ProjectId.Value)
                    .Select(p => p.ProjectName)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return new ReceiptOrIssueDto
            {
                Id = entity.Id,
                Date = entity.Date,
                DocumentNumber = entity.DocumentNumber,
                Description = entity.Description,
                Type = entity.Type,
                ProjectId = entity.ProjectId,
                ProjectTitle = projectTitle,
                Items = finalItems.Select(i => new ReceiptOrIssueItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    SourceWarehouseId = i.SourceWarehouseId,
                    SourceZoneId = i.SourceZoneId,
                    SourceSectionId = i.SourceSectionId,
                    DestinationWarehouseId = i.DestinationWarehouseId,
                    DestinationZoneId = i.DestinationZoneId,
                    DestinationSectionId = i.DestinationSectionId,
                    ProductName = productNames.ContainsKey(i.ProductId) ? productNames[i.ProductId] : "نامشخص"
                }).ToList()
            };
        }





        public async Task<List<StorageSectionDto>> GetSectionsByWarehouseAsync(int warehouseId)
        {
            var sections = await _dbContext.StorageSections
                .Include(s => s.Zone)
                    .ThenInclude(z => z.Warehouse)
                .Where(s => s.Zone.WarehouseId == warehouseId)
                .ToListAsync();

            var result = sections.Select(s => new StorageSectionDto
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

            return result;
        }
        public List<SelectListItem> GetZonesByWarehouse(int warehouseId)
        {
            return _dbContext.StorageZones
                .Where(z => z.WarehouseId == warehouseId)
                .Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name
                }).ToList();
        }

        public List<SelectListItem> GetSectionsByZone(int zoneId)
        {
            return _dbContext.StorageSections
                .Where(s => s.ZoneId == zoneId)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();
        }
        public async Task<List<SelectListItem>> GetGroupsByCategoryAsync(int categoryId)
        {
            return await _dbContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetStatusesByGroupAsync(int groupId)
        {
            return await _dbContext.Statuses
                .Where(s => s.GroupId == groupId)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetProductsByStatus(int statusId)
        {
            return await _dbContext.Products
                .Where(p => p.StatusId == statusId)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToListAsync();
        }



        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null) return false;

            // استخراج شناسه‌های مورد نیاز برای بارگذاری بخش‌ها و انبارها
            var sourceSectionIds = entity.Items
                .Where(i => i.SourceSectionId.HasValue)
                .Select(i => i.SourceSectionId!.Value)
                .Distinct()
                .ToList();

            var sourceZoneIds = entity.Items
                .Where(i => !i.SourceSectionId.HasValue && i.SourceZoneId.HasValue)
                .Select(i => i.SourceZoneId!.Value)
                .Distinct()
                .ToList();

            var destinationSectionIds = entity.Items
                .Where(i => i.DestinationSectionId.HasValue)
                .Select(i => i.DestinationSectionId!.Value)
                .Distinct()
                .ToList();

            var destinationZoneIds = entity.Items
                .Where(i => !i.DestinationSectionId.HasValue && i.DestinationZoneId.HasValue)
                .Select(i => i.DestinationZoneId!.Value)
                .Distinct()
                .ToList();

            var sourceSections = await _dbContext.StorageSections
                .Include(s => s.Zone)
                .ThenInclude(z => z!.Warehouse)
                .Where(s => sourceSectionIds.Contains(s.Id) || sourceZoneIds.Contains(s.ZoneId))
                .ToListAsync(cancellationToken);

            var destinationSections = await _dbContext.StorageSections
                .Include(s => s.Zone)
                .ThenInclude(z => z!.Warehouse)
                .Where(s => destinationSectionIds.Contains(s.Id) || destinationZoneIds.Contains(s.ZoneId))
                .ToListAsync(cancellationToken);

            var allWarehouseIds = entity.Items
                .SelectMany(i => new[] { i.SourceWarehouseId, i.DestinationWarehouseId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var allProductIds = entity.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var allZoneIds = entity.Items
                .SelectMany(i => new[] { i.SourceZoneId, i.DestinationZoneId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var allSectionIds = entity.Items
                .SelectMany(i => new[] { i.SourceSectionId, i.DestinationSectionId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var inventories = await _dbContext.Inventories
                .Where(inv => allWarehouseIds.Contains(inv.WarehouseId) &&
                              allProductIds.Contains(inv.ProductId) &&
                              (inv.ZoneId == null || allZoneIds.Contains(inv.ZoneId.Value)) &&
                              (inv.SectionId == null || allSectionIds.Contains(inv.SectionId.Value)))
                .ToListAsync(cancellationToken);

            foreach (var item in entity.Items)
            {
                var sourceInventory = inventories.FirstOrDefault(i =>
                    i.WarehouseId == item.SourceWarehouseId &&
                    i.ZoneId == item.SourceZoneId &&
                    i.SectionId == item.SourceSectionId &&
                    i.ProductId == item.ProductId);

                var destinationInventory = inventories.FirstOrDefault(i =>
                    i.WarehouseId == item.DestinationWarehouseId &&
                    i.ZoneId == item.DestinationZoneId &&
                    i.SectionId == item.DestinationSectionId &&
                    i.ProductId == item.ProductId);

                // برگرداندن تغییرات موجودی — عکس عملیات Create
                switch (entity.Type)
                {
                    case ReceiptOrIssueType.Receipt:
                        // رسید یعنی قبلاً کالا به مقصد اضافه شده؛ پس الان باید موجودی مقصد رو کم کنیم و موجودی مبدأ رو زیاد کنیم
                        if (destinationInventory != null)
                        {
                            destinationInventory.Quantity -= item.Quantity;
                            if (destinationInventory.Quantity < 0)
                                throw new InvalidOperationException($"موجودی کالا {item.ProductId} در انبار مقصد نمی‌تواند منفی شود.");
                        }

                        if (sourceInventory != null)
                        {
                            sourceInventory.Quantity += item.Quantity;
                        }
                        break;

                    case ReceiptOrIssueType.Issue:
                        // حواله یعنی قبلاً کالا از مبدأ کم شده؛ پس الان باید موجودی مبدأ رو زیاد کنیم و موجودی مقصد رو کم کنیم
                        if (sourceInventory != null)
                        {
                            sourceInventory.Quantity += item.Quantity;
                        }

                        if (destinationInventory != null)
                        {
                            destinationInventory.Quantity -= item.Quantity;
                            if (destinationInventory.Quantity < 0)
                                throw new InvalidOperationException($"موجودی کالا {item.ProductId} در انبار مقصد نمی‌تواند منفی شود.");
                        }
                        break;

                    case ReceiptOrIssueType.Transfer:
                        // انتقال — مثل موارد بالا موجودی‌ها برعکس تعدیل می‌شوند
                        if (destinationInventory != null)
                        {
                            destinationInventory.Quantity -= item.Quantity;
                            if (destinationInventory.Quantity < 0)
                                throw new InvalidOperationException($"موجودی کالا {item.ProductId} در انبار مقصد نمی‌تواند منفی شود.");
                        }

                        if (sourceInventory != null)
                        {
                            sourceInventory.Quantity += item.Quantity;
                        }
                        break;
                }
            }

            // حذف اقلام و خود سند
            _dbContext.ReceiptOrIssueItems.RemoveRange(entity.Items);
            _dbContext.ReceiptOrIssues.Remove(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

    }
}
