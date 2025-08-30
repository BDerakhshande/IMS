using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using IMS.Application.ProjectManagement.ViewModels;
using System.Globalization;

namespace IMS.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    public class ProjectReportController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IEmployerService _employerService;
        private readonly IProjectTypeService _projectTypeService;

        public ProjectReportController(
            IProjectService projectService,
            IEmployerService employerService,
            IProjectTypeService projectTypeService)
        {
            _projectService = projectService;
            _employerService = employerService;
            _projectTypeService = projectTypeService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadSelectListsAsync();

            var model = new ProjectReportRequestDto();

            // مقدار پیش‌فرض امروز
            var now = DateTime.Now;
            var pc = new PersianCalendar();
            string todayShamsi = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";

            // اختصاص رشته شمسی به فیلدهای ViewModel برای نمایش
            model.Filter.StartDateFromShamsi = todayShamsi;
            model.Filter.StartDateToShamsi = todayShamsi;
            model.Filter.EndDateFromShamsi = todayShamsi;
            model.Filter.EndDateToShamsi = todayShamsi;

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Index(ProjectReportRequestDto model)
        {
            await LoadSelectListsAsync();

            // تبدیل رشته‌های شمسی به DateTime
            model.Filter.StartDateFrom = ParsePersianDate(model.Filter.StartDateFromShamsi);
            model.Filter.StartDateTo = ParsePersianDate(model.Filter.StartDateToShamsi);
            model.Filter.EndDateFrom = ParsePersianDate(model.Filter.EndDateFromShamsi);
            model.Filter.EndDateTo = ParsePersianDate(model.Filter.EndDateToShamsi);

            // دریافت گزارش با فیلتر تبدیل‌شده
            var reportData = await _projectService.GetProjectReportAsync(model.Filter);
            model.ReportItems = reportData;

            return View(model);
        }


        private async Task LoadSelectListsAsync()
        {
            var employers = await _employerService.GetAllEmployersAsync();
            var projectTypes = await _projectTypeService.GetAllAsync();

            ViewBag.Employers = new SelectList(employers, "Id", "CompanyName");
            ViewBag.ProjectTypes = new SelectList(projectTypes, "Id", "Name");
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
