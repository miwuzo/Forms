window.themeHelper = {
    setTheme: function (isDark) {
        localStorage.setItem('theme', isDark ? 'dark' : 'light');
        this.applyTheme(isDark);
    },
    getTheme: function () {
        return localStorage.getItem('theme') === 'dark';
    },
    applyTheme: function (isDark) {
        if (isDark) {
            document.body.classList.add('bg-dark');
            document.documentElement.classList.add('bg-dark');
        } else {
            document.body.classList.remove('bg-dark');
            document.documentElement.classList.remove('bg-dark');
        }
    },
    initTheme: function() {
        var isDark = localStorage.getItem('theme') === 'dark';
        this.applyTheme(isDark);
        return isDark;
    }
};

document.addEventListener('DOMContentLoaded', function() {
    window.themeHelper.initTheme();
});

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.themeHelper.initTheme();
    });
} else {
    window.themeHelper.initTheme();
}

window.logout = function () {
    window.location.href = '/Identity/Account/Logout?returnUrl=/';
}; 