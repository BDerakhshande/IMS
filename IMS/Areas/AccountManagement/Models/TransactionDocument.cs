namespace IMS.Areas.AccountManagement.Models
{
    public class TransactionDocument
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime DocumentDate { get; set; } = DateTime.Today; // مقدار پیش‌فرض
        public string Description { get; set; }
        public Status Status { get; set; }
        public int DocumentTypeId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string ModifiedBy { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
