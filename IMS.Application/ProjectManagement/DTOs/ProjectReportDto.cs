using IMS.Domain.ProjectManagement.Enums;

namespace IMS.Application.ProjectManagement.DTOs
{
    public class ProjectReportDto
    {
        public string ProjectName { get; set; } = null!;
        public string EmployerName { get; set; } = null!;
        public string ProjectTypeName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;

        // 🔹 اضافه‌ها
        public string ProjectManager { get; set; } = null!;
    }

    public class ProjectReportFilterDto
    {
        // فیلترهای تاریخ
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }

        public string? StartDateFromShamsi { get; set; }
        public string? StartDateToShamsi { get; set; }
        public string? EndDateFromShamsi { get; set; }
        public string? EndDateToShamsi { get; set; }

        // فیلترهای موجود
        public int? EmployerId { get; set; }
        public int? ProjectTypeId { get; set; }
        public string? EmployerName { get; set; }
        public string? ProjectTypeName { get; set; }


        // 🔹 فیلترهای جدید
        public string? ProjectName { get; set; }
        public string? ProjectManager { get; set; }
        public ProjectStatus? Status { get; set; }
    }

    public class ProjectReportRequestDto
    {
        public ProjectReportFilterDto Filter { get; set; } = new();
        public List<ProjectReportDto> ReportItems { get; set; } = new();
    }
}
