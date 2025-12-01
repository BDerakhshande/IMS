using System.Globalization;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore.Options;
using Rotativa.AspNetCore;
using IMS.Application.ProjectManagement.Service;
namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class ConversionController : Controller
    {
        private readonly IConversionService _conversionService;
        private readonly IWarehouseDbContext _warehouseDbContext;
        private readonly IApplicationDbContext _projectContext;
        private readonly IProjectService _projectService;
        public ConversionController(IConversionService conversionService, IWarehouseDbContext warehouseDbContext,
            IApplicationDbContext projectContext, IProjectService projectService)
        {
            _conversionService = conversionService;
            _warehouseDbContext = warehouseDbContext;
            _projectContext = projectContext;
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var documents = await _conversionService.GetConversionDocumentsAsync();
            ViewBag.CreatedDocumentId = TempData["CreatedDocumentId"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(documents);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Load all necessary data
            var categories = await _warehouseDbContext.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();
            // Load groups with their category relationships
            var groups = await _warehouseDbContext.Groups
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    CategoryId = g.CategoryId
                }).ToListAsync();
            // Load statuses with their group relationships
            var statuses = await _warehouseDbContext.Statuses
                .Select(s => new StatusDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    GroupId = s.GroupId
                }).ToListAsync();
            // Load products with their status relationships
            var products = await _warehouseDbContext.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    StatusId = p.StatusId
                }).ToListAsync();
            // Load warehouses
            var warehouses = await _warehouseDbContext.Warehouses
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.Name
                }).ToListAsync();
            // Load zones with warehouse relationships
            var zones = await _warehouseDbContext.StorageZones
                .Select(z => new StorageZoneDto
                {
                    Id = z.Id,
                    Name = z.Name,
                    WarehouseId = z.WarehouseId
                }).ToListAsync();
            // Load sections with zone relationships
            var sections = await _warehouseDbContext.StorageSections
                .Select(s => new StorageSectionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ZoneId = s.ZoneId
                }).ToListAsync();
            var projects = await _projectContext.Projects
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProjectName
                }).ToListAsync();
            // Persian date setup
            PersianCalendar pc = new PersianCalendar();
            DateTime now = DateTime.Now;
            string persianDateString = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";
            var model = new ConversionCreateViewModel
            {
                Categories = categories,
                Groups = groups,
                Statuses = statuses,
                Products = products,
                Warehouses = warehouses,
                Zones = zones,
                Sections = sections,
                ConsumedItems = new List<ConversionConsumedItemDto> { new ConversionConsumedItemDto() },
                ProducedItems = new List<ConversionProducedItemDto> { new ConversionProducedItemDto() },
                DateString = persianDateString,
                DocumentNumber = await _conversionService.GetNextConversionDocumentNumberAsync(),
                Projects = projects,
                ProjectId = null
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
                        ModelState.AddModelError("DateString", "تاریخ وارد شده معتبر نیست.");
                    }
                }
                else
                {
                    ModelState.AddModelError("DateString", "فرمت تاریخ صحیح نیست.");
                }
            }

            // Validation دستی
            var errors = new List<string>();
            if (model.ConsumedItems == null || !model.ConsumedItems.Any())
                errors.Add("حداقل یک کالای مصرفی الزامی است.");
            if (model.ProducedItems == null || !model.ProducedItems.Any())
                errors.Add("حداقل یک کالای تولیدی الزامی است.");

            // Map واقعی ZoneId و SectionId
            if (model.ConsumedItems != null)
            {
                for (int i = 0; i < model.ConsumedItems.Count; i++)
                {
                    var item = model.ConsumedItems[i];
                    var productName = await _warehouseDbContext.Products
                        .Where(p => p.Id == item.ProductId)
                        .Select(p => p.Name)
                        .FirstOrDefaultAsync() ?? "نامشخص";

                    if (item.ProductId <= 0) errors.Add($"قلم مصرفی {i + 1}: انتخاب کالا الزامی است.");
                    if (item.WarehouseId <= 0) errors.Add($"قلم مصرفی {i + 1}: انتخاب انبار الزامی است.");

                    // Map ZoneId واقعی
                    if (item.ZoneId > 0)
                    {
                        var realZoneId = await _warehouseDbContext.StorageZones
                            .Where(z => z.Id == item.ZoneId && z.WarehouseId == item.WarehouseId)
                            .Select(z => (int?)z.Id)
                            .FirstOrDefaultAsync();
                        if (realZoneId.HasValue)
                            item.ZoneId = realZoneId.Value;
                        else
                            errors.Add($"قلم مصرفی {i + 1} ('{productName}'): Zone وارد شده صحیح نیست.");
                    }
                    else
                        errors.Add($"قلم مصرفی {i + 1} ('{productName}'): انتخاب Zone الزامی است.");

                    // Map SectionId واقعی
                    if (item.SectionId > 0)
                    {
                        var realSectionId = await _warehouseDbContext.StorageSections
                            .Where(s => s.Id == item.SectionId)
                            .Select(s => (int?)s.Id)
                            .FirstOrDefaultAsync();
                        if (realSectionId.HasValue)
                            item.SectionId = realSectionId.Value;
                        else
                            errors.Add($"قلم مصرفی {i + 1} ('{productName}'): Section وارد شده صحیح نیست.");
                    }
                    else
                        errors.Add($"قلم مصرفی {i + 1} ('{productName}'): انتخاب Section الزامی است.");

                    if (item.Quantity <= 0) errors.Add($"قلم مصرفی {i + 1}: تعداد معتبر (>0) الزامی است.");
                }
            }

            if (errors.Any())
            {
                foreach (var e in errors) ModelState.AddModelError("", e);
                await PopulateSelectListsAsync(model);
                return View(model);
            }

            try
            {
                var (documentId, documentNumber) = await _conversionService.ConvertAndRegisterDocumentAsync(
                    model.ConsumedItems, model.ProducedItems);

                TempData["SuccessMessage"] = $"سند با موفقیت ایجاد شد. شماره سند: {documentNumber}";

                var newModel = new ConversionCreateViewModel
                {
                    ConsumedItems = new List<ConversionConsumedItemDto> { new ConversionConsumedItemDto() },
                    ProducedItems = new List<ConversionProducedItemDto> { new ConversionProducedItemDto() },
                    DateString = model.DateString
                };
                await PopulateSelectListsAsync(newModel);
                return View(newModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطا در ایجاد سند: " + ex.Message);
                await PopulateSelectListsAsync(model);
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _warehouseDbContext.conversionDocuments
                .Include(d => d.ConsumedItems)
                .Include(d => d.ProducedItems)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                TempData["ErrorMessage"] = "سند مورد نظر یافت نشد.";
                return RedirectToAction("Index");
            }
            var pc = new PersianCalendar();
            var persianDate = $"{pc.GetYear(document.CreatedAt):0000}/{pc.GetMonth(document.CreatedAt):00}/{pc.GetDayOfMonth(document.CreatedAt):00}";
            var model = new ConversionCreateViewModel
            {
                DocumentId = document.Id,
                DocumentNumber = document.DocumentNumber,
                Date = document.CreatedAt,
                DateString = persianDate,
                ConsumedItems = document.ConsumedItems.Select(i => new ConversionConsumedItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    WarehouseId = i.WarehouseId,
                    ZoneId = i.ZoneId,
                    SectionId = i.SectionId,
                    ProjectId = i.ProjectId,
                    InventoryItemIds = i.UniqueCodes?.Select(uc => uc.InventoryItemId).ToList() ?? new List<int>() // اضافه کردن InventoryItemIds برای unique codes
                }).ToList(),
                ProducedItems = document.ProducedItems.Select(i => new ConversionProducedItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    WarehouseId = i.WarehouseId,
                    ZoneId = i.ZoneId,
                    SectionId = i.SectionId,
                    ProjectId = i.ProjectId,
                    UniqueCodes = i.UniqueCodes?.Select(uc => uc.UniqueCode).ToList() ?? new List<string>() // اضافه کردن UniqueCodes برای produced
                }).ToList()
            };
            await PopulateSelectListsAsync(model);
            return View("Create", model); // Use the same view as Create for editing
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, ConversionCreateViewModel model)
        //{
        // // تبدیل تاریخ شمسی به میلادی (مشابه Create)
        // if (!string.IsNullOrEmpty(model.DateString))
        // {
        // var parts = model.DateString.Split('/');
        // if (parts.Length == 3 &&
        // int.TryParse(parts[0], out int year) &&
        // int.TryParse(parts[1], out int month) &&
        // int.TryParse(parts[2], out int day))
        // {
        // try
        // {
        // PersianCalendar pc = new PersianCalendar();
        // model.Date = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        // }
        // catch
        // {
        // ModelState.AddModelError("DateString", "تاریخ وارد شده معتبر نیست.");
        // }
        // }
        // else
        // {
        // ModelState.AddModelError("DateString", "فرمت تاریخ صحیح نیست.");
        // }
        // }
        // if (model.ConsumedItems == null || !model.ConsumedItems.Any())
        // ModelState.AddModelError(string.Empty, "حداقل یک کالای مصرفی باید انتخاب شود.");
        // if (model.ProducedItems == null || !model.ProducedItems.Any())
        // ModelState.AddModelError(string.Empty, "حداقل یک کالای تولیدی باید وارد شود.");
        // ModelState.Remove(nameof(model.Zones));
        // ModelState.Remove(nameof(model.Groups));
        // ModelState.Remove(nameof(model.Products));
        // ModelState.Remove(nameof(model.Sections));
        // ModelState.Remove(nameof(model.Statuses));
        // ModelState.Remove(nameof(model.Categories));
        // ModelState.Remove(nameof(model.Warehouses));
        // if (!ModelState.IsValid)
        // {
        // await PopulateSelectListsAsync(model); // برای نمایش گزینه‌ها در صورت خطا
        // return View("Create", model);
        // }
        // try
        // {
        // var (documentId, documentNumber) = await _conversionService.UpdateConversionDocumentAsync(
        // id,
        // model.ConsumedItems,
        // model.ProducedItems
        // );
        // TempData["SuccessMessage"] = "سند با موفقیت به‌روزرسانی شد.";
        // return RedirectToAction("Index");
        // }
        // catch (Exception ex)
        // {
        // TempData["ErrorMessage"] = "خطا در به‌روزرسانی سند: " + ex.Message;
        // await PopulateSelectListsAsync(model);
        // return View("Create", model);
        // }
        //}
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
            model.Projects = await _projectContext.Projects
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProjectName
                }).ToListAsync();
        }
        [HttpGet]
        public async Task<JsonResult> GetZonesByWarehouse(int warehouseId)
        {
            var zones = await _warehouseDbContext.StorageZones
                .Where(z => z.WarehouseId == warehouseId)
                .Select(z => new { value = z.Id, text = z.Name })
                .ToListAsync();
            return Json(zones);
        }
        [HttpGet]
        public async Task<JsonResult> GetSectionsByZone(int zoneId)
        {
            var sections = await _warehouseDbContext.StorageSections
                .Where(s => s.ZoneId == zoneId)
                .Select(s => new { value = s.Id, text = s.Name })
                .ToListAsync();
            return Json(sections);
        }
        [HttpGet]
        public async Task<JsonResult> GetGroupsByCategory(int categoryId)
        {
            var groups = await _warehouseDbContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .Select(g => new { value = g.Id, text = g.Name })
                .ToListAsync();
            return Json(groups);
        }
        [HttpGet]
        public async Task<JsonResult> GetStatusesByGroup(int groupId)
        {
            var statuses = await _warehouseDbContext.Statuses
                .Where(s => s.GroupId == groupId)
                .Select(s => new { value = s.Id, text = s.Name })
                .ToListAsync();
            return Json(statuses);
        }
        [HttpGet]
        public async Task<JsonResult> GetProductsByStatus(int statusId)
        {
            var products = await _warehouseDbContext.Products
                .Where(p => p.StatusId == statusId)
                .Select(p => new { value = p.Id, text = p.Name })
                .ToListAsync();
            return Json(products);
        }
        [HttpGet]
        public async Task<JsonResult> GetUniqueCodes(int productId, int? sourceWarehouseId = null)
        {
            if (productId <= 0)
                return Json(new List<object>());
            var items = await (from ii in _warehouseDbContext.InventoryItems
                               join inv in _warehouseDbContext.Inventories on ii.InventoryId equals inv.Id
                               join p in _warehouseDbContext.Products on inv.ProductId equals p.Id
                               join st in _warehouseDbContext.Statuses on p.StatusId equals st.Id
                               join g in _warehouseDbContext.Groups on st.GroupId equals g.Id
                               join c in _warehouseDbContext.Categories on g.CategoryId equals c.Id
                               join w in _warehouseDbContext.Warehouses on inv.WarehouseId equals w.Id
                               join z in _warehouseDbContext.StorageZones on inv.ZoneId equals z.Id into zGroup
                               from z in zGroup.DefaultIfEmpty()
                               join s in _warehouseDbContext.StorageSections on inv.SectionId equals s.Id into sGroup
                               from s in sGroup.DefaultIfEmpty()
                               where inv.ProductId == productId
                                     && (!sourceWarehouseId.HasValue || inv.WarehouseId == sourceWarehouseId.Value)
                               select new
                               {
                                   id = ii.Id,
                                   text = "C" + (c.Code ?? "NA") +
                                          "G" + (g.Code ?? "NA") +
                                          "S" + (st.Code ?? "NA") +
                                          "P" + (p.Code ?? "NA") + "_" + (ii.UniqueCode ?? "NA"),
                                   hierarchy = "C" + (c.Code ?? "NA") +
                                               "G" + (g.Code ?? "NA") +
                                               "S" + (st.Code ?? "NA") +
                                               "P" + (p.Code ?? "NA") +
                                               " (" + (ii.UniqueCode ?? "NA") + ")"
                               }).ToListAsync();
            return Json(items);
        }
        [HttpGet]
        public async Task<JsonResult> GetUniqueCodeDetails(int uniqueCodeId)
        {
            var item = await (from ii in _warehouseDbContext.InventoryItems
                              join inv in _warehouseDbContext.Inventories on ii.InventoryId equals inv.Id
                              join p in _warehouseDbContext.Products on inv.ProductId equals p.Id
                              join st in _warehouseDbContext.Statuses on p.StatusId equals st.Id
                              join g in _warehouseDbContext.Groups on st.GroupId equals g.Id
                              join c in _warehouseDbContext.Categories on g.CategoryId equals c.Id
                              join w in _warehouseDbContext.Warehouses on inv.WarehouseId equals w.Id
                              join z in _warehouseDbContext.StorageZones on inv.ZoneId equals z.Id into zGroup
                              from z in zGroup.DefaultIfEmpty()
                              join s in _warehouseDbContext.StorageSections on inv.SectionId equals s.Id into sGroup
                              from s in sGroup.DefaultIfEmpty()
                              where ii.Id == uniqueCodeId
                              select new
                              {
                                  id = ii.Id,
                                  text = "C" + (c.Code ?? "NA") +
                                         "G" + (g.Code ?? "NA") +
                                         "S" + (st.Code ?? "NA") +
                                         "P" + (p.Code ?? "NA") + "_" + (ii.UniqueCode ?? "NA"),
                                  hierarchy = "C" + (c.Code ?? "NA") +
                                              "G" + (g.Code ?? "NA") +
                                              "S" + (st.Code ?? "NA") +
                                              "P" + (p.Code ?? "NA") +
                                              "(Warehouse:" + w.Id +
                                              ", Zone:" + (z.Name ?? "نامشخص") +
                                              ", Section:" + (s.Name ?? "نامشخص") +
                                              ", UniqueCode:" + (ii.UniqueCode ?? "NA") + ")",
                                  sourceWarehouseId = inv.WarehouseId,
                                  sourceZoneId = inv.ZoneId,
                                  sourceSectionId = inv.SectionId
                              }).FirstOrDefaultAsync();
            return Json(item);
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
        public IActionResult Print(int id)
        {
            var conversion = _warehouseDbContext.conversionDocuments.FirstOrDefault(d => d.Id == id);
            if (conversion == null)
                return NotFound();
            // بارگذاری داده‌ها به صورت دیکشنری برای سرعت
            var categories = _warehouseDbContext.Categories.ToDictionary(c => c.Id, c => c.Name);
            var groups = _warehouseDbContext.Groups.ToDictionary(g => g.Id, g => g.Name);
            var statuses = _warehouseDbContext.Statuses.ToDictionary(s => s.Id, s => s.Name);
            var products = _warehouseDbContext.Products.ToDictionary(p => p.Id, p => p.Name);
            var warehouses = _warehouseDbContext.Warehouses.ToDictionary(w => w.Id, w => w.Name);
            var zones = _warehouseDbContext.StorageZones.ToDictionary(z => z.Id, z => z.Name);
            var sections = _warehouseDbContext.StorageSections.ToDictionary(s => s.Id, s => s.Name);
            var consumedItems = _warehouseDbContext.conversionConsumedItems
                .Where(i => i.ConversionDocumentId == id)
                .Select(i => new ConversionItemViewModel
                {
                    CategoryName = categories.ContainsKey(i.CategoryId) ? categories[i.CategoryId] : "—",
                    GroupName = groups.ContainsKey(i.GroupId) ? groups[i.GroupId] : "—",
                    StatusName = statuses.ContainsKey(i.StatusId) ? statuses[i.StatusId] : "—",
                    ProductName = products.ContainsKey(i.ProductId) ? products[i.ProductId] : "—",
                    WarehouseName = warehouses.ContainsKey(i.WarehouseId) ? warehouses[i.WarehouseId] : "—",
                    ZoneName = zones.ContainsKey(i.ZoneId) ? zones[i.ZoneId] : "—",
                    SectionName = sections.ContainsKey(i.SectionId) ? sections[i.SectionId] : "—",
                    Quantity = i.Quantity
                }).ToList();
            var producedItems = _warehouseDbContext.conversionProducedItems
                .Where(i => i.ConversionDocumentId == id)
                .Select(i => new ConversionItemViewModel
                {
                    CategoryName = categories.ContainsKey(i.CategoryId) ? categories[i.CategoryId] : "—",
                    GroupName = groups.ContainsKey(i.GroupId) ? groups[i.GroupId] : "—",
                    StatusName = statuses.ContainsKey(i.StatusId) ? statuses[i.StatusId] : "—",
                    ProductName = products.ContainsKey(i.ProductId) ? products[i.ProductId] : "—",
                    WarehouseName = warehouses.ContainsKey(i.WarehouseId) ? warehouses[i.WarehouseId] : "—",
                    ZoneName = zones.ContainsKey(i.ZoneId) ? zones[i.ZoneId] : "—",
                    SectionName = sections.ContainsKey(i.SectionId) ? sections[i.SectionId] : "—",
                    Quantity = i.Quantity
                }).ToList();
            var viewModel = new ConversionPrintViewModel
            {
                DocumentNumber = conversion.DocumentNumber,
                CreatedAt = conversion.CreatedAt,
                ConsumedItems = consumedItems,
                ProducedItems = producedItems
            };
            return new ViewAsPdf("Print", viewModel)
            {
                FileName = $"Conversion_{id}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }
    }
}