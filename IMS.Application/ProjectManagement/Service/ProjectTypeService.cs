using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.DTOs;
using IMS.Domain.ProjectManagement.Entities;
using Microsoft.EntityFrameworkCore;
namespace IMS.Application.ProjectManagement.Service
{
    public class ProjectTypeService : IProjectTypeService
    {
        private readonly IApplicationDbContext _context;

        public ProjectTypeService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectTypeDto>> GetAllAsync()
        {
            var entities = await _context.ProjectTypes.ToListAsync();
            return entities.Select(e => new ProjectTypeDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description
            }).ToList();
        }

        public async Task<ProjectTypeDto?> GetByIdAsync(int id)
        {
            var entity = await _context.ProjectTypes.FindAsync(id);
            if (entity == null) return null;

            return new ProjectTypeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };
        }

        public async Task<int> CreateAsync(ProjectTypeDto dto, CancellationToken cancellationToken = default)
        {
            var entity = new ProjectType
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.ProjectTypes.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(ProjectTypeDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProjectTypes.FindAsync(dto.Id);
            if (entity == null) return false;

            entity.Name = dto.Name;
            entity.Description = dto.Description;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProjectTypes.FindAsync(id);
            if (entity == null) return false;

            _context.ProjectTypes.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

   
   
}
