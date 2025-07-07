using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }  // مثلا نام انبار

        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }      // نام قسمت ذخیره‌سازی (اختیاری)

        public int? SectionId { get; set; }
        public string? SectionName { get; set; }   // نام بخش (اختیاری)

        public int ProductId { get; set; }
        public string ProductName { get; set; }    // نام کالا

        public decimal Quantity { get; set; }
    }

}
