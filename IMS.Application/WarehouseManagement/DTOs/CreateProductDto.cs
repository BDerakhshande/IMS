using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "نام کالا الزامی است")]
        public string Name { get; set; } = null!;

        public string? Code { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "وضعیت کالا باید انتخاب شود")]
        public int StatusId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "قیمت باید عدد مثبت باشد")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "تعداد باید عدد مثبت باشد")]
        public int Quantity { get; set; }
    }
}
