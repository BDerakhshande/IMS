using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ReceiptOrIssueItem
    {
        public int Id { get; set; }

        public int ReceiptOrIssueId { get; set; }
        public ReceiptOrIssue ReceiptOrIssue { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        // مبدأ
        public int? SourceWarehouseId { get; set; }
        // برای ارتباط با انبار مبدأ اگر نیاز باشد:
        public Warehouse? SourceWarehouse { get; set; }

        public int? SourceZoneId { get; set; }
        // ارتباط با زون مبدأ:
        public StorageZone? SourceZone { get; set; }

        public int? SourceSectionId { get; set; }
        public StorageSection? SourceSection { get; set; }

        // مقصد
        public int? DestinationWarehouseId { get; set; }
        public Warehouse? DestinationWarehouse { get; set; }

        public int? DestinationZoneId { get; set; }
        public StorageZone? DestinationZone { get; set; }

        public int? DestinationSectionId { get; set; }
        public StorageSection? DestinationSection { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        public int? StatusId { get; set; }
        public Status? Status { get; set; }
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
