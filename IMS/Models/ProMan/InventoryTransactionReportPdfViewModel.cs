using IMS.Application.WarehouseManagement.DTOs;

namespace IMS.Models.ProMan
{
    public class InventoryTransactionReportPdfViewModel
    {
        public IEnumerable<InventoryTransactionReportDto> Items { get; set; } = new List<InventoryTransactionReportDto>();
        public InventoryTransactionReportItemDto Filter { get; set; } = new InventoryTransactionReportItemDto();
        public string WarehouseName { get; set; }
        public string ZoneName { get; set; }
        public string SectionName { get; set; }

        public string CategoryName { get; set; }
        public string GroupName { get; set; }
        public string StatusName { get; set; }
        public string ProductName { get; set; }

    }
}
