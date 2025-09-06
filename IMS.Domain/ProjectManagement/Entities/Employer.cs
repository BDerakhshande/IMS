using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Enums;

namespace IMS.Domain.ProjectManagement.Entities
{
    public class Employer
    {
        public int Id { get; set; } // non-nullable

        // نام شرکت (nullable)
        public string? CompanyName { get; set; }

        // شناسه ملی شرکت (nullable)
        public string? NationalId { get; set; }

        // شماره ثبت شرکت (non-nullable)
        public long RegistrationNumber { get; set; }

        // نوع شخصیت حقوقی شرکت (non-nullable)
        public LegalPersonType LegalPersonType { get; set; }

        // آدرس شرکت (nullable)
        public string? Address { get; set; }

        // شماره تلفن شرکت (nullable)
        public string? PhoneNumber { get; set; }

        // وب‌سایت شرکت (nullable)
        public string? Website { get; set; }

        // نام نماینده شرکت (nullable)
        public string? RepresentativeName { get; set; }

        // سمت نماینده شرکت (nullable)
        public string? RepresentativePosition { get; set; }

        // موبایل نماینده شرکت (nullable)
        public string? RepresentativeMobile { get; set; }

        // ایمیل نماینده شرکت (nullable)
        public string? RepresentativeEmail { get; set; }

        // نوع همکاری شرکت با ما (nullable)
        public CooperationType? CooperationType { get; set; }

        // تاریخ شروع همکاری (nullable)
        public DateTime? CooperationStartDate { get; set; }

        // توضیحات تکمیلی (nullable)
        public string? AdditionalDescription { get; set; }
    }
}
