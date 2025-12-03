using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required(ErrorMessage = "قسمت (Zone) الزامی است و نمی‌تواند صفر یا خالی باشد.")]
        public int? ZoneId { get; set; } // تغییر به nullable
        [Required(ErrorMessage = "بخش (Section) الزامی است و نمی‌تواند صفر یا خالی باشد.")]
        public int? SectionId { get; set; } // تغییر به nullable
        public int? ProjectId { get; set; }

        public List<int> InventoryItemIds { get; set; } = new List<int>();
        // Optional برای نمایش
        public string? ProductName { get; set; }
        public string? ZoneName { get; set; }
        public string? SectionName { get; set; }
        public string? ProjectTitle { get; set; }
    }
}
