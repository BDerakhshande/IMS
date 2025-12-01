using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ConversionProducedItemDto
    {
        public int? Id { get; set; }
        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public int StatusId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int WarehouseId { get; set; }
        [Required(ErrorMessage = "قسمت (Zone) الزامی است و نمی‌تواند صفر یا خالی باشد.")]
        public int? ZoneId { get; set; }  // Nullable: empty → null
        [Required(ErrorMessage = "بخش (Section) الزامی است و نمی‌تواند صفر یا خالی باشد.")]
        public int? SectionId { get; set; }  // Nullable: empty → null
        public int? ProjectId { get; set; }
 
        public List<string>? UniqueCodes { get; set; } = null; // اگر null، یعنی کاربر هیچ کد یکتایی نخواسته
        public bool GenerateUniqueCodes { get; set; } = false; // اگر true، سیستم خودش کد یکتا بسازد

        // Optional برای UI
        public string? ProductName { get; set; }
        public string? ZoneName { get; set; }
        public string? SectionName { get; set; }
        public string? ProjectTitle { get; set; }
    }

}