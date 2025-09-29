using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ProMan
{
    public class ReceiptOrIssueViewModel
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; }
        public string DateString { get; set; }
        public DateTime Date { get; set; }
        public ReceiptOrIssueType? Type { get; set; }

        public string? Description { get; set; }

 
        public List<ReceiptOrIssueItemViewModel> Items { get; set; } = new();
    }
}
