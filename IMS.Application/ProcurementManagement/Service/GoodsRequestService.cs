using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.ProcurementManagement.Entities;
using IMS.Domain.ProcurementManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProcurementManagement.Service
{
    public class GoodsRequestService : IGoodsRequestService
    {
        private readonly IProcurementManagementDbContext _procurementDbContext;
        private readonly IWarehouseDbContext _warehouseDbContext;
        private readonly IProjectService _projectService;

        public GoodsRequestService(
            IProcurementManagementDbContext procurementDbContext,
            IWarehouseDbContext warehouseDbContext,
            IProjectService projectService)
        {
            _procurementDbContext = procurementDbContext;
            _warehouseDbContext = warehouseDbContext;
            _projectService = projectService;
        }


        public async Task<GoodsRequestResultDto> HandleGoodsRequestAsync(GoodsRequestInputDto input)
        {
            // دریافت گزارش موجودی کالا
            var inventoryReport = await GetInventoryReportByProductAsync(input.ProductId);
            foreach (var row in inventoryReport)
            {
                row.RequestedQuantity = input.RequestedQuantity;
            }

            var totalAvailable = inventoryReport.Sum(x => x.AvailableQuantity);

            // مقدار درخواست کلی را در تمام ردیف‌ها قرار بده
            foreach (var item in inventoryReport)
            {
                item.RequestedQuantity = input.RequestedQuantity;
                
            }

            var result = new GoodsRequestResultDto
            {
                ProductId = input.ProductId,
                RequestedQuantity = input.RequestedQuantity,
                RequestedByName = input.RequestedByName,
                DepartmentName = input.DepartmentName,
                Description = input.Description,
                InventoryReport = inventoryReport,
                IsNeedPurchase = totalAvailable < input.RequestedQuantity
            };

            if (!result.IsNeedPurchase)
            {
                // ثبت درخواست کالا
                var request = new GoodsRequest
                {
                    RequestDate = DateTime.Now,
                    RequestedByName = input.RequestedByName,
                    DepartmentName = input.DepartmentName,
                    Description = input.Description,
                    Status = RequestStatus.Approved,
                    Items = new List<GoodsRequestItem>
            {
                new GoodsRequestItem
                {
                    CategoryId = input.CategoryId,
                    GroupId = input.GroupId,
                    StatusId = input.StatusId,
                    ProductId = input.ProductId,
                    Quantity = input.RequestedQuantity,
                }
            }
                };

                _procurementDbContext.GoodsRequests.Add(request);
                await _procurementDbContext.SaveChangesAsync(CancellationToken.None);

                // کسر موجودی از انبارها به ترتیب بیشترین موجودی
                var inventories = await _warehouseDbContext.Inventories
                    .Where(i => i.ProductId == input.ProductId && i.Quantity > 0)
                    .OrderByDescending(i => i.Quantity)
                    .ToListAsync();

                decimal remainingToDeduct = input.RequestedQuantity;

                foreach (var inventory in inventories)
                {
                    if (remainingToDeduct <= 0)
                        break;

                    decimal deducted = 0;

                    if (inventory.Quantity >= remainingToDeduct)
                    {
                        deducted = remainingToDeduct;
                        inventory.Quantity -= remainingToDeduct;
                        remainingToDeduct = 0;
                    }
                    else
                    {
                        deducted = inventory.Quantity;
                        remainingToDeduct -= inventory.Quantity;
                        inventory.Quantity = 0;
                    }
                }

                await _warehouseDbContext.SaveChangesAsync(CancellationToken.None);

                result.Message = "درخواست شما با موفقیت ثبت شد و موجودی به‌روزرسانی شد.";
                result.CreatedRequestId = request.Id;
            }
            else
            {
                result.Message = "موجودی کافی نیست، لطفاً درخواست خرید ثبت نمایید.";
            }

            return result;
        }



        private async Task<List<GoodsRequestInventoryReportDto>> GetInventoryReportByProductAsync(
            int productId,
            int? categoryId = null,
            int? groupId = null,
            int? statusId = null)
        {
            var query = _warehouseDbContext.Inventories
                .Include(i => i.Warehouse)
                .Include(i => i.Zone)
                .Include(i => i.Section)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .Where(i => i.ProductId == productId && i.Quantity > 0)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(i => i.Product.Status.Group.CategoryId == categoryId.Value);

            if (groupId.HasValue)
                query = query.Where(i => i.Product.Status.GroupId == groupId.Value);

            if (statusId.HasValue)
                query = query.Where(i => i.Product.StatusId == statusId.Value);

            var inventory = await query
                .Select(i => new GoodsRequestInventoryReportDto
                {
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    ZoneId = i.ZoneId,
                    ZoneName = i.Zone != null ? i.Zone.Name : null,
                    SectionId = i.SectionId,
                    SectionName = i.Section != null ? i.Section.Name : null,
                    AvailableQuantity = i.Quantity,
                    RequestedQuantity = 0  // مقدار پیشفرض
                })
                .ToListAsync();

            return inventory;
        }








        public async Task<List<SelectListItem>> GetAllGroupsAsync()
        {
            return await _warehouseDbContext.Groups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();
        }




        public async Task<List<SelectListItem>> GetAllStatusesAsync()
        {
            return await _warehouseDbContext.Statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }



        public async Task<List<SelectListItem>> GetAllProductsAsync()
        {
            return await _warehouseDbContext.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId)
        {
            return await _warehouseDbContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToListAsync();
        }
        public async Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId)
        {
            return await _warehouseDbContext.Statuses
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId)
        {
            return await _warehouseDbContext.Products
                .Where(p => p.StatusId == statusId)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToListAsync();
        }


        public async Task<List<SelectListItem>> GetAllProjectsAsync()
        {
            var projects = await _projectService.GetAllProjectsAsync();

            return projects
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProjectName
                })
                .ToList();
        }

        public async Task<List<SelectListItem>> GetAllCategoriesAsync()
        {
            return await _warehouseDbContext.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }


    }
}
