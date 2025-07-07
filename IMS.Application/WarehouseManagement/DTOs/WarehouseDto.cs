using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class WarehouseDto
    {

        public int Id { get; set; }
        public string Name { get; set; }

        // کد انبار به صورت عددی یا متنی
        public string Code { get; set; }

        public string Location { get; set; }
        public string Description { get; set; }
        public string? Manager { get; set; }
        public string? StorageConditions { get; set; }
        public bool IsActive { get; set; }

        // برای نمایش مناطق داخل انبار
        public List<StorageZoneDto>? Zones { get; set; }

        // ✅ کد با فرمت استاندارد دو رقمی مثل: 01، 02، ...
        public string CodePadded => (Code ?? "").PadLeft(2, '0');

        // ✅ کد کامل برای نمایش، در صورت نیاز به فرمت استاندارد
        public string DisplayCode => $"{CodePadded}";


    }
}
