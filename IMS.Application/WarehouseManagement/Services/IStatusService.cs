using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IStatusService
    {
        Task<List<StatusDto>> GetStatusesByGroupIdAsync(int groupId);
        Task<List<StatusDto>> GetAllAsync(int groupId);
        Task<StatusDto?> GetStatusByIdAsync(int id);
        Task<StatusDto> CreateStatusAsync(StatusDto dto);
        Task<StatusDto> UpdateStatusAsync(StatusDto dto);
        Task<IEnumerable<SelectListItem>> GetSelectListByGroupIdAsync(int groupId);
        Task<bool> DeleteAsync(int id);

    }

}
