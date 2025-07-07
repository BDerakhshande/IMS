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

        // نمایش فرم ایجاد پروژه
        public async Task<IActionResult> Create()
        {
            var employers = await _employerService.GetAllEmployersAsync();
            var projectTypes = await _projectTypeService.GetAllAsync();
            // تبدیل به SelectListItem
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



            return View();
        }


        // ثبت پروژه جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _projectService.CreateProjectAsync(dto);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "خطا در ثبت پروژه");
            return View(dto);
        }

        // نمایش فرم ویرایش پروژه
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        // ثبت تغییرات ویرایش
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _projectService.UpdateProjectAsync(dto);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "خطا در به‌روزرسانی پروژه");
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

        // تایید حذف پروژه
        [HttpPost, ActionName("Delete")]
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
    }
}
