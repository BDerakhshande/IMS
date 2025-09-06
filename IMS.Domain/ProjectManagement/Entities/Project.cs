using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Enums;

namespace IMS.Domain.ProjectManagement.Entities
{
    public class Project
    {
        public int Id { get; set; }

        // نام پروژه
        public string ProjectName { get; set; } = null!;

        // تاریخ شروع پروژه
        public DateTime StartDate { get; set; }

        // تاریخ پایان پروژه
        public DateTime EndDate { get; set; }

        // نوع پروژه (Enum)
        public int ProjectTypeId { get; set; }

        // وضعیت پروژه (Enum)
        public ProjectStatus Status { get; set; }

        // شناسه مسئول پروژه (ارجاع به کاربر یا شخص)
        public string ProjectManager { get; set; }

        // درصد پیشرفت پروژه
        public double ProgressPercent { get; set; }

        // اولویت پروژه (Enum)
        public ProjectPriority Priority { get; set; }

        // محل اجرای پروژه
        public string Location { get; set; } = null!;

        // توضیحات تکمیلی پروژه
        public string? Description { get; set; } = null!;


        public ProjectType ProjectType { get; set; } = null!;

        public int EmployerId { get; set; } // کلید خارجی

        public Employer Employer { get; set; } = null!; // ناوبری به کارفرما


      

        

    }
}
