namespace IMS.Areas.AccountManagement.Models
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime DocumentDate { get; set; } // تاریخ میلادی
        public string PersianDate { get; set; } // تاریخ شمسی
        public string Description { get; set; }
        public Status Status { get; set; }
        public string DocumentTypeName { get; set; }
        public string ModifiedBy { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
