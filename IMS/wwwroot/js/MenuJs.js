document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');

    // مطمئن شو کلاس "expanded" در ابتدا وجود ندارد
    sidebar.classList.add('expanded');

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
