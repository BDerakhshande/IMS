document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    const allMenuItems = Array.from(sidebar.querySelectorAll('.menu-item, .has-submenu, .submenu-item'));

    // همیشه سایدبار باز
    sidebar.classList.add('expanded');

    // همه آیتم‌ها قابل فوکوس
    allMenuItems.forEach(item => item.setAttribute('tabindex', '0'));

    // تابع باز/بسته کردن زیرمنو
    function toggleSubmenu(item) {
        const submenu = item.nextElementSibling;
        if (submenu && submenu.classList.contains('submenu')) {
            submenu.classList.toggle('open');
            const toggleIcon = item.querySelector('.submenu-toggle');
            if (toggleIcon) toggleIcon.classList.toggle('active');
        }
    }

    // ✅ اضافه کردن رویداد کلیک
    allMenuItems.forEach(item => {
        if (item.classList.contains('has-submenu')) {
            item.addEventListener('click', function (e) {
                e.preventDefault(); // اگه آیتم <a href="#"> باشه
                toggleSubmenu(item);
            });
        }
    });

    // ✅ پشتیبانی از کیبورد
    sidebar.addEventListener('keydown', function (e) {
        const active = document.activeElement;
        if (!allMenuItems.includes(active)) return;

        let index = allMenuItems.indexOf(active);

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            let nextIndex = (index + 1) % allMenuItems.length;
            allMenuItems[nextIndex].focus();
        }
        else if (e.key === 'ArrowUp') {
            e.preventDefault();
            let prevIndex = (index - 1 + allMenuItems.length) % allMenuItems.length;
            allMenuItems[prevIndex].focus();
        }
        else if ((e.key === 'Enter' || e.key === ' ' || e.code === 'Space' || e.key === 'ArrowRight') && active.classList.contains('has-submenu')) {
            e.preventDefault();
            toggleSubmenu(active);

            const submenu = active.nextElementSibling;
            if (submenu && submenu.classList.contains('open')) {
                const subItems = submenu.querySelectorAll('.submenu-item');
                if (subItems.length > 0) subItems[0].focus();
            }
        }
        else if ((e.key === 'ArrowLeft' || e.key === 'Escape') && active.classList.contains('submenu-item')) {
            e.preventDefault();
            const parent = active.closest('.submenu').previousElementSibling;
            if (parent && parent.classList.contains('has-submenu')) {
                parent.focus();
            }
        }
    });
});
