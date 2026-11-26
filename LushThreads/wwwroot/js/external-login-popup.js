(function () {
    document.addEventListener('DOMContentLoaded', function () {
        var forms = document.querySelectorAll('form[data-external-login-popup="true"]');
        if (!forms.length) return;

        window.addEventListener('message', function (event) {
            if (event.origin !== window.location.origin) return;
            var data = event.data || {};
            if (data.type === 'external-login-success') {
                window.location.href = data.redirectUrl || '/';
            } else if (data.type === 'external-login-error') {
                window.location.reload();
            }
        });

        forms.forEach(function (form) {
            form.addEventListener('submit', function (e) {
                var width = 500;
                var height = 640;
                var left = Math.max(0, (window.screen.width - width) / 2);
                var top = Math.max(0, (window.screen.height - height) / 2);
                var windowName = 'externalLoginPopup';

                var popup = window.open(
                    '',
                    windowName,
                    'width=' + width + ',height=' + height + ',left=' + left + ',top=' + top + ',menubar=no,toolbar=no,status=no,resizable=yes,scrollbars=yes'
                );

                // Fall back to normal full-page redirect if popup was blocked
                if (!popup || popup.closed || typeof popup.closed === 'undefined') {
                    form.removeAttribute('target');
                    return;
                }

                var isPopupInput = form.querySelector('input[name="isPopup"]');
                if (!isPopupInput) {
                    isPopupInput = document.createElement('input');
                    isPopupInput.type = 'hidden';
                    isPopupInput.name = 'isPopup';
                    form.appendChild(isPopupInput);
                }
                isPopupInput.value = 'true';

                form.target = windowName;
                popup.focus();
            });
        });
    });
})();
