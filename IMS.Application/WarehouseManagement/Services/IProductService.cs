using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync(int statusId);
        Task<ProductDto> CreateAsync(ProductDto dto);

        Task<IEnumerable<SelectListItem>> GetSelectListByStatusIdAsync(int statusId);
        Task UpdateAsync(ProductDto dto);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<IEnumerable<StatusDto>> GetStatusesAsync();
        Task DeleteAsync(int id);
        Task<List<ProductDto>> GetByStatusIdAsync(int statusId);
       
    }
}

