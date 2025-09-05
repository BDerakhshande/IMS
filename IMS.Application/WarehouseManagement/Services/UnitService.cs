using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.Services
{
    public class UnitService : IUnitService
    {
        private readonly IWarehouseDbContext _context;

        public UnitService(IWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<List<UnitDto>> GetAllAsync()
        {
            return await _context.Units
                .AsNoTracking()
                .Select(u => new UnitDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Symbol = u.Symbol ?? ""
                })
                .ToListAsync();
        }

        public async Task<UnitDto?> GetByIdAsync(int id)
        {
            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
                return null;

            return new UnitDto
            {
                Id = unit.Id,
                Name = unit.Name,
                Symbol = unit.Symbol ?? ""
            };
        }

        public async Task<UnitDto> CreateAsync(UnitDto dto)
        {
            var unit = new Unit
            {
                Name = dto.Name,
                Symbol = dto.Symbol
            };

            _context.Units.Add(unit);
            await _context.SaveChangesAsync(CancellationToken.None);

            dto.Id = unit.Id;
            return dto;
        }

        public async Task UpdateAsync(UnitDto dto)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (unit == null)
                throw new System.Exception("واحد مورد نظر یافت نشد.");

            unit.Name = dto.Name;
            unit.Symbol = dto.Symbol;

            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteAsync(int id)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
            if (unit == null)
                throw new System.Exception("واحد مورد نظر یافت نشد.");

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
