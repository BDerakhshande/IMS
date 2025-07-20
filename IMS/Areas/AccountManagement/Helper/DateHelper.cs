using System.Globalization;

namespace IMS.Areas.AccountManagement.Helper
{
    public static class DateHelper
    {
        public static string ConvertToPersianDate(this DateTime? date)
        {
            if (!date.HasValue || date.Value == DateTime.MinValue)
            {
                return "--"; // مقدار پیش‌فرض برای تاریخ‌های null یا نامعتبر
            }

            try
            {
                var persianCalendar = new PersianCalendar();
                int year = persianCalendar.GetYear(date.Value);
                int month = persianCalendar.GetMonth(date.Value);
                int day = persianCalendar.GetDayOfMonth(date.Value);
                return $"{year:0000}/{month:00}/{day:00}";
            }
            catch
            {
                return "--"; // مقدار پیش‌فرض در صورت خطا
            }
        }

        // اضافه کردن نسخه‌ای برای DateTime غیرقابل‌نول برای سازگاری
        public static string ConvertToPersianDate(this DateTime date)
        {
            return ConvertToPersianDate((DateTime?)date);
        }
    }
}
