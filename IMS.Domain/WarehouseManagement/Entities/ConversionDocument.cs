using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ConversionDocument
    {

        public int Id { get; set; } // 👈 کلید اصلی

        // در صورتی که انبار در سطح سند نیاز نیست، این دو خط را حذف کن:
        // public int WarehouseId { get; set; }
        // public Warehouse Warehouse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ConversionConsumedItem> ConsumedItems { get; set; } = new();
        public List<ConversionProducedItem> ProducedItems { get; set; } = new();
    }

}
