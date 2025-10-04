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
        public int? SourceWarehouseId { get; set; }         
        public int? SourceZoneId { get; set; }             
        public int? SourceSectionId { get; set; }           

        // مقصد
        public int? DestinationWarehouseId { get; set; }   
        public int? DestinationZoneId { get; set; }        
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
    
        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }


        public int? PurchaseRequestId { get; set; }    
        public string? PurchaseRequestTitle { get; set; } 


        public List<string> UniqueCodes { get; set; } = new List<string>();


   
        public string? SelectedUniqueCode { get; set; }
    }
}
