using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryTransactionReportItemDto
    {
        public int? WarehouseId { get; set; }          // ✅ اضافه شده
        public int? ZoneId { get; set; }  

        public int? SectionId { get; set; }

        public int? CategoryId { get; set; }
        public int? GroupId { get; set; }
        public int? StatusId { get; set; }
        public int? ProductId { get; set; }

        public string? FromDateString { get; set; }  // تاریخ شمسی ورودی کاربر
        public string? ToDateString { get; set; }

        public DateTime? FromDate { get; set; }     // تاریخ میلادی تبدیل شده
        public DateTime? ToDate { get; set; }
        public string? DocumentType { get; set; }
        public string? WarehouseName { get; set; }
        public string? ZoneName { get; set; }
        public string? SectionName { get; set; }

        public string? CategoryName { get; set; }
        public string? GroupName { get; set; }
        public string? StatusName { get; set; }
        public string? ProductName { get; set; }

    }
}
