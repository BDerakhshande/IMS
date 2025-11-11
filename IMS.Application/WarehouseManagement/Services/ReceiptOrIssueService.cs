using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.Service;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.ProjectManagement.Entities;
using IMS.Domain.WarehouseManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IMS.Application.WarehouseManagement.Services
{
    public class ReceiptOrIssueService: IReceiptOrIssueService
    {
        private IWarehouseDbContext _dbContext;
        private readonly IApplicationDbContext _projectContext;
        private readonly IProcurementManagementDbContext _procurementContext;
    

        public ReceiptOrIssueService(IWarehouseDbContext warehouseDbContext, IApplicationDbContext projectContext , IProcurementManagementDbContext procurementContext)
        {
            _dbContext = warehouseDbContext;
            _projectContext = projectContext;
            _procurementContext = procurementContext;
        }
        public async Task<ReceiptOrIssueDto?> GetByIdAsync(int id)
        {
            var entity = await _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                    .ThenInclude(i => i.UniqueCodes)
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

            if (entity == null) return null;

            // جمع‌آوری شناسه‌ها
            var projectIds = entity.Items
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            var purchaseRequestIds = entity.Items
                .Where(i => i.PurchaseRequestId.HasValue)
                .Select(i => i.PurchaseRequestId!.Value)
                .Distinct()
                .ToList();

            // بارگذاری پروژه‌ها و درخواست‌ها با IDs مشخص
            var projectMap = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

            var purchaseRequestMap = await _procurementContext.PurchaseRequests
                .Where(pr => purchaseRequestIds.Contains(pr.Id))
                .ToDictionaryAsync(pr => pr.Id, pr => pr.Title);

            var dto = new ReceiptOrIssueDto
            {
                Id = entity.Id,
                Type = entity.Type,
                Date = entity.Date,
                DocumentNumber = entity.DocumentNumber,
                Description = entity.Description,
                Items = entity.Items.Select(i => new ReceiptOrIssueItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,

                    SourceWarehouseId = i.SourceWarehouseId,
                    SourceZoneId = i.SourceZoneId,
                    SourceSectionId = i.SourceSectionId,
                    SourceSectionName = i.SourceSection?.Name,
                    SourceZoneName = i.SourceSection?.Zone?.Name,
                    SourceWarehouseName = i.SourceSection?.Zone?.Warehouse?.Name ?? i.SourceWarehouse?.Name,

                    DestinationWarehouseId = i.DestinationWarehouseId,
                    DestinationZoneId = i.DestinationZoneId,
                    DestinationSectionId = i.DestinationSectionId,
                    DestinationSectionName = i.DestinationSection?.Name,
                    DestinationZoneName = i.DestinationSection?.Zone?.Name,
                    DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name ?? i.DestinationWarehouse?.Name,

                    CategoryId = i.CategoryId,
                    CategoryName = i.Category?.Name,
                    GroupId = i.GroupId,
                    GroupName = i.Group?.Name,
                    StatusId = i.StatusId,
                    StatusName = i.Status?.Name,
                    ProductName = i.Product?.Name,

                    ProjectId = i.ProjectId,
                    ProjectTitle = i.ProjectId.HasValue ? projectMap.GetValueOrDefault(i.ProjectId.Value) : null,

                    PurchaseRequestId = i.PurchaseRequestId,
                    PurchaseRequestTitle = i.PurchaseRequestId.HasValue ? purchaseRequestMap.GetValueOrDefault(i.PurchaseRequestId.Value) : null,

                    UniqueCodes = i.UniqueCodes.Select(uc => uc.UniqueCode).ToList(),
                    SelectedUniqueCode = i.UniqueCodes.FirstOrDefault()?.UniqueCode
                }).ToList()
            };

            return dto;
        }



        public async Task<List<ReceiptOrIssueDto>> GetAllAsync(int? warehouseId = null)
        {
           
            var query = _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                    .ThenInclude(i => i.UniqueCodes)
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

            var list = await query
                .OrderByDescending(r => r.Date)
                .ToListAsync();

          
            var projectIds = list
                .SelectMany(r => r.Items)
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            var projects = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

          
            var purchaseRequestIds = list
                .SelectMany(r => r.Items)
                .Where(i => i.PurchaseRequestId.HasValue)
                .Select(i => i.PurchaseRequestId!.Value)
                .Distinct()
                .ToList();

            var purchaseRequests = await _procurementContext.PurchaseRequests
                .Where(pr => purchaseRequestIds.Contains(pr.Id))
                .ToDictionaryAsync(pr => pr.Id, pr => pr.Title);

    
            var result = list.Select(entity => new ReceiptOrIssueDto
            {
                Id = entity.Id,
                Type = entity.Type,
                Date = entity.Date,
                DocumentNumber = entity.DocumentNumber,
                Description = entity.Description,

                Items = entity.Items.Select(i => new ReceiptOrIssueItemDto
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
                    SourceSectionName = i.SourceSection?.Name,
                    SourceZoneName = i.SourceSection?.Zone?.Name,
                    SourceWarehouseName = i.SourceSection?.Zone?.Warehouse?.Name ?? i.SourceWarehouse?.Name,
                    DestinationWarehouseId = i.DestinationWarehouseId,
                    DestinationZoneId = i.DestinationZoneId,
                    DestinationSectionId = i.DestinationSectionId,
                    DestinationSectionName = i.DestinationSection?.Name,
                    DestinationZoneName = i.DestinationSection?.Zone?.Name,
                    DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name ?? i.DestinationWarehouse?.Name,
                    ProjectId = i.ProjectId,
                    ProjectTitle = i.ProjectId.HasValue && projects.ContainsKey(i.ProjectId.Value)
                        ? projects[i.ProjectId.Value]
                        : null,
                    PurchaseRequestId = i.PurchaseRequestId,
                    PurchaseRequestTitle = i.PurchaseRequestId.HasValue && purchaseRequests.ContainsKey(i.PurchaseRequestId.Value)
                        ? purchaseRequests[i.PurchaseRequestId.Value]
                        : null,

        
                    UniqueCodes = i.UniqueCodes.Select(uc => uc.UniqueCode).ToList(),

                   
                    SelectedUniqueCode = i.UniqueCodes.FirstOrDefault()?.UniqueCode
                }).ToList()
            }).ToList();

            return result;
        }

        public async Task<(ReceiptOrIssueDto? Result, List<string> Errors)> CreateAsync(
       ReceiptOrIssueDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Items collection cannot be empty.");

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var requestIds = dto.Items.Select(i => i.PurchaseRequestId).Distinct().ToList();

            var productNames = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            var purchaseRequestItems = await _procurementContext.PurchaseRequestItems
                .Include(pri => pri.PurchaseRequest)
                .Where(pri => productIds.Contains(pri.ProductId) && requestIds.Contains(pri.PurchaseRequestId))
                .ToListAsync(cancellationToken);

            var requestNumbers = purchaseRequestItems
                .Select(pri => pri.PurchaseRequest?.RequestNumber)
                .Where(rn => !string.IsNullOrWhiteSpace(rn))
                .Distinct()
                .ToList();

            var flatItems = await _procurementContext.PurchaseRequestFlatItems
                .Where(f => productIds.Contains(f.ProductId) && requestNumbers.Contains(f.RequestNumber))
                .ToListAsync(cancellationToken);

            var errors = new List<string>();
            var stoppedProducts = new HashSet<int>();

            foreach (var item in dto.Items)
            {
                var productName = productNames.GetValueOrDefault(item.ProductId, "نامشخص");

                if (item.ProductId <= 0)
                    errors.Add($"شناسه کالا معتبر نیست.");

                if ((item.UniqueCodes == null || !item.UniqueCodes.Any()) && item.Quantity <= 0)
                    errors.Add($"تعداد برای کالا {productName} باید بیشتر از صفر باشد.");

                var purchaseRequestItem = purchaseRequestItems
                    .FirstOrDefault(pri => pri.ProductId == item.ProductId && pri.PurchaseRequestId == item.PurchaseRequestId);

                string? requestNumber = purchaseRequestItem?.PurchaseRequest?.RequestNumber;

                if (purchaseRequestItem != null && purchaseRequestItem.IsSupplyStopped && dto.Type == ReceiptOrIssueType.Receipt)
                {
                    if (!stoppedProducts.Contains(item.ProductId))
                    {
                        stoppedProducts.Add(item.ProductId);
                        errors.Add($"آیتم {productName} متوقف شده است و امکان رسید ندارد.");
                    }
                }

                if (dto.Type == ReceiptOrIssueType.Receipt && purchaseRequestItem != null)
                {
                    var isInFlatItems = flatItems.Any(f => f.ProductId == item.ProductId && f.RequestNumber == requestNumber);
                    if (!isInFlatItems)
                        errors.Add($"برای {productName} کالا هیچ درخواست خریدی ثبت نشده است.");
                }
            }

            if (errors.Any())
                return (null, errors);

            var sourceSectionIds = dto.Items.Where(i => i.SourceSectionId.HasValue).Select(i => i.SourceSectionId!.Value).Distinct().ToList();
            var sourceZoneIds = dto.Items.Where(i => !i.SourceSectionId.HasValue && i.SourceZoneId.HasValue).Select(i => i.SourceZoneId!.Value).Distinct().ToList();
            var destinationSectionIds = dto.Items.Where(i => i.DestinationSectionId.HasValue).Select(i => i.DestinationSectionId!.Value).Distinct().ToList();
            var destinationZoneIds = dto.Items.Where(i => !i.DestinationSectionId.HasValue && i.DestinationZoneId.HasValue).Select(i => i.DestinationZoneId!.Value).Distinct().ToList();

            var sourceSections = await _dbContext.StorageSections
                .Include(s => s.Zone).ThenInclude(z => z!.Warehouse)
                .Where(s => sourceSectionIds.Contains(s.Id) || sourceZoneIds.Contains(s.ZoneId))
                .ToListAsync(cancellationToken);

            var destinationSections = await _dbContext.StorageSections
                .Include(s => s.Zone).ThenInclude(z => z!.Warehouse)
                .Where(s => destinationSectionIds.Contains(s.Id) || destinationZoneIds.Contains(s.ZoneId))
                .ToListAsync(cancellationToken);

            var entity = new ReceiptOrIssue
            {
                Date = dto.Date,
                DocumentNumber = dto.DocumentNumber,
                Description = dto.Description,
                Type = dto.Type,
                Items = new List<ReceiptOrIssueItem>()
            };

            var entityItemMap = new Dictionary<int, ReceiptOrIssueItem>();

            for (int idx = 0; idx < dto.Items.Count; idx++)
            {
                var itemDto = dto.Items[idx];
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
                    ProjectId = itemDto.ProjectId,
                    PurchaseRequestId = itemDto.PurchaseRequestId,
                    UniqueCodes = new List<ReceiptOrIssueItemUniqueCode>()
                };

                entity.Items.Add(newItem);
                entityItemMap[idx] = newItem;
            }

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

            var inventoryItems = await _dbContext.InventoryItems
                .Include(ii => ii.Inventory)
                .Where(ii => allWarehouseIds.Contains(ii.Inventory.WarehouseId)
                             && allProductIds.Contains(ii.Inventory.ProductId)
                             && (ii.Inventory.ZoneId == null || allZoneIds.Contains(ii.Inventory.ZoneId.Value))
                             && (ii.Inventory.SectionId == null || allSectionIds.Contains(ii.Inventory.SectionId.Value)))
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

            for (int idx = 0; idx < dto.Items.Count; idx++)
            {
                var dtoItem = dto.Items[idx];
                var item = entityItemMap[idx];
                var productName = productNames.GetValueOrDefault(item.ProductId, "نامشخص");

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

                if (destinationInventory == null)
                {
                    destinationInventory = new Inventory
                    {
                        WarehouseId = item.DestinationWarehouseId!.Value,
                        ZoneId = item.DestinationZoneId,
                        SectionId = item.DestinationSectionId,
                        ProductId = item.ProductId,
                        Quantity = 0
                    };
                    _dbContext.Inventories.Add(destinationInventory);
                    inventories.Add(destinationInventory);
                }

                decimal quantityToMove = 0;

                if (dtoItem.UniqueCodes != null && dtoItem.UniqueCodes.Any(uc => !string.IsNullOrWhiteSpace(uc)))
                {
                    var key = new
                    {
                        WarehouseId = item.SourceWarehouseId!.Value,
                        ZoneId = item.SourceZoneId,
                        SectionId = item.SourceSectionId,
                        ProductId = item.ProductId
                    };

                    var availableUniqueItems = inventoryItemsMap.ContainsKey(key)
                        ? inventoryItemsMap[key]
                        : new List<InventoryItem>();

                    foreach (var uniqueCode in dtoItem.UniqueCodes)
                    {
                        var matchingInvItem = availableUniqueItems.FirstOrDefault(ii => ii.UniqueCode == uniqueCode);
                        if (matchingInvItem == null)
                        {
                            errors.Add($"کد یکتا '{uniqueCode}' برای کالا {productName} در انبار مبدأ یافت نشد.");
                            continue;
                        }

                        item.UniqueCodes.Add(new ReceiptOrIssueItemUniqueCode { UniqueCode = uniqueCode });
                        availableUniqueItems.Remove(matchingInvItem);

                        // انتقال واقعی
                        matchingInvItem.InventoryId = destinationInventory.Id;
                        _dbContext.InventoryItems.Update(matchingInvItem);

                        // کاهش موجودی انبار مبدأ برای هر کد یکتا
                        if (sourceInventory != null)
                            sourceInventory.Quantity -= 1;

                        var productItem = await _dbContext.ProductItems
                            .FirstOrDefaultAsync(pi => pi.UniqueCode == uniqueCode);

                        if (productItem != null && productItem.ProjectId.HasValue)
                            item.ProjectId = productItem.ProjectId;
                    }

                    quantityToMove = item.UniqueCodes.Count;
                }
                // بلوک انتقال کالاهای بدون کد یکتا
                else
                {
                    var warehouseId = item.SourceWarehouseId ?? -1;
                    var zoneId = item.SourceZoneId;
                    var sectionId = item.SourceSectionId;
                    var productId = item.ProductId;

                    // دریافت همه رکوردهای Inventory مرتبط با کالا در انبار مبدأ
                    var inventoriesInSource = _dbContext.Inventories
                        .Where(inv => inv.WarehouseId == warehouseId &&
                                      inv.ZoneId == zoneId &&
                                      inv.SectionId == sectionId &&
                                      inv.ProductId == productId)
                        .ToList();

                    if (!inventoriesInSource.Any())
                    {
                        errors.Add($"کالای {productName} در انبار مبدأ موجود نیست.");
                        continue; // ❌ به جای return، ادامه پردازش سایر آیتم‌ها
                    }

                    // محاسبه موجودی واقعی عمومی
                    decimal totalNonUniqueQuantity = inventoriesInSource
                        .Sum(inv => inv.Quantity - inv.InventoryItems.Count); // هر InventoryItem = 1 واحد یکتا

                    // بررسی سه حالت
                    if (totalNonUniqueQuantity <= 0)
                    {
                        errors.Add($"کالای {productName} فقط به‌صورت کد یکتا در انبار مبدأ موجود است. انتقال بدون انتخاب کد یکتا ممکن نیست.");
                        continue;
                    }

                    if (totalNonUniqueQuantity < dtoItem.Quantity)
                    {
                        errors.Add($"موجودی کالای عمومی {productName} کافی نیست (موجودی عمومی: {totalNonUniqueQuantity}). بخشی از موجودی این کالا دارای کد یکتا است و باید انتخاب شود.");
                        continue;
                    }

                    // انتقال کالاهای عمومی
                    var remainingToTransfer = dtoItem.Quantity;

                    foreach (var inv in inventoriesInSource)
                    {
                        var nonUniqueInInventory = inv.Quantity - inv.InventoryItems.Count;
                        if (nonUniqueInInventory <= 0) continue;

                        var transferable = Math.Min(nonUniqueInInventory, remainingToTransfer);

                        // کاهش موجودی در رکورد مبدا
                        inv.Quantity -= transferable;

                        // افزایش موجودی در انبار مقصد
                        destinationInventory.Quantity += transferable;

                        _dbContext.Inventories.Update(inv);

                        remainingToTransfer -= transferable;
                        if (remainingToTransfer <= 0) break;
                    }

                    quantityToMove = dtoItem.Quantity;
                    _dbContext.Inventories.Update(destinationInventory);

                    // به‌روزرسانی مقدار منتقل شده در آیتم اصلی
                    dtoItem.Quantity = quantityToMove;

                  
                }

                item.Quantity = quantityToMove;

                if (sourceInventory != null && sourceInventory.Quantity < 0)
                    errors.Add($"موجودی کالا {productName} در انبار مبدأ کافی نیست.");
                if (destinationInventory.Quantity < 0)
                    errors.Add($"موجودی کالا {productName} در انبار مقصد به کمتر از صفر رسید.");
            }


            if (errors.Any())
                return (null, errors);

            using var transaction = await (_dbContext as DbContext)!.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _dbContext.ReceiptOrIssues.Add(entity);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await _procurementContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                errors.Add($"خطا در ذخیره تغییرات: {ex.Message}");
                return (null, errors);
            }

            var projectIds = entity.Items
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            var projectTitles = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName, cancellationToken);

            var resultDto = new ReceiptOrIssueDto
            {
                Id = entity.Id,
                Date = entity.Date,
                DocumentNumber = entity.DocumentNumber,
                Description = entity.Description,
                Type = entity.Type,
                Items = entity.Items.Select(i =>
                {
                    var inventory = inventories.FirstOrDefault(inv =>
                        inv.WarehouseId == i.SourceWarehouseId &&
                        inv.ZoneId == i.SourceZoneId &&
                        inv.SectionId == i.SourceSectionId &&
                        inv.ProductId == i.ProductId);

                    return new ReceiptOrIssueItemDto
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
                        SourceWarehouseName = inventory?.Warehouse?.Name,
                        SourceZoneName = inventory?.Zone?.Name,
                        SourceSectionName = inventory?.Section?.Name,
                        DestinationWarehouseId = i.DestinationWarehouseId,
                        DestinationZoneId = i.DestinationZoneId,
                        DestinationSectionId = i.DestinationSectionId,
                        DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name,
                        DestinationZoneName = i.DestinationSection?.Zone?.Name,
                        DestinationSectionName = i.DestinationSection?.Name,
                        ProductName = productNames.ContainsKey(i.ProductId) ? productNames[i.ProductId] : "نامشخص",
                        UniqueCodes = i.UniqueCodes.Select(uc => uc.UniqueCode).ToList(),
                        ProjectId = i.ProjectId,
                        ProjectTitle = i.ProjectId.HasValue && projectTitles.ContainsKey(i.ProjectId.Value)
                            ? projectTitles[i.ProjectId.Value]
                            : null,
                        PurchaseRequestId = i.PurchaseRequestId,
                        SelectedUniqueCode = i.UniqueCodes.FirstOrDefault()?.UniqueCode
                    };
                }).ToList()
            };

            return (resultDto, new List<string>());
        }

        public async Task<(ReceiptOrIssue? entity, List<string> errors)> UpdateAsync(int id, ReceiptOrIssueDto dto, CancellationToken cancellationToken)
        {
            var dbContext = _dbContext as DbContext ?? throw new InvalidOperationException("DbContext required for transaction");
            using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var errors = new List<string>();

            var entity = await _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                    .ThenInclude(i => i.UniqueCodes)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null)
                return (null, new List<string> { "سند مورد نظر یافت نشد." });

            // بارگذاری داده‌های مورد نیاز مشابه CreateAsync
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var requestIds = dto.Items.Select(i => i.PurchaseRequestId).Distinct().ToList();

            var productNames = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            var purchaseRequestItems = await _procurementContext.PurchaseRequestItems
                .Include(pri => pri.PurchaseRequest)
                .Where(pri => productIds.Contains(pri.ProductId) && requestIds.Contains(pri.PurchaseRequestId))
                .ToListAsync(cancellationToken);

            var requestNumbers = purchaseRequestItems
                .Select(pri => pri.PurchaseRequest?.RequestNumber)
                .Where(rn => !string.IsNullOrWhiteSpace(rn))
                .Distinct()
                .ToList();

            var flatItems = await _procurementContext.PurchaseRequestFlatItems
                .Where(f => productIds.Contains(f.ProductId) && requestNumbers.Contains(f.RequestNumber))
                .ToListAsync(cancellationToken);

            var inventoryItems = await _dbContext.InventoryItems
                .Include(ii => ii.Inventory)
                .Where(ii => productIds.Contains(ii.Inventory.ProductId))
                .ToListAsync(cancellationToken);

            var inventories = inventoryItems.Select(ii => ii.Inventory).Distinct().ToList();
            var inventoryItemsMap = inventoryItems
                .GroupBy(ii => new
                {
                    ii.Inventory.WarehouseId,
                    ii.Inventory.ZoneId,
                    ii.Inventory.SectionId,
                    ii.Inventory.ProductId
                })
                .ToDictionary(g => g.Key, g => g.ToList());

            // اعتبارسنجی مشابه CreateAsync
            var stoppedProducts = new HashSet<int>();
            foreach (var item in dto.Items)
            {
                var productName = productNames.GetValueOrDefault(item.ProductId, "نامشخص");

                if (item.ProductId <= 0)
                    errors.Add($"شناسه کالا معتبر نیست.");

                var pri = purchaseRequestItems.FirstOrDefault(pr => pr.ProductId == item.ProductId && pr.PurchaseRequestId == item.PurchaseRequestId);
                var requestNumber = pri?.PurchaseRequest?.RequestNumber;

                if (pri != null && pri.IsSupplyStopped && dto.Type == ReceiptOrIssueType.Receipt)
                {
                    if (!stoppedProducts.Contains(item.ProductId))
                    {
                        stoppedProducts.Add(item.ProductId);
                        errors.Add($"آیتم {productName} متوقف شده است و امکان رسید ندارد.");
                    }
                }

                if (dto.Type == ReceiptOrIssueType.Receipt && pri != null)
                {
                    if (!flatItems.Any(f => f.ProductId == item.ProductId && f.RequestNumber == requestNumber))
                        errors.Add($"برای {productName} هیچ درخواست خریدی ثبت نشده است.");
                }

                var hasUniqueInInventory = inventoryItems.Any(ii =>
                    ii.Inventory.WarehouseId == item.SourceWarehouseId &&
                    ii.Inventory.ZoneId == item.SourceZoneId &&
                    ii.Inventory.SectionId == item.SourceSectionId &&
                    ii.Inventory.ProductId == item.ProductId &&
                    !string.IsNullOrEmpty(ii.UniqueCode));

                if ((item.UniqueCodes == null || !item.UniqueCodes.Any()) && item.Quantity <= 0 && !hasUniqueInInventory)
                    errors.Add($"تعداد برای کالا {productName} باید بیشتر از صفر باشد.");
            }

            if (errors.Any())
                return (null, errors);

            // ------------------ مدیریت آیتم‌ها ------------------
            var existingItemsMap = entity.Items.ToDictionary(i => i.Id);

            foreach (var dtoItem in dto.Items)
            {
                ReceiptOrIssueItem? item;
                bool isNew = false;

                if (dtoItem.Id > 0 && existingItemsMap.TryGetValue(dtoItem.Id, out item))
                {
                    // آیتم موجود → بروزرسانی
                    item.Quantity = dtoItem.Quantity; // مقدار اصلی حفظ می‌شود
                    item.SourceWarehouseId = dtoItem.SourceWarehouseId;
                    item.SourceZoneId = dtoItem.SourceZoneId;
                    item.SourceSectionId = dtoItem.SourceSectionId;
                    item.DestinationWarehouseId = dtoItem.DestinationWarehouseId;
                    item.DestinationZoneId = dtoItem.DestinationZoneId;
                    item.DestinationSectionId = dtoItem.DestinationSectionId;
                    item.ProjectId = dtoItem.ProjectId;
                    item.PurchaseRequestId = dtoItem.PurchaseRequestId;
                }
                else
                {
                    // آیتم جدید → ایجاد
                    item = new ReceiptOrIssueItem
                    {
                        CategoryId = dtoItem.CategoryId,
                        GroupId = dtoItem.GroupId,
                        StatusId = dtoItem.StatusId,
                        ProductId = dtoItem.ProductId,
                        Quantity = dtoItem.Quantity, // مقدار اصلی DTO
                        SourceWarehouseId = dtoItem.SourceWarehouseId,
                        SourceZoneId = dtoItem.SourceZoneId,
                        SourceSectionId = dtoItem.SourceSectionId,
                        DestinationWarehouseId = dtoItem.DestinationWarehouseId,
                        DestinationZoneId = dtoItem.DestinationZoneId,
                        DestinationSectionId = dtoItem.DestinationSectionId,
                        ProjectId = dtoItem.ProjectId,
                        PurchaseRequestId = dtoItem.PurchaseRequestId,
                        UniqueCodes = new List<ReceiptOrIssueItemUniqueCode>()
                    };
                    entity.Items.Add(item);
                    isNew = true;
                }

                // مدیریت کدهای یکتا
                if (dtoItem.UniqueCodes != null && dtoItem.UniqueCodes.Any(uc => !string.IsNullOrWhiteSpace(uc)))
                {
                    var key = new
                    {
                        WarehouseId = item.SourceWarehouseId!.Value,
                        ZoneId = item.SourceZoneId,
                        SectionId = item.SourceSectionId,
                        ProductId = item.ProductId
                    };

                    var availableUniqueItems = inventoryItemsMap.ContainsKey(key) ? inventoryItemsMap[key] : new List<InventoryItem>();

                    // پیدا کردن یا ایجاد انبار مقصد
                    var destinationInventory = inventories.FirstOrDefault(inv =>
                        inv.WarehouseId == item.DestinationWarehouseId &&
                        inv.ZoneId == item.DestinationZoneId &&
                        inv.SectionId == item.DestinationSectionId &&
                        inv.ProductId == item.ProductId);

                    if (destinationInventory == null)
                    {
                        destinationInventory = new Inventory
                        {
                            WarehouseId = item.DestinationWarehouseId!.Value,
                            ZoneId = item.DestinationZoneId,
                            SectionId = item.DestinationSectionId,
                            ProductId = item.ProductId,
                            Quantity = 0 // موجودی اولیه صفر است، فقط کد یکتا‌ها اضافه می‌شوند
                        };
                        _dbContext.Inventories.Add(destinationInventory);
                        await _dbContext.SaveChangesAsync(CancellationToken.None);
                        inventories.Add(destinationInventory);
                    }

                    foreach (var uniqueCode in dtoItem.UniqueCodes)
                    {
                        var matchingInvItem = availableUniqueItems.FirstOrDefault(ii => ii.UniqueCode == uniqueCode);
                        if (matchingInvItem == null)
                        {
                            errors.Add($"کد یکتا '{uniqueCode}' برای کالا {productNames.GetValueOrDefault(item.ProductId, "نامشخص")} در انبار مبدأ یافت نشد.");
                            continue;
                        }

                        if (!item.UniqueCodes.Any(uc => uc.UniqueCode == uniqueCode))
                            item.UniqueCodes.Add(new ReceiptOrIssueItemUniqueCode { UniqueCode = uniqueCode });

                        // انتقال واقعی به انبار مقصد
                        matchingInvItem.InventoryId = destinationInventory.Id;
                        _dbContext.InventoryItems.Update(matchingInvItem);

                        // کاهش موجودی انبار مبدأ
                        var sourceInventory = inventories.FirstOrDefault(inv =>
                            inv.WarehouseId == item.SourceWarehouseId &&
                            inv.ZoneId == item.SourceZoneId &&
                            inv.SectionId == item.SourceSectionId &&
                            inv.ProductId == item.ProductId);

                        if (sourceInventory != null)
                            sourceInventory.Quantity -= 1;

                        // افزایش موجودی انبار مقصد
                        destinationInventory.Quantity += 1;

                        availableUniqueItems.Remove(matchingInvItem);
                    }
                }
                else
                {
                    // اگر هیچ کد یکتایی نبود و انبار مقصد وجود نداشت، مقدار Quantity آیتم را به عنوان موجودی اولیه انبار مقصد هم ست کنید
                    var destinationInventory = inventories.FirstOrDefault(inv =>
                        inv.WarehouseId == item.DestinationWarehouseId &&
                        inv.ZoneId == item.DestinationZoneId &&
                        inv.SectionId == item.DestinationSectionId &&
                        inv.ProductId == item.ProductId);

                    if (destinationInventory == null && item.DestinationWarehouseId.HasValue)
                    {
                        destinationInventory = new Inventory
                        {
                            WarehouseId = item.DestinationWarehouseId.Value,
                            ZoneId = item.DestinationZoneId,
                            SectionId = item.DestinationSectionId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity // موجودی اولیه برابر Quantity آیتم
                        };
                        _dbContext.Inventories.Add(destinationInventory);
                        await _dbContext.SaveChangesAsync(CancellationToken.None);
                        inventories.Add(destinationInventory);
                    }
                }
            }

            // حذف آیتم‌های حذف شده
            var dtoItemIds = dto.Items.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
            var itemsToRemove = entity.Items.Where(i => i.Id > 0 && !dtoItemIds.Contains(i.Id)).ToList();

            foreach (var rem in itemsToRemove)
            {
                entity.Items.Remove(rem);
                _dbContext.ReceiptOrIssueItems.Remove(rem);
            }

            if (errors.Any())
                return (null, errors);

            // ذخیره تغییرات در تراکنش
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _procurementContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                errors.Add($"خطا در ذخیره تغییرات: {ex.Message}");
                return (null, errors);
            }

            return (entity, errors);
        }


        public async Task<List<StorageSectionDto>> GetSectionsByWarehouseAsync(int warehouseId)
        {
            var sections = await _dbContext.StorageSections
                .Include(s => s.Zone)
                    .ThenInclude(z => z.Warehouse)
                .Where(s => s.Zone.WarehouseId == warehouseId)
                .ToListAsync();

            var result = await _dbContext.StorageSections
    .Where(s => s.Zone.WarehouseId == warehouseId)
    .Select(s => new StorageSectionDto
    {
        Id = s.Id,
        Name = s.Name,
        SectionCode = s.SectionCode,
        ZoneId = s.ZoneId,
        Dimensions = s.Dimensions,
        ZoneCode = s.Zone!.ZoneCode,
        WarehouseCode = s.Zone.Warehouse!.Code
    })
    .ToListAsync();

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
                .ThenInclude(i => i.UniqueCodes) // بارگذاری کدهای یکتا
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null) return false;

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

            var inventoryItems = await _dbContext.InventoryItems
                .Where(ii => allProductIds.Contains(ii.Inventory.ProductId) &&
                             allWarehouseIds.Contains(ii.Inventory.WarehouseId))
                .ToListAsync(cancellationToken);

            using var transaction = await (_dbContext as DbContext)!.Database.BeginTransactionAsync(cancellationToken);


            try
            {
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

                    // بازگرداندن موجودی‌ها — عکس عملیات Create
                    switch (entity.Type)
                    {
                        case ReceiptOrIssueType.Receipt:
                        case ReceiptOrIssueType.Transfer:
                            if (destinationInventory != null)
                            {
                                destinationInventory.Quantity -= item.Quantity;
                                if (destinationInventory.Quantity < 0)
                                    throw new InvalidOperationException($"موجودی کالا {item.ProductId} در انبار مقصد نمی‌تواند منفی شود.");
                            }
                            if (sourceInventory != null)
                                sourceInventory.Quantity += item.Quantity;
                            break;

                        case ReceiptOrIssueType.Issue:
                            if (sourceInventory != null)
                                sourceInventory.Quantity += item.Quantity;
                            if (destinationInventory != null)
                            {
                                destinationInventory.Quantity -= item.Quantity;
                                if (destinationInventory.Quantity < 0)
                                    throw new InvalidOperationException($"موجودی کالا {item.ProductId} در انبار مقصد نمی‌تواند منفی شود.");
                            }
                            break;
                    }

                    // بازگرداندن کدهای یکتا به انبار مبدأ
                    foreach (var uc in item.UniqueCodes)
                    {
                        var invItem = inventoryItems.FirstOrDefault(ii => ii.UniqueCode == uc.UniqueCode);
                        if (invItem != null)
                        {
                            invItem.InventoryId = sourceInventory?.Id ?? invItem.InventoryId;
                            _dbContext.InventoryItems.Update(invItem);
                        }
                    }
                }

                // حذف اقلام و خود سند
                _dbContext.ReceiptOrIssueItems.RemoveRange(entity.Items);
                _dbContext.ReceiptOrIssues.Remove(entity);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }


    }
}
