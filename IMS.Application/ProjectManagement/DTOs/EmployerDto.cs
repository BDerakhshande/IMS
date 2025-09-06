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
        public int Id { get; set; } // non-nullable

        [StringLength(200, ErrorMessage = "نام شرکت نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string? CompanyName { get; set; } // nullable مطابق با دیتابیس

        [RegularExpression(@"^\d{10}$", ErrorMessage = "شناسه ملی باید 10 رقم باشد")]
        public string? NationalId { get; set; } // nullable

        public long RegistrationNumber { get; set; } // non-nullable

        [Required(ErrorMessage = "نوع شخصیت حقوقی را انتخاب کنید")]
        public LegalPersonType LegalPersonType { get; set; } // non-nullable

        public string? Address { get; set; } // nullable

        [RegularExpression(@"^\d{8,11}$", ErrorMessage = "شماره تلفن معتبر نیست")]
        public string? PhoneNumber { get; set; } // nullable

        public string? Website { get; set; } // nullable

        public string? RepresentativeName { get; set; } // nullable

        public string? RepresentativePosition { get; set; } // nullable

        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست (مثال: 09121234567)")]
        public string? RepresentativeMobile { get; set; } // nullable

        [EmailAddress(ErrorMessage = "ایمیل معتبر وارد کنید")]
        public string? RepresentativeEmail { get; set; } // nullable

        public CooperationType? CooperationType { get; set; } // nullable

        public DateTime? CooperationStartDate { get; set; } // nullable

        public string? AdditionalDescription { get; set; } // nullable
    }
}
