using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Domain.ProcurementManagement.Entities
{
    public class GoodsRequestItem
    {
        public int Id { get; set; }

        public int GoodsRequestId { get; set; }
        public GoodsRequest GoodsRequest { get; set; }

        public int CategoryId { get; set; }
      

        public int GroupId { get; set; }
      

        public int StatusId { get; set; }
       
        public int ProductId { get; set; }
       
        public int WarehouseId { get; set; }
      
        public int ZoneId { get; set; }
        
        public int SectionId { get; set; }
      

        public decimal Quantity { get; set; }
    }

}
