using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Domain.ProjectManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using IMS.Areas.AccountManagement.Helper;

namespace IMS.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    public class EmployersController : Controller
    {
        private readonly IEmployerService _employerService;

        public EmployersController(IEmployerService employerService)
        {
            _employerService = employerService;
        }

        public async Task<IActionResult> Index()
        {
            var employers = await _employerService.GetAllEmployersAsync();
            return View(employers);
        }


        private List<SelectListItem> GetSelectListItems<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.GetType()
                            .GetMember(e.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            ?.GetName() ?? e.ToString()
                }).ToList();
        }
        public IActionResult Create()
        {
            ViewBag.LegalPersonTypes = GetSelectListItems<LegalPersonType>();
            ViewBag.CooperationType = GetSelectListItems<CooperationType>();

            var pc = new PersianCalendar();
            var now = DateTime.Now;
            var todayShamsi = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";

            var dto = new EmployerDto
            {
                CooperationStartDate = now,
               
            };

            ViewBag.TodayShamsi = todayShamsi; // برای نمایش در ویو
            return View(dto);
        }


        [HttpPost]
        public async Task<IActionResult> Create(EmployerDto dto, string CooperationStartDatePersian, string? CooperationEndDatePersian)
        {
            dto.CooperationStartDate = ParsePersianDate(CooperationStartDatePersian) ?? DateTime.Now;

            if (!ModelState.IsValid)
            {
                ViewBag.LegalPersonTypes = GetSelectListItems<LegalPersonType>();
                ViewBag.CooperationType = GetSelectListItems<CooperationType>();
                return View(dto);
            }

            await _employerService.CreateEmployerAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _employerService.GetEmployerByIdAsync(id);
            if (dto == null) return NotFound();

            ViewBag.LegalPersonTypes = GetSelectListItems<LegalPersonType>();
            ViewBag.CooperationType = GetSelectListItems<CooperationType>();

            var pc = new PersianCalendar();
            ViewBag.CooperationStartDatePersian = $"{pc.GetYear(dto.CooperationStartDate):0000}/{pc.GetMonth(dto.CooperationStartDate):00}/{pc.GetDayOfMonth(dto.CooperationStartDate):00}";
          

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployerDto dto, string CooperationStartDatePersian, string? CooperationEndDatePersian)
        {
            dto.CooperationStartDate = ParsePersianDate(CooperationStartDatePersian) ?? DateTime.Now;
           
            if (!ModelState.IsValid)
            {
                ViewBag.LegalPersonTypes = GetSelectListItems<LegalPersonType>();
                ViewBag.CooperationType = GetSelectListItems<CooperationType>();
                return View(dto);
            }

            var result = await _employerService.UpdateEmployerAsync(dto);
            if (!result) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _employerService.DeleteEmployerAsync(id);
            return RedirectToAction(nameof(Index));
        }


        private DateTime? ParsePersianDate(string? persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
            {
                Console.WriteLine("Persian date is null or empty");
                return null;
            }

            try
            {
                var parts = persianDate.Split('/');
                if (parts.Length != 3)
                {
                    Console.WriteLine($"Invalid date format: {persianDate}");
                    return null;
                }

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                var pc = new PersianCalendar();
                var result = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                Console.WriteLine($"Parsed date: {persianDate} -> {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing date {persianDate}: {ex.Message}");
                return null;
            }
        }
    }
}
