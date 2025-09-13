using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<SelectListItem>> GetSelectListAsync();
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryDto dto);
        Task<bool> UpdateAsync(int id, CategoryDto dto);
        Task DeleteAsync(int categoryId);
        Task<bool> IsCodeExistsAsync(string code);
        Task<string> GenerateNextCodeAsync<TEntity>(
        Expression<Func<TEntity, string>> codeSelector,
        Expression<Func<TEntity, int>> orderSelector
        ) where TEntity : class;
    }

}
