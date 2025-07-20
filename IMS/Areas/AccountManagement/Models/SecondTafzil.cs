using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    public class SecondTafzil
    {

        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }   // کد حساب تفصیل ۲

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }   // نام حساب تفصیل ۲


        [Required]
        public int TafzilId { get; set; }
        public Tafzil Tafzil { get; set; }

        public int? CostCenterId { get; set; }
        public CostCenter? CostCenter { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}
