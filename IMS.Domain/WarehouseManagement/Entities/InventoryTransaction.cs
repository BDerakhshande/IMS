using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class InventoryTransaction
    {
        public int Id { get; set; }

        // اطلاعات مکانی
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } // navigation

        public int? ZoneId { get; set; }
        public StorageZone Zone { get; set; } // navigation

        public int? SectionId { get; set; }
        public StorageSection Section { get; set; } // navigation

        // اطلاعات کالا
        public int CategoryId { get; set; }   // دسته
        public Category Category { get; set; } // navigation

        public int GroupId { get; set; }      // گروه
        public Group Group { get; set; } // navigation

        public int StatusId { get; set; }     // وضعیت (مثلاً نو، معیوب، برگشتی)
        public Status Status { get; set; } // navigation

        public int ProductId { get; set; }    // کالا
        public Product Product { get; set; } // navigation

        // تغییرات موجودی
        public decimal QuantityChange { get; set; }  // مقدار تغییر (مثبت یا منفی)
        public decimal FinalQuantity { get; set; }   // موجودی نهایی پس از عملیات

        public DateTime Date { get; set; }
    }

}
