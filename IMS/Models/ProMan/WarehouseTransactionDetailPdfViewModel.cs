using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.WarehouseManagement.DTOs;

namespace IMS.Models.ProMan
{
    public class WarehouseTransactionDetailPdfViewModel
    {
        public List<WarehouseTransactionDetailDto> Transactions { get; set; } = new List<WarehouseTransactionDetailDto>();

        public List<ProjectDto> Projects { get; set; } = new List<ProjectDto>();

        public string? SelectedProjectName { get; set; }

        public string? SelectedTransactionType { get; set; }

        // برای نام‌های فارسی انواع تراکنش
        public Dictionary<string, string> TransactionTypeNames { get; set; } = new Dictionary<string, string>
        {
            { "Conversion", "تبدیل" },
            { "Receipt", "رسید" },
            { "Issue", "حواله" },
            { "Transfer", "انتقال" }
        };
    }
}
