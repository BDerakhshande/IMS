using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ProMan
{
    public class ReceiptOrIssueViewModel
    {
        public int Id { get; set; }
        public string UniqueCode { get; set; } 
        public string DocumentNumber { get; set; }
        public string DateString { get; set; }
        public DateTime Date { get; set; }
        public ReceiptOrIssueType? Type { get; set; }

        public string? Description { get; set; }

        // فیلد برای نمایش کد یکتای انتخاب‌شده
        public string? SelectedUniqueCode { get; set; }

        //  لیست کدهای یکتا برای نمایش در Dropdown (در صورت نیاز)
        public List<string> AvailableUniqueCodes { get; set; } = new();
        public List<ReceiptOrIssueItemViewModel> Items { get; set; } = new();
    }
}
