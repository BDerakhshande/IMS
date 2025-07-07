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
    public class StatusService : IStatusService
    {
        private readonly IWarehouseDbContext _context;

        public StatusService(IWarehouseDbContext context)
        {
            _context = context;
        }
        public async Task<List<StatusDto>> GetAllAsync(int groupId)
        {
            var rawData = await _context.Statuses
                .Where(s => s.GroupId == groupId) // فیلتر بر اساس groupId
                .Include(s => s.Group)
                    .ThenInclude(g => g.Category)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Code,
                    GroupId = s.Group.Id,
                    GroupName = s.Group.Name,
                    GroupCode = s.Group.Code, // اضافه شده
                    CategoryId = s.Group.Category.Id,
                    CategoryName = s.Group.Category.Name,
                    CategoryCodeRaw = s.Group.Category.Code
                })
                .ToListAsync();

            return rawData.Select(s =>
            {
                string formattedCategoryCode;
                if (int.TryParse(s.CategoryCodeRaw, out var num))
                    formattedCategoryCode = num.ToString("D2"); // مثلاً "05" вместо "C05"
                else if (!string.IsNullOrEmpty(s.CategoryCodeRaw))
                    formattedCategoryCode = s.CategoryCodeRaw;
                else
                    formattedCategoryCode = "00";

                return new StatusDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    GroupId = s.GroupId,
                    GroupName = s.GroupName,
                    GroupCode = s.GroupCode, // اضافه شده
                    CategoryId = s.CategoryId,
                    CategoryName = s.CategoryName,
                    CategoryCode = formattedCategoryCode
                };
            })
            .OrderBy(s => s.CategoryName)
            .ThenBy(s => s.GroupName)
            .ThenBy(s => s.Code)
            .ToList();
        }


        public async Task<List<StatusDto>> GetStatusesByGroupIdAsync(int groupId)
        {
            return await _context.Statuses
                .Where(s => s.GroupId == groupId)
                .Include(s => s.Group)
                    .ThenInclude(g => g.Category)
                .Select(s => new StatusDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,                            // کد وضعیت
                    GroupId = s.Group.Id,
                    GroupName = s.Group.Name,
                    CategoryId = s.Group.Category.Id,
                    CategoryName = s.Group.Category.Name,
                    CategoryCode = s.Group.Category.Code
                })
                .AsNoTracking()
                .ToListAsync();
        }



        public async Task<StatusDto> CreateStatusAsync(StatusDto dto)
        {
            // بررسی وجود گروه
            var group = await _context.Groups
                .Where(g => g.Id == dto.GroupId)
                .FirstOrDefaultAsync();

            if (group == null)
                throw new Exception("گروه مورد نظر یافت نشد.");

            // ایجاد موجودیت وضعیت
            var status = new Status
            {
                Name = dto.Name,
                GroupId = dto.GroupId,
                Code = dto.Code
            };

            _context.Statuses.Add(status);
            await _context.SaveChangesAsync(CancellationToken.None);

            // مقداردهی نهایی DTO
            dto.Id = status.Id;
            dto.Code = status.Code;
            dto.GroupCode = $"G{group.Code.PadLeft(2, '0')}";

            return dto;
        }




        public async Task<StatusDto?> GetStatusByIdAsync(int id)
        {
            return await _context.Statuses
                .Where(s => s.Id == id)
                .Include(s => s.Group)
                    .ThenInclude(g => g.Category)
                .Select(s => new StatusDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    GroupId = s.Group.Id,
                    GroupName = s.Group.Name,
                    CategoryId = s.Group.Category.Id,
                    CategoryName = s.Group.Category.Name,
                    Code = s.Code
                   
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<StatusDto> UpdateStatusAsync(StatusDto dto)
        {
            // بررسی ورودی
            if (dto.Id <= 0)
                throw new ArgumentException("شناسه وضعیت نامعتبر است");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("نام وضعیت نمی‌تواند خالی باشد");

            var status = await _context.Statuses
                .Include(s => s.Group)
                    .ThenInclude(g => g.Category)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (status == null)
                throw new ArgumentException("وضعیتی با این شناسه یافت نشد");

            // بررسی وجود گروه
            var group = await _context.Groups
                .Include(g => g.Category)
                .FirstOrDefaultAsync(g => g.Id == dto.GroupId);

            if (group == null)
                throw new ArgumentException("گروه انتخاب‌شده معتبر نیست");

            // بررسی تکراری نبودن نام در همان گروه (به جز خودش)
            bool isDuplicate = await _context.Statuses
                .AnyAsync(s => s.Id != dto.Id && s.GroupId == dto.GroupId && s.Name == dto.Name.Trim());

            if (isDuplicate)
                throw new InvalidOperationException("وضعیت دیگری با این نام در همین گروه وجود دارد");

            // به‌روزرسانی وضعیت
            status.Name = dto.Name.Trim();
            status.GroupId = dto.GroupId;
            status.Code = dto.Code;

            await _context.SaveChangesAsync(CancellationToken.None);

            // بازگرداندن نتیجه
            return new StatusDto
            {
                Id = status.Id,
                Name = status.Name,
                GroupId = group.Id,
                GroupName = group.Name,
                CategoryId = group.Category.Id,
                CategoryName = group.Category.Name,
                Code = dto.Code
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Statuses.FindAsync(id);
            if (entity == null)
                return false;

            bool hasProducts = await _context.Products.AnyAsync(p => p.StatusId == id);
            if (hasProducts)
                throw new InvalidOperationException("این وضعیت توسط محصولات استفاده شده و قابل حذف نیست.");

            _context.Statuses.Remove(entity);
            await _context.SaveChangesAsync(CancellationToken.None);

            return true;
        }




        public async Task<IEnumerable<SelectListItem>> GetSelectListByGroupIdAsync(int groupId)
        {
            return await _context.Statuses
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Name} ({s.Code})"
                })
                .ToListAsync();
        }

    }
}

