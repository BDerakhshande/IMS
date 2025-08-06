using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProcurementManagement.Service;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.ProcurementManagement.Entities;
using IMS.Domain.ProcurementManagement.Enums;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Areas.ProcurementManagement.Controllers
{

    [Area("ProcurementManagement")]
    public class GoodsRequestController : Controller
    {
        private readonly IGoodsRequestService _goodsRequestService;

        public GoodsRequestController(IGoodsRequestService goodsRequestService)
        {
            _goodsRequestService = goodsRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new GoodsRequestInputDto();
            await PopulateSelectListsAsync(model);
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GoodsRequestInputDto input)
        {
            var result = await _goodsRequestService.HandleGoodsRequestAsync(input);

            // مقداردهی ViewBag برای نمایش گزارش موجودی
            ViewBag.InventoryReport = result.InventoryReport;

            if (result.IsNeedPurchase)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                await PopulateSelectListsAsync(input); // بارگذاری لیست‌ها با حفظ انتخاب‌ها

                // حذف ModelState مقدار RequestedQuantity برای نمایش مقدار صحیح
                ModelState.Remove(nameof(input.RequestedQuantity));

                return View(input);
            }

            TempData["SuccessMessage"] = result.Message;

            await PopulateSelectListsAsync(input); // بارگذاری لیست‌ها برای فرم جدید

            // حذف ModelState مقدار RequestedQuantity برای نمایش مقدار صحیح
            ModelState.Remove(nameof(input.RequestedQuantity));

            return View(input);
        }





        private async Task PopulateSelectListsAsync(GoodsRequestInputDto model)
        {
            model.Categories = await _goodsRequestService.GetAllCategoriesAsync();
            model.Groups = await _goodsRequestService.GetAllGroupsAsync();
            model.Statuses = await _goodsRequestService.GetAllStatusesAsync();
            model.Products = await _goodsRequestService.GetAllProductsAsync();
            model.Projects = await _goodsRequestService.GetAllProjectsAsync();
        }





        [HttpGet]
        public async Task<IActionResult> GetGroups(int categoryId)
        {
            var groups = await _goodsRequestService.GetGroupsByCategoryIdAsync(categoryId);
            return Json(groups);
        }


        [HttpGet]
        public async Task<IActionResult> GetStatuses(int groupId)
        {
            var statuses = await _goodsRequestService.GetStatusesByGroupIdAsync(groupId);
            return Json(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int statusId)
        {
            var products = await _goodsRequestService.GetProductsByStatusIdAsync(statusId);
            return Json(products);
        }


    }
}
