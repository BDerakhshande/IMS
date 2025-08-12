using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProcurementManagement.Enums;

namespace IMS.Application.ProcurementManagement.DTOs
{
    public class PurchaseRequestDto
    {
        public int Id { get; set; }

        public string RequestNumber { get; set; } = null!;

        public DateTime RequestDate { get; set; }

     

        public int RequestTypeId { get; set; }

        public string? RequestTypeName { get; set; } // برای نمایش نام تامین‌کننده

        public string? Title { get; set; }

        public string? Notes { get; set; }

        public Status Status { get; set; }

        public List<PurchaseRequestItemDto> Items { get; set; } = new List<PurchaseRequestItemDto>();
    }
}
