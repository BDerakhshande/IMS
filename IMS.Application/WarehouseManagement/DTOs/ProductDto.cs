using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام کالا را وارد کنید")]
        public string Name { get; set; } = null!;

        public string? Code { get; set; }
        public string? Description { get; set; }

        public bool IsUnique { get; set; } = false;  // مشخص می‌کند کالا یکتا است یا خیر

        [Range(1, int.MaxValue, ErrorMessage = "تعداد باید بیشتر از ۰ باشد")]
        public int Quantity { get; set; } = 1; // تعداد برای کالاهای یکتا

        public int StatusId { get; set; }
        public string? StatusCode { get; set; }

        public int GroupId { get; set; }
        public string? GroupCode { get; set; }

        public string? CategoryCode { get; set; }
        public int CategoryId { get; set; }

        public decimal Price { get; set; }

        public string ProductsFullCode => $"C{(CategoryCode ?? "")}G{(GroupCode ?? "")}S{(StatusCode ?? "")}P{(Code ?? "")}";

        public UnitDto Unit { get; set; } = new UnitDto();

        [Required(ErrorMessage = "واحد کالا را وارد کنید")]
        public int UnitId { get; set; }

        public string StatusName { get; set; } = "";
        public string GroupName { get; set; } = "";
        public string CategoryName { get; set; } = "";
    }


}
