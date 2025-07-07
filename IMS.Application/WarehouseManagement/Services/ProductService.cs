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
    public class ProductService : IProductService
    {
        private readonly IWarehouseDbContext _context;

        public ProductService(IWarehouseDbContext context)
        {
            _context = context;
        }


        public async Task<List<ProductDto>> GetAllAsync(int statusId)
        {
            var rawData = await _context.Products
     .AsNoTracking()
     .Where(p => p.StatusId == statusId)
     .Include(p => p.Status)
         .ThenInclude(s => s.Group)
             .ThenInclude(g => g.Category)
     .Select(p => new ProductDto
     {
         Id = p.Id,
         Name = p.Name,
         Code = p.Code,
         Description = p.Description,
         Price = p.Price,

         StatusId = p.StatusId,
         StatusCode = p.Status.Code,
         StatusName = p.Status.Name,

         GroupId = p.Status.Group.Id,
         GroupCode = p.Status.Group.Code,
         GroupName = p.Status.Group.Name,

         CategoryId = p.Status.Group.Category.Id,
         CategoryCode = p.Status.Group.Category.Code,
         CategoryName = p.Status.Group.Category.Name
     })
     .OrderBy(p => p.CategoryName)
     .ThenBy(p => p.GroupName)
     .ThenBy(p => p.Code)
     .ToListAsync();


            return rawData;
        }




        public async Task<ProductDto> CreateAsync(ProductDto dto)
        {
            // 1. دریافت وضعیت به همراه گروه و دسته‌بندی آن
            var status = await _context.Statuses
                .Include(s => s.Group)
                .ThenInclude(g => g.Category)
                .FirstOrDefaultAsync(s => s.Id == dto.StatusId);

            if (status == null)
                throw new Exception("وضعیت انتخاب‌شده یافت نشد.");

            // 2. ایجاد موجودیت محصول
            var product = new Product
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                Price = dto.Price,
                StatusId = dto.StatusId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(CancellationToken.None); 

            // 3. ایجاد موجودی با مقدار صفر برای تمام ترکیب‌های ممکن
            var warehouses = await _context.Warehouses
                .Include(w => w.Zones)
                    .ThenInclude(z => z.Sections)
                .ToListAsync();

            var inventoryList = new List<Inventory>();

            foreach (var warehouse in warehouses)
            {
                foreach (var zone in warehouse.Zones)
                {
                    foreach (var section in zone.Sections)
                    {
                        inventoryList.Add(new Inventory
                        {
                            ProductId = product.Id,
                            WarehouseId = warehouse.Id,
                            ZoneId = zone.Id,
                            SectionId = section.Id,
                            Quantity = 0
                        });
                    }
                }
            }

            if (inventoryList.Any())
            {
                _context.Inventories.AddRange(inventoryList);
                await _context.SaveChangesAsync(CancellationToken.None);
            }

            // 4. بازگرداندن DTO
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Code = product.Code,
                Description = product.Description,
                Price = product.Price,

                StatusId = status.Id,
                StatusCode = status.Code,
                StatusName = status.Name,

                GroupId = status.Group.Id,
                GroupCode = status.Group.Code,
                GroupName = status.Group.Name,

                CategoryId = status.Group.Category.Id,
                CategoryCode = status.Group.Category.Code,
                CategoryName = status.Group.Category.Name
            };
        }






        public async Task UpdateAsync(ProductDto dto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (product == null)
                throw new Exception("کالای مورد نظر یافت نشد.");

           

            // به‌روزرسانی فیلدها
            product.Name = dto.Name;
            product.Code = dto.Code;
            product.Description = dto.Description;
            product.StatusId = dto.StatusId;
            product.Price = dto.Price;
           
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Status) // اینجا فقط وضعیت را لود کردی
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Code = product.Code,
                Description = product.Description,
                Price = product.Price,
                // اینجا فیلدهای زیر را ندادی
                StatusId = product.StatusId,  // اضافه کن
                StatusCode = product.Status?.Code ?? "",  // اضافه کن
                StatusName = product.Status?.Name ?? "",  // اضافه کن

                // همچنین اگر ProductDto فیلدهای مربوط به Group و Category را داره، باید آنها را هم بیاوری:
                GroupId = product.Status?.Group?.Id ?? 0,
                GroupCode = product.Status?.Group?.Code ?? "",
                GroupName = product.Status?.Group?.Name ?? "",

                CategoryId = product.Status?.Group?.Category?.Id ?? 0,
                CategoryCode = product.Status?.Group?.Category?.Code ?? "",
                CategoryName = product.Status?.Group?.Category?.Name ?? "",
            };
        }





        public async Task<IEnumerable<StatusDto>> GetStatusesAsync()
        {
            return await _context.Statuses
                .Select(s => new StatusDto { Id = s.Id, Name = s.Name })
                .ToListAsync();
        }



        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new Exception("محصول مورد نظر یافت نشد.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(CancellationToken.None);
        }



        public async Task<List<ProductDto>> GetByStatusIdAsync(int statusId)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.StatusId == statusId)
                .Include(p => p.Status)
                    .ThenInclude(s => s.Group)
                        .ThenInclude(g => g.Category)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Description = p.Description,
                    StatusId = p.StatusId,
                    StatusName = p.Status.Name,
                    GroupId = p.Status.Group.Id,
                    GroupName = p.Status.Group.Name,
                    CategoryId = p.Status.Group.Category.Id,
                    CategoryName = p.Status.Group.Category.Name,
                    Price = p.Price,
                    
                })
                .OrderBy(p => p.CategoryName)
                .ThenBy(p => p.GroupName)
                .ThenBy(p => p.StatusName)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }
        public async Task<IEnumerable<SelectListItem>> GetSelectListByStatusIdAsync(int statusId)
        {
            var products = await _context.Products
                .Where(p => p.StatusId == statusId)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} ({p.Code})"
                })
                .ToListAsync();

            return products;
        }



    }
}
