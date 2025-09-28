using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class StatusDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="نام طیقه را وارد کنید")]
        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;  

        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string GroupCode { get; set; } = null!;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryCode { get; set; } = null!;

       
        public string FullCode => $"C{(CategoryCode ?? "")}G{(GroupCode ?? "")}S{(Code ?? "")}";
        public List<ProductDto>? Products { get; set; }
        public int ProductsCount => Products?.Count ?? 0;
    }
}
