using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ConversionConsumedItemUniqueCode
    {
        public int Id { get; set; }

        public int ConversionConsumedItemId { get; set; }
        public ConversionConsumedItem ConversionConsumedItem { get; set; } = null!;


        public int InventoryItemId { get; set; } 
        public InventoryItem InventoryItem { get; set; } = null!;
    }
}

      