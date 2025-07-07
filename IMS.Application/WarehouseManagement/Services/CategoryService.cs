using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly IWarehouseDbContext _context;

        public CategoryService(IWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Groups = c.Groups
                        .Select(g => new GroupDto
                        {
                            Id = g.Id,
                            Name = g.Name,
                            CategoryId = c.Id
                        })
                        .ToList()
                })
                .ToListAsync();
        }



        // دریافت یک دسته‌بندی بر اساس Id
        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null) return null;

            return new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code
            };
        }

        public async Task<CategoryDto> CreateAsync(CategoryDto dto)
        {
            var entity = new Category
            {
                Name = dto.Name,
                Code = dto.Code?.Trim() ?? "" // اطمینان از اینکه خالی نیست
            };

            _context.Categories.Add(entity);
            await _context.SaveChangesAsync(CancellationToken.None);

            return new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code // ✅ باید از entity گرفته شود
            };
        }



        public async Task<bool> IsCodeExistsAsync(string code)
        {
            return await _context.Categories.AnyAsync(c => c.Code == code);
        }



        // ویرایش دسته‌بندی
        public async Task<bool> UpdateAsync(int id, CategoryDto dto)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null) return false;

            entity.Name = dto.Name;
            entity.Code = dto.Code;
            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }

        public async Task DeleteAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Groups)
                    .ThenInclude(g => g.Statuses)
                        .ThenInclude(s => s.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                throw new Exception("دسته‌بندی یافت نشد.");

            // بررسی وابستگی‌ها:
            bool hasDependencies =
                category.Groups.Any() || // اگر گروه دارد
                category.Groups.Any(g => g.Statuses.Any()) || // اگر وضعیت دارد
                category.Groups.Any(g => g.Statuses.Any(s => s.Products.Any())); // اگر کالا دارد

            if (hasDependencies)
                throw new Exception("امکان حذف دسته‌بندی وجود ندارد چون به گروه، وضعیت یا کالا وابسته است.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(CancellationToken.None);
        }



        public async Task<IEnumerable<SelectListItem>> GetSelectListAsync()
        {
            return await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Name} ({c.Code})"
                })
                .ToListAsync();
        }


    }
}
