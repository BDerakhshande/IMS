using ClosedXML.Parser;
using IMS.Areas.AccountManagement.Data;
using IMS.Areas.AccountManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.AccountManagement.Controllers
{
    [Area("AccountManagement")]
    public class ChartOfAccountsController : Controller
    {
        private readonly AccountManagementDbContext _context;

        public ChartOfAccountsController(AccountManagementDbContext context)
        {
            _context = context;
        }

        // نمایش حساب‌های کل
        public IActionResult Index()
        {
            var accounts = _context.Accounts
                .OrderBy(a => a.Code) // مرتب سازی بر اساس کد حساب
                .ToList();

            return View(accounts);
        }

        public IActionResult Moeins(int accountId)
        {
            var account = _context.Accounts
                                  .Where(a => a.Id == accountId)
                                  .FirstOrDefault();

            if (account == null)
            {
                return NotFound();
            }

            var moeins = _context.Moeins
                                 .Include(m => m.Account)
                                 .Where(m => m.AccountId == accountId)
                                 .ToList();

            ViewBag.AccountName = account.Name;

            return View(moeins);
        }


        public IActionResult Tafzils(int moeinId)
        {
            var tafzils = _context.Tafzils
                .Where(t => t.MoeinId == moeinId)
                .Include(t => t.Moein)
                .ThenInclude(m => m.Account)
                .ToList();

            ViewBag.Moeins = _context.Moeins.ToList();

            return View(tafzils);
        }


        public IActionResult SecondTafzils(int tafzilId)
        {
            var secondTafzils = _context.SecondTafzils
                .Where(st => st.TafzilId == tafzilId)
                .Include(st => st.Tafzil)
                    .ThenInclude(t => t.Moein)
                        .ThenInclude(m => m.Account)
                .ToList();

            ViewBag.Tafzils = _context.Tafzils
                .Include(t => t.Moein)
                    .ThenInclude(m => m.Account)
                .ToList();

            return View(secondTafzils);
        }




        [HttpGet]
        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAccount(Account account)
        {
            try
            {
                // بررسی تکراری بودن کد حساب
                bool isDuplicateCode = _context.Accounts.Any(a => a.Code == account.Code);

                if (isDuplicateCode)
                {
                    ViewBag.ErrorMessage = "کدی که وارد کرده‌اید قبلاً ثبت شده است. لطفاً کد دیگری انتخاب کنید.";
                    return View(account);
                }

                // اضافه کردن حساب کل به پایگاه داده
                _context.Accounts.Add(account);
                _context.SaveChanges();

                // هدایت به صفحه اصلی
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // در صورت بروز خطای دیگر، پیام خطا نمایش داده می‌شود
                ViewBag.ErrorMessage = "خطا در ذخیره اطلاعات. لطفاً دوباره تلاش کنید.";
                return View(account);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var account = _context.Accounts.Find(id);
            if (account == null)
            {
                return NotFound();
            }

            // حذف حساب از پایگاه داده
            _context.Accounts.Remove(account);
            _context.SaveChanges();

            // هدایت به صفحه اصلی (لیست حساب‌ها)
            return RedirectToAction(nameof(Index));
        }




        [HttpGet]
        public IActionResult EditAccount(int id)
        {
            var account = _context.Accounts.Find(id);

            return View(account); // ارسال مدل به ویو برای ویرایش
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAccount(Account account)
        {
            try
            {
                // پیدا کردن حساب موجود بر اساس Id
                var existingAccount = _context.Accounts.FirstOrDefault(a => a.Id == account.Id);

                if (existingAccount == null)
                {
                    return NotFound(); // اگر حساب پیدا نشد
                }

                // به‌روزرسانی اطلاعات حساب
                existingAccount.Code = account.Code;
                existingAccount.Name = account.Name;

                _context.SaveChanges(); // ذخیره تغییرات

                return RedirectToAction(nameof(Index)); // هدایت به صفحه اصلی
            }
            catch (Exception ex)
            {
                // اگر خطایی رخ داد، پیام خطا به ویو ارسال می‌شود
                ViewBag.ErrorMessage = "خطا در ذخیره اطلاعات. لطفاً دوباره تلاش کنید.";
                return View(account); // بازگشت به فرم ویرایش با همان داده‌ها
            }
        }




        public IActionResult CreateMoeins(int accountId)
        {
            ViewBag.Accounts = _context.Accounts.ToList();  // ارسال لیست حساب‌ها به ویو

            var model = new Moein { AccountId = accountId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateMoeins(Moein moein)
        {
            // بررسی تکراری بودن کد معین
            bool isDuplicateCode = _context.Moeins.Any(m => m.Code == moein.Code);

            if (isDuplicateCode)
            {
                // اضافه کردن پیام خطا به ویو
                TempData["ErrorMessage"] = "کدی مشابه برای حساب معین قبلاً ثبت شده است.";
                ViewBag.Accounts = _context.Accounts.ToList();
                return View(moein);
            }

            // بررسی انتخاب حساب کل
            if (moein.AccountId == 0)
            {
                // اضافه کردن پیام خطا به ویو
                TempData["ErrorMessage"] = "لطفاً یک حساب کل انتخاب کنید.";
                ViewBag.Accounts = _context.Accounts.ToList();
                return View(moein);
            }

            // پیدا کردن حساب کل مربوط به معین
            var account = _context.Accounts.FirstOrDefault(a => a.Id == moein.AccountId);
            if (account == null)
            {
                // اضافه کردن پیام خطا به ویو
                TempData["ErrorMessage"] = "حساب کل انتخاب‌شده یافت نشد.";
                ViewBag.Accounts = _context.Accounts.ToList();
                return View(moein);
            }

            // اضافه کردن معین به دیتابیس و حساب کل
            _context.Moeins.Add(moein);
            account.Moeins.Add(moein);
            _context.SaveChanges();

            // هدایت به صفحه لیست حساب‌ها
            TempData["SuccessMessage"] = "معین با موفقیت ایجاد شد.";
            return RedirectToAction("Index", "ChartOfAccounts");
        }


        // اکشن برای نمایش صفحه ویرایش معین
        public IActionResult EditMoeins(int id)
        {
            var moein = _context.Moeins
                .Include(m => m.Account)  // بارگذاری اطلاعات حساب معین
                .FirstOrDefault(m => m.Id == id);

            if (moein == null)
            {
                return NotFound(); // در صورتی که معین پیدا نشد
            }

            ViewBag.Accounts = _context.Accounts.ToList(); // ارسال لیست حساب‌ها به ویو برای انتخاب حساب
            return View(moein); // ارسال مدل Moein به ویو
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditMoeins(Moein moein)
        {
            var existingMoein = _context.Moeins
                .FirstOrDefault(m => m.Id == moein.Id);

            if (existingMoein == null)
            {
                return NotFound(); // اگر معین پیدا نشد
            }

            try
            {
                // بررسی اینکه آیا حساب کل تغییر کرده؟
                if (existingMoein.AccountId != moein.AccountId)
                {
                    // اگر حساب کل تغییر کرده، باید کد جدید بدهیم
                    // پیدا کردن تمام معین‌های این حساب کل جدید
                    var moeinsOfNewAccount = _context.Moeins
                        .Where(m => m.AccountId == moein.AccountId)
                        .OrderBy(m => m.Code)
                        .ToList();

                    // اگر معین‌های دیگری وجود دارند، کد جدید یکی بیشتر از آخرین کد باشد
                    if (moeinsOfNewAccount.Any())
                    {
                        var lastMoein = moeinsOfNewAccount.Last();
                        int lastCode = int.Parse(lastMoein.Code);
                        existingMoein.Code = (lastCode + 1).ToString();
                    }
                    else
                    {
                        // اگر این اولین معین برای این حساب کل است
                        existingMoein.Code = "1";
                    }

                    // تغییر حساب کل
                    existingMoein.AccountId = moein.AccountId;
                }

                // به‌روزرسانی نام معین
                existingMoein.Name = moein.Name;

                _context.SaveChanges(); // ذخیره تغییرات

                // مرتب کردن معین‌ها بر اساس کد
                var allMoeins = _context.Moeins.OrderBy(m => m.Code).ToList();

                return RedirectToAction("Moeins", "ChartOfAccounts");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در ذخیره اطلاعات. لطفاً دوباره تلاش کنید.";
                ViewBag.Accounts = _context.Accounts.ToList(); // لیست حساب‌ها برای پر کردن در ویو
                return View(moein);
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMoeins(int id)
        {
            var moein = _context.Moeins.FirstOrDefault(m => m.Id == id);
            if (moein == null)
            {
                TempData["ErrorMessage"] = "حساب معین مورد نظر یافت نشد.";
                return RedirectToAction("Index", "ChartOfAccounts");
            }

            int accountId = moein.AccountId; // استخراج accountId قبل از حذف

            try
            {
                _context.Moeins.Remove(moein);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "حساب معین با موفقیت حذف شد.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در حذف حساب معین: " + ex.Message;
            }

            return RedirectToAction("Moeins", "ChartOfAccounts", new { accountId = accountId });
        }



        public IActionResult CreateTafzil1(int? accountId)
        {
            // ارسال اطلاعات حساب معین برای نمایش در ویو
            if (accountId == null)
            {
                TempData["ErrorMessage"] = "شناسه حساب کل معتبر نیست.";
                return RedirectToAction("Index", "ChartOfAccounts");
            }

            // ارسال لیست حساب‌های معین مرتبط با حساب کل به ویو
            ViewBag.AccountId = accountId;
            var moeins = _context.Moeins.Where(m => m.AccountId == accountId).ToList();
            ViewBag.Moeins = moeins;

            var model = new Tafzil { MoeinId = moeins.FirstOrDefault()?.Id ?? 0 };  // تنظیم مدل برای تفصیل جدید
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTafzil1(Tafzil tafzil)
        {
            // بررسی تکراری بودن کد تفصیل
            bool isDuplicateCode = _context.Tafzils.Any(t => t.Code == tafzil.Code);

            if (isDuplicateCode)
            {
                // اضافه کردن پیام خطا به ویو
                TempData["ErrorMessage"] = "کد تفصیل وارد شده قبلاً ثبت شده است.";

                // بررسی موجود بودن Moein و سپس دسترسی به AccountId
                if (tafzil.Moein != null)
                {
                    ViewBag.AccountId = tafzil.Moein.AccountId;
                    var moeins = _context.Moeins.Where(m => m.AccountId == tafzil.Moein.AccountId).ToList();
                    ViewBag.Moeins = moeins;
                }
                else
                {
                    ViewBag.AccountId = null;
                    ViewBag.Moeins = new List<Moein>();  // در صورت نبودن Moein، لیستی خالی ارسال می‌شود
                }

                return View(tafzil);  // بازگشت به ویو با مدل وارد شده
            }


            // پیدا کردن حساب معین مربوط به تفصیل
            var moein = _context.Moeins.FirstOrDefault(m => m.Id == tafzil.MoeinId);

            if (moein == null)
            {
                TempData["ErrorMessage"] = "حساب معین انتخاب‌شده یافت نشد.";
                return RedirectToAction("Index", "ChartOfAccounts");
            }

            // اضافه کردن تفصیل به دیتابیس
            _context.Tafzils.Add(tafzil);
            moein.Tafzils.Add(tafzil);
            _context.SaveChanges();

            // هدایت به صفحه لیست تفصیل‌ها
            TempData["SuccessMessage"] = "تفصیل با موفقیت ایجاد شد.";
            return RedirectToAction("Index", "ChartOfAccounts");
        }


        // در کنترلر مربوطه
        public IActionResult EditTafzil1(int id)
        {
            var tafzil = _context.Tafzils.FirstOrDefault(t => t.Id == id);
            if (tafzil == null)
            {
                return NotFound();
            }

            // بارگذاری لیست حساب‌های معین
            ViewBag.Moeins = _context.Moeins.ToList();

            return View(tafzil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTafzil1(int id, IMS.Areas.AccountManagement.Models.Tafzil model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var existingTafzil = _context.Tafzils.FirstOrDefault(t => t.Id == id);
            if (existingTafzil == null)
            {
                return NotFound();
            }

            try
            {
                // اگر Moein تغییر کرده باشد، باید کد جدید تولید کنیم
                if (existingTafzil.MoeinId != model.MoeinId)
                {
                    var tafzilsOfNewMoein = _context.Tafzils
                        .Where(t => t.MoeinId == model.MoeinId)
                        .OrderBy(t => t.Code)
                        .ToList();

                    if (tafzilsOfNewMoein.Any())
                    {
                        var lastTafzil = tafzilsOfNewMoein.Last();
                        int lastCode = int.Parse(lastTafzil.Code);
                        existingTafzil.Code = (lastCode + 1).ToString();
                    }
                    else
                    {
                        existingTafzil.Code = "1"; // اولین تفصیل برای این معین
                    }

                    existingTafzil.MoeinId = model.MoeinId;
                }

                // به‌روزرسانی نام تفصیل
                existingTafzil.Name = model.Name;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "تفصیل با موفقیت ویرایش شد.";
                return RedirectToAction("Tafzils", "ChartOfAccounts", new { moeinId = existingTafzil.MoeinId });
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "خطا در ذخیره اطلاعات. لطفاً دوباره تلاش کنید.";
                ViewBag.Moeins = _context.Moeins.ToList();
                return View(model);
            }
        }


        // GET: EditSecondTafzil
        public IActionResult EditSecondTafzil(int id)
        {
            var secondTafzil = _context.SecondTafzils.FirstOrDefault(st => st.Id == id);
            if (secondTafzil == null)
            {
                return NotFound();
            }

            ViewBag.Tafzils = _context.Tafzils.ToList();
            ViewBag.CostCenters = _context.CostCenters.ToList(); // اضافه کردن لیست مراکز هزینه

            return View(secondTafzil);
        }

        // POST: EditSecondTafzil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSecondTafzil(int id, IMS.Areas.AccountManagement.Models.SecondTafzil model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var existingSecondTafzil = _context.SecondTafzils.FirstOrDefault(st => st.Id == id);
            if (existingSecondTafzil == null)
            {
                return NotFound();
            }

            try
            {
                // اگر TafzilId تغییر کرده باشد، کد جدید تولید شود
                if (existingSecondTafzil.TafzilId != model.TafzilId)
                {
                    var secondTafzilsOfNewTafzil = _context.SecondTafzils
                        .Where(st => st.TafzilId == model.TafzilId)
                        .OrderBy(st => st.Code)
                        .ToList();

                    if (secondTafzilsOfNewTafzil.Any())
                    {
                        var lastSecondTafzil = secondTafzilsOfNewTafzil.Last();
                        int lastCode = int.Parse(lastSecondTafzil.Code);
                        existingSecondTafzil.Code = (lastCode + 1).ToString();
                    }
                    else
                    {
                        existingSecondTafzil.Code = "1";
                    }

                    existingSecondTafzil.TafzilId = model.TafzilId;
                }

                // به‌روزرسانی فیلدها
                existingSecondTafzil.Name = model.Name;
                existingSecondTafzil.CostCenterId = model.CostCenterId;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "تفصیل ۲ با موفقیت ویرایش شد.";
                return RedirectToAction("SecondTafzils", "ChartOfAccounts", new { tafzilId = existingSecondTafzil.TafzilId });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "خطا در ذخیره اطلاعات. لطفاً دوباره تلاش کنید.";
                ViewBag.Tafzils = _context.Tafzils.ToList();
                ViewBag.CostCenters = _context.CostCenters.ToList(); // بارگذاری مجدد لیست مراکز هزینه
                return View(model);
            }
        }


        // GET: CreateSecondTafzil
        public IActionResult CreateSecondTafzil()
        {
            var tafzils = _context.Tafzils.ToList();
            var costCenters = _context.CostCenters.ToList();

            if (!tafzils.Any())
            {
                TempData["ErrorMessage"] = "هیچ حساب تفصیل ۱ ثبت نشده است.";
                return RedirectToAction("Index", "ChartOfAccounts");
            }



            ViewBag.Tafzils = tafzils;
            ViewBag.CostCenters = costCenters;

            return View(new SecondTafzil());
        }


        // POST: CreateSecondTafzil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSecondTafzil(SecondTafzil secondTafzil)
        {
            // بررسی اعتبار اطلاعات وارد شده (کد و نام)
            if (string.IsNullOrEmpty(secondTafzil.Code) || secondTafzil.Code.Length > 50)
            {
                TempData["ErrorMessage"] = "کد حساب تفصیل ۲ معتبر نیست.";
                ViewBag.Tafzils = _context.Tafzils.ToList();
                ViewBag.CostCenters = _context.CostCenters.ToList();
                return View(secondTafzil);
            }

            if (string.IsNullOrEmpty(secondTafzil.Name) || secondTafzil.Name.Length > 200)
            {
                TempData["ErrorMessage"] = "نام حساب تفصیل ۲ معتبر نیست.";
                ViewBag.Tafzils = _context.Tafzils.ToList();
                ViewBag.CostCenters = _context.CostCenters.ToList();
                return View(secondTafzil);
            }

            // بررسی تکراری بودن کد تفصیل ۲
            bool isDuplicateCode = _context.SecondTafzils
                                          .Any(s => s.Code == secondTafzil.Code);
            if (isDuplicateCode)
            {
                TempData["ErrorMessage"] = "کد تفصیل ۲ وارد شده قبلاً ثبت شده است.";
                ViewBag.Tafzils = _context.Tafzils.ToList();
                ViewBag.CostCenters = _context.CostCenters.ToList();
                return View(secondTafzil);
            }

            // بررسی معتبر بودن تفصیل ۱
            var tafzil = _context.Tafzils.FirstOrDefault(t => t.Id == secondTafzil.TafzilId);
            if (tafzil == null)
            {
                TempData["ErrorMessage"] = "حساب تفصیل ۱ انتخاب‌شده یافت نشد.";
                ViewBag.Tafzils = _context.Tafzils.ToList();
                ViewBag.CostCenters = _context.CostCenters.ToList();
                return View(secondTafzil);
            }

            //// بررسی معتبر بودن مرکز هزینه
            //var costCenter = _context.CostCenters.FirstOrDefault(c => c.Id == secondTafzil.CostCenterId);
            //if (costCenter == null)
            //{
            //    TempData["ErrorMessage"] = "مرکز هزینه انتخاب‌شده یافت نشد.";
            //    ViewBag.Tafzils = _context.Tafzils.ToList();
            //    ViewBag.CostCenters = _context.CostCenters.ToList();
            //    return View(secondTafzil);
            //}

            // ذخیره تفصیل ۲
            _context.SecondTafzils.Add(secondTafzil);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "تفصیل ۲ با موفقیت ایجاد شد.";
            return RedirectToAction("Index", "ChartOfAccounts");
        }
        [HttpGet]
        public IActionResult SearchSecondTafzil(string term)
        {
            var results = _context.SecondTafzils
                .Include(st => st.Tafzil)
                    .ThenInclude(t => t.Moein)
                        .ThenInclude(m => m.Account)
                .Where(st => st.Name.Contains(term) || st.Code.Contains(term))
                .Select(st => new
                {
                    AccountCode = st.Tafzil.Moein.Account.Code,
                    AccountName = st.Tafzil.Moein.Account.Name,
                    MoeinCode = st.Tafzil.Moein.Code,
                    MoeinName = st.Tafzil.Moein.Name,
                    TafzilCode = st.Tafzil.Code,
                    TafzilName = st.Tafzil.Name,
                    SecondTafzilCode = st.Code,
                    SecondTafzilName = st.Name
                })
                .ToList();

            return Json(results);
        }




        public IActionResult AccountsByTafzil(int tafzilId)
        {
            var tafzil = _context.Tafzils.Include(t => t.Moein).ThenInclude(m => m.Account)
                            .FirstOrDefault(t => t.Id == tafzilId);
            return View(tafzil);
        }
    }
}
