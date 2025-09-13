using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class WarehouseDto
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "نام انبار الزامی است")]
        [StringLength(100, ErrorMessage = "حداکثر طول نام انبار 100 کاراکتر است")]
        public string Name { get; set; }
        [Required(ErrorMessage = "کد انبار الزامی است")]
        // کد انبار به صورت عددی یا متنی
        public string Code { get; set; }

        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? Manager { get; set; }
        public string? StorageConditions { get; set; }
        public bool IsActive { get; set; }

     
        public List<StorageZoneDto>? Zones { get; set; }

      
        public string CodePadded => Code ?? "";


        public string DisplayCode => $"{CodePadded}";


    }
}
