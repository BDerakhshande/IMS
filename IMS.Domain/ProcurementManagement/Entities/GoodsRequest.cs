using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProcurementManagement.Enums;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Domain.ProcurementManagement.Entities
{
    public class GoodsRequest
    {
        public int Id { get; set; }

        public DateTime RequestDate { get; set; }
        public string RequestedByName { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public RequestStatus Status { get; set; }
       

        public ICollection<GoodsRequestItem> Items { get; set; } = new List<GoodsRequestItem>();
    }

}
