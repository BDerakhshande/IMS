using IMS.Application.ProjectManagement.Service;
using IMS.Domain.ProjectManagement.Enums;
using IMS.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    public class ProjectManagementHomeController : Controller
    {
        private IApplicationDbContext _projectContext;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProjectManagementHomeController(IApplicationDbContext projectContext , UserManager<ApplicationUser> userManager)
        {
            _projectContext = projectContext;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                ViewData["Username"] = user.UserName;
            }
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
