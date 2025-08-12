using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;

namespace IMS.Application.ProcurementManagement.Service
{
    public interface IRequestTypeService
    {
        Task<List<RequestTypeDto>> GetAllAsync();
        Task<RequestTypeDto?> GetByIdAsync(int id);
        Task<RequestTypeDto> CreateAsync(RequestTypeDto dto);
        Task<bool> UpdateAsync(RequestTypeDto dto);
        Task<bool> DeleteAsync(int id);
    }

}
