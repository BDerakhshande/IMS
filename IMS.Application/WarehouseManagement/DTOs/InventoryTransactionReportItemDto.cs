using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryTransactionReportItemDto
    {
        public string? WarehouseName { get; set; }
        public string? DepartmentName { get; set; }
        public string? SectionName { get; set; }
        public string? CategoryName { get; set; }
        public string? GroupName { get; set; }
        public string? StatusName { get; set; }
        public string? ProductName { get; set; }
        public string? DocumentType { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }
}
