using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class UnitDto
    {
  
        public int? Id { get; set; }          // شناسه واحد
        public string Name { get; set; } = "";   // نام واحد، مثل "عدد"
        public string Symbol { get; set; } = ""; // نماد واحد، مثل "pcs"
    }
}
