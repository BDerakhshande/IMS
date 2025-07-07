using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    public class ProjectTypeController : Controller
    {
        private readonly IProjectTypeService _projectTypeService;

        public ProjectTypeController(IProjectTypeService projectTypeService)
        {
            _projectTypeService = projectTypeService;
        }

        // لیست انواع پروژه‌ها
        public async Task<IActionResult> Index()
        {
            var types = await _projectTypeService.GetAllAsync();
            return View(types);
        }

        // نمایش فرم ایجاد نوع پروژه
        public IActionResult Create()
        {
            return View();
        }

        // ثبت نوع پروژه جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectTypeDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var id = await _projectTypeService.CreateAsync(dto);
            if (id > 0)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "خطا در ثبت نوع پروژه");
            return View(dto);
        }

        // نمایش فرم ویرایش نوع پروژه
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _projectTypeService.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        // ثبت تغییرات ویرایش
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectTypeDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _projectTypeService.UpdateAsync(dto);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "خطا در به‌روزرسانی نوع پروژه");
            return View(dto);
        }

        // نمایش تایید حذف نوع پروژه
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _projectTypeService.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        // تایید حذف نوع پروژه
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _projectTypeService.DeleteAsync(id);
            if (!result)
            {
                ModelState.AddModelError("", "خطا در حذف نوع پروژه");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // مشاهده جزئیات نوع پروژه
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _projectTypeService.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }
    }
}
