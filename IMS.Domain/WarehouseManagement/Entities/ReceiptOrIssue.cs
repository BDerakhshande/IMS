using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ReceiptOrIssue
    {

        public int Id { get; set; }

        [Required]
        public string DocumentNumber { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
       

        [Required]
        public ReceiptOrIssueType? Type { get; set; }
        public string? Description { get; set; }
       
        public ICollection<ReceiptOrIssueItem> Items { get; set; } = new List<ReceiptOrIssueItem>();

    }
}
