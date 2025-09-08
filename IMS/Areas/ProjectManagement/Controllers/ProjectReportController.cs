using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using IMS.Application.ProjectManagement.ViewModels;
using System.Globalization;
using IMS.Domain.ProjectManagement.Enums;
using IMS.Application.ProjectManagement.Helper;
using ClosedXML.Excel;
using IMS.Areas.ProjectManagement.Helper;
using IMS.Models.ProMan;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace IMS.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    public class ProjectReportController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IEmployerService _employerService;
        private readonly IProjectTypeService _projectTypeService;

        public ProjectReportController(
            IProjectService projectService,
            IEmployerService employerService,
            IProjectTypeService projectTypeService)
        {
            _projectService = projectService;
            _employerService = employerService;
            _projectTypeService = projectTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadSelectListsAsync();

            var model = new ProjectReportRequestDto();

            // مقدار پیش‌فرض امروز
            var now = DateTime.Now;
            var pc = new PersianCalendar();
            string todayShamsi = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";

            // پر کردن مقادیر اولیه فیلتر تاریخ
            model.Filter.StartDateFromShamsi = todayShamsi;
            model.Filter.StartDateToShamsi = todayShamsi;
            model.Filter.EndDateFromShamsi = todayShamsi;
            model.Filter.EndDateToShamsi = todayShamsi;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProjectReportRequestDto model)
        {
            await LoadSelectListsAsync();

            // 📌 تبدیل تاریخ شمسی به میلادی
            model.Filter.StartDateFrom = ParsePersianDate(model.Filter.StartDateFromShamsi);
            model.Filter.StartDateTo = ParsePersianDate(model.Filter.StartDateToShamsi);
            model.Filter.EndDateFrom = ParsePersianDate(model.Filter.EndDateFromShamsi);
            model.Filter.EndDateTo = ParsePersianDate(model.Filter.EndDateToShamsi);

            // 📌 گرفتن داده‌های گزارش
            model.ReportItems = await _projectService.GetProjectReportAsync(model.Filter);

            return View(model);
        }

        private async Task LoadSelectListsAsync()
        {
            var employers = await _employerService.GetAllEmployersAsync();
            var projectTypes = await _projectTypeService.GetAllAsync();

            ViewBag.Employers = new SelectList(employers, "Id", "CompanyName");
            ViewBag.ProjectTypes = new SelectList(projectTypes, "Id", "Name");

            // 📌 لیست وضعیت‌ها (برای DropDown)
            ViewBag.Statuses = Enum.GetValues(typeof(ProjectStatus))
                                   .Cast<ProjectStatus>()
                                   .Select(s => new SelectListItem
                                   {
                                       Value = ((int)s).ToString(),
                                       Text = s.GetDisplayName()
                                   }).ToList();
        }

        private DateTime? ParsePersianDate(string? persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                return null;

            try
            {
                var parts = persianDate.Split('/');
                if (parts.Length != 3) return null;

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                var pc = new PersianCalendar();
                return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch
            {
                return null;
            }
        }




        [HttpPost]
        public async Task<IActionResult> ExportProjectsToExcel([FromBody] ProjectReportFilterDto filter)
        {
            // دریافت داده‌ها از سرویس
            var reportData = await _projectService.GetProjectReportAsync(filter);

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("گزارش پروژه‌ها");

                // عنوان ستون‌ها
                worksheet.Cell(1, 1).Value = "نام پروژه";
                worksheet.Cell(1, 2).Value = "کارفرما";
                worksheet.Cell(1, 3).Value = "نوع پروژه";
                worksheet.Cell(1, 4).Value = "مدیر پروژه";
                worksheet.Cell(1, 5).Value = "تاریخ شروع";
                worksheet.Cell(1, 6).Value = "تاریخ پایان";
                worksheet.Cell(1, 7).Value = "وضعیت";

                // استایل سرستون‌ها
                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // درج داده‌ها
                int row = 2;
                foreach (var item in reportData)
                {
                    worksheet.Cell(row, 1).Value = item.ProjectName;
                    worksheet.Cell(row, 2).Value = item.EmployerName;
                    worksheet.Cell(row, 3).Value = item.ProjectTypeName;
                    worksheet.Cell(row, 4).Value = item.ProjectManager;
                    worksheet.Cell(row, 5).Value = item.StartDate.ToShamsi(); // اگر میخوای تاریخ شمسی باشه
                    worksheet.Cell(row, 6).Value = item.EndDate.ToShamsi();
                    worksheet.Cell(row, 7).Value = item.Status;
                    row++;
                }

                // اندازه ستون‌ها به اندازه محتوا
                worksheet.Columns().AdjustToContents();

                // تولید فایل در حافظه
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"ProjectReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }



        [HttpPost]
        public async Task<IActionResult> ExportPdf(ProjectReportFilterDto filter)
        {
            // تبدیل تاریخ شمسی به میلادی
            filter.StartDateFrom = ParsePersianDate(filter.StartDateFromShamsi);
            filter.StartDateTo = ParsePersianDate(filter.StartDateToShamsi);
            filter.EndDateFrom = ParsePersianDate(filter.EndDateFromShamsi);
            filter.EndDateTo = ParsePersianDate(filter.EndDateToShamsi);

            // دریافت داده‌ها
            var reportData = await _projectService.GetProjectReportAsync(filter);

            // تبدیل ID به نام کارفرما و نوع پروژه در صورت نیاز
            string employerName = filter.EmployerName;
            if (string.IsNullOrEmpty(employerName) && filter.EmployerId.HasValue)
            {
                var employer = await _employerService.GetEmployerByIdAsync(filter.EmployerId.Value);
                employerName = employer?.CompanyName ?? "";
            }

            string projectTypeName = filter.ProjectTypeName;
            if (string.IsNullOrEmpty(projectTypeName) && filter.ProjectTypeId.HasValue)
            {
                var projectType = await _projectTypeService.GetByIdAsync(filter.ProjectTypeId.Value);
                projectTypeName = projectType?.Name ?? "";
            }

            // پر کردن ViewModel برای چاپ
            var vm = new ProjectReportPrintViewModel
            {
                ReportItems = reportData,
                ProjectNameFilter = filter.ProjectName,
                EmployerFilter = employerName,
                ProjectTypeFilter = projectTypeName,
                ProjectManagerFilter = filter.ProjectManager,
                StatusFilter = filter.Status?.GetDisplayName(),
                StartDateFromFilter = filter.StartDateFromShamsi,
                StartDateToFilter = filter.StartDateToShamsi,
                EndDateFromFilter = filter.EndDateFromShamsi,
                EndDateToFilter = filter.EndDateToShamsi
            };

            // تولید PDF با Rotativa
            return new ViewAsPdf("~/Areas/ProjectManagement/Views/ProjectReport/ProjectReportPrint.cshtml", vm)
            {
                FileName = $"ProjectReport_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10),
                CustomSwitches = "--disable-smart-shrinking --print-media-type --background"
            };
        }




    }
}
