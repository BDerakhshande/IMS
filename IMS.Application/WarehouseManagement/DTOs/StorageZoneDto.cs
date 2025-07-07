using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class StorageZoneDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ZoneCode { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseCode { get; set; }

        public List<StorageSectionDto>? Sections { get; set; }

        // فقط تا سطح Zone کد بده
        public string FullCode =>
            $"{(WarehouseCode ?? "").PadLeft(2, '0')}-{(ZoneCode ?? "").PadLeft(2, '0')}";
    }
}
