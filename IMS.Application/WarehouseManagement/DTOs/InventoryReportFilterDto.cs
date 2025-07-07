using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryReportFilterDto
    {
        public int? CategoryId { get; set; }
        public int? GroupId { get; set; }
        public int? StatusId { get; set; }
        public int? ProductId { get; set; }

        public int? WarehouseId { get; set; }
        public int? ZoneId { get; set; }
        public int? SectionId { get; set; }

        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }


        public List<InventoryReportItemDto> Items { get; set; } = new List<InventoryReportItemDto>();

    }
}
