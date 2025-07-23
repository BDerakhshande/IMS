using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; } = null!;
        public string Code { get; set; } = null!; // ← از Entity خوانده شود

        public List<StatusDto>? Statuses { get; set; }

        public string GroupCode => $"{CategoryCode}G{(Code ?? "")}";

    }
}
