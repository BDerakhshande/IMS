using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class StatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;  // کد وضعیت (مثلاً شماره یا رشته)

        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string GroupCode { get; set; } = null!;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryCode { get; set; } = null!;

       
        public string FullCode => $"C{(CategoryCode ?? "").PadLeft(2, '0')}-G{(GroupCode ?? "").PadLeft(2, '0')}-S{(Code ?? "").PadLeft(2, '0')}";
        public List<ProductDto>? Products { get; set; }
        public int ProductsCount => Products?.Count ?? 0;
    }
}
