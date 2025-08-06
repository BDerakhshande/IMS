using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.ProcurementManagement.DTOs
{
    public class GoodsRequestInventoryReportDto
    {
        // Warehouse and location specifications
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }

        // Current inventory quantity
        public decimal AvailableQuantity { get; set; }

        // Related goods request info (if applicable)
        public int? GoodsRequestId { get; set; }
        public decimal? RequestedQuantity { get; set; }
        public bool NeedsPurchase => (RequestedQuantity.HasValue && RequestedQuantity.Value > AvailableQuantity);
        public string StatusMessage
        {
            get
            {
                if (AvailableQuantity <= 0)
                    return "نیاز به خرید";
                if (RequestedQuantity.HasValue && RequestedQuantity.Value > AvailableQuantity)
                    return "موجودی ناکافی";
                return "موجودی کافی است";
            }
        }


    }
}
