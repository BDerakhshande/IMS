using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryTransactionReportDto
    {
        public string Date { get; set; }
        public string CategoryName { get; set; }
        public string GroupName { get; set; }
        public string StatusName { get; set; }
        public string ProductName { get; set; }

        public string SourceWarehouseName { get; set; }
        public string SourceDepartmentName { get; set; }
        public string SourceSectionName { get; set; }

        public string DestinationWarehouseName { get; set; }
        public string DestinationDepartmentName { get; set; }
        public string DestinationSectionName { get; set; }

        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public decimal Quantity { get; set; }

        public string? ConversionType { get; set; }  
    }

}
