using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ConversionConsumedItem
    {
        public int Id { get; set; }
        public int ConversionDocumentId { get; set; }
        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public int StatusId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int WarehouseId { get; set; }
        public int ZoneId { get; set; }
        public int SectionId { get; set; }
        public Product Product { get; set; }
    }
}
