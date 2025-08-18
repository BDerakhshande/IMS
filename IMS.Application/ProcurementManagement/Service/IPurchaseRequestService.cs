using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.ProcurementManagement.Service
{
    public interface IPurchaseRequestService
    {
        Task<List<PurchaseRequestDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PurchaseRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(PurchaseRequestDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(PurchaseRequestDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<List<SelectListItem>> GetGroupsByCategoryAsync(int categoryId);
        Task<List<SelectListItem>> GetProductsByStatus(int statusId);
        Task<List<SelectListItem>> GetStatusesByGroupAsync(int groupId);
        Task<List<SelectListItem>> GetCategoriesAsync();
      
    }
}
