using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProcurementManagement.Enums;

namespace IMS.Domain.ProcurementManagement.Entities
{
    // هدر درخواست خرید
    public class PurchaseRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string RequestNumber { get; set; } = null!; // شماره درخواست

        public DateTime RequestDate { get; set; } // تاریخ درخواست

        public DateTime? DueDate { get; set; } // تاریخ سررسید

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [MaxLength(250)]
        public string? Title { get; set; } // عنوان درخواست

        public string? Notes { get; set; } // توضیحات کلی

        public Status Status { get; set; }

        public ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();

      
    }
}
