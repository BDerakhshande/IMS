using IMS.Application.ProcurementManagement.DTOs;

namespace IMS.Models.ProMan
{
    public class PurchaseRequestPrintViewModel
    {
        public List<PurchaseRequestFlatItemDto> FlatItems { get; set; }
        public string RequestNumber { get; set; }
        public string RequestTitle { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string RequestTypeName { get; set; }
        public string ProjectName { get; set; }
        public DateTime PrintDate { get; set; } = DateTime.Now;
    }
}
