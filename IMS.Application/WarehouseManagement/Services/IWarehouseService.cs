using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IWarehouseService
    {
        Task<IEnumerable<SelectListItem>> GetSelectListAsync();

        Task<int> CreateWarehouseAsync(WarehouseDto dto);
        Task<List<StorageZoneDto>> GetAllZonesAsync();
        Task<List<StorageSectionDto>> GetSectionsByZoneAsync(int zoneId);
        Task<int> CreateZoneAsync(StorageZoneDto dto);
        Task<StorageZoneDto> GetZoneByIdAsync(int id);

        Task UpdateZoneAsync(StorageZoneDto dto);

        Task<int> CreateSectionAsync(StorageSectionDto dto);
        Task<List<WarehouseDto>> GetAllWarehousesWithHierarchyAsync();
        Task<List<StorageZoneDto>> GetZonesByWarehouseIdAsync(int warehouseId);
        Task UpdateWarehouseAsync(WarehouseDto dto);
        Task DeleteWarehouseAsync(int id);
        Task DeleteZoneAsync(int id);

        Task<List<WarehouseDto>> GetAllWarehousesAsync();
        Task<WarehouseDto?> GetWarehouseHierarchyAsync(int warehouseId);

        Task<StorageSectionDto> GetSectionByIdAsync(int id);


       
        Task UpdateSectionAsync(StorageSectionDto dto);
        Task DeleteSectionAsync(int id);


        Task<List<StorageSectionDto>> GetAllSectionsAsync();


        Task<List<StorageSectionDto>> GetSectionsByWarehouseIdAsync(int warehouseId);
    }
}
