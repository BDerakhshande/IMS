using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProcurementManagement.Enums;

namespace IMS.Application.ProcurementManagement.DTOs
{
    public class PurchaseRequestDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان درخواست خرید الزامی است.")]
        public string RequestNumber { get; set; } = null!;

        [Required(ErrorMessage = "تاریخ درخواست الزامی است.")]
        public DateTime RequestDate { get; set; }

        [Required(ErrorMessage = "نوع درخواست الزامی است.")]
        public int RequestTypeId { get; set; }

        public string? RequestTypeName { get; set; } // نمایش نام تامین‌کننده

        [Required(ErrorMessage = "عنوان درخواست خرید الزامی است.")]
        public string? Title { get; set; }

        public string? Notes { get; set; }

        public Status Status { get; set; }

        public List<PurchaseRequestItemDto> Items { get; set; } = new List<PurchaseRequestItemDto>();
    }
}
