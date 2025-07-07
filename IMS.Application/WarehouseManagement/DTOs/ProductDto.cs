using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? Description { get; set; }

        public int StatusId { get; set; }
        public string StatusCode { get; set; }

        public int GroupId { get; set; }
        public string GroupCode { get; set; }

        public int CategoryId { get; set; }
        public string CategoryCode { get; set; }

        public decimal Price { get; set; }


        public string ProductsFullCode => $"C{(CategoryCode ?? "").PadLeft(2, '0')}-G{(GroupCode ?? "").PadLeft(2, '0')}-S{(StatusCode ?? "").PadLeft(2, '0')}-P{(Code ?? "").PadLeft(2, '0')}";




        public string StatusName { get; set; } = "";
        public string GroupName { get; set; } = "";
        public string CategoryName { get; set; } = "";

    }

}
