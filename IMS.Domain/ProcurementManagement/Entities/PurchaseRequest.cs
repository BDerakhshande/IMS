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
    // هدر درخواست خرید
    public class PurchaseRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "شماره درخواست الزامی است.")]
        [MaxLength(50, ErrorMessage = "حداکثر طول شماره درخواست ۵۰ کاراکتر است.")]
        public string RequestNumber { get; set; } = null!; // شماره درخواست

        [Required(ErrorMessage = "تاریخ درخواست الزامی است.")]
        public DateTime RequestDate { get; set; } // تاریخ درخواست

        [Required(ErrorMessage = "عنوان درخواست خرید الزامی است.")]
        [MaxLength(250, ErrorMessage = "حداکثر طول عنوان درخواست ۲۵۰ کاراکتر است.")]
        public string Title { get; set; } // عنوان درخواست

        public string? Notes { get; set; } // توضیحات کلی

        [Required(ErrorMessage = "نوع درخواست الزامی است.")]
        public int RequestTypeId { get; set; } // نوع درخواست
        public RequestType RequestType { get; set; }

        public Status Status { get; set; }

        public ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
    }
}
