using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ProMan
{
    public class PurchaseRequestItemViewModel
    {
        public int Id { get; set; }
        public int PurchaseRequestId { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }

        // لیست‌ها برای dropdownهای هر آیتم
        public List<SelectListItem>? AvailableCategories { get; set; }
        public List<SelectListItem>? AvailableGroups { get; set; }
        public List<SelectListItem>? AvailableStatuses { get; set; }
        public List<SelectListItem>? AvailableProducts { get; set; }
        public List<SelectListItem>? AvailableProjects { get; set; }
    }
}
