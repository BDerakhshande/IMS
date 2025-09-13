using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class StorageSectionDto
    {
        
        public int Id { get; set; }
        [Required(ErrorMessage = "نام بخش الزامی است")]
        public string Name { get; set; }
        [Required(ErrorMessage = "شناسه بخش را وارد کنید")]
        public string SectionCode { get; set; }
        public int ZoneId { get; set; }
        public decimal Capacity { get; set; }
        public string? Dimensions { get; set; }
        public string? ZoneCode { get; set; }
        public string? ZoneName { get; set; }
        public string? WarehouseCode { get; set; }
        public string? WarehouseName { get; set; }

        public string FullCode =>
            $"{(WarehouseCode ?? "")}"+
            $"{(ZoneCode ?? "").PadLeft(2, '0')}" +
            $"{(SectionCode ?? "").PadLeft(3, '0')}";
    }
}
