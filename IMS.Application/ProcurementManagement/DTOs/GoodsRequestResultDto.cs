using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.ProcurementManagement.DTOs
{
    public class GoodsRequestResultDto
    {
        public int ProductId { get; set; }
        public decimal RequestedQuantity { get; set; }
        public string RequestedByName { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public string ProjectName { get; set; } // Added to match input
        public List<GoodsRequestInventoryReportDto> InventoryReport { get; set; }
        public bool IsNeedPurchase { get; set; }
        public string? Message { get; set; }
        public int? CreatedRequestId { get; set; } // Added to return the created request ID if successful
    }

}
