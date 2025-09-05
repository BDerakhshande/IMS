using IMS.Application.ProjectManagement.DTOs;

namespace IMS.Models.ProMan
{
    public class ProjectReportPrintViewModel
    {
        public List<ProjectReportDto> ReportItems { get; set; } = new();

        // 🔹 فیلترها برای نمایش در پرینت
        public string? ProjectNameFilter { get; set; }
        public string? EmployerFilter { get; set; }
        public string? ProjectTypeFilter { get; set; }
        public string? ProjectManagerFilter { get; set; }
        public string? StatusFilter { get; set; }

        // 🔹 تاریخ‌ها (شمسی برای نمایش در گزارش چاپی)
        public string? StartDateFromFilter { get; set; }
        public string? StartDateToFilter { get; set; }
        public string? EndDateFromFilter { get; set; }
        public string? EndDateToFilter { get; set; }
    }
}
