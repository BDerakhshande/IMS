using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class InventoryUpdateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public int? ZoneId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "مقدار باید عدد مثبت باشد.")]
        public int NewQuantity { get; set; }
    }
}
