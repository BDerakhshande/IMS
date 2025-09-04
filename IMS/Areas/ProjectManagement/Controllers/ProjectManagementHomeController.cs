using IMS.Application.ProjectManagement.Service;
using IMS.Domain.ProjectManagement.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    public class ProjectManagementHomeController : Controller
    {
        private IApplicationDbContext _projectContext;
        public ProjectManagementHomeController(IApplicationDbContext projectContext)
        {
            _projectContext = projectContext;
        }
        public async Task<IActionResult> Index()
        {
            // فقط پروژه‌هایی که در حال اجرا هستند
            var projects = await _projectContext.Projects
                                         .Include(p => p.Employer)
                                         .Include(p => p.ProjectType)
                                         .Where(p => p.Status == ProjectStatus.InProgress)
                                         .ToListAsync();

            return View(projects);
        }
    }
}
