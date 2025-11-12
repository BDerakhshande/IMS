using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class InventoryReceiptLog
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int ZoneId { get; set; }
        public StorageZone StorageZone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string DocumentType { get; set; }

        public int SectionId { get; set; }
        public StorageSection StorageSection { get; set; }
        public decimal Quantity { get; set; }
        public bool IsUnique { get; set; }
        public string UniqueCode { get; set; } 
   
    }

}
