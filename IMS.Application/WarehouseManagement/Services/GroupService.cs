using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class GroupService : IGroupService
    {
        private readonly IWarehouseDbContext _context;

        public GroupService(IWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<List<GroupDto>> GetAllAsync(int categoryId)
        {
            var rawData = await _context.Groups
                .Where(g => g.CategoryId == categoryId)
                .Include(g => g.Category)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Code,
                    g.CategoryId,
                    CategoryCodeRaw = g.Category != null ? g.Category.Code : null
                })
                .ToListAsync();

            Console.WriteLine($"GetAllAsync called with CategoryId: {categoryId}, RawData Count: {rawData?.Count ?? 0}");

            if (rawData == null)
            {
                Console.WriteLine("RawData is null, returning empty list.");
                return new List<GroupDto>();
            }

            var result = rawData.Select(g =>
            {
                string formattedCategoryCode;
                if (int.TryParse(g.CategoryCodeRaw, out var num))
                    formattedCategoryCode = $"C{num:D2}";
                else if (!string.IsNullOrEmpty(g.CategoryCodeRaw))
                    formattedCategoryCode = $"C{g.CategoryCodeRaw}";
                else
                    formattedCategoryCode = "C00";

                return new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Code = g.Code,
                    CategoryId = g.CategoryId,
                    CategoryCode = formattedCategoryCode
                };
            }).ToList();

            Console.WriteLine($"Returning {result.Count} groups for CategoryId: {categoryId}");
            return result;
        }





        public async Task<GroupDto?> GetByIdAsync(int id)
        {
            var rawGroup = await _context.Groups
                .Where(g => g.Id == id)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Code, 
                    g.CategoryId,
                    CategoryCodeRaw = g.Category.Code
                })
                .FirstOrDefaultAsync();

            if (rawGroup == null)
                return null;

            // اعمال فرمت به صورت دستی
            string formattedCategoryCode;
            if (int.TryParse(rawGroup.CategoryCodeRaw, out var num))
                formattedCategoryCode = "C" + num.ToString("D2");
            else
                formattedCategoryCode = "C" + rawGroup.CategoryCodeRaw;

            return new GroupDto
            {
                Id = rawGroup.Id,
                Name = rawGroup.Name,
                Code = rawGroup.Code, 
                CategoryId = rawGroup.CategoryId,
                CategoryCode = formattedCategoryCode
            };
        }




        public async Task<GroupDto> CreateAsync(GroupDto dto)
        {
            var entity = new Group
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Code = dto.Code  
            };

            _context.Groups.Add(entity);
            await _context.SaveChangesAsync(CancellationToken.None);

            var categoryCode = await _context.Categories
                .Where(c => c.Id == dto.CategoryId)
                .Select(c => c.Code)
                .FirstOrDefaultAsync();

            dto.Id = entity.Id;
            dto.Code = entity.Code;   
            dto.CategoryCode = $"C{categoryCode?.PadLeft(2, '0')}";

            return dto;
        }





        public async Task<string?> GetFormattedCategoryCodeByIdAsync(int categoryId)
        {
            var categoryCode = await _context.Categories
                .Where(c => c.Id == categoryId)
                .Select(c => c.Code)
                .FirstOrDefaultAsync();

            if (categoryCode == null)
                return null;

            return $"C{categoryCode.PadLeft(2, '0')}";
        }





        public async Task<GroupDto?> UpdateAsync(int id, GroupDto dto)
        {
            var entity = await _context.Groups.FindAsync(id);
            if (entity == null)
                return null;

            var codeToSearch = dto.CategoryCode?.Trim().ToUpperInvariant() ?? "";

            if (codeToSearch.StartsWith("C"))
                codeToSearch = codeToSearch.Substring(1);

            if (int.TryParse(codeToSearch, out var numericCode))
                codeToSearch = numericCode.ToString();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == codeToSearch);

            if (category == null)
                throw new InvalidOperationException("کد دسته‌بندی وارد شده وجود ندارد.");

            // به‌روزرسانی تمام فیلدها
            entity.Name = dto.Name;
            entity.CategoryId = category.Id;
            entity.Code = dto.Code; // ← اضافه کردن به‌روزرسانی Code

            await _context.SaveChangesAsync(CancellationToken.None);

            return new GroupDto
            {
                Id = entity.Id,
                Name = entity.Name,
                CategoryId = category.Id,
                CategoryCode = int.TryParse(category.Code, out var n) ? $"C{n:D2}" : $"C{category.Code}",
                Code = entity.Code
            };
        }


        public async Task<IEnumerable<SelectListItem>> GetSelectListByCategoryIdAsync(int categoryId)
        {
            var groups = await _context.Groups
                .Where(g => g.CategoryId == categoryId)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = $"{g.Name} ({g.Code})"
                })
                .ToListAsync();

            return groups;
        }





        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Groups.FindAsync(id);
            if (entity == null) return false;

            // بررسی وجود وضعیت‌های وابسته به این گروه
            bool hasStatuses = await _context.Statuses.AnyAsync(s => s.GroupId == id);
            if (hasStatuses)
                throw new InvalidOperationException("گروه دارای وضعیت‌های وابسته است و قابل حذف نیست.");

            _context.Groups.Remove(entity);
            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }

    }

}

