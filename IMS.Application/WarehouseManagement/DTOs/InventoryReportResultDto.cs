using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryReportResultDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public int? ZoneId { get; set; }
        public string ZoneName { get; set; }

        public int? SectionId { get; set; }
        public string SectionName { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public int GroupId { get; set; }
        public string GroupName { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public decimal Quantity { get; set; }
    }
}
