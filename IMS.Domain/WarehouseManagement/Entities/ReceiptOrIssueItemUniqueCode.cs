using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ReceiptOrIssueItemUniqueCode
    {
        public int Id { get; set; }

        public int ReceiptOrIssueItemId { get; set; }
        public ReceiptOrIssueItem ReceiptOrIssueItem { get; set; } = null!;

        [Required]
        public string UniqueCode { get; set; } = null!;
    }
}
