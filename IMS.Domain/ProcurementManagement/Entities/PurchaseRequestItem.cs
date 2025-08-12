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
    // آیتم‌های درخواست خرید
    public class PurchaseRequestItem
    {
        public int Id { get; set; }

        public int PurchaseRequestId { get; set; }
        public PurchaseRequest PurchaseRequest { get; set; } = null!;

        // ارتباط با سلسله مراتب کالا
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public int StatusId { get; set; }
        public Status Status { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // توضیحات اختصاصی این سطر
        public string? Description { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; } // مثال: عدد، متر، جعبه

        // پروژه / مرکز هزینه
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }


        public bool IsSupplyStopped { get; set; } = false;


    }
}
