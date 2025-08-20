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
using IMS.Application.ProjectManagement.Service;
using IMS.Application.ProcurementManagement.Service;

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
        private readonly IApplicationDbContext _projectContext;
        private readonly IProjectService _projectService;
        private readonly IProcurementManagementDbContext _procurementContext;

        public ReceiptOrIssueController(IReceiptOrIssueService service, IWarehouseService warehouseService, IProductService productService
            , ICategoryService categoryService ,IGroupService groupService ,IStatusService statusService , IWarehouseDbContext context, IApplicationDbContext projectContext ,
            IProjectService projectService , IProcurementManagementDbContext procurementManagementDb)
        {
            _service = service;
            _warehouseService = warehouseService;
            _productService = productService;
            _categoryService = categoryService; _groupService = groupService; _statusService = statusService;
            _context = context;
            _projectContext = projectContext;_projectService = projectService; _procurementContext = procurementManagementDb;
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
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("Items", "باید حداقل یک آیتم وارد کنید.");
            }

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

            ModelState.Remove(nameof(model.Type));
            if (!model.Type.HasValue)
                ModelState.AddModelError(nameof(model.Type), "نوع سند را وارد کنید.");

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var dto = MapViewModelToDto(model);
                var createdDto = await _service.CreateAsync(dto);

                return Json(new { success = true, documentId = createdDto.Id });
            }
            catch (ArgumentException ex)
            {
                // خطاهای اعتبارسنجی
                string message = ex.Message switch
                {
                    string m when m.Contains("Items collection cannot be empty") => "باید حداقل یک آیتم وارد کنید.",
                    string m when m.Contains("ProductId must be greater than zero") => "شناسه کالا معتبر نیست.",
                    string m when m.Contains("Quantity must be greater than zero") => "تعداد باید بیشتر از صفر باشد.",
                    _ => "خطای اعتبارسنجی رخ داد."
                };

                return Json(new { success = false, errors = new[] { message } });
            }
            catch (InvalidOperationException ex)
            {
                // خطاهای عملیات نامعتبر
                string message = ex.Message; // می‌توانید پیام‌های خاص را هم فارسی کنید
                if (message.Contains("موجودی مبدأ یا مقصد برای کالای"))
                {
                    // پیام فارسی را مستقیم بفرست
                }

                return Json(new { success = false, errors = new[] { message } });
            }
            catch (Exception)
            {
                // سایر خطاها
                return Json(new { success = false, errors = new[] { "خطای غیرمنتظره‌ای رخ داد. لطفاً با پشتیبانی تماس بگیرید." } });
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
            var warehouses = await _warehouseService.GetAllWarehousesAsync() ?? new List<WarehouseDto>();
            var categories = await _categoryService.GetAllAsync() ?? new List<CategoryDto>();

            var warehouseItems = warehouses.Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name }).ToList();
            
            ViewBag.Warehouses = warehouseItems;

            var categoryItems = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

            ViewBag.Categories = new SelectList(categories.Select(c => new { c.Id, c.Name }), "Id", "Name");


            List<StorageZoneDto> zones = new();

            if (warehouses.Any())
            {
                zones = await _warehouseService.GetZonesByWarehouseIdAsync(warehouses.First().Id);
            }

            var zoneItems = zones.Select(z => new SelectListItem { Value = z.Id.ToString(), Text = z.Name }).ToList();
            zoneItems.Insert(0, new SelectListItem { Value = "", Text = "انتخاب کنید" });
            ViewBag.Zones = zoneItems;

            if (zones.Any())
            {
                var sections = await _warehouseService.GetSectionsByZoneAsync(zones.First().Id);
                var sectionItems = sections.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
                sectionItems.Insert(0, new SelectListItem { Value = "", Text = "انتخاب کنید" });
                ViewBag.Sections = sectionItems;
            }
            else
            {
                ViewBag.Sections = new List<SelectListItem> { new SelectListItem { Value = "", Text = "انتخاب کنید" } };
            }

            var projects = await _projectContext.Projects
                .Select(p => new { p.Id, p.ProjectName })
                .ToListAsync();

            var projectItems = projects.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.ProjectName }).ToList();
            projectItems.Insert(0, new SelectListItem { Value = "", Text = "انتخاب کنید" });
            ViewBag.Projects = projectItems;


            var purchaseRequests = await _procurementContext.PurchaseRequests
    .Select(pr => new { pr.Id, pr.Title })
    .ToListAsync();

            var purchaseRequestItems = purchaseRequests
                .Select(pr => new SelectListItem { Value = pr.Id.ToString(), Text = pr.Title })
                .ToList();

            purchaseRequestItems.Insert(0, new SelectListItem { Value = "", Text = "انتخاب کنید" });
            ViewBag.PurchaseRequests = purchaseRequestItems;




            ViewBag.Groups = new List<SelectListItem> { new SelectListItem { Value = "", Text = "انتخاب کنید" } };
            ViewBag.Statuses = new List<SelectListItem> { new SelectListItem { Value = "", Text = "انتخاب کنید" } };
            ViewBag.Products = new List<SelectListItem> { new SelectListItem { Value = "", Text = "انتخاب کنید" } };
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
            var allProjects = await _projectService.GetAllProjectsAsync();
            item.AvailableProjects = allProjects.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.ProjectName,
                Selected = p.Id == item.ProjectId
            }).ToList();

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
                    ProductId = i.ProductId,
                    ProjectId = i.ProjectId,
                    PurchaseRequestId = i.PurchaseRequestId  // جدید
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

            // 1. استخراج ProjectId های یکتا از آیتم‌ها
            var projectIds = dto.Items
                .Where(i => i.ProjectId.HasValue)
                .Select(i => i.ProjectId!.Value)
                .Distinct()
                .ToList();

            // 2. بارگذاری عنوان پروژه‌ها از DbContext پروژه
            var allProjects = await _projectContext.Projects
                .Select(p => new { p.Id, p.ProjectName })
                .ToListAsync();

            var projectTitles = allProjects.ToDictionary(p => p.Id, p => p.ProjectName);

            // 3. پر کردن آیتم‌ها با اطلاعات کامل
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
                    DestinationSectionId = i.DestinationSectionId,
                    ProjectId = i.ProjectId,
                    ProjectTitle = i.ProjectId.HasValue && projectTitles.ContainsKey(i.ProjectId.Value)
                        ? projectTitles[i.ProjectId.Value]
                        : null,
                    AvailableProjects = allProjects.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.ProjectName,
                        Selected = (p.Id == i.ProjectId)
                    }).ToList()
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

            if (model.Items == null)
                model.Items = new List<ReceiptOrIssueItemViewModel>();

            await PopulateSelectLists();

            foreach (var item in model.Items)
            {
                await PopulateItemDependencies(item);
            }

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReceiptOrIssueViewModel model)
        {
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("Items", "باید حداقل یک آیتم وارد کنید.");
            }

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

            ModelState.Remove(nameof(model.Type));
            if (!model.Type.HasValue)
                ModelState.AddModelError(nameof(model.Type), "نوع سند را وارد کنید.");

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var dto = MapViewModelToDto(model);
                var updatedDto = await _service.UpdateAsync(dto.Id, dto);

                if (updatedDto == null)
                {
                    return Json(new { success = false, errors = new[] { "سند مورد نظر یافت نشد یا ویرایش نشد." } });
                }

                return Json(new { success = true, documentId = dto.Id });
            }
            catch (Exception ex)
            {
                // اگر خطای خاص نداشتن آیتم یا هر خطای دیگر
                if (ex.Message.Contains("Items collection cannot be empty"))
                {
                    return Json(new { success = false, errors = new[] { "باید حداقل یک آیتم وارد کنید." } });
                }

                // اگر پیام خطا مربوط به موجودی ناکافی کالا بود
                if (ex.Message.Contains("موجودی کالا"))
                {
                    return Json(new { success = false, errors = new[] { ex.Message } });
                }

                // سایر خطاهای پیش‌بینی نشده
                return Json(new { success = false, errors = new[] { "خطای غیرمنتظره‌ای رخ داد. لطفاً با پشتیبانی تماس بگیرید." } });
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
        public async Task<IActionResult> Print(int id)
        {
            var receipt = await _context.ReceiptOrIssues
    .Include(r => r.Items)
        .ThenInclude(i => i.Category)
    .Include(r => r.Items)
        .ThenInclude(i => i.Group)
    .Include(r => r.Items)
        .ThenInclude(i => i.Status)
    .Include(r => r.Items)
        .ThenInclude(i => i.Product)
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
    .FirstOrDefaultAsync(r => r.Id == id);


            if (receipt == null)
                return NotFound();

            // فرض می‌کنیم هر آیتم خودش ProjectId دارد:
            var projectIds = receipt.Items
                .Where(i => i.ProjectId != null)
                .Select(i => i.ProjectId.Value)
                .Distinct()
                .ToList();

            var projectsDict = await _projectContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

            var itemsWithProject = receipt.Items.Select(i => new ReceiptItemPrintViewModel
            {
                Item = i,
                ProjectName = i.ProjectId != null && projectsDict.ContainsKey(i.ProjectId.Value)
                    ? projectsDict[i.ProjectId.Value]
                    : "—"
            }).ToList();

            var viewModel = new ReceiptPrintViewModel
            {
                Receipt = receipt,
                ItemsWithProject = itemsWithProject
            };

            return new ViewAsPdf("Print", viewModel)
            {
                FileName = $"Receipt_{id}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
            };
        }




        public JsonResult CheckProductInPurchaseRequest(int productId, int purchaseRequestId)
        {
            var exists = _procurementContext.PurchaseRequestItems
                            .Any(i => i.ProductId == productId && i.PurchaseRequestId == purchaseRequestId);

            return Json(new { exists });
        }




    }
}
