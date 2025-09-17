using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using IMS.Models.ProMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace IMS.Areas.WarehouseManagement.Controllers
{
    [Area("WarehouseManagement")]
    public class WarehouseTransactionDetailController : Controller
    {
        private readonly IWarehouseTransactionDetailService _transactionDetailService;
        private readonly IApplicationDbContext _projectContext;
        public WarehouseTransactionDetailController(
        IWarehouseTransactionDetailService transactionDetailService,
        IApplicationDbContext projectContext)  
        {
            _transactionDetailService = transactionDetailService;
            _projectContext = projectContext;  
        }

        // GET: /WarehouseManagement/WarehouseTransactionDetail
        public async Task<IActionResult> Index(
            string? projectName,
            string? transactionType,
            bool isSearchClicked = false) // پارامتر مشخص می‌کند که کاربر جستجو زده
        {
            // فراخوانی سرویس با سه مقدار خروجی
            var (transactions, projects, transactionTypes) = await _transactionDetailService
                .GetAllTransactionsWithProjectsAsync(projectName, transactionType, isSearchClicked);

            // ساخت ViewModel برای ویو
            var viewModel = new WarehouseTransactionDetailViewModel
            {
                Transactions = transactions,
                Projects = projects,
                TransactionTypes = transactionTypes, 
                SelectedProjectName = projectName,
                SelectedTransactionType = transactionType,
                IsSearchClicked = isSearchClicked
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportToExcel(string? projectName, string? transactionType)
        {
            var fileContent = await _transactionDetailService.ExportTransactionsToExcelAsync(projectName, transactionType);
            return File(fileContent,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "WarehouseTransactions.xlsx");
        }


        [HttpPost]
        public async Task<IActionResult> ExportToPdf(string? projectName, string? transactionType)
        {
            // گرفتن داده‌ها از سرویس با فیلترهای اعمال شده
            var (transactions, projects, transactionTypes) = await _transactionDetailService
                .GetAllTransactionsWithProjectsAsync(projectName, transactionType, true);

            // آماده‌سازی ViewModel برای PDF
            var vm = new WarehouseTransactionDetailPdfViewModel
            {
                Transactions = transactions,
                Projects = projects,
                TransactionTypes = transactionTypes,
                SelectedProjectName = projectName,
                SelectedTransactionType = transactionType
            };

            // بازگرداندن PDF با Rotativa
            return new ViewAsPdf("WarehouseTransactionPdfView", vm)
            {
                FileName = $"WarehouseTransactions_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10),
                CustomSwitches = "--disable-smart-shrinking --print-media-type --background"
            };
        }

    }


}
