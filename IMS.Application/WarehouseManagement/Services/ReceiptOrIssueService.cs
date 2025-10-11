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

          
            var projectMap = await _projectContext.Projects
                .Where(p => entity.Items.Any(i => i.ProjectId == p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

          
            var purchaseRequestMap = await _procurementContext.PurchaseRequests
                .Where(pr => entity.Items.Any(i => i.PurchaseRequestId == pr.Id))
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

                if (item.Quantity <= 0)
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

                if (!item.SourceWarehouseId.HasValue) continue;

                var key = new
                {
                    WarehouseId = item.SourceWarehouseId.Value,
                    ZoneId = item.SourceZoneId,
                    SectionId = item.SourceSectionId,
                    ProductId = item.ProductId
                };

                var productName = productNames.GetValueOrDefault(item.ProductId, "نامشخص");

                if (dtoItem.UniqueCodes?.Any() == true)
                {
                    var availableUniqueItems = inventoryItemsMap.ContainsKey(key) ? inventoryItemsMap[key] : new List<InventoryItem>();
                    var requestedUniqueCodes = dtoItem.UniqueCodes;

                    foreach (var uniqueCode in requestedUniqueCodes)
                    {
                        var matchingInvItem = availableUniqueItems.FirstOrDefault(ii => ii.UniqueCode == uniqueCode);
                        if (matchingInvItem == null)
                        {
                            errors.Add($"کد یکتا '{uniqueCode}' برای کالا {productName} در انبار مبدأ یافت نشد.");
                            continue;
                        }

                        item.UniqueCodes.Add(new ReceiptOrIssueItemUniqueCode { UniqueCode = uniqueCode });

                        // ست کردن Project مرتبط
                        var productItem = await _dbContext.ProductItems
                            .Include(pi => pi.Project)
                            .FirstOrDefaultAsync(pi => pi.UniqueCode == uniqueCode);

                        if (productItem != null && productItem.ProjectId.HasValue)
                            item.ProjectId = productItem.ProjectId;

                        // ست کردن سلسله مراتب انبار مبدأ
                        var inventory = await _dbContext.Inventories
                            .Include(inv => inv.Warehouse)
                            .Include(inv => inv.Zone)
                            .Include(inv => inv.Section)
                            .FirstOrDefaultAsync(inv =>
                                inv.ProductId == item.ProductId &&
                                inv.WarehouseId == item.SourceWarehouseId &&
                                inv.ZoneId == item.SourceZoneId &&
                                inv.SectionId == item.SourceSectionId);

                        if (inventory != null)
                        {
                            item.SourceWarehouseName = inventory.Warehouse.Name;
                            item.SourceZoneName = inventory.Zone?.Name;
                            item.SourceSectionName = inventory.Section?.Name;
                        }

                        _dbContext.InventoryItems.Remove(matchingInvItem);
                        availableUniqueItems.Remove(matchingInvItem);
                    }

                    item.Quantity = item.UniqueCodes.Count;
                    if (item.Quantity < dtoItem.Quantity)
                        errors.Add($"تعداد کدهای یکتا مشخص شده ({item.Quantity}) برای کالا {productName} کمتر از مقدار درخواستی ({dtoItem.Quantity}) است.");
                }
            }

            if (errors.Any())
                return (null, errors);

            // منطق اصلی موجودی‌ها
            foreach (var item in entity.Items)
            {
                var sourceInventory = inventories.FirstOrDefault(i => i.WarehouseId == item.SourceWarehouseId &&
                    i.ZoneId == item.SourceZoneId &&
                    i.SectionId == item.SourceSectionId &&
                    i.ProductId == item.ProductId);

                var destinationInventory = inventories.FirstOrDefault(i => i.WarehouseId == item.DestinationWarehouseId &&
                    i.ZoneId == item.DestinationZoneId &&
                    i.SectionId == item.DestinationSectionId &&
                    i.ProductId == item.ProductId);

                var productName = productNames.ContainsKey(item.ProductId) ? productNames[item.ProductId] : "نامشخص";

                if (item.UniqueCodes.Count == 0)
                {
                    if (sourceInventory == null || sourceInventory.Quantity < item.Quantity)
                    {
                        errors.Add($"موجودی عمومی کالا {productName} در انبار مبدأ ({sourceInventory?.Quantity ?? 0}) کافی نیست. مقدار درخواستی: {item.Quantity}.");
                        continue;
                    }
                }

                if (sourceInventory == null)
                {
                    errors.Add($"موجودی مبدأ برای کالای {productName} یافت نشد.");
                    continue;
                }

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
                    inventories.Add(destinationInventory);
                    _dbContext.Inventories.Add(destinationInventory);
                }

                sourceInventory.Quantity -= item.Quantity;
                destinationInventory.Quantity += item.Quantity;

                if (sourceInventory.Quantity < 0)
                    errors.Add($"موجودی کالا {productName} در انبار مبدأ کافی نیست.");
                if (destinationInventory.Quantity < 0)
                    errors.Add($"موجودی کالا {productName} در انبار مقصد به کمتر از صفر رسید.");
            }

            if (errors.Any())
                return (null, errors);

            // 1. بعد از اعمال تغییرات روی موجودی‌ها و اضافه کردن entity
            _dbContext.ReceiptOrIssues.Add(entity);

            // 2. ذخیره تغییرات
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _procurementContext.SaveChangesAsync(cancellationToken);

            // 3. بارگذاری پروژه‌ها برای DTO
            var projectIds = entity.Items
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            var projectTitles = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName, cancellationToken);

            // 4. ساخت DTO خروجی
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

            // 5. بازگرداندن نتیجه
            return (resultDto, new List<string>());
        }



        public async Task<(ReceiptOrIssueDto? Result, List<string> Errors)> UpdateAsync(
       int id, ReceiptOrIssueDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Items collection cannot be empty.");

            // جمع‌آوری شناسه‌ها
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var requestIds = dto.Items.Select(i => i.PurchaseRequestId).Distinct().ToList();

            // بارگذاری محصولات
            var productNames = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            // بارگذاری PurchaseRequestItems مرتبط
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

            // اعتبارسنجی آیتم‌ها
            foreach (var item in dto.Items)
            {
                var productName = productNames.GetValueOrDefault(item.ProductId, "نامشخص");

                if (item.ProductId <= 0)
                    errors.Add($"شناسه کالا معتبر نیست.");
                if (item.Quantity <= 0)
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

            // بارگذاری موجودیت موجود
            var entity = await _dbContext.ReceiptOrIssues
                .Include(r => r.Items).ThenInclude(i => i.UniqueCodes)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null)
                return (null, new List<string> { "سند مورد نظر یافت نشد." });

            // برگرداندن تغییرات قبلی موجودی‌ها و حذف UniqueCodes قبلی
            foreach (var oldItem in entity.Items)
            {
                // بازگردانی موجودی مبدأ
                var sourceInventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == oldItem.SourceWarehouseId &&
                    i.ZoneId == oldItem.SourceZoneId &&
                    i.SectionId == oldItem.SourceSectionId &&
                    i.ProductId == oldItem.ProductId,
                    cancellationToken);

                if ((entity.Type == ReceiptOrIssueType.Issue || entity.Type == ReceiptOrIssueType.Transfer) && sourceInventory != null)
                {
                    sourceInventory.Quantity += oldItem.Quantity;
                }

                // بازگردانی موجودی مقصد
                var destinationInventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == oldItem.DestinationWarehouseId &&
                    i.ZoneId == oldItem.DestinationZoneId &&
                    i.SectionId == oldItem.DestinationSectionId &&
                    i.ProductId == oldItem.ProductId,
                    cancellationToken);

                if ((entity.Type == ReceiptOrIssueType.Receipt || entity.Type == ReceiptOrIssueType.Transfer) && destinationInventory != null)
                {
                    destinationInventory.Quantity -= oldItem.Quantity;
                    if (destinationInventory.Quantity < 0)
                    {
                        var productName = productNames.GetValueOrDefault(oldItem.ProductId, "نامشخص");
                        errors.Add($"موجودی کالا {productName} در انبار مقصد به کمتر از صفر رسید.");
                    }
                }

                // حذف UniqueCodes مرتبط از InventoryItems
                foreach (var uc in oldItem.UniqueCodes)
                {
                    var inventoryItem = await _dbContext.InventoryItems.FirstOrDefaultAsync(ii => ii.UniqueCode == uc.UniqueCode, cancellationToken);
                    if (inventoryItem != null)
                        _dbContext.InventoryItems.Add(inventoryItem); // بازگردانی آیتم به انبار
                }
            }

            if (errors.Any())
                return (null, errors);

            // بارگذاری بخش‌ها و زون‌ها
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

            // بروزرسانی entity
            entity.Date = dto.Date;
            entity.DocumentNumber = dto.DocumentNumber;
            entity.Description = dto.Description;
            entity.Type = dto.Type;

            // آماده‌سازی آیتم‌ها و UniqueCodes جدید
            var finalItems = new List<ReceiptOrIssueItem>();
            foreach (var dtoItem in dto.Items)
            {
                var sourceSection = dtoItem.SourceSectionId.HasValue
                    ? sourceSections.FirstOrDefault(s => s.Id == dtoItem.SourceSectionId.Value)
                    : sourceSections.FirstOrDefault(s => s.ZoneId == dtoItem.SourceZoneId);

                var destinationSection = dtoItem.DestinationSectionId.HasValue
                    ? destinationSections.FirstOrDefault(s => s.Id == dtoItem.DestinationSectionId.Value)
                    : destinationSections.FirstOrDefault(s => s.ZoneId == dtoItem.DestinationZoneId);

                var existingItem = dtoItem.Id > 0 ? entity.Items.FirstOrDefault(x => x.Id == dtoItem.Id) : null;

                if (existingItem != null)
                {
                    existingItem.ProductId = dtoItem.ProductId;
                    existingItem.Quantity = dtoItem.Quantity;
                    existingItem.CategoryId = dtoItem.CategoryId;
                    existingItem.GroupId = dtoItem.GroupId;
                    existingItem.StatusId = dtoItem.StatusId;
                    existingItem.SourceWarehouseId = dtoItem.SourceWarehouseId;
                    existingItem.SourceZoneId = dtoItem.SourceZoneId;
                    existingItem.SourceSectionId = sourceSection?.Id;
                    existingItem.DestinationWarehouseId = dtoItem.DestinationWarehouseId;
                    existingItem.DestinationZoneId = dtoItem.DestinationZoneId;
                    existingItem.DestinationSectionId = destinationSection?.Id;
                    existingItem.ProjectId = dtoItem.ProjectId;
                    existingItem.PurchaseRequestId = dtoItem.PurchaseRequestId;
                    existingItem.UniqueCodes.Clear();
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
                        SourceSectionId = sourceSection?.Id,
                        DestinationWarehouseId = dtoItem.DestinationWarehouseId,
                        DestinationZoneId = dtoItem.DestinationZoneId,
                        DestinationSectionId = destinationSection?.Id,
                        ProjectId = dtoItem.ProjectId,
                        PurchaseRequestId = dtoItem.PurchaseRequestId,
                        UniqueCodes = new List<ReceiptOrIssueItemUniqueCode>()
                    });
                }
            }

            // حذف آیتم‌هایی که در DTO جدید نیستند
            var toBeRemoved = entity.Items.Where(ei => dto.Items.All(ni => ni.Id != ei.Id)).ToList();
            _dbContext.ReceiptOrIssueItems.RemoveRange(toBeRemoved);
            entity.Items = finalItems;

            // بارگذاری موجودی‌ها
            var allWarehouseIds = dto.Items.SelectMany(i => new[] { i.SourceWarehouseId, i.DestinationWarehouseId }).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var allZoneIds = dto.Items.SelectMany(i => new[] { i.SourceZoneId, i.DestinationZoneId }).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var allSectionIds = dto.Items.SelectMany(i => new[] { i.SourceSectionId, i.DestinationSectionId }).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

            var inventories = await _dbContext.Inventories
                .Where(inv => allWarehouseIds.Contains(inv.WarehouseId)
                              && productIds.Contains(inv.ProductId)
                              && (inv.ZoneId == null || allZoneIds.Contains(inv.ZoneId.Value))
                              && (inv.SectionId == null || allSectionIds.Contains(inv.SectionId.Value)))
                .ToListAsync(cancellationToken);

            // بارگذاری InventoryItems برای UniqueCodes
            var inventoryItems = await _dbContext.InventoryItems
                .Include(ii => ii.Inventory)
                .Where(ii => allWarehouseIds.Contains(ii.Inventory.WarehouseId)
                             && productIds.Contains(ii.Inventory.ProductId)
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

            // پردازش UniqueCodes بر اساس DTO
            foreach (var dtoItem in dto.Items)
            {
                var item = entity.Items.FirstOrDefault(ei => ei.ProductId == dtoItem.ProductId &&
                    (dtoItem.Id == 0 || ei.Id == dtoItem.Id)); // مطابقت بر اساس Id یا ProductId اگر جدید

                if (item == null || !item.SourceWarehouseId.HasValue) continue;

                var key = new
                {
                    WarehouseId = item.SourceWarehouseId.Value,
                    ZoneId = item.SourceZoneId,
                    SectionId = item.SourceSectionId,
                    ProductId = item.ProductId
                };

                var productName = productNames.GetValueOrDefault(item.ProductId, "نامشخص");

                if (dtoItem.UniqueCodes?.Any() == true)
                {
                    // حالت با UniqueCodes مشخص شده توسط کاربر
                    var availableUniqueItems = inventoryItemsMap.ContainsKey(key) ? inventoryItemsMap[key] : new List<InventoryItem>();
                    var requestedUniqueCodes = dtoItem.UniqueCodes;

                    foreach (var uniqueCode in requestedUniqueCodes)
                    {
                        var matchingInvItem = availableUniqueItems.FirstOrDefault(ii => ii.UniqueCode == uniqueCode);
                        if (matchingInvItem == null)
                        {
                            errors.Add($"کد یکتا '{uniqueCode}' برای کالا {productName} در انبار مبدأ یافت نشد.");
                            continue;
                        }

                        item.UniqueCodes.Add(new ReceiptOrIssueItemUniqueCode { UniqueCode = uniqueCode });
                        _dbContext.InventoryItems.Remove(matchingInvItem);
                        availableUniqueItems.Remove(matchingInvItem);
                    }

                    if (item.UniqueCodes.Count < dtoItem.Quantity)
                    {
                        errors.Add($"تعداد کدهای یکتا مشخص شده ({item.UniqueCodes.Count}) برای کالا {productName} کمتر از مقدار درخواستی ({dtoItem.Quantity}) است.");
                    }
                }
                else
                {
                    // حالت بدون UniqueCodes (استفاده از موجودی عمومی)
                    // هیچ UniqueCode assign نمی‌شود، Quantity همان dtoItem.Quantity باقی می‌ماند
                    // چک موجودی عمومی بعداً در switch انجام می‌شود
                }
            }

            if (errors.Any())
                return (null, errors);

            // اعمال تغییرات موجودی‌ها (Quantity از dto حفظ می‌شود مگر در صورت خطا)
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

                // برای حالت بدون UniqueCodes، چک کنیم که موجودی عمومی کافی باشد (فقط برای Issue/Transfer)
                if (dto.Type == ReceiptOrIssueType.Issue || dto.Type == ReceiptOrIssueType.Transfer)
                {
                    if (item.UniqueCodes.Count == 0 && sourceInventory != null)
                    {
                        if (sourceInventory.Quantity < item.Quantity)
                        {
                            errors.Add($"موجودی عمومی کالا {productName} در انبار مبدأ ({sourceInventory.Quantity}) کافی نیست. مقدار درخواستی: {item.Quantity}.");
                            continue;
                        }
                    }
                }

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
                    inventories.Add(destinationInventory);
                    _dbContext.Inventories.Add(destinationInventory);
                }

                switch (dto.Type)
                {
                    case ReceiptOrIssueType.Receipt:
                        destinationInventory.Quantity += item.Quantity;
                        if (destinationInventory.Quantity < 0)
                            errors.Add($"موجودی کالا {productName} در انبار مقصد به کمتر از صفر رسید.");
                        break;
                    case ReceiptOrIssueType.Issue:
                        if (sourceInventory == null)
                            errors.Add($"موجودی مبدأ برای کالای {productName} یافت نشد.");
                        else
                        {
                            sourceInventory.Quantity -= item.Quantity;
                            if (sourceInventory.Quantity < 0)
                                errors.Add($"موجودی کالا {productName} در انبار مبدأ کافی نیست.");
                        }
                        break;
                    case ReceiptOrIssueType.Transfer:
                        if (sourceInventory == null || destinationInventory == null)
                            errors.Add($"موجودی مبدأ یا مقصد برای کالای {productName} یافت نشد.");
                        else
                        {
                            sourceInventory.Quantity -= item.Quantity;
                            destinationInventory.Quantity += item.Quantity;
                            if (sourceInventory.Quantity < 0)
                                errors.Add($"موجودی کالا {productName} در انبار مبدأ کافی نیست.");
                            if (destinationInventory.Quantity < 0)
                                errors.Add($"موجودی کالا {productName} در انبار مقصد به کمتر از صفر رسید.");
                        }
                        break;
                }
            }

            if (errors.Any())
                return (null, errors);

            // بروزرسانی PurchaseRequestItems برای Issue و Transfer
            var pendingItemsToUpdate = entity.Items
                .Where(i => i.PurchaseRequestId.HasValue)
                .Select(i => new { i.PurchaseRequestId, i.ProductId, i.Quantity })
                .ToList();

            if (pendingItemsToUpdate.Any() && (dto.Type == ReceiptOrIssueType.Issue || dto.Type == ReceiptOrIssueType.Transfer))
            {
                var pendingRequestIds = pendingItemsToUpdate.Select(p => p.PurchaseRequestId!.Value).Distinct().ToList();
                var productIdsForPending = pendingItemsToUpdate.Select(p => p.ProductId).Distinct().ToList();

                var pendingItems = await _procurementContext.PurchaseRequestItems
                    .Where(pr => pendingRequestIds.Contains(pr.PurchaseRequestId)
                              && productIdsForPending.Contains(pr.ProductId))
                    .ToListAsync(cancellationToken);

                foreach (var item in pendingItemsToUpdate)
                {
                    var pendingItem = pendingItems.FirstOrDefault(p => p.PurchaseRequestId == item.PurchaseRequestId && p.ProductId == item.ProductId);
                    if (pendingItem != null && !pendingItem.IsSupplyStopped && !pendingItem.IsFullyDelivered)
                    {
                        pendingItem.RemainingQuantity -= item.Quantity;
                        if (pendingItem.RemainingQuantity < 0)
                            pendingItem.RemainingQuantity = 0;
                        pendingItem.IsFullySupplied = pendingItem.RemainingQuantity <= 0;
                        pendingItem.IsFullyDelivered = pendingItem.RemainingQuantity <= 0 && !pendingItem.IsSupplyStopped;
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _procurementContext.SaveChangesAsync(cancellationToken);

            // بارگذاری پروژه‌ها
            var projectIds = entity.Items.Where(i => i.ProjectId.HasValue).Select(i => i.ProjectId!.Value).Distinct().ToList();
            var projectTitles = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName, cancellationToken);

            // ایجاد DTO خروجی
            var resultDto = new ReceiptOrIssueDto
            {
                Id = entity.Id,
                Date = entity.Date,
                DocumentNumber = entity.DocumentNumber,
                Description = entity.Description,
                Type = entity.Type,
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
                    SourceWarehouseName = i.SourceSection?.Zone?.Warehouse?.Name,
                    DestinationWarehouseId = i.DestinationWarehouseId,
                    DestinationZoneId = i.DestinationZoneId,
                    DestinationSectionId = i.DestinationSectionId,
                    DestinationSectionName = i.DestinationSection?.Name,
                    DestinationZoneName = i.DestinationSection?.Zone?.Name,
                    DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name,
                    ProductName = productNames.ContainsKey(i.ProductId) ? productNames[i.ProductId] : "نامشخص",
                    UniqueCodes = i.UniqueCodes.Select(uc => uc.UniqueCode).ToList(),
                    ProjectId = i.ProjectId,
                    ProjectTitle = i.ProjectId.HasValue && projectTitles.ContainsKey(i.ProjectId.Value)
                        ? projectTitles[i.ProjectId.Value]
                        : null,
                    PurchaseRequestId = i.PurchaseRequestId
                }).ToList()
            };

            return (resultDto, errors);
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
