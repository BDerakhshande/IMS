using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class StorageZoneDto
    {
        [Required(ErrorMessage ="شناسه را وارد کنید")]
        public int Id { get; set; }
        [Required(ErrorMessage = "نام قسمت الزامی است")]
        public string Name { get; set; }
        public string ZoneCode { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseCode { get; set; }
       
        public string WarehouseName { get; set; }
        public List<StorageSectionDto>? Sections { get; set; }

        // فقط تا سطح Zone کد بده
        public string FullCode =>
            $"{(WarehouseCode ?? "")}{(ZoneCode ?? "").PadLeft(2, '0')}";
    }
}
