using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IInventoryReportService
    {

        Task<List<InventoryReportItemDto>> GetInventoryReportAsync(
    InventoryReportFilterDto? filter = null,
    CancellationToken cancellationToken = default);

        
        Task<List<SelectListItem>> GetZonesByWarehouseIdAsync(int warehouseId);

        Task<List<SelectListItem>> GetAllSectionsAsync();
        Task<List<SelectListItem>> GetAllZonesAsync();
        Task<List<SelectListItem>> GetAllGroupsAsync();
        Task<List<SelectListItem>> GetAllStatusesAsync();
        Task<List<SelectListItem>> GetAllProductsAsync();
      
    }
}