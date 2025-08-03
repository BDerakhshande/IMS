using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ProductInfoDto
    {
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
    }

}
