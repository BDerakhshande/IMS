using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    public class TransactionDocumentDetailsViewModel
    {
        // Document Properties
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "تاریخ سند الزامی است")]
        [Display(Name = "تاریخ سند")]
        [DataType(DataType.Date)]
        public string PersianDocumentDate { get; set; }
        public DateTime DocumentDate { get; set; }


        [Required(ErrorMessage = "شرح سند الزامی است")]
        [Display(Name = "شرح سند")]
        public string Description { get; set; }

        [Display(Name = "شماره سند")]
        public string DocumentNumber { get; set; }

        [Display(Name = "نوع سند")]
        public string DocumentTypeName { get; set; }

        [Required(ErrorMessage = "نوع سند الزامی است")]
        [Display(Name = "نوع سند")]
        public int? SelectedDocumentTypeId { get; set; }
        public Status DocumentStatus { get; set; }
        public List<SelectListItem> DocumentTypes { get; set; } = new List<SelectListItem>();

        [Display(Name = "مجموع مبلغ")]
        public decimal TotalAmount { get; set; }

        // Accounts and Transactions
        public List<TransactionDetailsViewModel> Transactions { get; set; } = new List<TransactionDetailsViewModel>();

        [Display(Name = "حساب کل")]
        public List<SelectListItem> MainAccounts { get; set; } = new List<SelectListItem>();

        public Dictionary<int, List<SelectListItem>> SubAccounts { get; set; } = new Dictionary<int, List<SelectListItem>>();

        public Dictionary<int, List<SelectListItem>> DetailAccounts1 { get; set; } = new Dictionary<int, List<SelectListItem>>();

        public Dictionary<int, List<SelectListItem>> DetailAccounts2 { get; set; } = new Dictionary<int, List<SelectListItem>>();
    }

    public class TransactionDetailsViewModel
    {
        // Transaction Properties
        public int TransactionId { get; set; }

        [Display(Name = "کد کل")]
        public int? MainAccountId { get; set; }

        [Display(Name = "کد معین")]
        public int? SubAccountId { get; set; }

        [Display(Name = "کد تفصیل ۱")]
        public int? DetailAccount1Id { get; set; }

        [Display(Name = "کد تفصیل ۲")]
        public int? DetailAccount2Id { get; set; }

        [Display(Name = "کد حساب کل")]
        public string AccountCode { get; set; }

        [Display(Name = "کد حساب معین")]
        public string MoeinCode { get; set; }

        [Display(Name = "کد تفصیل")]
        public string TafzilCode { get; set; }

        [Display(Name = "کد تفصیل دوم")]
        public string SecondTafzilCode { get; set; }

        [Required(ErrorMessage = "مبلغ بدهکار الزامی است")]
        [Display(Name = "مبلغ بدهکار")]
        public decimal Debit { get; set; }

        [Required(ErrorMessage = "مبلغ بستانکار الزامی است")]
        [Display(Name = "مبلغ بستانکار")]
        public decimal Credit { get; set; }

        [MaxLength(500, ErrorMessage = "شرح تراکنش نمی‌تواند بیش از ۵۰۰ کاراکتر باشد")]
        [Display(Name = "شرح تراکنش")]
        public string? DescriptionTran { get; set; }

    }
}
