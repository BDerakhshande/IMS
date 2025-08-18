using System.Globalization;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProcurementManagement.Service;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.ProcurementManagement.Entities;
using IMS.Domain.ProcurementManagement.Enums;
using IMS.Infrastructure.Persistence.ProcurementManagement;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace IMS.Areas.ProcurementManagement.Controllers
{
    [Area("ProcurementManagement")]
    public class PurchaseRequestController : Controller
    {
        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly IProcurementManagementDbContext _procurementManagementDbContext;
        private readonly ICategoryService _categoryService;
        private readonly IApplicationDbContext _applicationDbContext;
        private readonly IWarehouseDbContext _warehouseContext;

      

        public PurchaseRequestController(IPurchaseRequestService purchaseRequestService , IProcurementManagementDbContext procurementManagementDbContext,
            ICategoryService categoryService, IApplicationDbContext applicationDbContext , IWarehouseDbContext warehouseContext)
        {
            _purchaseRequestService = purchaseRequestService;
            _procurementManagementDbContext = procurementManagementDbContext;
            _categoryService = categoryService;
            _applicationDbContext = applicationDbContext;
            _warehouseContext = warehouseContext;

        }

        
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var dtos = await _purchaseRequestService.GetAllAsync(cancellationToken);
          
            return View(dtos);
        }



        public async Task<IActionResult> Create()
        {
            var vm = new PurchaseRequestViewModel();

            // مقداردهی RequestDate با تاریخ امروز به صورت شمسی
            vm.RequestDate = DateTime.Now;
            vm.RequestDateString = ConvertToPersianDateString(vm.RequestDate);
           

            // گرفتن شماره درخواست بعدی
            vm.RequestNumber = await GetNextRequestNumberAsync();

            // حتما یه آیتم اولیه اضافه کن تا در حلقه PopulateSelectListsAsync مشکلی نباشه
            vm.Items = new List<PurchaseRequestItemViewModel>
    {
        new PurchaseRequestItemViewModel()
    };

            await PopulateSelectListsAsync(vm);

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseRequestViewModel vm, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(vm.RequestDateString))
            {
                try
                {
                    vm.RequestDate = ParsePersianDate(vm.RequestDateString);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(vm.RequestDateString), "تاریخ وارد شده نامعتبر است.");
                }
            }

            //if (!ModelState.IsValid)
            //{
            //    await PopulateSelectListsAsync(vm);
            //    return View(vm);
            //}

            var dto = MapToDto(vm);

            var id = await _purchaseRequestService.CreateAsync(dto, cancellationToken);

            return RedirectToAction(nameof(Index), new { id });
        }


        // GET: ProcurementManagement/PurchaseRequest/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _purchaseRequestService.GetByIdAsync(id, cancellationToken);
            if (dto == null)
                return NotFound();

            var vm = MapToViewModel(dto);
            await PopulateSelectListsAsync(vm);

            // تبدیل تاریخ میلادی به رشته تاریخ شمسی برای فرم
            vm.RequestDateString = ConvertToPersianDateString(vm.RequestDate);

            return View(vm);
        }

        // POST: ProcurementManagement/PurchaseRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseRequestViewModel vm, CancellationToken cancellationToken)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!string.IsNullOrWhiteSpace(vm.RequestDateString))
            {
                try
                {
                    vm.RequestDate = ParsePersianDate(vm.RequestDateString);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(vm.RequestDateString), "تاریخ وارد شده نامعتبر است.");
                }
            }

          

            var dto = MapToDto(vm);

            var updated = await _purchaseRequestService.UpdateAsync(dto, cancellationToken);

            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Index), new { id });
        }


        // POST: ProcurementManagement/PurchaseRequest/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _purchaseRequestService.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }





        private PurchaseRequestDto MapToDto(PurchaseRequestViewModel vm)
        {
            return new PurchaseRequestDto
            {
                Id = vm.Id,
                RequestNumber = vm.RequestNumber,
                RequestDate = vm.RequestDate,
                RequestTypeId = vm.RequestTypeId,
                Title = vm.Title,
                Notes = vm.Notes,
                Status = vm.Status,
                Items = vm.Items.Select(i => new PurchaseRequestItemDto
                {
                    Id = i.Id,
                    PurchaseRequestId = i.PurchaseRequestId,
                    CategoryId = i.CategoryId,
                    GroupId = i.GroupId,
                    StatusId = i.StatusId,
                    
                    ProductId = i.ProductId,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    ProjectId = i.ProjectId
                }).ToList()
            };
        }

        private async Task PopulateCategorySelectList()
        {
            var categories = await _categoryService.GetAllAsync() ?? new List<CategoryDto>();
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.Groups = new List<SelectListItem> { new SelectListItem { Value = "", Text = "انتخاب کنید" } };
            ViewBag.Statuses = new List<SelectListItem> { new SelectListItem { Value = "", Text = "انتخاب کنید" } };
            ViewBag.Products = new List<SelectListItem> { new SelectListItem { Value = "", Text = "انتخاب کنید" } };
        }



        private PurchaseRequestViewModel MapToViewModel(PurchaseRequestDto dto)
        {
            foreach (var i in dto.Items)
            {
                Console.WriteLine($"DTO ItemId: {i.Id}, StatusId: {i.StatusId}");
            }

            var vm = new PurchaseRequestViewModel
            {
                Id = dto.Id,
                RequestNumber = dto.RequestNumber,
                RequestDate = dto.RequestDate,
                RequestTypeId = dto.RequestTypeId,
                RequestTypeName = dto.RequestTypeName,
                Title = dto.Title,
                Notes = dto.Notes,
                Status = dto.Status,
                Items = dto.Items.Select(i => new PurchaseRequestItemViewModel
                {
                    Id = i.Id,
                    PurchaseRequestId = i.PurchaseRequestId,
                    CategoryId = i.CategoryId,
                    CategoryName = i.CategoryName,
                    GroupId = i.GroupId,
                    GroupName = i.GroupName,
                    StatusId = i.StatusId,
                    Status = i.Status,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    ProjectId = i.ProjectId,
                    ProjectName = i.ProjectName
                }).ToList()
            };

            return vm;
        }




        private async Task PopulateSelectListsAsync(PurchaseRequestViewModel vm)
        {
            vm.AvailableRequestName = await GetSuppliersAsync();

            var categories = await _purchaseRequestService.GetCategoriesAsync();
            var projects = await GetProjectsAsync();

            foreach (var item in vm.Items)
            {

                // پر کردن دسته بندی
                if (item.AvailableCategories == null || !item.AvailableCategories.Any())
                    item.AvailableCategories = categories;

                // پر کردن گروه‌ها بر اساس دسته‌بندی (اگر دسته‌بندی انتخاب شده است)
                if (item.CategoryId != 0)
                {
                    item.AvailableGroups = await _purchaseRequestService.GetGroupsByCategoryAsync(item.CategoryId);
                }
                else
                {
                    item.AvailableGroups = new List<SelectListItem>();
                }

                // پر کردن وضعیت‌ها بر اساس گروه (اگر گروه انتخاب شده است)
                if (item.GroupId != 0)
                {
                    item.AvailableStatuses = await _purchaseRequestService.GetStatusesByGroupAsync(item.GroupId);
                }
                else
                {
                    item.AvailableStatuses = new List<SelectListItem>
        {
            new SelectListItem("انتخاب وضعیت...", "")
        };
                }
                Console.WriteLine($"ItemId: {item.Id}, StatusId: {item.StatusId}");

                // پر کردن محصولات بر اساس وضعیت (اگر وضعیت انتخاب شده است)
                if (item.StatusId != 0)
                {
                    item.AvailableProducts = await _purchaseRequestService.GetProductsByStatus(item.StatusId);
                }
                else
                {
                    item.AvailableProducts = new List<SelectListItem>
        {
            new SelectListItem("انتخاب کالا...", "")
        };
                }

                // پروژه‌ها را حتماً مقداردهی کن
                if (item.AvailableProjects == null || !item.AvailableProjects.Any())
                {
                    item.AvailableProjects = projects;
                }
            }
        }


        public async Task<List<SelectListItem>> GetSuppliersAsync()
        {
            var requestTypes = await _procurementManagementDbContext.RequestTypes
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            return requestTypes;
        }


        private async Task<List<SelectListItem>> GetProjectsAsync()
        {
            var projects = await _applicationDbContext.Projects
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProjectName
                })
                .ToListAsync();

            return projects;
        }




        [HttpGet]
        public async Task<IActionResult> GetGroups(int categoryId)
        {
            var groups = await _purchaseRequestService.GetGroupsByCategoryAsync(categoryId);
            return Json(groups);
        }


        [HttpGet]
        public async Task<IActionResult> GetStatuses(int groupId)
        {
            var statuses = await _purchaseRequestService.GetStatusesByGroupAsync(groupId);
            return Json(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int statusId)
        {
            var products = await _purchaseRequestService.GetProductsByStatus(statusId);
            return Json(products);
        }

       
        private DateTime ParsePersianDate(string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                throw new ArgumentException("تاریخ شمسی نمی‌تواند خالی باشد.");

            var parts = persianDate.Split('/');
            if (parts.Length != 3)
                throw new FormatException("فرمت تاریخ شمسی باید yyyy/MM/dd باشد.");

            var pc = new PersianCalendar();

            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
        private string ConvertToPersianDateString(DateTime date)
        {
            var pc = new PersianCalendar();

            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);

            return $"{year}/{month:00}/{day:00}";
        }

        private async Task<string> GetNextRequestNumberAsync()
        {
            // گرفتن همه شماره‌های درخواست‌ها به صورت رشته
            var existingNumbers = await _procurementManagementDbContext.PurchaseRequests
                .Select(r => r.RequestNumber)
                .ToListAsync();

            // تبدیل رشته‌ها به عدد صحیح، اگر تبدیل ممکن نبود عدد صفر در نظر گرفته می‌شود
            var existingInts = existingNumbers
                .Select(s => int.TryParse(s, out int n) ? n : 0)
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();

            // پیدا کردن کوچک‌ترین عدد مثبت که استفاده نشده است
            int nextNumber = 1;
            foreach (var number in existingInts)
            {
                if (number == nextNumber)
                    nextNumber++;
                else if (number > nextNumber)
                    break;
            }

            // تبدیل عدد به رشته و برگرداندن آن
            return nextNumber.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> SetStatusOpen(int id)
        {
            var pr = await _procurementManagementDbContext.PurchaseRequests.FindAsync(id);
            if (pr == null) return NotFound();

            pr.Status = Status.Open;

            await _procurementManagementDbContext.SaveChangesAsync(CancellationToken.None);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SetStatusCompleted(int id)
        {
            var pr = await _procurementManagementDbContext.PurchaseRequests.FindAsync(id);
            if (pr == null) return NotFound();

            pr.Status = Status.Completed;

            await _procurementManagementDbContext.SaveChangesAsync(CancellationToken.None);

            return RedirectToAction(nameof(Index));
        }

    


    }
}
