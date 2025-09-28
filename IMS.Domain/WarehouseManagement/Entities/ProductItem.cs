using IMS.Domain.ProjectManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class ProductItem
    {
        public int Id { get; set; }

        // کد یکتا
        public string UniqueCode { get; set; } = null!;

        // ارتباط با کالا
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // پروژه اختیاری
        public int? ProjectId { get; set; }  
        public Project? Project { get; set; }

        


        // شماره ترتیبی محصول
        public int Sequence { get; set; } = 1; 
        // وضعیت محصول
        public ProductItemStatus ProductItemStatus { get; set; } = ProductItemStatus.Ready;

    }

}
