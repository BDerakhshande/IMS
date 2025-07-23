const toggleBtn = document.getElementById('toggleBtn');
const sidebar = document.getElementById('sidebar');
const logo = document.getElementById('logo');

toggleBtn.addEventListener('click', () => {

    sidebar.classList.toggle('expanded');
    toggleBtn.innerHTML = sidebar.classList.contains('expanded') ? '✕' : '☰';
    logo.style.display = sidebar.classList.contains('expanded') ? 'inline-block' : 'none';

    if (!sidebar.classList.contains('expanded')) {
        document.querySelectorAll('.submenu').forEach(sub => sub.classList.remove('open'));
        document.querySelectorAll('.menu-item.has-submenu').forEach(item => item.classList.remove('open'));
        sidebar.classList.remove('has-open-submenu');
    }
});

document.querySelectorAll('.has-submenu').forEach(menuItem => {
    const submenuToggle = menuItem.querySelector('.submenu-toggle');
    const submenu = menuItem.nextElementSibling;

    submenuToggle.addEventListener('click', (e) => {
        e.stopPropagation();
        if (!sidebar.classList.contains('expanded')) return;

        toggleSubmenu(menuItem, submenu);
    });

    menuItem.addEventListener('click', () => {
        if (!sidebar.classList.contains('expanded')) return;

        toggleSubmenu(menuItem, submenu);
    });
});

function toggleSubmenu(menuItem, submenu) {
    const isOpen = menuItem.classList.contains('open');

    document.querySelectorAll('.has-submenu').forEach(item => {
        if (item !== menuItem) {
            item.classList.remove('open');
            const sub = item.nextElementSibling;
            if (sub && sub.classList.contains('submenu')) {
                sub.classList.remove('open');
            }
        }
    });

    if (!isOpen) {
        menuItem.classList.add('open');
        submenu.classList.add('open');
        sidebar.classList.add('has-open-submenu');
    } else {
        menuItem.classList.remove('open');
        submenu.classList.remove('open');
        sidebar.classList.remove('has-open-submenu');
    }
}
