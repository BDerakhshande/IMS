using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Entities;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ConversionProducedItem
    {
        public int Id { get; set; }
        public int ConversionDocumentId { get; set; }
        public ConversionDocument ConversionDocument { get; set; }
        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public int StatusId { get; set; }
        public int ProductId { get; set; }
     
        public decimal Quantity { get; set; }
        public int WarehouseId { get; set; }
        public int ZoneId { get; set; }
        public int SectionId { get; set; }
        public Product Product { get; set; }
        public Warehouse Warehouse { get; set; }
        public StorageZone Zone { get; set; }
        public StorageSection Section { get; set; }

        public Category Category { get; set; }
        public Group Group { get; set; }
        public Status Status { get; set; }
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

    }
}
