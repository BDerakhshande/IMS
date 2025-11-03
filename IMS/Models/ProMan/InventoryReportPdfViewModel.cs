using IMS.Application.WarehouseManagement.DTOs;

namespace IMS.Models.ProMan
{
    public class InventoryReportPdfViewModel
    {
        public List<InventoryReportResultDto> Items { get; set; }
        public InventoryReportFilterDto Filter { get; set; }

        // دیکشنری‌ها برای نام‌ها
        public Dictionary<int, string> WarehouseNames { get; set; }
        public Dictionary<int, string> CategoryNames { get; set; }
        public Dictionary<int, string> GroupNames { get; set; }
        public Dictionary<int, string> StatusNames { get; set; }
        public Dictionary<int, string> ProductNames { get; set; }
        public Dictionary<int, string> SectionNames { get; set; }
        public Dictionary<int, string> ZoneNames { get; set; }
        public List<string> UniqueCodesFilter { get; set; } = new List<string>();
    
    }
}

