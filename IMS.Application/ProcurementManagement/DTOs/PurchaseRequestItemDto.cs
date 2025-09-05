using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Entities;


namespace IMS.Application.ProcurementManagement.DTOs
{
    public class PurchaseRequestItemDto
    {
        public int Id { get; set; }

        public int PurchaseRequestId { get; set; }

        [Required(ErrorMessage = "دسته‌بندی کالا الزامی است.")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "گروه کالا الزامی است.")]
        public int GroupId { get; set; }
        public string? GroupName { get; set; }

        [Required(ErrorMessage = "وضعیت کالا الزامی است.")]
        public int StatusId { get; set; }
        public string? Status { get; set; }

        [Required(ErrorMessage = "کالا الزامی است.")]
        public int ProductId { get; set; }
        public string? ProductName { get; set; }

        public string? Description { get; set; }

        // 🔹 تعداد اولیه درخواست
        [Required(ErrorMessage = "تعداد کالا الزامی است.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مقدار باید بزرگ‌تر از صفر باشد.")]
        public decimal InitialQuantity { get; set; }

        public decimal Quantity => RemainingQuantity;
        // 🔹 تعداد باقی‌مانده برای تأمین
        public decimal RemainingQuantity { get; set; }

        public string? Unit { get; set; }

        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }

        // 🔹 اطلاعات تکمیلی برای گزارش‌گیری
        public decimal TotalStock { get; set; }
        public decimal PendingRequests { get; set; }
        public decimal NeedToSupply { get; set; }

        public string RequestNumber { get; set; } = "";
        public bool IsSupplyStopped { get; set; } = false;
        public bool IsFullySupplied { get; set; } = false;
        public bool IsAddedToFlatItems { get; set; }
        // اضافه کردن ستون جدید برای تحویل کامل
        public bool IsFullyDelivered { get; set; } = false;

    }
}
