using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Domain.ProjectManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployerDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _employerService.CreateEmployerAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _employerService.GetEmployerByIdAsync(id);
            if (dto == null) return NotFound();
            ViewBag.LegalPersonTypes = GetSelectListItems<LegalPersonType>();
            ViewBag.CooperationType = GetSelectListItems<CooperationType>();
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployerDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _employerService.UpdateEmployerAsync(dto);
            if (!result) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _employerService.DeleteEmployerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
