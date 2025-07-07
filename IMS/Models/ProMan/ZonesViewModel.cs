using IMS.Application.WarehouseManagement.DTOs;

namespace IMS.Models.ProMan
{
    public class ZonesViewModel
    {
        public int WarehouseId { get; set; }
        public List<StorageZoneDto> Zones { get; set; }
    }
}
