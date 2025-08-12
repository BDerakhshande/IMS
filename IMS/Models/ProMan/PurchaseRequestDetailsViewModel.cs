using IMS.Application.ProcurementManagement.DTOs;
using IMS.Domain.ProcurementManagement.Enums;

namespace IMS.Models.ProMan
{
    public class PurchaseRequestDetailsViewModel
    {
        public int PurchaseRequestId { get; set; }
        public string RequestNumber { get; set; } = null!;
        public DateTime RequestDate { get; set; }
        public string? Title { get; set; }
        public Status Status { get; set; }
        public List<PurchaseRequestItemDto> Items { get; set; } = new List<PurchaseRequestItemDto>();
    }
}
