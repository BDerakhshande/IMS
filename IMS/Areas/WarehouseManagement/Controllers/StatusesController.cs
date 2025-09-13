using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class StatusesController : Controller
    {
        private readonly IStatusService _statusService;
        private readonly IGroupService _groupService;
        private readonly ICategoryService _categoryService;

        public StatusesController(IStatusService statusService , IGroupService groupService , ICategoryService categoryService)
        {
            _statusService = statusService;
            _groupService = groupService;
            _categoryService = categoryService;
        }



        public async Task<IActionResult> Index(int groupId)
        {
            var allStatuses = await _statusService.GetAllAsync(groupId);
            ViewBag.GroupId = groupId;
            return View(allStatuses);
        }


        [HttpGet]
        public async Task<IActionResult> Create(int groupId)
        {
            if (groupId == 0)
                return BadRequest("شناسه گروه نامعتبر است.");

            // گرفتن کد بعدی
            var nextCode = await _categoryService.GenerateNextCodeAsync<Status>(
                s => s.Code,
                s => s.Id
            );

            var dto = new StatusDto
            {
                GroupId = groupId,
                Code = nextCode // 👈 پر کردن مقدار پیش‌فرض کد
            };

            return View(dto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StatusDto dto)
        {
            try
            {
                await _statusService.CreateStatusAsync(dto);
                return RedirectToAction("Index", new { groupId = dto.GroupId });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("کد طبقه تکراری"))
                    ModelState.AddModelError(nameof(dto.Code), ex.Message); // این خط تغییر یافته
                else
                    ModelState.AddModelError(string.Empty, ex.Message);

                return View(dto);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var status = await _statusService.GetStatusByIdAsync(id);
            if (status == null)
                return NotFound();

            return View(status);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StatusDto dto)
        {
            try
            {
                await _statusService.UpdateStatusAsync(dto);
                return RedirectToAction("Index", new { groupId = dto.GroupId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Code", ex.Message);

                return View(dto);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var status = await _statusService.GetStatusByIdAsync(id);
            var groupId = status.GroupId;

            await _statusService.DeleteAsync(id);
            return RedirectToAction("Index", new { groupId });
        }




    }
}
