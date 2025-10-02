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

                CategoryCode = item.Product?.Status?.Group?.Category?.Code,
                GroupCode = item.Product?.Status?.Group?.Code,
                StatusCode = item.Product?.Status?.Code,
                ProductCode = item.Product?.Code,

                Sequence = item.Sequence,
                ProjectId = item.ProjectId,
                ProjectName = projectName,
                ItemStatus = item.ProductItemStatus
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
                .AsNoTracking()
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

                CategoryCode = item.Product?.Status?.Group?.Category?.Code,
                GroupCode = item.Product?.Status?.Group?.Code,
                StatusCode = item.Product?.Status?.Code,
                ProductCode = item.Product?.Code,

                Sequence = item.Sequence,
                ProjectId = item.ProjectId,
                ProjectName = item.ProjectId.HasValue
                    ? projects.FirstOrDefault(p => p.Id == item.ProjectId.Value)?.ProjectName
                    : null,

                ItemStatus = item.ProductItemStatus
            }).ToList();
        }

        public async Task<ProductItemDto?> UpdateAsync(ProductItemDto dto)
        {
            var entity = await _warehouseContext.ProductItems
                .FirstOrDefaultAsync(pi => pi.Id == dto.Id);

            if (entity == null) return null;

            entity.Sequence = dto.Sequence;
            entity.ProjectId = dto.ProjectId;
            entity.ProductItemStatus = dto.ItemStatus;

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