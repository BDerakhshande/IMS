using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ConversionDocumentDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<ProductInfoDto> ConsumedProducts { get; set; } = new();
        public List<ProductInfoDto> ProducedProducts { get; set; } = new();

       
    }
}
