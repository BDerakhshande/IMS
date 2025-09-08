using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Enums;

namespace IMS.Application.ProjectManagement.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام پروژه الزامی است")]
        [StringLength(200, ErrorMessage = "نام پروژه نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد")]
        public string ProjectName { get; set; } = null!;

        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "تاریخ پایان الزامی است")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "نوع پروژه الزامی است")]
        public int? ProjectTypeId { get; set; }

        [Required(ErrorMessage = "وضعیت پروژه الزامی است")]
        public ProjectStatus Status { get; set; }

        [Required(ErrorMessage = "نام مدیر پروژه الزامی است")]
        [StringLength(100, ErrorMessage = "نام مدیر پروژه نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد")]
        public string ProjectManager { get; set; } = null!;

        [Range(0, 100, ErrorMessage = "درصد پیشرفت باید بین ۰ تا ۱۰۰ باشد")]
        public double ProgressPercent { get; set; }

        public ProjectPriority Priority { get; set; }

        [Required(ErrorMessage = "موقعیت مکانی الزامی است")]
        [StringLength(200, ErrorMessage = "موقعیت نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد")]
        public string Location { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۱۰۰۰ کاراکتر باشد")]
        
        public string? Description { get; set; } = null!;

        [Required(ErrorMessage = "کارفرما الزامی است")]
        public int? EmployerId { get; set; }

        public string? EmployerName { get; set; }
        public string? ProjectTypeName { get; set; }
    }
}
