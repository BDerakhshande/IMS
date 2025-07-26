document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('toggleBtn');
    const sidebar = document.getElementById('sidebar');
    const menuItems = document.querySelectorAll('.has-submenu');

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('expanded');
        });
    }

    menuItems.forEach(item => {
        item.addEventListener('click', function () {
            const submenu = item.nextElementSibling;

            if (!submenu || !submenu.classList.contains('submenu')) return;

            // بستن همه‌ی ساب‌منوهای دیگر
            document.querySelectorAll('.submenu').forEach(sub => {
                if (sub !== submenu) sub.classList.remove('open');
            });

            // باز و بسته کردن این یکی
            submenu.classList.toggle('open');

            // چرخش آیکون فلش
            const icon = item.querySelector('.submenu-toggle');
            if (icon) {
                icon.classList.toggle('fa-chevron-down');
                icon.classList.toggle('fa-chevron-up');
            }

            // وضعیت فعال بودن منو
            menuItems.forEach(mi => mi.classList.remove('active'));
            item.classList.add('active');
        });
    });
});
