using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ReceiptOrIssueItemDto
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }

        public int? CategoryId { get; set; }
        public int? GroupId { get; set; }
        public int? StatusId { get; set; }
        public int ProductId { get; set; }
        // مبدأ
        public int? SourceWarehouseId { get; set; }         // انبار مبدأ
        public int? SourceZoneId { get; set; }              // قسمت مبدأ (اختیاری)
        public int? SourceSectionId { get; set; }           // بخش مبدأ (اختیاری)

        // مقصد
        public int? DestinationWarehouseId { get; set; }    // انبار مقصد
        public int? DestinationZoneId { get; set; }         // قسمت مقصد (اختیاری)
        public int? DestinationSectionId { get; set; }



        public string? CategoryName { get; set; }
        public string? GroupName { get; set; }
        public string? StatusName { get; set; }
        public string? ProductName { get; set; }



        public string? SourceWarehouseName { get; set; }
        public string? SourceZoneName { get; set; }
        public string? SourceSectionName { get; set; }
        public string? DestinationWarehouseName { get; set; }
        public string? DestinationZoneName { get; set; }
        public string? DestinationSectionName { get; set; }
        // اطلاعات پروژه
        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }


        public int? PurchaseRequestId { get; set; }      // ارتباط به درخواست خرید
        public string? PurchaseRequestTitle { get; set; } // عنوان درخواست خرید


        public List<string> UniqueCodes { get; set; } = new List<string>();
    }
}
