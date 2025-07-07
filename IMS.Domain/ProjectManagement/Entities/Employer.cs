using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Enums;

namespace IMS.Domain.ProjectManagement.Entities
{
    public class Employer
    {
        public int Id { get; set; }

        // نام شرکت
        public string CompanyName { get; set; } = null!;

        // شناسه ملی شرکت
        public string NationalId { get; set; }

        // شماره ثبت شرکت
        public long RegistrationNumber { get; set; }

        // نوع شخصیت حقوقی شرکت (حقیقی یا حقوقی)
        public LegalPersonType LegalPersonType { get; set; }


        // آدرس شرکت
        public string Address { get; set; } = null!;

        // شماره تلفن شرکت (قابل اضافه کردن ماسک تلفن بعدا)
        public string PhoneNumber { get; set; } = null!;

        // وب‌سایت شرکت
        public string Website { get; set; } = null!;

        // نام نماینده شرکت
        public string RepresentativeName { get; set; } = null!;

        // سمت نماینده شرکت
        public string RepresentativePosition { get; set; } = null!;

        // موبایل نماینده شرکت (قابل اضافه کردن ماسک موبایل)
        public string RepresentativeMobile { get; set; } = null!;

        // ایمیل نماینده شرکت
        public string RepresentativeEmail { get; set; } = null!;

        // نوع همکاری شرکت با ما (قرارداد، مشارکت و غیره)
        public CooperationType CooperationType { get; set; }

        // تاریخ شروع همکاری
        public DateTime CooperationStartDate { get; set; }

        // توضیحات تکمیلی
        public string AdditionalDescription { get; set; } = null!;
    }
}
