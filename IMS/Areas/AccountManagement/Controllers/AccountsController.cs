using IMS.Areas.AccountManagement.Models;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IMS.Areas.AccountManagement.Data;
using ClosedXML.Excel;

namespace IMS.Areas.AccountManagement.Controllers
{
    public class AccountsController : Controller
    {
        private readonly AccountManagementDbContext _context;

        public AccountsController(AccountManagementDbContext context)
        {
            _context = context;
        }

      
        // اکشن نمایش فهرست حساب‌ها
        public async Task<IActionResult> Index()
        {
            
                TempData["ErrorMessage"] = "شما دسترسی به این بخش ندارید.";
                return RedirectToAction("Index", "Home"); // هدایت به صفحه اصلی
         

            
        }

        // اکشن برای استخراج فهرست حساب‌ها به فرمت اکسل
        public async Task<IActionResult> ExportToExcel()
        {
          

            // گرفتن لیست حساب‌ها از دیتابیس
            var accounts = await _context.Accounts.ToListAsync();

            // ایجاد فایل اکسل
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Accounts");

                // اضافه کردن هدرها به اکسل
                worksheet.Cell(1, 1).Value = "ردیف";
                worksheet.Cell(1, 2).Value = "نام حساب";
                worksheet.Cell(1, 3).Value = "موجودی اولیه";

                // اضافه کردن داده‌ها به اکسل
                for (int i = 0; i < accounts.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = i + 1;
                    worksheet.Cell(i + 2, 2).Value = accounts[i].Name;

                }

                // تنظیم استایل برای هدر
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

                // ذخیره فایل اکسل در حافظه
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    // بازگشت به کاربر به عنوان فایل دانلود
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Accounts.xlsx");
                }
            }
        }

        // اکشن نمایش فرم ایجاد حساب جدید
        public IActionResult Create()
        {
          
            return View();
        }

        // اکشن ارسال داده‌ها برای ایجاد حساب جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account account)
        {
           

            // بررسی وجود حساب با همان نام
            bool accountExists = await _context.Accounts.AnyAsync(a => a.Name == account.Name);

            if (accountExists)
            {
                ModelState.AddModelError("Name", "حسابی با این نام از قبل وجود دارد.");
                return View(account);
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // اکشن نمایش فرم ویرایش حساب
        public async Task<IActionResult> Edit(int id)
        {
            
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();
            return View(account);
        }

        // اکشن ارسال داده‌ها برای ویرایش حساب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Account account)
        {
           

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // اکشن حذف حساب
        public async Task<IActionResult> Delete(int id)
        {
            

            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
