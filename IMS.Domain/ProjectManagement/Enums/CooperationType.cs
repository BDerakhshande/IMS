using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IMS.Domain.ProjectManagement.Enums
{
    //نوع قرارداد
    public enum CooperationType
    {
        [Display(Name = "قرارداد رسمی")]
        Contract,        // قرارداد رسمی

        [Display(Name = "مشارکت")]
        Partnership,     // مشارکت

        [Display(Name = "پیمانکاری فرعی")]
        Subcontract,     // پیمانکاری فرعی

        [Display(Name = "قرارداد خدمات")]
        ServiceAgreement,// قرارداد خدمات

        [Display(Name = "سفارش خرید")]
        PurchaseOrder    // سفارش خرید
    }
}
