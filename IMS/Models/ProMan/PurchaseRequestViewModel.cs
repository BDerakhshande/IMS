using IMS.Domain.ProcurementManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ProMan
{
    public class PurchaseRequestViewModel
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = null!;
        public string RequestDateString { get; set; }
        public DateTime RequestDate { get; set; }
        public string DueDateString { get; set; }
        public DateTime? DueDate { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public Status Status { get; set; }

        public List<PurchaseRequestItemViewModel> Items { get; set; } = new List<PurchaseRequestItemViewModel>();

        public List<SelectListItem>? AvailableSuppliers { get; set; }
        public List<SelectListItem>? AvailableStatuses { get; set; }
    }
}
