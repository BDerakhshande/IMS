using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Enums;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ReceiptOrIssueDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? DateString { get; set; }
        public ReceiptOrIssueType? Type { get; set; }
        public string? Description { get; set; }
        public List<ReceiptOrIssueItemDto> Items { get; set; } = new();
        public string DocumentNumber { get; set; }

        // اطلاعات پروژه
        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
    }
}
