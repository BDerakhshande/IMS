using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryCreateDto
    {
        // محل نگهداری
        public int WarehouseId { get; set; }
        public int? ZoneId { get; set; }
        public int? SectionId { get; set; }

        // کالا (بر اساس انتخاب سلسله‌مراتبی)
        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public int StatusId { get; set; }
        public int ProductId { get; set; }

        // مقدار موجودی
        public decimal Quantity { get; set; }

        // نمایش فقط در ویو (برای DropDownها)
        public IEnumerable<SelectListItem>? Warehouses { get; set; }
        public IEnumerable<SelectListItem>? Zones { get; set; }
        public IEnumerable<SelectListItem>? Sections { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IEnumerable<SelectListItem>? Groups { get; set; }
        public IEnumerable<SelectListItem>? Statuses { get; set; }
        public IEnumerable<SelectListItem>? Products { get; set; }
    }
}
