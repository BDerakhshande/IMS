using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.DTOs;

namespace IMS.Application.ProjectManagement.Service
{
    public interface IEmployerService
    {
        Task<List<EmployerDto>> GetAllEmployersAsync();
        Task<EmployerDto?> GetEmployerByIdAsync(int id);
        Task<int> CreateEmployerAsync(EmployerDto employerDto);
        Task<bool> UpdateEmployerAsync(EmployerDto employerDto);
        Task<bool> DeleteEmployerAsync(int id);
    }
}
