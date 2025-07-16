using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IInventoryTurnoverService
    {
        Task<List<InventoryTurnoverDto>> GetInventoryTurnoverAsync(InventoryTurnoverFilterDto filter);

        Task<List<SelectListItem>> GetZonesByWarehouseIdAsync(int warehouseId);

        Task<List<SelectListItem>> GetSectionsByZoneIdsAsync(List<int> zoneIds);
        Task<List<SelectListItem>> GetWarehousesAsync();
    }

}
