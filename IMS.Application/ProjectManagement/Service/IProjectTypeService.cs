using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.DTOs;

namespace IMS.Application.ProjectManagement.Service
{
    public interface IProjectTypeService
    {
        Task<List<ProjectTypeDto>> GetAllAsync();
        Task<ProjectTypeDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(ProjectTypeDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(ProjectTypeDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
