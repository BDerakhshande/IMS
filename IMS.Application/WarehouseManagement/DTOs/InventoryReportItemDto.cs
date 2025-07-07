using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryReportItemDto
    {
        // کالا
        public string CategoryName { get; set; }
        public string GroupName { get; set; }
        public string StatusName { get; set; }
        public string ProductName { get; set; }

        // انبار
        public string WarehouseName { get; set; }
        public string? ZoneName { get; set; }
        public string? SectionName { get; set; }

        public decimal Quantity { get; set; }
    }
}
