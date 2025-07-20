using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Entities;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ConversionDocument
    {

        public int Id { get; set; }

        public string DocumentNumber { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
        public List<ConversionConsumedItem> ConsumedItems { get; set; } = new();
        public List<ConversionProducedItem> ProducedItems { get; set; } = new();
    }

}
