using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Enums
{
    public enum ProductItemStatus
    {
        [Display(Name = "ریخته‌گری")]
        Casting,

        [Display(Name = "تراش‌خورده")]
        Machined,

        [Display(Name = "آماده")]
        Ready
    }
}
