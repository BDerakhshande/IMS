using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ConversionProducedItemUniqueCode
    {
        public int Id { get; set; }
        public int ConversionProducedItemId { get; set; }
        public ConversionProducedItem ConversionProducedItem { get; set; } = null!;
        [Required]
       
        public string UniqueCode { get; set; } = null!;
       
    }
}
