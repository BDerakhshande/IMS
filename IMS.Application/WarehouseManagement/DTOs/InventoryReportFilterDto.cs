using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryReportFilterDto
    {
        public List<WarehouseFilter>? Warehouses { get; set; } = new List<WarehouseFilter>();

        public List<int>? ZoneIds { get; set; }
        public List<int>? SectionIds { get; set; }

        public int? CategoryId { get; set; }
        public int? GroupId { get; set; }
        public int? StatusId { get; set; }
        public int? ProductId { get; set; }

        public decimal? MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }

        public string? ProductSearch { get; set; } // ← این خط را اضافه کن

        public List<InventoryReportResultDto>? Items { get; set; }
    }
    public class WarehouseFilter
    {
        public int WarehouseId { get; set; }
        public List<int> ZoneIds { get; set; } = new List<int>();
        public List<int> SectionIds { get; set; } = new List<int>();
    }
}