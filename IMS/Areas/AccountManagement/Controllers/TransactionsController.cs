using IMS.Areas.AccountManagement.Models;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IMS.Areas.AccountManagement.Data;

namespace IMS.Areas.AccountManagement.Controllers
{
    [Area("AccountManagement")]
    public class TransactionsController : Controller
    {
        private readonly AccountManagementDbContext _context;

        public TransactionsController(AccountManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var persianCalendar = new PersianCalendar();
            var documents = await _context.TransactionDocuments
                .Include(d => d.Transactions)
                .Include(d => d.DocumentType)
                .Select(d => new TransactionViewModel
                {
                    Id = d.Id,
                    DocumentNumber = d.DocumentNumber,
                    DocumentDate = d.DocumentDate,
                    PersianDate = $"{persianCalendar.GetYear(d.DocumentDate)}/{persianCalendar.GetMonth(d.DocumentDate):D2}/{persianCalendar.GetDayOfMonth(d.DocumentDate):D2}",
                    Description = d.Description,
                    Status = d.Status,
                    DocumentTypeName = d.DocumentType.Name,
                    ModifiedBy = d.ModifiedBy,
                    TotalAmount = d.Transactions.Sum(t => t.Debit)
                })
                .ToListAsync();

            return View(documents);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var document = await _context.TransactionDocuments.FindAsync(id);
            if (document == null)
            {
                TempData["ErrorMessage"] = "سند مورد نظر یافت نشد.";
                return RedirectToAction(nameof(Index));
            }

            document.Status = Status.Confirmation;
            _context.TransactionDocuments.Update(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "سند با موفقیت تایید شد.";
            return RedirectToAction(nameof(Index));
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var document = await _context.TransactionDocuments
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                TempData["ErrorMessage"] = "سند مورد نظر یافت نشد.";
                return RedirectToAction(nameof(Index));
            }

            // تغییر وضعیت سند به رد شده
            document.Status = Status.Reject;

            _context.TransactionDocuments.Update(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "سند با موفقیت رد شد.";
            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Fetch the document first to check its status
                var document = await _context.TransactionDocuments.FindAsync(id);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "سند مورد نظر یافت نشد.";
                    return RedirectToAction(nameof(Index));
                }

                // Fetch related transactions for the document
                var transactions = await _context.Transactions
                    .Where(t => t.TransactionDocumentId == id)
                    .Include(t => t.Moein)
                    .Include(t => t.Account)
                    .Include(t => t.Tafzil)
                    .Include(t => t.SecondTafzil)
                    .Include(t => t.TransactionDocument)
                    .ToListAsync();

                if (!transactions.Any())
                    return NotFound();

                // Fetch document types for dropdown
                var documentTypes = await _context.DocumentTypes
                    .Select(dt => new SelectListItem
                    {
                        Value = dt.Id.ToString(),
                        Text = dt.Name
                    }).ToListAsync();

                // Fetch main accounts
                var mainAccounts = await _context.Accounts
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.Code} - {a.Name}"
                    }).ToListAsync();

                // Fetch sub accounts grouped by main account ID
                var subAccounts = await _context.Moeins
                    .GroupBy(m => m.AccountId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(m => new SelectListItem
                        {
                            Value = m.Id.ToString(),
                            Text = $"{m.Code} - {m.Name}"
                        }).ToList()
                    );

                // Fetch detail accounts 1 grouped by sub account ID
                var detailAccounts1 = await _context.Tafzils
                    .GroupBy(t => t.MoeinId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(t => new SelectListItem
                        {
                            Value = t.Id.ToString(),
                            Text = $"{t.Code} - {t.Name}"
                        }).ToList()
                    );

                // Fetch detail accounts 2 grouped by detail account 1 ID
                var detailAccounts2 = await _context.SecondTafzils
                    .GroupBy(st => st.TafzilId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(st => new SelectListItem
                        {
                            Value = st.Id.ToString(),
                            Text = $"{st.Code} - {st.Name}"
                        }).ToList()
                    );

                // Build the view model
                var viewModel = new TransactionDocumentDetailsViewModel
                {
                    DocumentId = document.Id,
                    DocumentDate = document.DocumentDate,
                    Description = document.Description,
                    SelectedDocumentTypeId = document.DocumentTypeId,
                    DocumentStatus = document.Status, // تنظیم وضعیت سند
                    DocumentTypes = documentTypes,
                    MainAccounts = mainAccounts,
                    SubAccounts = subAccounts,
                    DetailAccounts1 = detailAccounts1,
                    DetailAccounts2 = detailAccounts2,
                    Transactions = transactions.Select(t => new TransactionDetailsViewModel
                    {
                        TransactionId = t.Id,
                        MainAccountId = t.AccountId,
                        SubAccountId = t.MoeinId,
                        DetailAccount1Id = t.TafzilId ?? 0, // مدیریت null برای TafzilId
                        DetailAccount2Id = t.SecondTafzilId ?? 0, // مدیریت null برای SecondTafzilId
                        Debit = t.Debit,
                        Credit = t.Credit,
                        DescriptionTran = t.Description ?? string.Empty // اگر Description null باشد، رشته خالی قرار دهید
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطایی در بارگذاری جزئیات سند رخ داد: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocumentDetails(TransactionDocumentDetailsViewModel model)
        {
            // یافتن سند با تراکنش‌های مرتبط
            var document = await _context.TransactionDocuments
                .Include(d => d.Transactions)
                .FirstOrDefaultAsync(d => d.Id == model.DocumentId);

            if (document == null)
            {
                TempData["ErrorMessage"] = "سند مورد نظر یافت نشد.";
                return RedirectToAction(nameof(Index));
            }

            // بررسی وضعیت سند
            if (document.Status == Status.Confirmation)
            {
                TempData["ErrorMessage"] = "سند تأیید شده است و قابلیت ویرایش ندارد.";
                return RedirectToAction(nameof(Index));
            }

            // به‌روزرسانی اطلاعات کلی سند
            document.Description = model.Description;
            document.ModifiedBy = "سیستم";
            document.Status = Status.AwaitingApproval;

            // اعتبارسنجی تراکنش‌ها
            var errorMessages = new List<string>();
            bool hasError = false;

            // بررسی شرح سند
            if (string.IsNullOrWhiteSpace(model.Description))
            {
                errorMessages.Add("لطفاً شرح سند را وارد کنید.");
                hasError = true;
            }

            // بررسی وجود تراکنش‌ها
            if (model.Transactions == null || !model.Transactions.Any())
            {
                errorMessages.Add("لطفاً حداقل یک تراکنش وارد کنید.");
                hasError = true;
            }
            else
            {
                //// بررسی شرح و حساب‌های تراکنش‌ها
                //if (model.Transactions.Any(t => string.IsNullOrWhiteSpace(t.DescriptionTran)))
                //{
                //    errorMessages.Add("لطفاً شرح برای همه تراکنش‌ها وارد کنید.");
                //    hasError = true;
                //}

                if (model.Transactions.Any(t => !t.MainAccountId.HasValue || !t.SubAccountId.HasValue))
                {
                    errorMessages.Add("لطفاً حساب کل و معین را برای همه تراکنش‌ها انتخاب کنید.");
                    hasError = true;
                }

                //// بررسی وجود مقدار برای بدهکار یا بستانکار
                //if (model.Transactions.Any(t => t.Debit == 0 && t.Credit == 0))
                //{
                //    errorMessages.Add("هر تراکنش باید حداقل یک مقدار بدهکار یا بستانکار داشته باشد.");
                //    hasError = true;
                //}

                // محاسبه مجموع بدهکار و بستانکار
                decimal totalDebit = model.Transactions.Sum(t => t.Debit);
                decimal totalCredit = model.Transactions.Sum(t => t.Credit);
                if (totalDebit != totalCredit)
                {
                    errorMessages.Add("مجموع بدهکاری و بستانکاری باید برابر باشند.");
                    hasError = true;
                }
            }

            // در صورت وجود خطا، بازگشت به View
            if (hasError)
            {
                TempData["ErrorMessage"] = string.Join("؛ ", errorMessages);
                await PopulateModelLists(model);
                return View("Details", model);
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();


                _context.Transactions.RemoveRange(document.Transactions);


                var newTransactions = new List<Transaction>();
                foreach (var t in model.Transactions)
                {
                    var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == t.MainAccountId);
                    var moein = await _context.Moeins.FirstOrDefaultAsync(m => m.Id == t.SubAccountId);
                    var tafzil = t.DetailAccount1Id.HasValue
                        ? await _context.Tafzils.FirstOrDefaultAsync(tf => tf.Id == t.DetailAccount1Id)
                        : null;
                    var secondTafzil = t.DetailAccount2Id.HasValue
                        ? await _context.SecondTafzils.FirstOrDefaultAsync(stf => stf.Id == t.DetailAccount2Id)
                        : null;

                    if (account == null || moein == null)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "حساب کل یا معین نامعتبر است.";
                        await PopulateModelLists(model);
                        return View("Details", model);
                    }

                    var newTransaction = new Transaction
                    {
                        TransactionDocumentId = model.DocumentId,
                        AccountId = account.Id,
                        MoeinId = moein.Id,
                        TafzilId = tafzil?.Id,
                        SecondTafzilId = secondTafzil?.Id,
                        Debit = t.Debit,
                        Credit = t.Credit,
                        Description = t.DescriptionTran,
                        DocumentTypeId = document.DocumentTypeId,
                       
                    };

                    newTransactions.Add(newTransaction);
                }

                // افزودن تراکنش‌های جدید
                await _context.Transactions.AddRangeAsync(newTransactions);
                _context.TransactionDocuments.Update(document);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "سند با موفقیت ذخیره شد.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطایی در ذخیره سند رخ داد: {ex.Message}";
                await PopulateModelLists(model);
                return View("Details", model);
            }
        }



        // متد کمکی برای پر کردن لیست‌های ViewModel
        private async Task PopulateModelLists(TransactionDocumentDetailsViewModel model)
        {
            model.DocumentTypes = await _context.DocumentTypes
                .Select(dt => new SelectListItem
                {
                    Value = dt.Id.ToString(),
                    Text = dt.Name
                }).ToListAsync();

            model.MainAccounts = await _context.Accounts
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Code} - {a.Name}"
                }).ToListAsync();

            model.SubAccounts = await _context.Moeins
                .GroupBy(m => m.AccountId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = $"{m.Code} - {m.Name}"
                    }).ToList()
                );

            model.DetailAccounts1 = await _context.Tafzils
                .GroupBy(t => t.MoeinId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = $"{t.Code} - {t.Name}"
                    }).ToList()
                );

            model.DetailAccounts2 = await _context.SecondTafzils
                .GroupBy(st => st.TafzilId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(st => new SelectListItem
                    {
                        Value = st.Id.ToString(),
                        Text = $"{st.Code} - {st.Name}"
                    }).ToList()
                );
        }






        private DateTime ConvertPersianToGregorian(string persianDate)
        {
            // فقط بخش تاریخ را جدا کن (اگر ساعت هم باشد)
            string datePart = persianDate.Split(' ')[0];

            var parts = datePart.Split('/');
            if (parts.Length != 3)
                throw new FormatException("فرمت تاریخ نامعتبر است.");

            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            var pc = new PersianCalendar();
            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }







        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Calculate new document number
                var maxDocumentNumber = (await _context.TransactionDocuments
                    .Select(d => d.DocumentNumber)
                    .ToListAsync())
                    .Select(d => int.TryParse(d, out int number) ? number : 0)
                    .DefaultIfEmpty(0)
                    .Max();
                var nextDocumentNumber = (maxDocumentNumber + 1).ToString();

                // Load main accounts
                var mainAccounts = await _context.Accounts
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.Code} - {a.Name}"
                    }).ToListAsync();

                // Load sub accounts grouped by main account ID
                var subAccounts = _context.Moeins
                    .GroupBy(m => m.AccountId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(m => new SelectListItem
                        {
                            Value = m.Id.ToString(),
                            Text = $"{m.Code} - {m.Name}"
                        }).ToList()
                    );


                // Load detail accounts 1 grouped by sub account ID
                var detailAccounts1 = _context.Tafzils
                    .GroupBy(t => t.MoeinId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(t => new SelectListItem
                        {
                            Value = t.Id.ToString(),
                            Text = $"{t.Code} - {t.Name}"
                        }).ToList()
                    );


                // Load detail accounts 2 grouped by detail account 1 ID
                var detailAccounts2 = _context.SecondTafzils
                    .GroupBy(st => st.TafzilId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(st => new SelectListItem
                        {
                            Value = st.Id.ToString(),
                            Text = $"{st.Code} - {st.Name}"
                        }).ToList()
                    );


                // Prepare the view model
                var model = new TransactionDocumentDetailsViewModel
                {
                    DocumentTypes = await _context.DocumentTypes.Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    }).ToListAsync(),
                    DocumentDate = DateTime.Today,
                    DocumentNumber = nextDocumentNumber,
                    Transactions = new List<TransactionDetailsViewModel> { new TransactionDetailsViewModel() },
                    MainAccounts = mainAccounts,
                    SubAccounts = subAccounts,
                    DetailAccounts1 = detailAccounts1,
                    DetailAccounts2 = detailAccounts2
                };
                Console.WriteLine($"SubAccounts keys: {string.Join(", ", subAccounts.Keys)}");
                Console.WriteLine($"DetailAccounts1 keys: {string.Join(", ", detailAccounts1.Keys)}");
                Console.WriteLine($"DetailAccounts2 keys: {string.Join(", ", detailAccounts2.Keys)}");
                return View(model);
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = $"خطایی در بارگذاری فرم ایجاد سند رخ داد: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionDocumentDetailsViewModel model)
        {
            // تابع کمکی برای پر کردن داده‌های مدل
            async Task PopulateModelLists()
            {
                model.MainAccounts = await _context.Accounts
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.Code} - {a.Name}"
                    }).ToListAsync();

                model.SubAccounts = await _context.Moeins
                    .GroupBy(m => m.AccountId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(m => new SelectListItem
                        {
                            Value = m.Id.ToString(),
                            Text = $"{m.Code} - {m.Name}"
                        }).ToList()
                    );

                model.DetailAccounts1 = await _context.Tafzils
                    .GroupBy(t => t.MoeinId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(t => new SelectListItem
                        {
                            Value = t.Id.ToString(),
                            Text = $"{t.Code} - {t.Name}"
                        }).ToList()
                    );

                model.DetailAccounts2 = await _context.SecondTafzils
                    .GroupBy(st => st.TafzilId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(st => new SelectListItem
                        {
                            Value = st.Id.ToString(),
                            Text = $"{st.Code} - {st.Name}"
                        }).ToList()
                    );

                model.DocumentTypes = await _context.DocumentTypes
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    }).ToListAsync();
            }

            // اعتبارسنجی فیلدهای موردنیاز
            bool hasError = false;
            var errorMessages = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.PersianDocumentDate))
            {
                try
                {
                    model.DocumentDate = ConvertPersianToGregorian(model.PersianDocumentDate);
                }
                catch
                {
                    errorMessages.Add("تاریخ وارد شده معتبر نیست."); // خطا اینجاست
                    hasError = true;
                }
            }







            if (!model.SelectedDocumentTypeId.HasValue)
            {
                errorMessages.Add("لطفاً نوع سند را وارد کنید.");
                hasError = true;
            }
            if (string.IsNullOrWhiteSpace(model.Description))
            {
                errorMessages.Add("لطفاً شرح سند را وارد کنید.");
                hasError = true;
            }
            if (model.DocumentDate == default)
            {
                errorMessages.Add("لطفاً تاریخ سند را وارد کنید.");
                hasError = true;
            }

            // اعتبارسنجی تراکنش‌ها
            if (model.Transactions == null || model.Transactions.Count == 0)
            {
                errorMessages.Add("لطفاً حداقل یک تراکنش وارد کنید.");
                hasError = true;
            }
            else
            {
                //if (model.Transactions.Any(t => string.IsNullOrWhiteSpace(t.DescriptionTran)))
                //{
                //    errorMessages.Add("لطفاً شرح برای همه تراکنش‌ها وارد کنید.");
                //    hasError = true;
                //}
                if (model.Transactions.Any(t => !t.MainAccountId.HasValue || !t.SubAccountId.HasValue))
                {
                    errorMessages.Add("لطفاً حساب کل و معین را برای همه تراکنش‌ها انتخاب کنید.");
                    hasError = true;
                }
            }

            if (hasError)
            {
                TempData["ErrorMessage"] = string.Join("؛ ", errorMessages);
                await PopulateModelLists();
                return View(model);
            }

            // محاسبه مجموع بدهکار و بستانکار
            decimal totalDebit = model.Transactions.Sum(t => t.Debit);
            decimal totalCredit = model.Transactions.Sum(t => t.Credit);
            model.TotalAmount = totalDebit;

            if (totalDebit != totalCredit)
            {
                TempData["ErrorMessage"] = "مجموع بدهکاری و بستانکاری باید برابر باشند.";
                await PopulateModelLists();
                return View(model);
            }

            try
            {
                // استفاده از تراکنش دیتابیس
                using var transaction = await _context.Database.BeginTransactionAsync();

                // محاسبه شماره سند جدید
                var maxDocumentNumber = (await _context.TransactionDocuments
                    .Select(d => d.DocumentNumber)
                    .ToListAsync())
                    .Select(d => int.TryParse(d, out int number) ? number : 0)
                    .DefaultIfEmpty(0)
                    .Max();
                var documentNumber = (maxDocumentNumber + 1).ToString();

                // بررسی یکتایی شماره سند
                var existingDocument = await _context.TransactionDocuments
                    .FirstOrDefaultAsync(d => d.DocumentNumber == documentNumber);
                if (existingDocument != null)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "شماره سند قبلاً استفاده شده است. لطفاً دوباره تلاش کنید.";
                    model.DocumentNumber = documentNumber;
                    await PopulateModelLists();
                    return View(model);
                }

                var transactionDocument = new TransactionDocument
                {
                    DocumentNumber = documentNumber,
                    DocumentDate = model.DocumentDate,
                    Description = model.Description,
                    DocumentTypeId = model.SelectedDocumentTypeId.GetValueOrDefault(),
                    ModifiedBy = "سیستم",
                    Status = Status.AwaitingApproval,
                    
                    Transactions = new List<Transaction>()
                };

                foreach (var trans in model.Transactions)
                {
                    var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == trans.MainAccountId);
                    var moein = await _context.Moeins.FirstOrDefaultAsync(m => m.Id == trans.SubAccountId);

                    if (account == null || moein == null)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "حساب کل یا معین نامعتبر است.";
                        model.DocumentNumber = documentNumber;
                        await PopulateModelLists();
                        return View(model);
                    }

                    var tafzilId = trans.DetailAccount1Id.HasValue
                        ? (await _context.Tafzils.FirstOrDefaultAsync(t => t.Id == trans.DetailAccount1Id))?.Id
                        : null;

                    var secondTafzilId = trans.DetailAccount2Id.HasValue
                        ? (await _context.SecondTafzils.FirstOrDefaultAsync(st => st.Id == trans.DetailAccount2Id))?.Id
                        : null;

                    var newTransaction = new Transaction
                    {
                       
                        AccountId = account.Id,
                        MoeinId = moein.Id,
                        TafzilId = tafzilId,
                        SecondTafzilId = secondTafzilId,
                        Debit = trans.Debit,
                        Credit = trans.Credit,
                        DocumentDate = model.DocumentDate,
                        Description = trans.DescriptionTran,
                        DocumentTypeId = model.SelectedDocumentTypeId.GetValueOrDefault(),
                        
                       
                    };

                    transactionDocument.Transactions.Add(newTransaction);
                }

                await _context.TransactionDocuments.AddAsync(transactionDocument);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "سند با موفقیت ذخیره شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطایی در ذخیره سند رخ داد: {ex.Message}";
                await PopulateModelLists();
                return View(model);
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetSubAccounts(int mainAccountId)
        {
            try
            {
                Console.WriteLine($"GetSubAccounts called with mainAccountId: {mainAccountId}");
                var subAccounts = await _context.Moeins
                    .Where(m => m.AccountId == mainAccountId)
                    .Select(m => new { Id = m.Id, Text = $"{m.Code} - {m.Name}" })
                    .ToListAsync();
                Console.WriteLine($"GetSubAccounts result count: {subAccounts.Count}, Data: {string.Join(", ", subAccounts.Select(s => s.Text))}");

                return Json(new { success = true, data = subAccounts });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSubAccounts: {ex.Message}");
                return Json(new { success = false, message = $"خطا در بارگذاری حساب‌های معین: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDetailAccounts1(int subAccountId)
        {
            try
            {
                Console.WriteLine($"GetDetailAccounts1 called with subAccountId: {subAccountId}");
                var detailAccounts1 = await _context.Tafzils
                    .Where(t => t.MoeinId == subAccountId)
                    .Select(t => new { Id = t.Id, Text = $"{t.Code} - {t.Name}" })
                    .ToListAsync();
                Console.WriteLine($"GetDetailAccounts1 result count: {detailAccounts1.Count}, Data: {string.Join(", ", detailAccounts1.Select(s => s.Text))}");

                return Json(new { success = true, data = detailAccounts1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDetailAccounts1: {ex.Message}");
                return Json(new { success = false, message = $"خطا در بارگذاری تفصیل‌های ۱: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDetailAccounts2(int detailAccount1Id)
        {
            try
            {
                Console.WriteLine($"GetDetailAccounts2 called with detailAccount1Id: {detailAccount1Id}");
                var detailAccounts2 = await _context.SecondTafzils
                    .Where(st => st.TafzilId == detailAccount1Id)
                    .Select(st => new { Id = st.Id, Text = $"{st.Code} - {st.Name}" })
                    .ToListAsync();
                Console.WriteLine($"GetDetailAccounts2 result count: {detailAccounts2.Count}, Data: {string.Join(", ", detailAccounts2.Select(s => s.Text))}");

                return Json(new { success = true, data = detailAccounts2 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDetailAccounts2: {ex.Message}");
                return Json(new { success = false, message = $"خطا در بارگذاری تفصیل‌های ۲: {ex.Message}" });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Find the transaction document by ID, including related transactions
            var transactionDocument = await _context.TransactionDocuments
                .Include(td => td.Transactions)
                .FirstOrDefaultAsync(td => td.Id == id);

            // If the document is not found, show an error message
            if (transactionDocument == null)
            {
                TempData["ErrorMessage"] = "سند مورد نظر یافت نشد.";
                return RedirectToAction("Index");
            }

            // Remove related transactions first
            if (transactionDocument.Transactions != null && transactionDocument.Transactions.Any())
            {
                _context.Transactions.RemoveRange(transactionDocument.Transactions);
            }

            // Remove the transaction document
            _context.TransactionDocuments.Remove(transactionDocument);

            // Save changes to the database
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "سند با موفقیت حذف شد.";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public IActionResult SearchTafzil2(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Json(new object[0]);

            var results = _context.SecondTafzils
                .Where(x => x.Code.Contains(searchTerm) || x.Name.Contains(searchTerm))
                .Select(x => new {
                    kolCode = x.Tafzil.Moein.Account.Code,
                    kolName = x.Tafzil.Moein.Account.Name,
                    moienCode = x.Tafzil.Moein.Code,
                    moienName = x.Tafzil.Moein.Name,
                    tafzil1Code = x.Tafzil.Code,
                    tafzil1Name = x.Tafzil.Name,
                    tafzil2Code = x.Code,
                    tafzil2Name = x.Name,
                    tafzil2Id = x.Id
                })
                .ToList();

            return Json(results);
        }






    }
}
