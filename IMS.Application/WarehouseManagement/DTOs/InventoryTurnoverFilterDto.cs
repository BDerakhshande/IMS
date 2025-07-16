using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryTurnoverFilterDto
    {
        public int WarehouseId { get; set; }

        public int ZoneId { get; set; }       // اجباری
        public int SectionId { get; set; }    // اجباری

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
