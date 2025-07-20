using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    // ایجاد enum برای نوع تراکنش
    public enum TransactionType
    {
        Deposit,  // واریزی
        Withdrawal // برداشته
    }

    public class CostCenter
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public TransactionType Type { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<SecondTafzil> SecondTafzils { get; set; }
    }
}
