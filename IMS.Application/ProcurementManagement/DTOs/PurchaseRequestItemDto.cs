using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Entities;


namespace IMS.Application.ProcurementManagement.DTOs
{
    public class PurchaseRequestItemDto
    {
        public int Id { get; set; }

        public int PurchaseRequestId { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } // برای نمایش در UI

        public int GroupId { get; set; }
        public string? GroupName { get; set; } // برای نمایش در UI


        public int statusId { get; set; }
        public Status Status { get; set; }

        public int ProductId { get; set; }
        public string? ProductName { get; set; } // برای نمایش در UI

        public string? Description { get; set; }

        public decimal Quantity { get; set; }

        public string? Unit { get; set; }

        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; } // برای نمایش

    }
}
