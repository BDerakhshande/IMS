using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class ProductItemDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public int StatusId { get; set; }

        // پروژه اختیاری
        public int? ProjectId { get; set; }

        // وضعیت آیتم (اجباری)
        [Required(ErrorMessage = "وضعیت آیتم الزامی است.")]
        public ProductItemStatus ItemStatus { get; set; } = ProductItemStatus.Ready;

        // کد یکتا
        public string? UniqueCode { get; set; }

        // نام‌ها (برای UI)
        public string? CategoryName { get; set; }
        public string? GroupName { get; set; }
        public string? StatusName { get; set; }
        public string? ProductName { get; set; }
        public string? ProjectName { get; set; }

        // کدها
        public string? CategoryCode { get; set; }
        public string? GroupCode { get; set; }
        public string? StatusCode { get; set; }
        public string? ProductCode { get; set; }

        // شماره ترتیبی (برای مرتب‌سازی یا ساخت UniqueCode)
        public int Sequence { get; set; } = 1;

        // نمایش سلسله‌مراتبی
        public string DisplayHierarchyByCode =>
            UniqueCode ?? $"C{CategoryCode}G{GroupCode}S{StatusCode}P{ProductCode}_{Sequence}";
    }


}
