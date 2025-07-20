using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    public class Moein
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }   // کد حساب معین

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }   // نام حساب معین

        // ارتباط با حساب کل
        public int AccountId { get; set; }
        public Account Account { get; set; }

        // ارتباط به تفصیل‌ها
        public List<Tafzil> Tafzils { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
