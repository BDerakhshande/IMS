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

        // 🔹 تعداد اولیه درخواست (همیشه ثابت می‌مونه)
        [Required(ErrorMessage = "تعداد کالا الزامی است.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مقدار باید بزرگ‌تر از صفر باشد.")]
        public decimal InitialQuantity { get; set; }

        // 🔹 تعداد باقی‌مانده برای تأمین (Update میشه در طول فرآیندها)
        public decimal RemainingQuantity { get; set; }

        [MaxLength(50, ErrorMessage = "حداکثر طول واحد ۵۰ کاراکتر است.")]
        public int? UnitId { get; set; }

        // پروژه / مرکز هزینه
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public bool IsSupplyStopped { get; set; } = false;
        public bool IsFullySupplied { get; set; } = false;
        public bool IsAddedToFlatItems { get; set; }
        // ستون جدید برای تحویل کامل
        public bool IsFullyDelivered { get; set; } = false;

        // متدی برای بروزرسانی وضعیت‌ها
        public void UpdateSupplyStatus()
        {
            // اگر RemainingQuantity صفر شد و تامین متوقف نشده است
            IsFullyDelivered = RemainingQuantity <= 0 && !IsSupplyStopped;

            // همزمان می‌توان IsFullySupplied را بروزرسانی کرد
            IsFullySupplied = RemainingQuantity <= 0;
        }
    }
}
