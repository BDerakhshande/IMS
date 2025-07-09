using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IInventoryTransactionReportService
    {
        Task<List<InventoryTransactionReportDto>> GetReportAsync(
            string? warehouseName = null,
            string? departmentName = null,
            string? sectionName = null,
            string? categoryName = null,
            string? groupName = null,
            string? statusName = null,
            string? productName = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? documentType = null);





        Task<List<SelectListItem>> GetZonesByWarehouseIdAsync(int warehouseId);
        Task<List<SelectListItem>> GetAllZonesAsync();
        Task<List<SelectListItem>> GetAllSectionsAsync();
        Task<List<SelectListItem>> GetAllGroupsAsync();
        Task<List<SelectListItem>> GetAllStatusesAsync();
        Task<List<SelectListItem>> GetAllProductsAsync();
        Task<List<SelectListItem>> GetSectionsByZoneIdsAsync(List<int> zoneIds);
        Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId);
        Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId);
        Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId);
    }
}
