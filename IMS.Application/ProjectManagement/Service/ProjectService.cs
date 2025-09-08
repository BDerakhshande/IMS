using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Helper;
using IMS.Domain.ProjectManagement.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProjectManagement.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IApplicationDbContext _context;

        public ProjectService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectDto>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.Employer)
                .Include(p => p.ProjectType)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ProjectTypeId = p.ProjectTypeId,
                    Status = p.Status,
                    ProjectManager = p.ProjectManager,
                    ProgressPercent = p.ProgressPercent,
                    Priority = p.Priority,
                    Location = p.Location,
                    Description = p.Description,
                    EmployerId = p.EmployerId,
                    EmployerName = p.Employer.CompanyName,
                    ProjectTypeName = p.ProjectType.Name
                })
                .ToListAsync();
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(int id)
        {
            var p = await _context.Projects
                .Include(p => p.Employer)
                .Include(p => p.ProjectType)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return null;

            return new ProjectDto
            {
                Id = p.Id,
                ProjectName = p.ProjectName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ProjectTypeId = p.ProjectTypeId,
                Status = p.Status,
                ProgressPercent = p.ProgressPercent,
                Priority = p.Priority,
                Location = p.Location,
                ProjectManager = p.ProjectManager,
                Description = p.Description,
                EmployerId = p.EmployerId,
                EmployerName = p.Employer.CompanyName,
                ProjectTypeName = p.ProjectType.Name
            };
        }

        public async Task<bool> CreateProjectAsync(ProjectDto dto)
        {
            var project = new Project
            {
                ProjectName = dto.ProjectName,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ProjectTypeId = dto.ProjectTypeId.Value,
                Status = dto.Status,
                ProjectManager = dto.ProjectManager,
                ProgressPercent = dto.ProgressPercent,
                Priority = dto.Priority,
                Location = dto.Location,
                Description = dto.Description,
                EmployerId = dto.EmployerId.Value
            };

            _context.Projects.Add(project);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        public async Task<bool> UpdateProjectAsync(ProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(dto.Id);
            if (project == null)
                return false;

            project.ProjectName = dto.ProjectName;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.ProjectTypeId = dto.ProjectTypeId.Value;
            project.Status = dto.Status;
            project.ProjectManager = dto.ProjectManager;
            project.ProgressPercent = dto.ProgressPercent;
            project.Priority = dto.Priority;
            project.Location = dto.Location;
            project.Description = dto.Description;
            project.EmployerId = dto.EmployerId.Value;

            _context.Projects.Update(project);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return false;

            _context.Projects.Remove(project);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }



        public async Task<List<ProjectReportDto>> GetProjectReportAsync(ProjectReportFilterDto filter)
        {
            var query = _context.Projects
                .Include(p => p.Employer)
                .Include(p => p.ProjectType)
                .AsQueryable();

            // 📌 فیلتر تاریخ شروع
            if (filter.StartDateFrom.HasValue)
                query = query.Where(p => p.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                query = query.Where(p => p.StartDate <= filter.StartDateTo.Value);

            // 📌 فیلتر تاریخ پایان
            if (filter.EndDateFrom.HasValue)
                query = query.Where(p => p.EndDate >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                query = query.Where(p => p.EndDate <= filter.EndDateTo.Value);

            // 📌 فیلتر کارفرما
            if (filter.EmployerId.HasValue)
                query = query.Where(p => p.EmployerId == filter.EmployerId.Value);

            // 📌 فیلتر نوع پروژه
            if (filter.ProjectTypeId.HasValue)
                query = query.Where(p => p.ProjectTypeId == filter.ProjectTypeId.Value);

            // 📌 فیلتر نام پروژه
            if (!string.IsNullOrEmpty(filter.ProjectName))
                query = query.Where(p => p.ProjectName.ToLower().Contains(filter.ProjectName.ToLower()));

            // 📌 فیلتر مدیر پروژه
            if (!string.IsNullOrEmpty(filter.ProjectManager))
                query = query.Where(p => p.ProjectManager.ToLower().Contains(filter.ProjectManager.ToLower()));

            // 📌 فیلتر وضعیت پروژه
            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            // 📌 فیلتر نام کارفرما (متنی)
            if (!string.IsNullOrEmpty(filter.EmployerName))
                query = query.Where(p => p.Employer != null &&
                                         p.Employer.CompanyName.ToLower().Contains(filter.EmployerName.ToLower()));

            // 📌 فیلتر نام نوع پروژه (متنی)
            if (!string.IsNullOrEmpty(filter.ProjectTypeName))
                query = query.Where(p => p.ProjectType != null &&
                                         p.ProjectType.Name.ToLower().Contains(filter.ProjectTypeName.ToLower()));

            // 📌 مرتب‌سازی پیش‌فرض بر اساس تاریخ شروع (نزولی)
            query = query.OrderByDescending(p => p.StartDate);

            // 📌 انتخاب خروجی
            var result = await query.Select(p => new ProjectReportDto
            {
                ProjectName = p.ProjectName,
                EmployerName = p.Employer != null ? p.Employer.CompanyName : "",
                ProjectTypeName = p.ProjectType != null ? p.ProjectType.Name : "",
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status.GetDisplayName(), // 📌 نمایش فارسی وضعیت
                ProjectManager = p.ProjectManager
            }).ToListAsync();

            return result;
        }




    }
}
