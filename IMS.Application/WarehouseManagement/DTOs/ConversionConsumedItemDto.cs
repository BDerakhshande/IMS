using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ConversionConsumedItemDto
    {
        
        public int? Id { get; set; } 

        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public int StatusId { get; set; }


        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int WarehouseId { get; set; }
        public int ZoneId { get; set; }
        public int SectionId { get; set; }

        // Optional: اگر می‌خواهی نام کالا و موقعیت برای نمایش بیاوری
        public string? ProductName { get; set; }
        public string? ZoneName { get; set; }
        public string? SectionName { get; set; }
    }
}
