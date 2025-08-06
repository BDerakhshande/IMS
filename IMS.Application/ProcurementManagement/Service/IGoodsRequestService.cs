using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Domain.ProcurementManagement.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.ProcurementManagement.Service
{
    public interface IGoodsRequestService
    {
        Task<GoodsRequestResultDto> HandleGoodsRequestAsync(GoodsRequestInputDto input);
        Task<List<SelectListItem>> GetAllGroupsAsync();
        Task<List<SelectListItem>> GetAllStatusesAsync();
        Task<List<SelectListItem>> GetAllProjectsAsync();
        Task<List<SelectListItem>> GetAllCategoriesAsync();
        Task<List<SelectListItem>> GetAllProductsAsync();
        Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId);
        Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId);
        Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId);
    }
}
