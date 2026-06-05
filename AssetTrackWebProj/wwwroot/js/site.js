// Auto-dismiss TempData feedback banners after a short delay.
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.alert.auto-dismiss').forEach(function (el) {
            setTimeout(function () {
                if (window.bootstrap && bootstrap.Alert) {
                    bootstrap.Alert.getOrCreateInstance(el).close();
                } else {
                    el.style.display = 'none';
                }
            }, 5000);
        });
    });
})();
