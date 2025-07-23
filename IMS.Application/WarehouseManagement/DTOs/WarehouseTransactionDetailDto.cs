using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class WarehouseTransactionDetailDto
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; }
        public string TransactionType { get; set; }
        public string? ProjectName { get; set; }
        public DateTime Date { get; set; }

        public string? SourceWarehouse { get; set; }
        public string? SourceZone { get; set; }
        public string? SourceSection { get; set; }

        public string? DestinationWarehouse { get; set; }
        public string? DestinationZone { get; set; }
        public string? DestinationSection { get; set; }

        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string GroupName { get; set; }
        public string StatusName { get; set; }
        public decimal Quantity { get; set; }
    }

}
