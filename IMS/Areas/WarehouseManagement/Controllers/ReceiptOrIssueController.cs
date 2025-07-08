using System.Diagnostics;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Infrastructure.Persistence.WarehouseManagement;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore.Options;
using Rotativa.AspNetCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class ReceiptOrIssueController : Controller
    {
        private readonly IReceiptOrIssueService _service;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IGroupService _groupService;
        private readonly IStatusService _statusService;
        private readonly IWarehouseDbContext _context;

        public ReceiptOrIssueController(IReceiptOrIssueService service, IWarehouseService warehouseService, IProductService productService
            , ICategoryService categoryService ,IGroupService groupService ,IStatusService statusService , IWarehouseDbContext context)
        {
            _service = service;
            _warehouseService = warehouseService;
            _productService = productService;
            _categoryService = categoryService; _groupService = groupService; _statusService = statusService;
            _context = context;
        }



        public async Task<IActionResult> Index()
        {
            var list = await _service.GetAllAsync();
            return View(list); 
        }



        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ReceiptOrIssueViewModel
            {
                DateString = ConvertToPersianDateString(DateTime.Now),
                DocumentNumber = await GetNextDocumentNumberAsync(),
                Items = new List<ReceiptOrIssueItemViewModel>()
            };

            await PopulateSelectLists();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceiptOrIssueViewModel model)
        {
            bool exists = await _context.ReceiptOrIssues
                .AnyAsync(r => r.DocumentNumber == model.DocumentNumber && r.Id != model.Id);

            if (exists)
                ModelState.AddModelError(nameof(model.DocumentNumber), "شماره سند تکراری است.");

            try
            {
                model.Date = ParsePersianDate(model.DateString);
            }
            catch
            {
                ModelState.AddModelError("DateString", "تاریخ وارد شده نامعتبر است.");
            }

            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var dto = MapViewModelToDto(model);
                var createdDto = await _service.CreateAsync(dto);

                // اینجا ID سند جدید را برمی‌گردونی
                return Json(new { success = true, documentId = createdDto.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new[] { "خطا در ایجاد رکورد: " + ex.Message } });
            }
        }



        private async Task<string> GetNextDocumentNumberAsync()
        {
            // گرفتن شماره های موجود به صورت عددی (اگر رشته‌ای هستند ابتدا تبدیل می‌شوند)
            var existingNumbers = await _context.ReceiptOrIssues
                .Select(r => r.DocumentNumber)
                .ToListAsync();

            var existingInts = existingNumbers
                .Select(s => int.TryParse(s, out int n) ? n : 0)
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();

            // پیدا کردن کوچک‌ترین عدد مثبت که استفاده نشده (برای پر کردن جای خالی‌ها)
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

        private async Task PopulateSelectLists()
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            var categories = await _categoryService.GetAllAsync();

            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            List<StorageZoneDto> zones = new();

            if (warehouses.Any())
            {
                zones = await _warehouseService.GetZonesByWarehouseIdAsync(warehouses.First().Id);
            }

            ViewBag.Zones = new SelectList(zones, "Id", "Name");

            if (zones.Any())
            {
                var sections = await _warehouseService.GetSectionsByZoneAsync(zones.First().Id);
                ViewBag.Sections = new SelectList(sections, "Id", "Name");
            }
            else
            {
                ViewBag.Sections = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            // برای DropDownهای آبشاری مقادیر خالی می‌گذاریم (فقط برای View آماده‌سازی می‌کنیم)
            ViewBag.Groups = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.Statuses = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.Products = new SelectList(Enumerable.Empty<SelectListItem>());
        }



        private async Task PopulateItemDependencies(ReceiptOrIssueItemViewModel item)
        {
            // --- پر کردن لیست گروه‌ها ---
            if (item.CategoryId.HasValue)
            {
                var groups = await _groupService.GetAllAsync(item.CategoryId.Value) ?? new List<GroupDto>();
                item.AvailableGroups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name,
                    Selected = g.Id == item.GroupId // تنظیم مقدار انتخاب‌شده
                }).ToList();
            }

            // --- پر کردن لیست وضعیت‌ها ---
            if (item.GroupId.HasValue)
            {
                var statuses = await _statusService.GetAllAsync(item.GroupId.Value) ?? new List<StatusDto>();
                item.AvailableStatuses = statuses.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == item.StatusId // تنظیم مقدار انتخاب‌شده
                }).ToList();
            }

            // --- پر کردن لیست محصولات ---
            if (item.StatusId.HasValue)
            {
                var products = await _productService.GetAllAsync(item.StatusId.Value) ?? new List<ProductDto>();
                item.AvailableProducts = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = p.Id == item.ProductId // تنظیم مقدار انتخاب‌شده
                }).ToList();
            }

            var allWarehouses = await _warehouseService.GetAllWarehousesAsync();

            item.AvailableSourceWarehouses = allWarehouses.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = w.Name,
                Selected = w.Id == item.SourceWarehouseId
            }).ToList();

            item.AvailableDestinationWarehouses = allWarehouses.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = w.Name,
                Selected = w.Id == item.DestinationWarehouseId
            }).ToList();

            if (item.SourceWarehouseId.HasValue)
            {
                var sourceZones = await _warehouseService.GetZonesByWarehouseIdAsync(item.SourceWarehouseId.Value);
                item.AvailableSourceZones = sourceZones.Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name,
                    Selected = z.Id == item.SourceZoneId
                }).ToList();
            }
            else
            {
                item.AvailableSourceZones = new List<SelectListItem>();
            }

            if (item.SourceZoneId.HasValue)
            {
                var sourceSections = await _warehouseService.GetSectionsByZoneAsync(item.SourceZoneId.Value);
                item.AvailableSourceSections = sourceSections.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == item.SourceSectionId
                }).ToList();
            }
            else
            {
                item.AvailableSourceSections = new List<SelectListItem>();
            }

            if (item.DestinationWarehouseId.HasValue)
            {
                var destinationZones = await _warehouseService.GetZonesByWarehouseIdAsync(item.DestinationWarehouseId.Value);
                item.AvailableDestinationZones = destinationZones.Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name,
                    Selected = z.Id == item.DestinationZoneId
                }).ToList();
            }
            else
            {
                item.AvailableDestinationZones = new List<SelectListItem>();
            }

            if (item.DestinationZoneId.HasValue)
            {
                var destinationSections = await _warehouseService.GetSectionsByZoneAsync(item.DestinationZoneId.Value);
                item.AvailableDestinationSections = destinationSections.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == item.DestinationSectionId
                }).ToList();
            }
            else
            {
                item.AvailableDestinationSections = new List<SelectListItem>();
            }
        }







        private ReceiptOrIssueDto MapViewModelToDto(ReceiptOrIssueViewModel vm)
        {
            return new ReceiptOrIssueDto
            {
                Id = vm.Id,
                DateString = vm.DateString,
                Date = ParsePersianDate(vm.DateString),
                Type = vm.Type,
                Description = vm.Description,
                DocumentNumber = vm.DocumentNumber,
                Items = vm.Items.Select(i => new ReceiptOrIssueItemDto
                {
                    Quantity = i.Quantity,
                    SourceWarehouseId = i.SourceWarehouseId,
                    SourceZoneId = i.SourceZoneId,
                    SourceSectionId = i.SourceSectionId,
                    DestinationWarehouseId = i.DestinationWarehouseId,
                    DestinationZoneId = i.DestinationZoneId,
                    DestinationSectionId = i.DestinationSectionId,
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    ProductId = i.ProductId
                }).ToList()
            };
        }




        private async Task<ReceiptOrIssueViewModel> MapDtoToViewModelAsync(ReceiptOrIssueDto dto)
        {
            var viewModel = new ReceiptOrIssueViewModel
            {
                Id = dto.Id,
                DateString = dto.DateString ?? ConvertToPersianDateString(dto.Date),
                Type = dto.Type,
                Description = dto.Description,
                DocumentNumber = dto.DocumentNumber,
                Items = new List<ReceiptOrIssueItemViewModel>()
            };

            foreach (var i in dto.Items)
            {
                var item = new ReceiptOrIssueItemViewModel
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    SourceSectionId = i.SourceSectionId,
                    DestinationSectionId = i.DestinationSectionId
                };

                // تعیین اطلاعات مبدأ
                if (i.SourceSectionId.HasValue)
                {
                    var section = await _warehouseService.GetSectionByIdAsync(i.SourceSectionId.Value);
                    if (section != null)
                    {
                        item.SourceZoneId = section.ZoneId;
                        var zone = await _warehouseService.GetZoneByIdAsync(section.ZoneId);
                        if (zone != null)
                            item.SourceWarehouseId = zone.WarehouseId;
                    }
                }
                else
                {
                    item.SourceZoneId = i.SourceZoneId;
                    item.SourceWarehouseId = i.SourceWarehouseId;
                }

                // تعیین اطلاعات مقصد
                if (i.DestinationSectionId.HasValue)
                {
                    var section = await _warehouseService.GetSectionByIdAsync(i.DestinationSectionId.Value);
                    if (section != null)
                    {
                        item.DestinationZoneId = section.ZoneId;
                        var zone = await _warehouseService.GetZoneByIdAsync(section.ZoneId);
                        if (zone != null)
                            item.DestinationWarehouseId = zone.WarehouseId;
                    }
                }
                else
                {
                    item.DestinationZoneId = i.DestinationZoneId;
                    item.DestinationWarehouseId = i.DestinationWarehouseId;
                }

                viewModel.Items.Add(item);
            }

            return viewModel;
        }






        private DateTime ParsePersianDate(string persianDate)
        {
            var parts = persianDate.Split('/');
            var pc = new System.Globalization.PersianCalendar();
            return pc.ToDateTime(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                0, 0, 0, 0);
        }

        private string ConvertToPersianDateString(DateTime date)
        {
            var pc = new System.Globalization.PersianCalendar();
            return $"{pc.GetYear(date)}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00}";
        }



        [HttpGet]
        public async Task<IActionResult> GetZones(int warehouseId)
        {
            var zones = await _warehouseService.GetZonesByWarehouseIdAsync(warehouseId);
            var result = zones.Select(z => new SelectListItem
            {
                Value = z.Id.ToString(),
                Text = z.Name
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSections(int zoneId)
        {
            var sections = await _warehouseService.GetSectionsByZoneAsync(zoneId);
            var result = sections.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();

            return Json(result);
        }



        [HttpGet]
        public async Task<IActionResult> GetGroups(int categoryId)
        {
            var groups = await _service.GetGroupsByCategoryAsync(categoryId);
            return Json(groups);
        }


        [HttpGet]
        public async Task<IActionResult> GetStatuses(int groupId)
        {
            var statuses = await _service.GetStatusesByGroupAsync(groupId);
            return Json(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int statusId)
        {
            var products = await _service.GetProductsByStatus(statusId);
            return Json(products);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            var model = await MapDtoToViewModelAsync(dto);

            await PopulateSelectLists();

            foreach (var item in model.Items)
            {

                //await FillSourceWarehouseAndZoneIfMissing(item);
                await PopulateItemDependencies(item);
              
            }
    
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReceiptOrIssueViewModel model)
        {
            try
            {
                model.Date = ParsePersianDate(model.DateString);
            }
            catch
            {
                ModelState.AddModelError("DateString", "تاریخ وارد شده نامعتبر است.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectLists();

                foreach (var item in model.Items)
                {
                    await PopulateItemDependencies(item);
                }

                return View(model);
            }


            try
            {
                var dto = MapViewModelToDto(model);
                var updatedDto = await _service.UpdateAsync(dto.Id, dto);
                if (updatedDto == null)
                {
                    ModelState.AddModelError("", "سند مورد نظر یافت نشد یا ویرایش نشد.");
                    await PopulateSelectLists();
                    return View(model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطا در ویرایش رکورد: " + ex.Message);
                await PopulateSelectLists();
                return View(model);
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool result = await _service.DeleteAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "سند مورد نظر یافت نشد یا حذف نشد.";
                }
                else
                {
                    TempData["SuccessMessage"] = "سند با موفقیت حذف شد.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در حذف سند: " + ex.Message;
            }

            return RedirectToAction("Index");
        }




        [HttpGet]
        public IActionResult Print(int id)
        {
            var receipt = _context.ReceiptOrIssues
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Category)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Group)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Status)
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceWarehouse)
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceZone)
                .Include(r => r.Items)
                    .ThenInclude(i => i.SourceSection)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationWarehouse)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationZone)
                .Include(r => r.Items)
                    .ThenInclude(i => i.DestinationSection)
                .FirstOrDefault(r => r.Id == id);

            if (receipt == null)
                return NotFound();

            return new ViewAsPdf("Print", receipt)
            {
                FileName = $"Receipt_{id}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                // اگر نیاز به تنظیمات اضافه دارید اینجا اضافه کنید
                // مثلا: CustomSwitches = "--disable-smart-shrinking"
            };
        }



    }
}
