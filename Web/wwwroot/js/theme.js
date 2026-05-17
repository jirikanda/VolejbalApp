window.volejbalTheme = {
    get: function () {
        return document.documentElement.getAttribute('data-theme') || 'light';
    },
    set: function (theme) {
        if (theme === 'dark') {
            document.documentElement.setAttribute('data-theme', 'dark');
        } else {
            document.documentElement.removeAttribute('data-theme');
        }
        try { localStorage.setItem('volejbal-theme', theme); } catch (e) { }
    },
    toggle: function () {
        var next = this.get() === 'dark' ? 'light' : 'dark';
        this.set(next);
        return next;
    }
};
