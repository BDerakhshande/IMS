using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class StorageSectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SectionCode { get; set; }
        public int ZoneId { get; set; }
        public decimal Capacity { get; set; }
        public string Dimensions { get; set; }

        public string? ZoneCode { get; set; }
        public string? WarehouseCode { get; set; }

        public string FullCode =>
            $"{(WarehouseCode ?? "").PadLeft(2, '0')}-" +
            $"{(ZoneCode ?? "").PadLeft(2, '0')}-" +
            $"{(SectionCode ?? "").PadLeft(2, '0')}";
    }
}
