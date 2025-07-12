using System.Globalization;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class ConversionController : Controller
    {
        private readonly IConversionService _conversionService;
        private readonly IWarehouseDbContext _warehouseDbContext;

        public ConversionController(IConversionService conversionService, IWarehouseDbContext warehouseDbContext)
        {
            _conversionService = conversionService;
            _warehouseDbContext = warehouseDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var documents = await _conversionService.GetConversionDocumentsAsync();
            return View(documents); 
        }




        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _warehouseDbContext.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

            var groups = await _warehouseDbContext.Groups
       .Select(g => new GroupDto
       {
           Id = g.Id,
           Name = g.Name,
           CategoryId = g.CategoryId
       })
       .ToListAsync();


            var statuses = await _warehouseDbContext.Statuses
    .Select(s => new StatusDto
    {
        Id = s.Id,
        Name = s.Name,
        GroupId = s.GroupId 
    }).ToListAsync();

            var products = await _warehouseDbContext.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    StatusId = p.StatusId 
                }).ToListAsync();

            var warehouses = await _warehouseDbContext.Warehouses
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.Name
                }).ToListAsync();

            var zones = await _warehouseDbContext.StorageZones
                .Select(z => new StorageZoneDto
                {
                    Id = z.Id,
                    Name = z.Name,
                    WarehouseId = z.WarehouseId
                }).ToListAsync();

            var sections = await _warehouseDbContext.StorageSections
                .Select(s => new StorageSectionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ZoneId = s.ZoneId
                }).ToListAsync();


            // محاسبه تاریخ روز شمسی به صورت رشته
            PersianCalendar pc = new PersianCalendar();
            DateTime now = DateTime.Now;
            string persianDateString = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";

            var model = new ConversionCreateViewModel
            {
                Categories = categories,
                Groups = groups,
                Statuses = statuses,
                Warehouses = warehouses,
                Zones = zones,
                Sections = sections,
                Products = products,
                ConsumedItems = new List<ConversionConsumedItemDto>(),
                ProducedItems = new List<ConversionProducedItemDto>(),
                DateString = persianDateString,
                DocumentNumber = await GetNextDocumentNumberAsync()
            };


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConversionCreateViewModel model)
        {
            // تبدیل تاریخ شمسی به میلادی
            if (!string.IsNullOrEmpty(model.DateString))
            {
                var parts = model.DateString.Split('/');
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out int year) &&
                    int.TryParse(parts[1], out int month) &&
                    int.TryParse(parts[2], out int day))
                {
                    try
                    {
                        PersianCalendar pc = new PersianCalendar();
                        model.Date = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                    }
                    catch
                    {
                        ModelState.AddModelError(nameof(model.DateString), "تاریخ وارد شده معتبر نیست.");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.DateString), "فرمت تاریخ صحیح نیست.");
                }
            }



            // بررسی وجود اقلام مصرفی
            if (model.ConsumedItems == null || !model.ConsumedItems.Any())
            {
                ModelState.AddModelError(string.Empty, "حداقل یک کالای مصرفی باید انتخاب شود.");
                await PopulateSelectListsAsync(model);
                return View(model);
            }

            // بررسی وجود اقلام تولیدی
            if (model.ProducedItems == null || !model.ProducedItems.Any())
            {
                ModelState.AddModelError(string.Empty, "حداقل یک کالای تولیدی باید وارد شود.");
                await PopulateSelectListsAsync(model);
                return View(model);
            }

            try
            {
                // فراخوانی سرویس برای ثبت سند تبدیل
                int documentId = await _conversionService.ConvertAndRegisterDocumentAsync(
                    model.ConsumedItems,
                    model.ProducedItems
                );

                // هدایت به صفحه جزئیات سند ثبت‌شده
                return RedirectToAction("Index", new { id = documentId });
            }
            catch (Exception ex)
            {
                // در صورت بروز خطا، نمایش پیام خطا در فرم
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectListsAsync(model);
                return View(model);
            }
        }


        private async Task<string> GetNextDocumentNumberAsync()
        {
            var existingNumbers = await _warehouseDbContext.conversionDocuments
                .Select(d => d.DocumentNumber)
                .ToListAsync();

            var existingInts = existingNumbers
                .Select(s => int.TryParse(s, out int n) ? n : 0)
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();

            int nextNumber = 1;
            foreach (var number in existingInts)
            {
                if (number == nextNumber)
                    nextNumber++;
                else if (number > nextNumber)
                    break;
            }

            return nextNumber.ToString();
        }


        private async Task PopulateSelectListsAsync(ConversionCreateViewModel model)
        {
            model.Categories = await _warehouseDbContext.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            model.Groups = await _warehouseDbContext.Groups
                .Select(g => new GroupDto { Id = g.Id, Name = g.Name, CategoryId = g.CategoryId })
                .ToListAsync();

            model.Statuses = await _warehouseDbContext.Statuses
                .Select(s => new StatusDto { Id = s.Id, Name = s.Name, GroupId = s.GroupId })
                .ToListAsync();

            model.Products = await _warehouseDbContext.Products
                .Select(p => new ProductDto { Id = p.Id, Name = p.Name, StatusId = p.StatusId })
                .ToListAsync();

            model.Warehouses = await _warehouseDbContext.Warehouses
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            model.Zones = await _warehouseDbContext.StorageZones
                .Select(z => new StorageZoneDto { Id = z.Id, Name = z.Name, WarehouseId = z.WarehouseId })
                .ToListAsync();

            model.Sections = await _warehouseDbContext.StorageSections
                .Select(s => new StorageSectionDto { Id = s.Id, Name = s.Name, ZoneId = s.ZoneId })
                .ToListAsync();
        }



        [HttpGet]
        public async Task<JsonResult> GetZonesByWarehouse(int warehouseId)
        {
            var zones = await _warehouseDbContext.StorageZones
                .Where(z => z.WarehouseId == warehouseId)
                .Select(z => new { z.Id, z.Name })
                .ToListAsync();
            return Json(zones);
        }
        [HttpGet]
        public async Task<JsonResult> GetSectionsByZone(int zoneId)
        {
            var sections = await _warehouseDbContext.StorageSections
                .Where(s => s.ZoneId == zoneId)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();
            return Json(sections);
        }

        [HttpGet]
        public async Task<JsonResult> GetGroupsByCategory(int categoryId)
        {
            var groups = await _warehouseDbContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .Select(g => new { id = g.Id, name = g.Name })
                .ToListAsync();
            return Json(groups);
        }

        [HttpGet]
        public async Task<JsonResult> GetStatusesByGroup(int groupId)
        {
            var statuses = await _warehouseDbContext.Statuses
                .Where(s => s.GroupId == groupId)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToListAsync();
            return Json(statuses);
        }

        [HttpGet]
        public async Task<JsonResult> GetProductsByStatus(int statusId)
        {
            var products = await _warehouseDbContext.Products
                .Where(p => p.StatusId == statusId)
                .Select(p => new { id = p.Id, name = p.Name })
                .ToListAsync();
            return Json(products);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _conversionService.DeleteConversionDocumentAsync(id);

                if (!result)
                {
                    TempData["ErrorMessage"] = "سند مورد نظر یافت نشد یا قبلاً حذف شده است.";
                }
                else
                {
                    TempData["SuccessMessage"] = "سند با موفقیت حذف شد.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در حذف سند: {ex.Message}";
                return RedirectToAction("Index");
            }
        }





    }
}

