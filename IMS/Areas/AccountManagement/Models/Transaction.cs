using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    public class Transaction
    {

        public int Id { get; set; }
        [Required]
        public int TransactionDocumentId { get; set; }
        public TransactionDocument TransactionDocument { get; set; }
        [Required]
        public int AccountId { get; set; }
        public Account Account { get; set; }
        [Required]
        public decimal Debit { get; set; }
        [Required]
        public decimal Credit { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; } // شرح برای هر تراکنش
        [Required]
        public int DocumentTypeId { get; set; }
        public DocumentType DocumentType { get; set; }
        [Required]
        public DateTime DocumentDate { get; set; }
        public int? CostCenterId { get; set; }
        public CostCenter CostCenter { get; set; }
        [Required]
        public int? MoeinId { get; set; }
        public Moein Moein { get; set; }
        public int? TafzilId { get; set; }
        public Tafzil Tafzil { get; set; }
        public int? SecondTafzilId { get; set; }
        public SecondTafzil SecondTafzil { get; set; }
     
       
    }
}
