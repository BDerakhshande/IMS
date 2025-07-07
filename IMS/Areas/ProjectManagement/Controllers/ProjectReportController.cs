using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using IMS.Application.ProjectManagement.ViewModels;

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
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProjectReportRequestDto model)
        {
            await LoadSelectListsAsync();

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
    }

}
