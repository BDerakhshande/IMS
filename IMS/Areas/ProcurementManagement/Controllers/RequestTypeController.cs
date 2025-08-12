using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProcurementManagement.Service;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Areas.ProcurementManagement.Controllers
{
    [Area("ProcurementManagement")]
    public class RequestTypeController : Controller
    {
        private readonly IRequestTypeService _service;

        public RequestTypeController(IRequestTypeService service)
        {
            _service = service;
        }

        // GET: ProcurementManagement/RequestType
        public async Task<IActionResult> Index()
        {
            var list = await _service.GetAllAsync();
            return View(list);
        }

      

        // GET: ProcurementManagement/RequestType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProcurementManagement/RequestType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RequestTypeDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _service.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // GET: ProcurementManagement/RequestType/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        // POST: ProcurementManagement/RequestType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RequestTypeDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            var updated = await _service.UpdateAsync(dto);
            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // GET: ProcurementManagement/RequestType/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        // POST: ProcurementManagement/RequestType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
