using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.ProcurementManagement.Service
{
    public interface IPurchaseRequestFlatItemService
    {
        Task<List<PurchaseRequestFlatItemDto>> GetFlatItemsAsync(
          string? requestNumber = null,
          DateTime? fromDate = null,
          DateTime? toDate = null,
          int? requestTypeId = null,
          int? projectId = null,
          List<ProductFilterDto>? products = null,  // ← لیست محصولات
          CancellationToken cancellationToken = default);




        Task<List<SelectListItem>> GetAllCategoriesAsync();
        Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId);
        Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId);
        Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId);
    }
}
