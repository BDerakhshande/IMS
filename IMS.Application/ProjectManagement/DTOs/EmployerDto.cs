using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Enums;

namespace IMS.Application.ProjectManagement.DTOs
{
    public class EmployerDto
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "وارد کردن نام شرکت الزامی است")]
        [StringLength(200, ErrorMessage = "نام شرکت نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string CompanyName { get; set; } = null!;

        [Required(ErrorMessage = "شناسه ملی الزامی است")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "شناسه ملی باید 10 رقم باشد")]
        public string NationalId { get; set; } = null!;

        [Required(ErrorMessage = "شماره ثبت الزامی است")]
        [Range(1, long.MaxValue, ErrorMessage = "شماره ثبت معتبر نیست")]
        public long RegistrationNumber { get; set; }

        [Required(ErrorMessage = "نوع شخصیت حقوقی را انتخاب کنید")]
        public LegalPersonType LegalPersonType { get; set; }

        [Required(ErrorMessage = "آدرس الزامی است")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [RegularExpression(@"^\d{8,11}$", ErrorMessage = "شماره تلفن معتبر نیست")]
        public string PhoneNumber { get; set; } = null!;

        public string Website { get; set; } = null!;

        [Required(ErrorMessage = "نام نماینده الزامی است")]
        public string RepresentativeName { get; set; } = null!;

        [Required(ErrorMessage = "سمت نماینده الزامی است")]
        public string RepresentativePosition { get; set; } = null!;

        [Required(ErrorMessage = "شماره موبایل نماینده الزامی است")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست (مثال: 09121234567)")]
        public string RepresentativeMobile { get; set; } = null!;

        [EmailAddress(ErrorMessage = "ایمیل معتبر وارد کنید")]
        public string RepresentativeEmail { get; set; } = null!;

        [Required(ErrorMessage = "نوع همکاری را انتخاب کنید")]
        public CooperationType CooperationType { get; set; }

        [Required(ErrorMessage = "تاریخ شروع همکاری الزامی است")]
        public DateTime CooperationStartDate { get; set; }



        [StringLength(500, ErrorMessage = "توضیحات بیشتر نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string AdditionalDescription { get; set; } = null!;
    }
}
