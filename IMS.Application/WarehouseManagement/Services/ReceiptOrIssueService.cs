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

            // استخراج تمام ProjectId‌های آیتم‌ها
            var projectIds = entity.Items
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            var projectMap = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

            // استخراج تمام PurchaseRequestId‌های آیتم‌ها
            var purchaseRequestIds = entity.Items
                .Where(i => i.PurchaseRequestId.HasValue)
                .Select(i => i.PurchaseRequestId!.Value)
                .Distinct()
                .ToList();

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

                        ProductName = i.Product?.Name,

                        ProjectId = i.ProjectId,
                        ProjectTitle = i.ProjectId.HasValue && projectMap.ContainsKey(i.ProjectId.Value)
                            ? projectMap[i.ProjectId.Value]
                            : null,

                        PurchaseRequestId = i.PurchaseRequestId,
                        PurchaseRequestTitle = i.PurchaseRequestId.HasValue && purchaseRequestMap.ContainsKey(i.PurchaseRequestId.Value)
                            ? purchaseRequestMap[i.PurchaseRequestId.Value]
                            : null
                    }).ToList()
            };

            return dto;
        }


        public async Task<List<ReceiptOrIssueDto>> GetAllAsync(int? warehouseId = null)
        {
            // بارگذاری ReceiptOrIssue ها به همراه آیتم‌ها و سلسله مراتب انبار
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

            // مرتب‌سازی descending بر اساس تاریخ
            var list = await query
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            // بارگذاری پروژه‌ها از DbContext دوم
            var projectIds = list
                .SelectMany(r => r.Items)
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            var projects = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

            // بارگذاری عناوین درخواست خرید از DbContext Procurement
            var purchaseRequestIds = list
                .SelectMany(r => r.Items)
                .Where(i => i.PurchaseRequestId.HasValue)
                .Select(i => i.PurchaseRequestId!.Value)
                .Distinct()
                .ToList();

            var purchaseRequests = await _procurementContext.PurchaseRequests
                .Where(pr => purchaseRequestIds.Contains(pr.Id))
                .ToDictionaryAsync(pr => pr.Id, pr => pr.Title);

            // تبدیل به DTO
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
                    SourceWarehouseName = i.SourceSection?.Zone?.Warehouse?.Name,
                    DestinationWarehouseId = i.DestinationWarehouseId,
                    DestinationZoneId = i.DestinationZoneId,
                    DestinationSectionId = i.DestinationSectionId,
                    DestinationSectionName = i.DestinationSection?.Name,
                    DestinationZoneName = i.DestinationSection?.Zone?.Name,
                    DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name,
                    ProjectId = i.ProjectId,
                    ProjectTitle = i.ProjectId.HasValue && projects.ContainsKey(i.ProjectId.Value)
                        ? projects[i.ProjectId.Value]
                        : null,
                    PurchaseRequestId = i.PurchaseRequestId,
                    PurchaseRequestTitle = i.PurchaseRequestId.HasValue && purchaseRequests.ContainsKey(i.PurchaseRequestId.Value)
                        ? purchaseRequests[i.PurchaseRequestId.Value]
                        : null
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

            // جمع‌آوری شناسه‌ها
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var requestIds = dto.Items.Select(i => i.PurchaseRequestId).Distinct().ToList();

            // بارگذاری محصولات
            var productNames = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            // بارگذاری همه PurchaseRequestItems مرتبط
            var purchaseRequestItems = await _procurementContext.PurchaseRequestItems
                .Include(pri => pri.PurchaseRequest)
                .Where(pri => productIds.Contains(pri.ProductId) && requestIds.Contains(pri.PurchaseRequestId))
                .ToListAsync(cancellationToken);

            // جمع‌آوری RequestNumber ها
            var requestNumbers = purchaseRequestItems
                .Select(pri => pri.PurchaseRequest?.RequestNumber)
                .Where(rn => !string.IsNullOrWhiteSpace(rn))
                .Distinct()
                .ToList();

            // بارگذاری همه FlatItems مرتبط
            var flatItems = await _procurementContext.PurchaseRequestFlatItems
                .Where(f => productIds.Contains(f.ProductId) && requestNumbers.Contains(f.RequestNumber))
                .ToListAsync(cancellationToken);

            // جمع‌آوری خطاها
            // جمع‌آوری خطاها
            var errors = new List<string>();

            // پردازش آیتم‌ها و جمع‌آوری خطاها
            foreach (var item in dto.Items)
            {
                // گرفتن نام محصول از دیکشنری
                var productName = productNames.GetValueOrDefault(item.ProductId, "نامشخص");

                // اعتبارسنجی اولیه
                if (item.ProductId <= 0)
                    errors.Add($"شناسه کالا معتبر نیست.");
                if (item.Quantity <= 0)
                    errors.Add($"تعداد برای کالا {productName} باید بیشتر از صفر باشد.");

                // یافتن آیتم PurchaseRequestItem
                var purchaseRequestItem = purchaseRequestItems
                    .FirstOrDefault(pri => pri.ProductId == item.ProductId && pri.PurchaseRequestId == item.PurchaseRequestId);





                string? requestNumber = null;
                if (purchaseRequestItem != null)
                {
                    requestNumber = purchaseRequestItem.PurchaseRequest?.RequestNumber;
                }
                var stoppedProducts = new HashSet<int>();

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

            // اگر خطایی بود، فقط خطاها را برگردان
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

            // ایجاد موجودیت ReceiptOrIssue
            var entity = new ReceiptOrIssue
            {
                Date = dto.Date,
                DocumentNumber = dto.DocumentNumber,
                Description = dto.Description,
                Type = dto.Type,
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

                entity.Items.Add(new ReceiptOrIssueItem
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
                    PurchaseRequestId = itemDto.PurchaseRequestId
                });
            }

            // بررسی موجودی‌ها
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

            foreach (var item in entity.Items)
            {
                var sourceInventory = inventories.FirstOrDefault(i => i.WarehouseId == item.SourceWarehouseId && i.ZoneId == item.SourceZoneId && i.SectionId == item.SourceSectionId && i.ProductId == item.ProductId);
                var destinationInventory = inventories.FirstOrDefault(i => i.WarehouseId == item.DestinationWarehouseId && i.ZoneId == item.DestinationZoneId && i.SectionId == item.DestinationSectionId && i.ProductId == item.ProductId);
                var productName = productNames.ContainsKey(item.ProductId) ? productNames[item.ProductId] : "نامشخص";
                
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

                // حفظ منطق موجودی بدون تغییر
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

            var pendingItemsToUpdate = dto.Items
                .Where(i => i.PurchaseRequestId.HasValue)
                .Select(i => new { i.PurchaseRequestId, i.ProductId, i.Quantity })
                .ToList();

            // فقط اگر سند حواله باشد
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
                    var pendingItem = pendingItems
                        .FirstOrDefault(p => p.PurchaseRequestId == item.PurchaseRequestId && p.ProductId == item.ProductId);
                    if (!pendingItem.IsSupplyStopped && !pendingItem.IsFullyDelivered)
                    {
                        pendingItem.RemainingQuantity -= item.Quantity;
                        if (pendingItem.RemainingQuantity < 0)
                            pendingItem.RemainingQuantity = 0;
                    }
                    pendingItem.IsFullySupplied = pendingItem.RemainingQuantity <= 0;
                    
                    pendingItem.IsFullyDelivered = pendingItem.RemainingQuantity <= 0 && !pendingItem.IsSupplyStopped;

                }


            }



            await _dbContext.SaveChangesAsync(cancellationToken);
            await _procurementContext.SaveChangesAsync(cancellationToken);

            // تبدیل به DTO برای بازگشت
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
                    SourceSectionName = i.SourceSection?.Name,
                    SourceZoneName = i.SourceSection?.Zone?.Name,
                    SourceWarehouseName = i.SourceSection?.Zone?.Warehouse?.Name,
                    DestinationSectionName = i.DestinationSection?.Name,
                    DestinationZoneName = i.DestinationSection?.Zone?.Name,
                    DestinationWarehouseName = i.DestinationSection?.Zone?.Warehouse?.Name,
                    ProductName = productNames.ContainsKey(i.ProductId) ? productNames[i.ProductId] : "نامشخص"
                }).ToList()
            };

            return (resultDto, errors);
        }



        public async Task<(ReceiptOrIssueDto? Result, List<string> Errors)> UpdateAsync(
            int id, ReceiptOrIssueDto dto, CancellationToken cancellationToken = default)
        {
            // Validate input
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Items collection cannot be empty.");

            // Collect identifiers
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var requestIds = dto.Items.Select(i => i.PurchaseRequestId).Distinct().ToList();

            // Load products
            var productNames = await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            // Load related PurchaseRequestItems
            var purchaseRequestItems = await _procurementContext.PurchaseRequestItems
                .Include(pri => pri.PurchaseRequest)
                .Where(pri => productIds.Contains(pri.ProductId) && requestIds.Contains(pri.PurchaseRequestId))
                .ToListAsync(cancellationToken);

            // Collect RequestNumbers
            var requestNumbers = purchaseRequestItems
                .Select(pri => pri.PurchaseRequest?.RequestNumber)
                .Where(rn => !string.IsNullOrWhiteSpace(rn))
                .Distinct()
                .ToList();

            // Load related FlatItems
            var flatItems = await _procurementContext.PurchaseRequestFlatItems
                .Where(f => productIds.Contains(f.ProductId) && requestNumbers.Contains(f.RequestNumber))
                .ToListAsync(cancellationToken);

            // Collect errors
            var errors = new List<string>();

            // Validate items
            foreach (var item in dto.Items)
            {
                var productName = productNames.GetValueOrDefault(item.ProductId, "نامشخص");

                // Basic validation
                if (item.ProductId <= 0)
                    errors.Add($"شناسه کالا معتبر نیست.");
                if (item.Quantity <= 0)
                    errors.Add($"تعداد برای کالا {productName} باید بیشتر از صفر باشد.");

                // Find related PurchaseRequestItem
                var purchaseRequestItem = purchaseRequestItems
                    .FirstOrDefault(pri => pri.ProductId == item.ProductId && pri.PurchaseRequestId == item.PurchaseRequestId);

                var stoppedProducts = new HashSet<int>();

                if (purchaseRequestItem != null && purchaseRequestItem.IsSupplyStopped && dto.Type == ReceiptOrIssueType.Receipt)
                {
                    if (!stoppedProducts.Contains(item.ProductId))
                    {
                        stoppedProducts.Add(item.ProductId);
                        errors.Add($"آیتم {productName} متوقف شده است و امکان رسید ندارد.");
                    }
                }


                string? requestNumber = purchaseRequestItem?.PurchaseRequest?.RequestNumber;
                if (dto.Type == ReceiptOrIssueType.Receipt && purchaseRequestItem != null)
                {
                    var isInFlatItems = flatItems.Any(f => f.ProductId == item.ProductId && f.RequestNumber == requestNumber);
                    if (!isInFlatItems)
                        errors.Add($"برای {productName} کالا هیچ درخواست خریدی ثبت نشده است.");
                }
            }

            // Return errors if any
            if (errors.Any())
                return (null, errors);

            // Load existing entity
            var entity = await _dbContext.ReceiptOrIssues
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null)
                return (null, new List<string> { "سند مورد نظر یافت نشد." });

            // Revert previous inventory changes
            foreach (var oldItem in entity.Items)
            {
                var sourceInventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == oldItem.SourceWarehouseId &&
                    i.ZoneId == oldItem.SourceZoneId &&
                    i.SectionId == oldItem.SourceSectionId &&
                    i.ProductId == oldItem.ProductId,
                    cancellationToken);

                var destinationInventory = await _dbContext.Inventories.FirstOrDefaultAsync(i =>
                    i.WarehouseId == oldItem.DestinationWarehouseId &&
                    i.ZoneId == oldItem.DestinationZoneId &&
                    i.SectionId == oldItem.DestinationSectionId &&
                    i.ProductId == oldItem.ProductId,
                    cancellationToken);

                var productName = productNames.ContainsKey(oldItem.ProductId) ? productNames[oldItem.ProductId] : "نامشخص";

                if (entity.Type == ReceiptOrIssueType.Issue || entity.Type == ReceiptOrIssueType.Transfer)
                {
                    if (sourceInventory != null)
                        sourceInventory.Quantity += oldItem.Quantity;
                }

                if (entity.Type == ReceiptOrIssueType.Receipt || entity.Type == ReceiptOrIssueType.Transfer)
                {
                    if (destinationInventory != null)
                    {
                        destinationInventory.Quantity -= oldItem.Quantity;
                        if (destinationInventory.Quantity < 0)
                            errors.Add($"موجودی کالا {productName} در انبار مقصد به کمتر از صفر رسید.");
                    }
                }
            }

            // Return errors if any
            if (errors.Any())
                return (null, errors);

            // Load sections and zones
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

            // Update entity
            entity.Date = dto.Date;
            entity.DocumentNumber = dto.DocumentNumber;
            entity.Description = dto.Description;
            entity.Type = dto.Type;

            // Update items
            var existingItems = entity.Items.ToList();
            var finalItems = new List<ReceiptOrIssueItem>();

            foreach (var dtoItem in dto.Items)
            {
                var sourceSection = dtoItem.SourceSectionId.HasValue
                    ? sourceSections.FirstOrDefault(s => s.Id == dtoItem.SourceSectionId.Value)
                    : sourceSections.FirstOrDefault(s => s.ZoneId == dtoItem.SourceZoneId);

                var destinationSection = dtoItem.DestinationSectionId.HasValue
                    ? destinationSections.FirstOrDefault(s => s.Id == dtoItem.DestinationSectionId.Value)
                    : destinationSections.FirstOrDefault(s => s.ZoneId == dtoItem.DestinationZoneId);

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
                    existingItem.SourceSectionId = sourceSection?.Id;
                    existingItem.DestinationWarehouseId = dtoItem.DestinationWarehouseId;
                    existingItem.DestinationZoneId = dtoItem.DestinationZoneId;
                    existingItem.DestinationSectionId = destinationSection?.Id;
                    existingItem.ProjectId = dtoItem.ProjectId;
                    existingItem.PurchaseRequestId = dtoItem.PurchaseRequestId;

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
                        PurchaseRequestId = dtoItem.PurchaseRequestId
                    });
                }
            }

            // Remove items not in the updated DTO
            var toBeRemoved = existingItems.Where(ei => dto.Items.All(ni => ni.Id != ei.Id)).ToList();
            _dbContext.ReceiptOrIssueItems.RemoveRange(toBeRemoved);

            entity.Items = finalItems;

            // Load inventories
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

            // Update inventories
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

            // Return errors if any
            if (errors.Any())
                return (null, errors);

            // Update PurchaseRequestItems for Issue or Transfer
            var pendingItemsToUpdate = finalItems
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
                    var pendingItem = pendingItems
                        .FirstOrDefault(p => p.PurchaseRequestId == item.PurchaseRequestId && p.ProductId == item.ProductId);
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

            // Save changes
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _procurementContext.SaveChangesAsync(cancellationToken);

            // Load project titles
            var projectIds = finalItems
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            var projectTitles = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName, cancellationToken);

            // Create result DTO
            var resultDto = new ReceiptOrIssueDto
            {
                Id = entity.Id,
                Date = entity.Date,
                DocumentNumber = entity.DocumentNumber,
                Description = entity.Description,
                Type = entity.Type,
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

            var result = sections.Select(s => new StorageSectionDto
            {
                Id = s.Id,
                Name = s.Name,
                SectionCode = s.SectionCode,
                ZoneId = s.ZoneId,
                //Capacity = s.Capacity,
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
