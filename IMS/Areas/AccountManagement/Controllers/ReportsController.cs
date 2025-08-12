using IMS.Areas.AccountManagement.Models;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IMS.Areas.AccountManagement.Data;
using ClosedXML.Excel;

namespace IMS.Areas.AccountManagement.Controllers
{
    [Area("AccountManagement")]
    public class ReportsController : Controller
    {
        private readonly AccountManagementDbContext _context;

        public ReportsController(AccountManagementDbContext context)
        {
            _context = context;
        }
        public static DateTime ConvertToGregorianDate(int year, int month, int day)
        {
            var persianCalendar = new PersianCalendar();
            try
            {
                return persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch
            {
                return DateTime.MinValue; // یا مقدار پیش‌فرض مناسب
            }
        }

        // GET: Reports
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new TransactionReportViewModel();
            LoadDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(TransactionReportViewModel viewModel)
        {
            // نرمال‌سازی و تبدیل تاریخ‌های شمسی
            DateTime? fromDate = null, toDate = null;

            string NormalizePersianDate(string persianDate)
            {
                if (string.IsNullOrWhiteSpace(persianDate))
                    return null;

                var parts = persianDate.Split('/');
                if (parts.Length != 3)
                    return null;

                return $"{parts[0]}/{int.Parse(parts[1]):D2}/{int.Parse(parts[2]):D2}";
            }

            bool TryParsePersianDate(string persianDate, out DateTime? result)
            {
                result = null;
                if (string.IsNullOrWhiteSpace(persianDate))
                    return false;

                var normalizedDate = NormalizePersianDate(persianDate);
                if (string.IsNullOrEmpty(normalizedDate))
                    return false;

                try
                {
                    var parsedDate = PersianDateTime.Parse(normalizedDate); // نام منحصربه‌فرد
                    result = parsedDate.ToDateTime();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // پردازش FromDate
            if (!string.IsNullOrEmpty(viewModel.FromDate))
            {
                if (TryParsePersianDate(viewModel.FromDate, out var parsedFromDate))
                {
                    fromDate = parsedFromDate;
                }
            }

            // پردازش ToDate
            if (!string.IsNullOrEmpty(viewModel.ToDate))
            {
                if (TryParsePersianDate(viewModel.ToDate, out var parsedToDate))
                {
                    toDate = parsedToDate;
                }
            }


            var query = _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Moein)
                .Include(t => t.Tafzil)
                .Include(t => t.SecondTafzil)
                .Include(t => t.CostCenter)
                .AsQueryable();


            if (fromDate.HasValue)
                query = query.Where(t => t.DocumentDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(t => t.DocumentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            // اعمال فیلترهای مبلغ
            if (viewModel.MinAmount.HasValue || viewModel.MaxAmount.HasValue)
            {
                query = query.Where(t =>
                    (t.Debit > 0 &&
                        (!viewModel.MinAmount.HasValue || t.Debit >= viewModel.MinAmount.Value) &&
                        (!viewModel.MaxAmount.HasValue || t.Debit <= viewModel.MaxAmount.Value)) ||
                    (t.Credit > 0 &&
                        (!viewModel.MinAmount.HasValue || t.Credit >= viewModel.MinAmount.Value) &&
                        (!viewModel.MaxAmount.HasValue || t.Credit <= viewModel.MaxAmount.Value))
                );
            }


            if (viewModel.MainAccountId.HasValue)
                query = query.Where(t => t.AccountId == viewModel.MainAccountId.Value);
            if (viewModel.SubAccountId.HasValue)
                query = query.Where(t => t.MoeinId == viewModel.SubAccountId.Value);
            if (viewModel.DetailAccount1Id.HasValue)
                query = query.Where(t => t.TafzilId == viewModel.DetailAccount1Id.Value);
            if (viewModel.DetailAccount2Id.HasValue)
                query = query.Where(t => t.SecondTafzilId == viewModel.DetailAccount2Id.Value);
            if (viewModel.CostCenterId.HasValue)
                query = query.Where(t => t.CostCenterId == viewModel.CostCenterId.Value);


            var persianCalendar = new PersianCalendar();
            var transactions = query
                .Select(t => new TransactionReportItem
                {

                    Date = t.DocumentDate == DateTime.MinValue ? null : t.DocumentDate,
                    PersianDate = t.DocumentDate == DateTime.MinValue
                        ? ""
                        : $"{persianCalendar.GetYear(t.DocumentDate)}/{persianCalendar.GetMonth(t.DocumentDate):D2}/{persianCalendar.GetDayOfMonth(t.DocumentDate):D2}",
                    MainAccount = $"{t.Account.Code} - {t.Account.Name}",
                    SubAccount = t.Moein != null ? $"{t.Moein.Code} - {t.Moein.Name}" : "",
                    DetailAccount1 = t.Tafzil != null ? $"{t.Tafzil.Code} - {t.Tafzil.Name}" : "",
                    DetailAccount2 = t.SecondTafzil != null ? $"{t.SecondTafzil.Code} - {t.SecondTafzil.Name}" : "",
                    CostCenterName = t.CostCenter != null && !string.IsNullOrEmpty(t.CostCenter.Name) ? t.CostCenter.Name : "--",
                    Debit = t.Debit,
                    Credit = t.Credit
                })
                .OrderBy(t => t.Date)
                .ToList();
            // پر کردن مدل
            viewModel.Transactions = transactions;
            viewModel.TotalDebit = transactions.Sum(t => t.Debit);
            viewModel.TotalCredit = transactions.Sum(t => t.Credit);

            // پر کردن dropdownها
            LoadDropdowns(viewModel);

            return View("Index", viewModel);
        }


        [HttpPost]
        private void LoadDropdowns(TransactionReportViewModel viewModel)
        {
            // لیست مراکز هزینه، حساب‌ها و دیگر موارد
            var costCenters = _context.CostCenters.ToList();
            var mainAccounts = _context.Accounts.ToList();
            var subAccounts = _context.Moeins.ToList();
            var detail1 = _context.Tafzils.ToList();
            var detail2 = _context.SecondTafzils.ToList();

            // افزودن گزینه "همه"
            ViewBag.CostCenters = new SelectList(
                new[] { new { Id = (int?)null, Name = "همه مراکز هزینه" } }.Concat(costCenters.Select(c => new { Id = (int?)c.Id, Name = c.Name })),
                "Id", "Name", viewModel.CostCenterId);

            ViewBag.MainAccounts = new SelectList(
                new[] { new { Id = (int?)null, Name = "همه کدهای کل" } }.Concat(mainAccounts.Select(c => new { Id = (int?)c.Id, Name = c.Code + " - " + c.Name })),
                "Id", "Name", viewModel.MainAccountId);

            ViewBag.SubAccounts = new SelectList(
                new[] { new { Id = (int?)null, Name = "همه کدهای معین" } }.Concat(subAccounts.Select(c => new { Id = (int?)c.Id, Name = c.Code + " - " + c.Name })),
                "Id", "Name", viewModel.SubAccountId);

            ViewBag.DetailAccounts1 = new SelectList(
                new[] { new { Id = (int?)null, Name = "همه تفصیل ۱" } }.Concat(detail1.Select(c => new { Id = (int?)c.Id, Name = c.Code + " - " + c.Name })),
                "Id", "Name", viewModel.DetailAccount1Id);

            ViewBag.DetailAccounts2 = new SelectList(
                new[] { new { Id = (int?)null, Name = "همه تفصیل ۲" } }.Concat(detail2.Select(c => new { Id = (int?)c.Id, Name = c.Code + " - " + c.Name })),
                "Id", "Name", viewModel.DetailAccount2Id);
        }



        [HttpGet]
        public JsonResult GetSubAccounts(int mainAccountId)
        {
            var subAccounts = _context.Moeins
                .Where(m => m.AccountId == mainAccountId)  // ارتباط با حساب کل از طریق AccountId در مدل Moein
                .Select(m => new { m.Id, m.Code, m.Name })
                .ToList();

            return Json(subAccounts);
        }

        [HttpGet]
        public JsonResult GetDetailAccounts1(int moeinId)
        {
            var detailAccounts1 = _context.Tafzils
                .Where(t => t.MoeinId == moeinId)
                .Select(t => new { t.Id, t.Code, t.Name })
                .ToList();

            return Json(detailAccounts1);
        }

        [HttpGet]
        public JsonResult GetDetailAccounts2(int tafzilId)
        {
            var detailAccounts2 = _context.SecondTafzils
                .Where(st => st.TafzilId == tafzilId)
                .Select(st => new { st.Id, st.Code, st.Name })
                .ToList();

            return Json(detailAccounts2);
        }

        [HttpPost]
        public IActionResult ExportToExcel(TransactionReportViewModel viewModel)
        {
            DateTime? fromDate = null, toDate = null;

            bool TryParsePersianDate(string persianDate, out DateTime? result)
            {
                result = null;
                if (string.IsNullOrWhiteSpace(persianDate))
                    return false;

                try
                {
                    var parsed = PersianDateTime.Parse(persianDate);
                    var dt = parsed.ToDateTime();
                    // چک معتبر بودن تاریخ
                    if (dt == DateTime.MinValue || dt == default)
                        return false;

                    result = dt;
                    return true;
                }
                catch
                {
                    return false;
                }
            }


            if (!string.IsNullOrEmpty(viewModel.FromDate))
                TryParsePersianDate(viewModel.FromDate, out fromDate);
            if (!string.IsNullOrEmpty(viewModel.ToDate))
                TryParsePersianDate(viewModel.ToDate, out toDate);

            var query = _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Moein)
                .Include(t => t.Tafzil)
                .Include(t => t.SecondTafzil)
                .Include(t => t.CostCenter)
                .AsQueryable();

            if (!string.IsNullOrEmpty(viewModel.FromDate) && TryParsePersianDate(viewModel.FromDate, out fromDate))
            {
                // فقط اگر مقدار معتبر داریم فیلتر کنیم
                query = query.Where(t => t.DocumentDate >= fromDate.Value);
            }
            if (!string.IsNullOrEmpty(viewModel.ToDate) && TryParsePersianDate(viewModel.ToDate, out toDate))
            {
                query = query.Where(t => t.DocumentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            if (viewModel.MinAmount.HasValue || viewModel.MaxAmount.HasValue)
            {
                query = query.Where(t =>
                    (t.Debit > 0 &&
                        (!viewModel.MinAmount.HasValue || t.Debit >= viewModel.MinAmount.Value) &&
                        (!viewModel.MaxAmount.HasValue || t.Debit <= viewModel.MaxAmount.Value)) ||
                    (t.Credit > 0 &&
                        (!viewModel.MinAmount.HasValue || t.Credit >= viewModel.MinAmount.Value) &&
                        (!viewModel.MaxAmount.HasValue || t.Credit <= viewModel.MaxAmount.Value))
                );
            }

            if (viewModel.MainAccountId.HasValue)
                query = query.Where(t => t.AccountId == viewModel.MainAccountId.Value);
            if (viewModel.SubAccountId.HasValue)
                query = query.Where(t => t.MoeinId == viewModel.SubAccountId.Value);
            if (viewModel.DetailAccount1Id.HasValue)
                query = query.Where(t => t.TafzilId == viewModel.DetailAccount1Id.Value);
            if (viewModel.DetailAccount2Id.HasValue)
                query = query.Where(t => t.SecondTafzilId == viewModel.DetailAccount2Id.Value);
            if (viewModel.CostCenterId.HasValue)
                query = query.Where(t => t.CostCenterId == viewModel.CostCenterId.Value);

            var transactions = query
                .Select(t => new
                {
                    Date = t.DocumentDate,
                    MainAccount = $"{t.Account.Code} - {t.Account.Name}",
                    SubAccount = t.Moein != null ? $"{t.Moein.Code} - {t.Moein.Name}" : "",
                    DetailAccount1 = t.Tafzil != null ? $"{t.Tafzil.Code} - {t.Tafzil.Name}" : "",
                    DetailAccount2 = t.SecondTafzil != null ? $"{t.SecondTafzil.Code} - {t.SecondTafzil.Name}" : "",
                    CostCenter = t.CostCenter != null ? t.CostCenter.Name : "--",
                    Debit = t.Debit,
                    Credit = t.Credit
                })
                .OrderBy(t => t.Date)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("گزارش تراکنش‌ها");
                var currentRow = 1;

                worksheet.Cell(currentRow, 2).Value = "کد کل";
                worksheet.Cell(currentRow, 3).Value = "کد معین";
                worksheet.Cell(currentRow, 4).Value = "تفصیل ۱";
                worksheet.Cell(currentRow, 5).Value = "تفصیل ۲";
                worksheet.Cell(currentRow, 6).Value = "مرکز هزینه";
                worksheet.Cell(currentRow, 7).Value = "بدهکار";
                worksheet.Cell(currentRow, 8).Value = "بستانکار";
                worksheet.Cell(currentRow, 9).Value = "مانده";

                decimal runningBalance = 0m;

                foreach (var t in transactions)
                {
                    currentRow++;
                   
                    worksheet.Cell(currentRow, 2).Value = t.MainAccount;
                    worksheet.Cell(currentRow, 3).Value = t.SubAccount;
                    worksheet.Cell(currentRow, 4).Value = t.DetailAccount1;
                    worksheet.Cell(currentRow, 5).Value = t.DetailAccount2;
                    worksheet.Cell(currentRow, 6).Value = t.CostCenter;
                    worksheet.Cell(currentRow, 7).Value = t.Debit;
                    worksheet.Cell(currentRow, 8).Value = t.Credit;

                    runningBalance += t.Debit - t.Credit;
                    worksheet.Cell(currentRow, 9).Value = runningBalance;
                }

                // مجموع
                currentRow++;
                worksheet.Cell(currentRow, 6).Value = "مجموع";
                worksheet.Cell(currentRow, 7).FormulaA1 = $"=SUM(G2:G{currentRow - 1})";
                worksheet.Cell(currentRow, 8).FormulaA1 = $"=SUM(H2:H{currentRow - 1})";

                worksheet.Cell(currentRow, 9).Value = runningBalance;

                // استایل بهتر
                worksheet.Range(1, 1, 1, 9).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "TransactionReport.xlsx");
                }
            }
        }


    }
}
