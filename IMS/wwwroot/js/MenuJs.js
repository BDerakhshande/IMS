document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('toggleBtn');
    const sidebar = document.getElementById('sidebar');

    // مطمئن شو کلاس "expanded" در ابتدا وجود ندارد
    sidebar.classList.remove('expanded');

    // مدیریت باز و بسته کردن سایدبار
    toggleBtn.addEventListener('click', function () {
        sidebar.classList.toggle('expanded');
    });

    // مدیریت زیرمنوها
    const menuItems = document.querySelectorAll('.has-submenu');
    menuItems.forEach(item => {
        item.addEventListener('click', function () {
            const submenu = this.nextElementSibling;
            if (submenu && submenu.classList.contains('submenu')) {
                submenu.classList.toggle('open');
                const toggleIcon = this.querySelector('.submenu-toggle');
                if (toggleIcon) toggleIcon.classList.toggle('active');
            }
        });
    });
});
