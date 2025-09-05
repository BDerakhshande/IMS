using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.ProcurementManagement.Entities
{
    public class PurchaseRequestFlatItem
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = null!;
        public string? RequestTitle { get; set; } // عنوان درخواست
        public DateTime RequestDate { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; } 
        public int ProductId { get; set; }
        public string? ProductName { get; set; }

        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public int UnitId { get; set; }

        public decimal TotalStock { get; set; }
        public decimal PendingRequests { get; set; }
        public decimal NeedToSupply { get; set; }

        public bool IsSupplyStopped { get; set; } = false;


        public int RequestTypeId { get; set; } // نوع درخواست
        public string? RequestTypeName { get; set; } // نام نوع درخواست
    }


}
