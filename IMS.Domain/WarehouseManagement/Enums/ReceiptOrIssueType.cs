using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Enums
{
    public enum ReceiptOrIssueType
    {
        [Display(Name = "رسید")]
        Receipt = 1,

        [Display(Name = "حواله")]
        Issue = 2,

        [Display(Name = "انتقال")]
        Transfer = 3
    }
}
