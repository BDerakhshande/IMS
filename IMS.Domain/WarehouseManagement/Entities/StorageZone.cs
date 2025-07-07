using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class StorageZone
    {
        public int Id { get; set; }

        public string Name { get; set; }         // نام قسمت (مثال: "قسمت فلزات")
        public string ZoneCode { get; set; }     // کد قسمت (مثال: "Z-METAL")
        public int WarehouseId { get; set; }     // FK به انبار
        public Warehouse Warehouse { get; set; } // Navigation Property
        public ICollection<StorageSection> Sections { get; set; } = new List<StorageSection>();
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    }
}
