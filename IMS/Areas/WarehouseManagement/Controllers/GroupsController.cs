using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        // GET: WarehouseManagement/Groups?categoryId=5
        public async Task<IActionResult> Index(int categoryId)
        {
            var groups = await _groupService.GetAllAsync(categoryId);
            ViewData["CategoryId"] = categoryId;  // ارسال دسته‌بندی به ویو
            return View(groups);
        }


        // GET: WarehouseManagement/Groups/Create?categoryId=5
        public async Task<IActionResult> Create(int categoryId)
        {
            var dto = new GroupDto
            {
                CategoryId = categoryId,
            };

            return View(dto);
        }



        // POST: WarehouseManagement/Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupDto dto)
        {
            try
            {
                var createdGroup = await _groupService.CreateAsync(dto);
                return RedirectToAction(nameof(Index), new { categoryId = createdGroup.CategoryId });
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا در صورت نیاز
                ModelState.AddModelError(string.Empty, "در فرآیند ایجاد گروه خطایی رخ داده است.");
                return View(dto);
            }
        }


        // GET: WarehouseManagement/Groups/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var group = await _groupService.GetByIdAsync(id);
            if (group == null)
                return NotFound();

            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GroupDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedDto = await _groupService.UpdateAsync(id, dto);
                    if (updatedDto == null)
                        return NotFound();

                    return RedirectToAction(nameof(Index), new { categoryId = updatedDto.CategoryId });

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"خطا در بروزرسانی: {ex.Message}");
                }
            }

            return View(dto);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var group = await _groupService.GetByIdAsync(id);
                if (group == null)
                    return NotFound();

                var deleted = await _groupService.DeleteAsync(id);
                if (!deleted)
                    return NotFound();

                return RedirectToAction(nameof(Index), new { categoryId = group.CategoryId });
            }
            catch (InvalidOperationException ex)
            {
                // پیام کاربر پسند
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index), new { categoryId = (await _groupService.GetByIdAsync(id))?.CategoryId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطای غیرمنتظره‌ای رخ داده است.";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
