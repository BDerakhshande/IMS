using IMS.Application.WarehouseManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IInventoryOperationService
    {
        Task<bool> AddAsync(InventoryCreateDto dto);
        Task<decimal> GetQuantityAsync(int productId, int warehouseId, int? zoneId, int? sectionId);
        Task<InventoryCreateDto> LoadAsync(InventoryCreateDto inputDto);


    }
}
