using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IReceiptOrIssueService
    {
        Task<ReceiptOrIssueDto?> GetByIdAsync(int id);


        Task<List<ReceiptOrIssueDto>> GetAllAsync(int? warehouseId = null);

        Task<ReceiptOrIssueDto> CreateAsync(ReceiptOrIssueDto dto, CancellationToken cancellationToken = default);
        Task<List<StorageSectionDto>> GetSectionsByWarehouseAsync(int warehouseId);

        List<SelectListItem> GetZonesByWarehouse(int warehouseId);


        List<SelectListItem> GetSectionsByZone(int zoneId);

        Task<ReceiptOrIssueDto?> UpdateAsync(int id, ReceiptOrIssueDto dto, CancellationToken cancellationToken = default);
        Task<List<SelectListItem>> GetGroupsByCategoryAsync(int categoryId);
        Task<List<SelectListItem>> GetStatusesByGroupAsync(int groupId);
        Task<List<SelectListItem>> GetProductsByStatus(int statusId);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
