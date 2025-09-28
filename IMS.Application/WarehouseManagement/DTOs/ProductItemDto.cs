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
        // وضعیت محصول (اجباری)
        [Required(ErrorMessage = "وضعیت محصول الزامی است.")]
        public ProductItemStatus Status { get; set; } = ProductItemStatus.Ready;

        // نام‌ها (برای نمایش در UI)
        public string? CategoryName { get; set; }
        public string? GroupName { get; set; }
        public string? StatusName { get; set; }
        public string? ProductName { get; set; }

        // کدها (برای نمایش سلسله‌مراتبی بر اساس کدها)
        public string? CategoryCode { get; set; }
        public string? GroupCode { get; set; }
        public string? StatusCode { get; set; }
        public string? ProductCode { get; set; }

        // شماره ترتیبی یا کد اختیاری
        public int Sequence { get; set; } = 1;
        public string? ProjectName { get; set; }

        // نمایش سلسله‌مراتبی بر اساس کدها
        public string DisplayHierarchyByCode =>
            $"C{CategoryCode}G{GroupCode}S{StatusCode}P{ProductCode}_{Sequence}";
    }


}
