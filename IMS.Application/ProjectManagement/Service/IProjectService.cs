using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.DTOs;

namespace IMS.Application.ProjectManagement.Service
{
    public interface IProjectService
    {
        Task<List<ProjectDto>> GetAllProjectsAsync();

        Task<ProjectDto?> GetProjectByIdAsync(int id);

        Task<bool> CreateProjectAsync(ProjectDto dto);

        Task<bool> UpdateProjectAsync(ProjectDto dto);

        Task<bool> DeleteProjectAsync(int id);
        Task<List<ProjectReportDto>> GetProjectReportAsync(ProjectReportFilterDto filter);

    }
}
