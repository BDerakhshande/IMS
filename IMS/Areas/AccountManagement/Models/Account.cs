using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }   // کد حساب (یونیک و خاص)

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }   // نام حساب (توضیح)

        // ارتباط به حساب‌های معین
        public List<Moein> Moeins { get; set; } // یک حساب کل می‌تواند چند حساب معین داشته باشد
        public List<Transaction> Transactions { get; set; }
    }
}
