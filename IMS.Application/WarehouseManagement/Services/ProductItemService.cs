using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.Services
{
    public class ProductItemService : IProductItemService
    {
        private readonly IWarehouseDbContext _warehouseContext;
        private readonly IApplicationDbContext _projectContext;

        public ProductItemService(IWarehouseDbContext warehouseContext, IApplicationDbContext projectContext)
        {
            _warehouseContext = warehouseContext;
            _projectContext = projectContext;
        }

        public async Task<ProductItemDto?> GetByIdAsync(int id)
        {
            var item = await _warehouseContext.ProductItems
                .AsNoTracking()
                .Include(pi => pi.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .FirstOrDefaultAsync(pi => pi.Id == id);

            if (item == null) return null;

            string? projectName = null;
            if (item.ProjectId.HasValue)
            {
                var project = await _projectContext.Projects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == item.ProjectId.Value);
                projectName = project?.ProjectName;
            }

            return new ProductItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                CategoryId = item.Product?.Status?.Group?.Category?.Id ?? 0,
                GroupId = item.Product?.Status?.Group?.Id ?? 0,
                StatusId = item.Product?.StatusId ?? 0,

                CategoryName = item.Product?.Status?.Group?.Category?.Name,
                GroupName = item.Product?.Status?.Group?.Name,
                StatusName = item.Product?.Status?.Name,
                ProductName = item.Product?.Name,

                // پر کردن کدها
                CategoryCode = item.Product?.Status?.Group?.Category?.Code,
                GroupCode = item.Product?.Status?.Group?.Code,
                StatusCode = item.Product?.Status?.Code,
                ProductCode = item.Product?.Code,

                Sequence = item.Sequence,
                ProjectId = item.ProjectId,
                ProjectName = projectName,

                Status = item.ProductItemStatus
            };


        }

        public async Task<List<ProductItemDto>> GetByProductIdAsync(int productId)
        {
            var items = await _warehouseContext.ProductItems
                .AsNoTracking()
                .Where(pi => pi.ProductId == productId)
                .Include(pi => pi.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .ToListAsync();

            var projectIds = items.Where(i => i.ProjectId.HasValue).Select(i => i.ProjectId.Value).Distinct().ToList();
            var projects = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToListAsync();

            return items.Select(item => new ProductItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                CategoryId = item.Product?.Status?.Group?.Category?.Id ?? 0,
                GroupId = item.Product?.Status?.Group?.Id ?? 0,
                StatusId = item.Product?.StatusId ?? 0,

                CategoryName = item.Product?.Status?.Group?.Category?.Name,
                GroupName = item.Product?.Status?.Group?.Name,
                StatusName = item.Product?.Status?.Name,
                ProductName = item.Product?.Name,

                // **پر کردن کدها**
                CategoryCode = item.Product?.Status?.Group?.Category?.Code,
                GroupCode = item.Product?.Status?.Group?.Code,
                StatusCode = item.Product?.Status?.Code,
                ProductCode = item.Product?.Code,

                Sequence = item.Sequence,
                ProjectId = item.ProjectId,
                ProjectName = item.ProjectId.HasValue
          ? projects.FirstOrDefault(p => p.Id == item.ProjectId.Value)?.ProjectName
          : null,

                Status = item.ProductItemStatus
            }).ToList();

        }

        public async Task<ProductItemDto> CreateAsync(ProductItemDto dto)
        {
            // 1. بررسی وجود محصول
            var product = await _warehouseContext.Products
                .Include(p => p.Status)
                    .ThenInclude(s => s.Group)
                        .ThenInclude(g => g.Category)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                throw new Exception("محصول انتخاب شده یافت نشد.");

            // 2. محاسبه شماره ترتیبی
            int nextSequence = 1;
            var lastItem = await _warehouseContext.ProductItems
                .Where(pi => pi.ProductId == dto.ProductId)
                .OrderByDescending(pi => pi.Sequence)
                .FirstOrDefaultAsync();

            if (lastItem != null)
                nextSequence = lastItem.Sequence + 1;

            dto.Sequence = nextSequence;

            // 3. تولید UniqueCode خودکار
            string uniqueCode = $"C{product.Status.Group.Category.Code}G{product.Status.Group.Code}S{product.Status.Code}P{product.Code}-{dto.Sequence}";

            // 4. ایجاد موجودیت ProductItem
            var entity = new ProductItem
            {
                ProductId = dto.ProductId,
                Sequence = dto.Sequence,
                ProjectId = dto.ProjectId,
                ProductItemStatus = dto.Status, // پیش‌فرض Ready
                UniqueCode = uniqueCode
            };

            _warehouseContext.ProductItems.Add(entity);
            await _warehouseContext.SaveChangesAsync(CancellationToken.None);

            // 5. بازگرداندن DTO کامل
            dto.Id = entity.Id;
        

            return dto;
        }


        public async Task<ProductItemDto?> UpdateAsync(ProductItemDto dto)
        {
            var entity = await _warehouseContext.ProductItems
                .FirstOrDefaultAsync(pi => pi.Id == dto.Id);

            if (entity == null) return null;

            entity.Sequence = dto.Sequence;
            entity.ProjectId = dto.ProjectId;
            entity.ProductItemStatus = dto.Status; // آپدیت وضعیت
         
            await _warehouseContext.SaveChangesAsync(CancellationToken.None);
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _warehouseContext.ProductItems
                .FirstOrDefaultAsync(pi => pi.Id == id);

            if (entity == null) return false;

            _warehouseContext.ProductItems.Remove(entity);
            await _warehouseContext.SaveChangesAsync(CancellationToken.None);

            return true;
        }

        public async Task<List<SelectListItem>> GetProjectsAsync()
        {
            var projects = await _projectContext.Projects
                .AsNoTracking()
                .OrderBy(p => p.ProjectName)
                .ToListAsync();

            return projects.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.ProjectName
            }).ToList();
        }
    }
}