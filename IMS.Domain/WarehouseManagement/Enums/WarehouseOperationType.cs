using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Enums
{
    public enum WarehouseOperationType
    {
        ReceiptOrIssue = 1,           // رسید و حواله
        TransferBetweenWarehouses = 2, // انتقال بین انبارها
        InventoryAdjustment = 3,      // تنظیمات موجودی (دارای زیرشاخه)
        Conversion = 4                // تبدیل کالا
    }
}
