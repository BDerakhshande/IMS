using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    public class TransactionReportViewModel
    {
        [Display(Name = "از تاریخ")]
        public string? FromDate { get; set; }

        [Display(Name = "تا تاریخ")]
        public string? ToDate { get; set; }



        [Display(Name = "از مبلغ")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "تا مبلغ")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "کد کل")]
        public int? MainAccountId { get; set; }

        [Display(Name = "کد معین")]
        public int? SubAccountId { get; set; }

        [Display(Name = "کد تفصیل ۱")]
        public int? DetailAccount1Id { get; set; }

        [Display(Name = "کد تفصیل ۲")]
        public int? DetailAccount2Id { get; set; }

        [Display(Name = "مرکز هزینه")]
        public int? CostCenterId { get; set; }

        // لیست تراکنش‌ها برای نمایش در گزارش
        public List<TransactionReportItem> Transactions { get; set; } = new List<TransactionReportItem>();

        // جمع بدهکار و بستانکار
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }

        // مانده حساب
        public decimal Balance => TotalDebit - TotalCredit;
    }

    public class TransactionReportItem
    {
        public DateTime? Date { get; set; }
        public string PersianDate { get; set; } // تاریخ شمسی
        public string MainAccount { get; set; } // کد و نام حساب کل (مثلاً 1001 - حساب‌های دریافتنی)
        public string SubAccount { get; set; } // کد و نام حساب معین
        public string DetailAccount1 { get; set; } // کد و نام تفصیل ۱
        public string DetailAccount2 { get; set; } // کد و نام تفصیل ۲
        public string CostCenterName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
