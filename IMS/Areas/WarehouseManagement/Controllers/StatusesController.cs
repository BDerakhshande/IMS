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

        public StatusesController(IStatusService statusService , IGroupService groupService)
        {
            _statusService = statusService;
            _groupService = groupService;
        }



        public async Task<IActionResult> Index(int groupId)
        {
            var allStatuses = await _statusService.GetAllAsync(groupId);
            ViewBag.GroupId = groupId;
            return View(allStatuses);
        }


        [HttpGet]
        public IActionResult Create(int groupId)
        {
            if (groupId == 0)
                return BadRequest("شناسه گروه نامعتبر است.");

            var dto = new StatusDto
            {
                GroupId = groupId
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
                // نمایش خطا در ویو
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
                ModelState.AddModelError(string.Empty, ex.Message);
             
                return View(dto);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _statusService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return RedirectToAction("Index", new { groupId = id });
            }
            catch (InvalidOperationException ex)
            {
                // پیام خطا را به ویو ارسال کنید یا در TempData بگذارید
                TempData["ErrorMessage"] = ex.Message;

                // به صفحه لیست یا هر صفحه دلخواه هدایت کنید
                return RedirectToAction("Index", new { groupId = id });
            }
        }



    }
}
