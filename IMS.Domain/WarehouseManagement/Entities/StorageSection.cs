using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class StorageSection
    {
        public int Id { get; set; }

        public string Name { get; set; }         // نام بخش (مثال: "بخش فولاد")
        public string SectionCode { get; set; }   // کد بخش (مثال: "SEC-STL")
        public int ZoneId { get; set; }          // FK به قسمت
        public StorageZone Zone { get; set; }    // Navigation Property
        public decimal Capacity { get; set; }    // ظرفیت بخش
        public string Dimensions { get; set; }   // ابعاد بخش

        public ICollection<Inventory> Inventories { get; set; }
    }
}
