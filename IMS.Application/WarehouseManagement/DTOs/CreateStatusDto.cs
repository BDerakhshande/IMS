using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class CreateStatusDto
    {
        [Required(ErrorMessage = "نام وضعیت را وارد کنید.")]
        public string Name { get; set; } = null!;



        [Required(ErrorMessage = "لطفا گروه را انتخاب کنید.")]
        [Range(1, int.MaxValue, ErrorMessage = "گروه انتخاب شده معتبر نیست.")]
        public int GroupId { get; set; }
    }
}
