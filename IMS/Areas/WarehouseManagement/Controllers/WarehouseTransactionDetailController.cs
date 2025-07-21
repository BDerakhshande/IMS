using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Application.WarehouseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        // در کنترلر
        public async Task<IActionResult> Index(string? projectName, string? transactionType)
        {
            var transactions = await _transactionDetailService.GetAllTransactionsAsync(projectName, transactionType);

            // لیست پروژه‌ها
            var projects = await _projectContext.Projects
                .OrderBy(p => p.ProjectName)
                .ToListAsync();

            // نوع تراکنش‌ها بدون تکرار در مقدار فارسی
            var transactionTypes = new Dictionary<string, string>
    {
        { "Receipt", "رسید" },
        { "Issue", "حواله" },
        { "Transfer", "انتقال" },
        { "Consumption", "تبدیل" },
        { "Production", "تبدیل" }
    };

            var uniqueTransactionTypes = transactionTypes
                .GroupBy(kv => kv.Value)
                .Select(g => g.First())
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            ViewData["Projects"] = projects;
            ViewData["TransactionType"] = transactionType;
            ViewData["TransactionTypes"] = uniqueTransactionTypes;

            return View(transactions);
        }



    }
}
