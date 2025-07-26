$(document).ready(function () {
    // استفاده از DataTables برای جدول
    var table = $('#tafzilTable').DataTable({
        paging: true, // فعال‌سازی صفحه‌بندی
        searching: true, // فعال‌سازی جستجو
        ordering: true, // فعال‌سازی مرتب‌سازی
        pageLength: 10, // تعداد رکوردهای نمایش داده شده در هر صفحه
        lengthMenu: [5, 10, 25, 50, 100], // منوی انتخاب تعداد رکورد
        info: true, // نمایش اطلاعات صفحه
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.6/i18n/fa.json" // زبان فارسی
        },
        columnDefs: [
            { targets: [0, 1], orderable: true }, // فعال‌سازی مرتب‌سازی برای ستون‌های ۰ و ۱
        ]
    });

    // افزودن تغییرات برای برجسته کردن سطر
    $('#tafzilTable tbody').on('mouseenter', 'tr', function () {
        $(this).css('background-color', '#e0f7fa'); // رنگ برجسته شدن سطر
    }).on('mouseleave', 'tr', function () {
        $(this).css('background-color', ''); // بازگشت به رنگ اصلی
    });
});
document.addEventListener("DOMContentLoaded", function () {
    const table = document.getElementById("tafzilTable");
    const rows = table.querySelectorAll("tbody tr");
    let selectedIndex = -1;

    // مشخص کردن سطر انتخاب شده با کلاس CSS مثلا 'selected'
    function highlightRow(index) {
        rows.forEach((row, i) => {
            row.classList.toggle("selected", i === index);
        });
    }

    document.addEventListener("keydown", function (event) {
        const key = event.key;

        // ArrowDown → سطر بعدی
        if (key === "ArrowDown") {
            if (selectedIndex < rows.length - 1) {
                selectedIndex++;
                highlightRow(selectedIndex);
                rows[selectedIndex].scrollIntoView({ block: "center", behavior: "smooth" });
                event.preventDefault();
            }
        }

        // ArrowUp → سطر قبلی
        if (key === "ArrowUp") {
            if (selectedIndex > 0) {
                selectedIndex--;
                highlightRow(selectedIndex);
                rows[selectedIndex].scrollIntoView({ block: "center", behavior: "smooth" });
                event.preventDefault();
            }
        }

        // Enter → رفتن به صفحه مرتبط با سطر انتخاب شده
        if (key === "Enter" && selectedIndex >= 0) {
            const row = rows[selectedIndex];
            if (row) {
                // چون در HTML هر tr روی کلیک رفتن صفحه دارد، کافیست کلیک کنیم
                row.click();
            }
        }

        // Delete → حذف سطر انتخاب شده با تایید کاربر
        if (key === "Delete" && selectedIndex >= 0) {
            const row = rows[selectedIndex];
            const deleteBtn = row.querySelector(".delete-btn");
            if (deleteBtn && confirm("آیا از حذف این تفصیل ۲ مطمئن هستید؟")) {
                window.location.href = deleteBtn.getAttribute("href");
            }
        }

        // Shift + E → ویرایش سطر انتخاب شده
        if (event.shiftKey && key.toLowerCase() === "e" && selectedIndex >= 0) {
            const row = rows[selectedIndex];
            const editBtn = row.querySelector(".edit-btn");
            if (editBtn) {
                window.location.href = editBtn.getAttribute("href");
                event.preventDefault();
            }
        }

        // Alt + Shift + N → افزودن تفصیل جدید
        if (event.altKey && event.shiftKey && key.toLowerCase() === "n") {
            const addBtn = document.querySelector(".btn-gradient-add");
            if (addBtn) {
                addBtn.click();
                event.preventDefault();
            }
        }



        if (key === "Escape") {
            history.back(); // برمی‌گرده به صفحه قبلی مرورگر
        }
    });
});