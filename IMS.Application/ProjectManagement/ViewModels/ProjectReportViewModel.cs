using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.ProjectManagement.ViewModels
{
    public class ProjectReportViewModel
    {
        // فیلترها
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public int? EmployerId { get; set; }
        public int? ProjectTypeId { get; set; }

        // نتایج گزارش
        public List<ProjectReportDto> ReportItems { get; set; } = new List<ProjectReportDto>();
    }

    public class ProjectReportDto
    {
        public string ProjectName { get; set; } = null!;
        public string EmployerName { get; set; } = null!;
        public string ProjectTypeName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
    }
}
