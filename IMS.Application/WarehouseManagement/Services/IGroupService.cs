using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IGroupService
    {
        Task<List<GroupDto>> GetAllAsync(int categoryId);
        Task<IEnumerable<SelectListItem>> GetSelectListByCategoryIdAsync(int categoryId);
        Task<GroupDto?> GetByIdAsync(int id);
        Task<GroupDto> CreateAsync(GroupDto dto);
        Task<GroupDto?> UpdateAsync(int id, GroupDto dto);
        Task<bool> DeleteAsync(int id);
        Task<string?> GetFormattedCategoryCodeByIdAsync(int categoryId);
    }
}
