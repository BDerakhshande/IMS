using System.Globalization;
using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IEmployerService _employerService;
        private readonly IProjectTypeService _projectTypeService;

        public ProjectController(IEmployerService employerService ,IProjectService projectService, IProjectTypeService projectTypeService)
        {
            _employerService = employerService;
            _projectService = projectService;
            _projectTypeService = projectTypeService;
        }

        // لیست پروژه‌ها
        public async Task<IActionResult> Index()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return View(projects);
        }

        public async Task<IActionResult> Create()
        {
            var employers = await _employerService.GetAllEmployersAsync();
            var projectTypes = await _projectTypeService.GetAllAsync();

            ViewBag.Employers = employers.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.CompanyName
            }).ToList();

            ViewBag.ProjectTypes = projectTypes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            // مقدار پیش‌فرض امروز شمسی
            var pc = new PersianCalendar();
            var now = DateTime.Now;
            var todayShamsi = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";

            var dto = new ProjectDto
            {
                StartDate = now,
                EndDate = now
            };

            ViewBag.TodayShamsi = todayShamsi; // مقدار پیش‌فرض برای ویو

            return View(dto);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectDto dto, string StartDate, string EndDate)
        {
            try
            {
                

                // Parse Persian dates
                dto.StartDate = ParsePersianDate(StartDate) ?? DateTime.Now;
                dto.EndDate = ParsePersianDate(EndDate) ?? DateTime.Now;

                // Attempt to create the project
                var result = await _projectService.CreateProjectAsync(dto);
                if (result)
                    return RedirectToAction(nameof(Index));
                else
                {
                    ModelState.AddModelError("", "خطا در ثبت پروژه");
                    await PopulateViewBag();
                    return View(dto);
                }
            }
            catch (DbUpdateException ex)
            {
                // Extract the inner SqlException message
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                if (errorMessage.Contains("Cannot insert the value NULL into column 'Location'"))
                {
                    ModelState.AddModelError("", "لطفاً فیلد مکان را پر کنید.");
                }
                else
                {
                    ModelState.AddModelError("", $"خطا در ثبت پروژه: {errorMessage}");
                }

                // Repopulate ViewBag data for the view
                await PopulateViewBag();
                return View(dto);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                ModelState.AddModelError("", $"خطای غیرمنتظره: {ex.Message}");
                await PopulateViewBag();
                return View(dto);
            }
        }

        // Helper method to populate ViewBag data
        private async Task PopulateViewBag()
        {
            var employers = await _employerService.GetAllEmployersAsync();
            var projectTypes = await _projectTypeService.GetAllAsync();

            ViewBag.Employers = employers.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.CompanyName
            }).ToList();

            ViewBag.ProjectTypes = projectTypes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            // Set default Persian calendar date
            var pc = new PersianCalendar();
            var now = DateTime.Now;
            var todayShamsi = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";
            ViewBag.TodayShamsi = todayShamsi;
        }


        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound();

            // Populate ViewBag for Employers and ProjectTypes
            var employers = await _employerService.GetAllEmployersAsync();
            var projectTypes = await _projectTypeService.GetAllAsync();

            ViewBag.Employers = employers.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.CompanyName,
                Selected = e.Id == project.EmployerId // Pre-select the current employer
            }).ToList();

            ViewBag.ProjectTypes = projectTypes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = p.Id == project.ProjectTypeId // Pre-select the current project type
            }).ToList();

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectDto dto, string StartDate, string EndDate)
        {
            dto.StartDate = ParsePersianDate(StartDate) ?? DateTime.Now;
            dto.EndDate = ParsePersianDate(EndDate) ?? DateTime.Now;

          

            var result = await _projectService.UpdateProjectAsync(dto);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "خطا در به‌روزرسانی پروژه");

            // Repopulate ViewBag again if update fails
            var employersFail = await _employerService.GetAllEmployersAsync();
            var projectTypesFail = await _projectTypeService.GetAllAsync();

            ViewBag.Employers = employersFail.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.CompanyName,
                Selected = e.Id == dto.EmployerId
            }).ToList();

            ViewBag.ProjectTypes = projectTypesFail.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = p.Id == dto.ProjectTypeId
            }).ToList();

            return View(dto);
        }

        // نمایش تایید حذف
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _projectService.DeleteProjectAsync(id);
            return RedirectToAction(nameof(Index));
        }


        // مشاهده جزئیات پروژه
        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound();

            return View(project);
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
