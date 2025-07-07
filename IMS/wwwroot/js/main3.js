const modeToggle = document.getElementById('mode-toggle');

// بررسی حالت ذخیره شده و اعمال آن
if (localStorage.getItem('darkMode') === 'true') {
    document.body.classList.add('dark-mode');
}

// تغییر حالت تاریک/روشن و ذخیره در localStorage
modeToggle.addEventListener('click', () => {
    document.body.classList.toggle('dark-mode');
    const isDarkMode = document.body.classList.contains('dark-mode');
    localStorage.setItem('darkMode', isDarkMode);
});

// شبیه‌سازی ورود به زیرسیستم‌ها
document.querySelectorAll('.access-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        const subsystem = this.closest('.subsystem-card').classList[1];
        alert(`در حال انتقال به زیرسیستم ${subsystem}...`);
        // در حالت واقعی اینجا باید به صفحه مربوطه منتقل شود
    });
});