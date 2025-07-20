using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    public class Tafzil
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }   // کد حساب تفصیل

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }   // نام حساب تفصیل

        // ارتباط با حساب معین
        public int MoeinId { get; set; }
        public Moein Moein { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<SecondTafzil> SecondTafzils { get; set; }
    }
}
