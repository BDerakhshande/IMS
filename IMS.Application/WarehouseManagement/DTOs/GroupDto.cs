using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class GroupDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="نام گروه را وارد کنید")]
        public string Name { get; set; } = null!;
        public int CategoryId { get; set; }
        
        public string? CategoryCode { get; set; } = null!;

        public string CategoryName { get; set; } = "";
        [Required(ErrorMessage = "کد گروه را وارد کنید")]
        public string Code { get; set; } = null!; 

        public List<StatusDto>? Statuses { get; set; }

        public string GroupCode => $"{CategoryCode}G{(Code ?? "")}";

    }
}
