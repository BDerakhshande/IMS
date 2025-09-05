using IMS.Application.WarehouseManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IUnitService
    {
        Task<List<UnitDto>> GetAllAsync();
        Task<UnitDto?> GetByIdAsync(int id);
        Task<UnitDto> CreateAsync(UnitDto dto);
        Task UpdateAsync(UnitDto dto);
        Task DeleteAsync(int id);
    }
}
