using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class ProductsController : Controller
    {

        private readonly IProductService _productsService;
        private readonly IStatusService _statusService;

        public ProductsController(IProductService productsService , IStatusService statusService)
        {
            _productsService = productsService;
            _statusService = statusService;
        }


        public async Task<IActionResult> Index(int statusId)
        {
            var allProducts = await _productsService.GetAllAsync(statusId);

            var status = await _statusService.GetStatusByIdAsync(statusId);
            if (status == null)
            {
                return NotFound();
            }

            ViewBag.StatusId = statusId;
            ViewBag.GroupId = status.GroupId; 

            return View(allProducts);
        }




        [HttpGet]
        public IActionResult Create(int statusId)
        {
           
            var dto = new ProductDto
            {
                StatusId = statusId
            };

            return View(dto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto dto)
        {
            try
            {
                await _productsService.CreateAsync(dto);
                return RedirectToAction(nameof(Index), new { statusId = dto.StatusId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(dto.Code), ex.Message);

                return View(dto);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productsService.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            ViewBag.StatusId = product.StatusId;
            return View(product);
        }


        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductDto dto)
        {
            try
            {
                await _productsService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index), new { statusId = dto.StatusId });
            }
            catch (Exception ex)
            {
          
                if (ex.Message.Contains("کدی که وارد کرده‌اید"))
                {
                    ModelState.AddModelError(nameof(dto.Code), ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                ViewBag.StatusId = dto.StatusId;
                return View(dto);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int statusId)
        {
            try
            {
                await _productsService.DeleteAsync(id);
                TempData["SuccessMessage"] = "محصول با موفقیت حذف شد.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "امکان حذف این کالا وجود ندارد زیرا با این کالا عملیات انجام شده است.";
            }
            return RedirectToAction(nameof(Index), new { statusId = statusId });
        }



        // GET: WarehouseManagement/Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productsService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var status = await _statusService.GetStatusByIdAsync(product.StatusId);
            ViewBag.StatusId = product.StatusId;
            ViewBag.StatusName = status?.Name ?? "نامشخص";

            return View(product);
        }


    }
}
