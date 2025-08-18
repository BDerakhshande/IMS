using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.ProcurementManagement.DTOs
{
    public class ProductFilterDto
    {
        public int? CategoryId { get; set; }
        public int? GroupId { get; set; }
        public int? StatusId { get; set; }
        public int? ProductId { get; set; }
        public string? Name { get; set; }
    }
}
