using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IInventoryService
    {
        Task CreateAsync(InventoryCreateDto dto);

        Task<InventoryCreateDto> LoadOrCreateAsync(InventoryCreateDto inputDto);
        Task<bool> UpdateQuantityAsync(int productId, int warehouseId, int? zoneId, int? sectionId, decimal newQuantity);
        Task<int> GetQuantityAsync(int productId, int warehouseId, int? zoneId, int? sectionId);


    }
}
