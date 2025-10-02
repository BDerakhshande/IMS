using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IProductItemService
    {
        Task<ProductItemDto?> GetByIdAsync(int id);
        Task<List<ProductItemDto>> GetByProductIdAsync(int productId);
        Task<ProductItemDto?> UpdateAsync(ProductItemDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<SelectListItem>> GetProjectsAsync();
    }
}
