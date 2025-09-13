using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class Warehouse
    {
        public int Id { get; set; }

        public string? Manager { get; set; }
        public string? StorageConditions { get; set; }
        public string Name { get; set; }         // نام انبار (مثال: "انبار مرکزی تهران")
        public string Code { get; set; }         // کد انبار (مثال: "WH-001")
        public string? Location { get; set; }     // موقعیت فیزیکی
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<StorageZone> Zones { get; set; } = new List<StorageZone>();
     
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();


    }
}
