using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
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
                .Include(p => p.Unit)
             .Select(p => new ProductDto
             {
                 Id = p.Id,
                 Name = p.Name,
                 Code = p.Code,
                 Description = p.Description,
                 Price = p.Price,

                 //IsUnique = p.IsUnique, // اضافه شد

                 StatusId = p.StatusId,
                 StatusCode = p.Status.Code,
                 StatusName = p.Status.Name,

                 GroupId = p.Status.Group.Id,
                 GroupCode = p.Status.Group.Code,
                 GroupName = p.Status.Group.Name,

                 CategoryId = p.Status.Group.Category.Id,
                 CategoryCode = p.Status.Group.Category.Code,
                 CategoryName = p.Status.Group.Category.Name,

                 Unit = new UnitDto
                 {
                     Id = p.Unit.Id,
                     Name = p.Unit.Name,
                     Symbol = p.Unit.Symbol
                 }
             })

                .OrderBy(p => p.CategoryName)
                .ThenBy(p => p.GroupName)
                .ThenBy(p => p.Code)
                .ToListAsync();

            return rawData;
        }

        public async Task<ProductDto> CreateAsync(ProductDto dto)
        {
            // بررسی تکراری بودن کد محصول
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Code == dto.Code && p.StatusId == dto.StatusId);

            if (existingProduct != null)
                throw new Exception("کدی که وارد کرده‌اید در این وضعیت قبلاً ثبت شده است.");

            // بررسی وضعیت
            var status = await _context.Statuses
                .Include(s => s.Group)
                    .ThenInclude(g => g.Category)
                .FirstOrDefaultAsync(s => s.Id == dto.StatusId);

            if (status == null)
                throw new Exception("وضعیت انتخاب‌شده یافت نشد.");

            // ایجاد محصول
            var product = new Product
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                Price = dto.Price,
                StatusId = dto.StatusId,
                UnitId = dto.Unit?.Id ?? 1,
                //IsUnique = dto.IsUnique
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(CancellationToken.None);

            // ایجاد Inventory صفر برای همه ترکیب‌های انبار
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

            // بازگرداندن DTO کامل بدون ایجاد ProductItem خودکار
            var unit = await _context.Units.FindAsync(product.UnitId);

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
                CategoryName = status.Group.Category.Name,

                Unit = new UnitDto
                {
                    Id = unit?.Id ?? 1,
                    Name = unit?.Name ?? "عدد",
                    Symbol = unit?.Symbol ?? "pcs"
                },

                // اینجا ProductItems را مقداردهی می‌کنیم به یک لیست خالی
                ProductItems = new List<ProductItemDto>()
            };

        }



        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Status)
                    .ThenInclude(s => s.Group)
                        .ThenInclude(g => g.Category)
                .Include(p => p.Unit)
                .Include(p => p.ProductItems) 
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

                StatusId = product.StatusId,
                StatusCode = product.Status?.Code ?? "",
                StatusName = product.Status?.Name ?? "",

                GroupId = product.Status?.Group?.Id ?? 0,
                GroupCode = product.Status?.Group?.Code ?? "",
                GroupName = product.Status?.Group?.Name ?? "",

                CategoryId = product.Status?.Group?.Category?.Id ?? 0,
                CategoryCode = product.Status?.Group?.Category?.Code ?? "",
                CategoryName = product.Status?.Group?.Category?.Name ?? "",

                Unit = new UnitDto
                {
                    Id = product.Unit?.Id ?? 1,
                    Name = product.Unit?.Name ?? "عدد",
                    Symbol = product.Unit?.Symbol ?? "pcs"
                },

          
                // تبدیل ProductItem‌ها به DTO
                ProductItems = product.ProductItems
                    .Select(pi => new ProductItemDto
                    {
                        Id = pi.Id,
                        ProductId = pi.ProductId,
                        Sequence = pi.Sequence,
                        UniqueCode = pi.UniqueCode,
                        ItemStatus = pi.ProductItemStatus,
                        // اگر پروژه یا کدهای سلسله‌مراتبی داری، اضافه کن
                        ProjectId = pi.ProjectId,
                        ProductCode = product.Code,
                     
                        StatusCode = product.Status?.Code,
                        StatusName = product.Status?.Name,
                  
                        GroupCode = product.Status?.Group?.Code,
                        GroupName = product.Status?.Group?.Name,
                  
                        CategoryCode = product.Status?.Group?.Category?.Code,
                        CategoryName = product.Status?.Group?.Category?.Name
                    })
                    .ToList()
            };
        }




        public async Task UpdateAsync(ProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Status)
                    .ThenInclude(s => s.Group)
                        .ThenInclude(g => g.Category)
                .Include(p => p.ProductItems) // اگر خواستیم ویرایش ProductItem‌ها
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (product == null)
                throw new Exception("کالای مورد نظر یافت نشد.");

            // بررسی تکراری بودن کد محصول
            var duplicateProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id != dto.Id && p.Code == dto.Code && p.StatusId == dto.StatusId);

            if (duplicateProduct != null)
                throw new Exception("کدی که وارد کرده‌اید در این وضعیت قبلاً ثبت شده است.");

            // بروزرسانی اطلاعات اصلی محصول
            product.Name = dto.Name;
            product.Code = dto.Code;
            product.Description = dto.Description;
            product.StatusId = dto.StatusId;
            product.Price = dto.Price;
            product.UnitId = dto.Unit?.Id ?? product.UnitId;
            //product.IsUnique = dto.IsUnique;

            await _context.SaveChangesAsync(CancellationToken.None);

            // اگر میخوای امکان ویرایش پروژه یا وضعیت ProductItem‌ها رو اضافه کنید
            if (dto.ProductItems != null && dto.ProductItems.Any())
            {
                foreach (var itemDto in dto.ProductItems)
                {
                    var item = product.ProductItems.FirstOrDefault(pi => pi.Id == itemDto.Id);
                    if (item != null)
                    {
                        item.ProjectId = itemDto.ProjectId;
                        item.ProductItemStatus = itemDto.ItemStatus;
                        // اگر میخوای UniqueCode رو هم تغییر بدی، اینجا میشه اضافه کرد
                        // item.UniqueCode = itemDto.UniqueCode;
                    }
                }

                await _context.SaveChangesAsync(CancellationToken.None);
            }
        }



        public async Task DeleteAsync(int id)
        {
            // پیدا کردن محصول همراه با Inventories و InventoryItems
            var product = await _context.Products
                .Include(p => p.Inventories)
                    .ThenInclude(i => i.InventoryItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new Exception("محصول مورد نظر یافت نشد.");

            // بررسی اینکه آیا عملیات مرتبط با محصول انجام شده
            bool hasOperations = await _context.conversionConsumedItems.AnyAsync(c => c.ProductId == id)
                || await _context.conversionProducedItems.AnyAsync(c => c.ProductId == id)
                || await _context.ReceiptOrIssueItems.AnyAsync(r => r.ProductId == id);

            if (hasOperations)
                throw new Exception("امکان حذف این کالا وجود ندارد زیرا با این کالا عملیات انجام شده است.");

            // حذف همه ProductItems مرتبط
            var productItems = await _context.ProductItems
                .Where(pi => pi.ProductId == id)
                .ToListAsync();

            if (productItems.Any())
                _context.ProductItems.RemoveRange(productItems);

            // حذف همه InventoryItems مرتبط با این Inventories
            var inventoryItems = product.Inventories
                .SelectMany(i => i.InventoryItems)
                .ToList();

            if (inventoryItems.Any())
                _context.InventoryItems.RemoveRange(inventoryItems);

            // حذف Inventories
            if (product.Inventories?.Any() == true)
                _context.Inventories.RemoveRange(product.Inventories);

            // حذف خود محصول
            _context.Products.Remove(product);

            // ذخیره تغییرات
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
                .Include(p => p.Unit)
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
                    Unit = new UnitDto
                    {
                        Id = p.Unit.Id,
                        Name = p.Unit.Name,
                        Symbol = p.Unit.Symbol
                    }
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

        public async Task<IEnumerable<StatusDto>> GetStatusesAsync()
        {
            return await _context.Statuses
                .Select(s => new StatusDto { Id = s.Id, Name = s.Name })
                .ToListAsync();
        }

      
    }
}
