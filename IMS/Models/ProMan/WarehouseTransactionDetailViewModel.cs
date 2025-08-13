using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.WarehouseManagement.DTOs;

namespace IMS.Models.ProMan
{
    public class WarehouseTransactionDetailViewModel
    {
        public List<WarehouseTransactionDetailDto> Transactions { get; set; } = new();
        public List<ProjectDto> Projects { get; set; } = new();
        // فیلترهای انتخابی
        public string? SelectedProjectName { get; set; }
        public string? SelectedTransactionType { get; set; }
    }
}
