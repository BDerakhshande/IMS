using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Domain.ProcurementManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProcurementManagement.Service
{
    public class RequestTypeService : IRequestTypeService
    {
        private readonly IProcurementManagementDbContext _context;  

        public RequestTypeService(IProcurementManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<RequestTypeDto>> GetAllAsync()
        {
            return await _context.RequestTypes
                .Select(rt => new RequestTypeDto { Id = rt.Id, Name = rt.Name })
                .ToListAsync();
        }

        public async Task<RequestTypeDto?> GetByIdAsync(int id)
        {
            var entity = await _context.RequestTypes.FindAsync(id);
            if (entity == null)
                return null;

            return new RequestTypeDto { Id = entity.Id, Name = entity.Name };
        }

        public async Task<RequestTypeDto> CreateAsync(RequestTypeDto dto)
        {
            var entity = new RequestType { Name = dto.Name };
            _context.RequestTypes.Add(entity);
            await _context.SaveChangesAsync(CancellationToken.None);

            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateAsync(RequestTypeDto dto)
        {
            var entity = await _context.RequestTypes.FindAsync(dto.Id);
            if (entity == null)
                return false;

            entity.Name = dto.Name;
            _context.RequestTypes.Update(entity);
            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.RequestTypes.FindAsync(id);
            if (entity == null)
                return false;

            _context.RequestTypes.Remove(entity);
            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }
    }
}
