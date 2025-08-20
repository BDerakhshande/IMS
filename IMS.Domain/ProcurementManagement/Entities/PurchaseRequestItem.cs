using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Entities;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Domain.ProcurementManagement.Entities
{
   
    public class PurchaseRequestItem
    {
        public int Id { get; set; }

        public int PurchaseRequestId { get; set; }
        public PurchaseRequest PurchaseRequest { get; set; } = null!;

        // ارتباط با سلسله مراتب کالا
        [Required(ErrorMessage = "دسته‌بندی کالا الزامی است.")]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required(ErrorMessage = "گروه کالا الزامی است.")]
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        [Required(ErrorMessage = "وضعیت کالا الزامی است.")]
        public int StatusId { get; set; }
        public Status Status { get; set; } = null!;

        [Required(ErrorMessage = "کالا الزامی است.")]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // توضیحات اختصاصی این سطر
        public string? Description { get; set; }

        [Required(ErrorMessage = "تعداد کالا الزامی است.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مقدار باید بزرگ‌تر از صفر باشد.")]
        public decimal Quantity { get; set; }

        [MaxLength(50, ErrorMessage = "حداکثر طول واحد ۵۰ کاراکتر است.")]
        public string? Unit { get; set; } // مثال: عدد، متر، جعبه

        // پروژه / مرکز هزینه
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public bool IsSupplyStopped { get; set; } = false;
    }
}
