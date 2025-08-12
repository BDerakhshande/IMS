using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.ProcurementManagement.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProcurementManagement.Service
{
    public class PurchaseRequestService : IPurchaseRequestService
    {
        private readonly IWarehouseDbContext _warehouseContext;
        private readonly IApplicationDbContext _projectContext;
        private readonly IProcurementManagementDbContext _procurementManagementDbContext;

        public PurchaseRequestService(
            IWarehouseDbContext warehouseContext,
            IApplicationDbContext projectContext,
            IProcurementManagementDbContext procurementManagementDbContext)
        {
            _warehouseContext = warehouseContext;
            _projectContext = projectContext;
            _procurementManagementDbContext = procurementManagementDbContext;
        }

        public async Task<List<PurchaseRequestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // 1. بارگذاری درخواست‌ها و آیتم‌ها (بدون Include دسته‌بندی‌ها، گروه‌ها و محصولات)
            var purchaseRequests = await _procurementManagementDbContext.PurchaseRequests
                .Include(pr => pr.RequestType)
                .Include(pr => pr.Items)  // فقط آیتم‌ها را Include می‌کنیم
                .ToListAsync(cancellationToken);

            // 2. استخراج شناسه‌های دسته‌بندی، گروه، محصول، وضعیت آیتم‌ها
            var categoryIds = purchaseRequests.SelectMany(pr => pr.Items)
                                              .Where(i => i.CategoryId != 0)
                                              .Select(i => i.CategoryId)
                                              .Distinct()
                                              .ToList();

            var groupIds = purchaseRequests.SelectMany(pr => pr.Items)
                                          .Where(i => i.GroupId != 0)
                                          .Select(i => i.GroupId)
                                          .Distinct()
                                          .ToList();

            var productIds = purchaseRequests.SelectMany(pr => pr.Items)
                                            .Where(i => i.ProductId != 0)
                                            .Select(i => i.ProductId)
                                            .Distinct()
                                            .ToList();

            var statusIds = purchaseRequests.SelectMany(pr => pr.Items)
                                           .Where(i => i.StatusId != 0)
                                           .Select(i => i.StatusId)
                                           .Distinct()
                                           .ToList();

            // 3. بارگذاری داده‌های مربوط به سلسله‌مراتب کالا از DbContext انبار
            var categories = await _warehouseContext.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            var groups = await _warehouseContext.Groups
                .Where(g => groupIds.Contains(g.Id))
                .ToListAsync(cancellationToken);

            var products = await _warehouseContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var statuses = await _warehouseContext.Statuses
                .Where(s => statusIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            // 4. تخصیص داده‌های انبار به آیتم‌ها
            foreach (var pr in purchaseRequests)
            {
                foreach (var item in pr.Items)
                {
                    item.Category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                    item.Group = groups.FirstOrDefault(g => g.Id == item.GroupId);
                    item.Product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    item.Status = statuses.FirstOrDefault(s => s.Id == item.StatusId);
                }
            }

            // 5. بارگذاری پروژه‌ها از DbContext پروژه‌ها فقط برای آیتم‌هایی که ProjectId دارند
            var projectIds = purchaseRequests.SelectMany(pr => pr.Items)
                                             .Where(i => i.ProjectId.HasValue)
                                             .Select(i => i.ProjectId!.Value)
                                             .Distinct()
                                             .ToList();

            var projects = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            // 6. اختصاص پروژه‌ها به آیتم‌ها
            foreach (var pr in purchaseRequests)
            {
                foreach (var item in pr.Items)
                {
                    if (item.ProjectId.HasValue)
                        item.Project = projects.FirstOrDefault(p => p.Id == item.ProjectId.Value);
                }
            }

            // 7. تبدیل به DTO (پیاده‌سازی MapToDto بر اساس مدل شما)
            return purchaseRequests.Select(pr => MapToDto(pr)).ToList();
        }
        public async Task<int> CreateAsync(PurchaseRequestDto dto, CancellationToken cancellationToken = default)
        {
            var entity = new PurchaseRequest
            {
                RequestNumber = dto.RequestNumber,
                RequestDate = dto.RequestDate,
                RequestTypeId = dto.RequestTypeId,
                Title = dto.Title,
                Notes = dto.Notes,
                Status = dto.Status,
                Items = dto.Items.Select(i => new PurchaseRequestItem
                {
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    ProductId = i.ProductId,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    ProjectId = i.ProjectId
                }).ToList()
            };

            _procurementManagementDbContext.PurchaseRequests.Add(entity);
            await _procurementManagementDbContext.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }



        public async Task<PurchaseRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // 1. بارگذاری درخواست خرید با آیتم‌ها و نوع درخواست (بدون Include سلسله‌مراتب آیتم‌ها)
            var pr = await _procurementManagementDbContext.PurchaseRequests
                .Include(p => p.RequestType)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (pr == null)
                return null;

            // 2. استخراج شناسه‌های دسته‌بندی، گروه، محصول، وضعیت آیتم‌ها از آیتم‌ها
            var categoryIds = pr.Items
                .Where(i => i.CategoryId != 0)
                .Select(i => i.CategoryId)
                .Distinct()
                .ToList();

            var groupIds = pr.Items
                .Where(i => i.GroupId != 0)
                .Select(i => i.GroupId)
                .Distinct()
                .ToList();

            var productIds = pr.Items
                .Where(i => i.ProductId != 0)
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var statusIds = pr.Items
                .Where(i => i.StatusId != 0)
                .Select(i => i.StatusId)
                .Distinct()
                .ToList();

            // 3. بارگذاری داده‌های سلسله‌مراتب کالا از DbContext انبار
            var categories = await _warehouseContext.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            var groups = await _warehouseContext.Groups
                .Where(g => groupIds.Contains(g.Id))
                .ToListAsync(cancellationToken);

            var products = await _warehouseContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var statuses = await _warehouseContext.Statuses
                .Where(s => statusIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            // 4. تخصیص داده‌های انبار به آیتم‌ها
            foreach (var item in pr.Items)
            {
                item.Category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                item.Group = groups.FirstOrDefault(g => g.Id == item.GroupId);
                item.Product = products.FirstOrDefault(p => p.Id == item.ProductId);
                item.Status = statuses.FirstOrDefault(s => s.Id == item.StatusId);
            }

            // 5. استخراج شناسه‌های پروژه‌ها از آیتم‌هایی که ProjectId دارند
            var projectIds = pr.Items
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            // 6. بارگذاری پروژه‌ها از DbContext پروژه‌ها
            var projects = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            // 7. اختصاص پروژه‌ها به آیتم‌ها
            foreach (var item in pr.Items)
            {
                if (item.ProjectId.HasValue)
                    item.Project = projects.FirstOrDefault(p => p.Id == item.ProjectId.Value);
            }
            foreach (var i in pr.Items)
            {
                Console.WriteLine($"Entity ItemId={i.Id}, StatusId={i.StatusId}");
            }

            // 8. تبدیل به DTO و برگرداندن نتیجه
            return MapToDto(pr);
        }

  

        public async Task<bool> UpdateAsync(PurchaseRequestDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _procurementManagementDbContext.PurchaseRequests
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);

            if (entity == null)
                return false;

            // به‌روزرسانی اطلاعات هدر درخواست خرید
            entity.RequestNumber = dto.RequestNumber;
            entity.RequestDate = dto.RequestDate;
            entity.RequestTypeId = dto.RequestTypeId;
            entity.Title = dto.Title;
            entity.Notes = dto.Notes;
            entity.Status = dto.Status;

            // لیست آیدی‌های آیتم‌های جدید از DTO
            var dtoItemIds = dto.Items.Where(i => i.Id != 0).Select(i => i.Id).ToList();

            // --- حذف آیتم‌هایی که در DTO وجود ندارند ---
            var itemsToRemove = entity.Items
                .Where(ei => !dtoItemIds.Contains(ei.Id))
                .ToList();

            _procurementManagementDbContext.PurchaseRequestItems.RemoveRange(itemsToRemove);

            // --- بروزرسانی آیتم‌های موجود ---
            foreach (var existingItem in entity.Items.Where(ei => dtoItemIds.Contains(ei.Id)))
            {
                var dtoItem = dto.Items.First(i => i.Id == existingItem.Id);

                existingItem.CategoryId = dtoItem.CategoryId;
                existingItem.GroupId = dtoItem.GroupId;
                existingItem.StatusId = dtoItem.StatusId;
                existingItem.ProductId = dtoItem.ProductId;
                existingItem.Description = dtoItem.Description;
                existingItem.Quantity = dtoItem.Quantity;
                existingItem.Unit = dtoItem.Unit;
                existingItem.ProjectId = dtoItem.ProjectId;
            }

            // --- اضافه کردن آیتم‌های جدید ---
            var newItems = dto.Items
                .Where(i => i.Id == 0)
                .Select(i => new PurchaseRequestItem
                {
                    PurchaseRequestId = entity.Id,
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    ProductId = i.ProductId,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    ProjectId = i.ProjectId
                })
                .ToList();

            if (newItems.Any())
                _procurementManagementDbContext.PurchaseRequestItems.AddRange(newItems);

            await _procurementManagementDbContext.SaveChangesAsync(cancellationToken);

            return true;
        }



        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _procurementManagementDbContext.PurchaseRequests.FindAsync(new object?[] { id }, cancellationToken);
            if (entity == null)
                return false;

            _procurementManagementDbContext.PurchaseRequests.Remove(entity);
            await _procurementManagementDbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        private PurchaseRequestDto MapToDto(PurchaseRequest pr)
        {
            return new PurchaseRequestDto
            {
                Id = pr.Id,
                RequestNumber = pr.RequestNumber,
                RequestDate = pr.RequestDate,
                RequestTypeId = pr.RequestTypeId,
                RequestTypeName = pr.RequestType.Name,
                Title = pr.Title,
                Notes = pr.Notes,
                Status = pr.Status,
                Items = pr.Items.Select(i => new PurchaseRequestItemDto
                {
                    Id = i.Id,
                    PurchaseRequestId = i.PurchaseRequestId,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category?.Name,
                    GroupId = i.GroupId,
                    GroupName = i.Group?.Name,
                    StatusId = i.StatusId,
                    Status = i.Status?.Name,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    ProjectId = i.ProjectId,
                    ProjectName = i.Project?.ProjectName
                }).ToList()
            };
        }


        public async Task<List<SelectListItem>> GetCategoriesAsync()
        {
            return await _warehouseContext.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetGroupsByCategoryAsync(int categoryId)
        {
            return await _warehouseContext.Groups
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
            return await _warehouseContext.Statuses
                .Where(s => s.GroupId == groupId)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetProductsByStatus(int statusId)
        {
            return await _warehouseContext.Products
                .Where(p => p.StatusId == statusId)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToListAsync();
        }
    }
}
