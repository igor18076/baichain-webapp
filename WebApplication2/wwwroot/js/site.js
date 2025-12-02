// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Site JS: animations, modal (kitten), and accordion for "Почему мы"
(function () {
    'use strict';

    // Avoid double initialization
    if (window.__bc_site_js_inited) return;
    window.__bc_site_js_inited = true;

    function onReady(fn) {
        if (document.readyState !== 'loading') {
            fn();
        } else {
            document.addEventListener('DOMContentLoaded', fn);
        }
    }

    // Simple intersection-based reveal for elements with .bc-animate
    function initAnimations() {
        var els = document.querySelectorAll('.bc-animate');
        if (!els.length) return;

        if (!('IntersectionObserver' in window)) {
            els.forEach(function (el) { el.classList.add('bc-in-view'); });
            return;
        }

        var io = new IntersectionObserver(function (entries, obs) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('bc-in-view');
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });

        els.forEach(function (el) { io.observe(el); });
    }

    // Thanks / kitten popup handlers
    function initThanksPopup() {
        var overlay = document.getElementById('bc-thanks-overlay');
        if (!overlay) return;

        function hide() { overlay.classList.remove('is-visible'); }
        function show() { overlay.classList.add('is-visible'); }

        var btnClose = overlay.querySelector('.bc-thanks-close');
        var btnOk = overlay.querySelector('.bc-thanks-ok');
        var backdrop = overlay.querySelector('.bc-thanks-backdrop');

        if (btnClose && !btnClose._bcBound) {
            btnClose.addEventListener('click', hide);
            btnClose._bcBound = true;
        }
        if (btnOk && !btnOk._bcBound) {
            btnOk.addEventListener('click', hide);
            btnOk._bcBound = true;
        }
        if (backdrop && !backdrop._bcBound) {
            backdrop.addEventListener('click', hide);
            backdrop._bcBound = true;
        }

        if (!overlay._bcBound) {
            overlay.addEventListener('click', function (e) {
                if (e.target === overlay) hide();
            });
            overlay._bcBound = true;
        }
    }

    // Accordion for "Почему мы"
    function initWhyAccordion() {
        var headers = document.querySelectorAll('.bc-why .bc-why-header');
        if (!headers || !headers.length) return;

        function closeAll() {
            var items = document.querySelectorAll('.bc-why .bc-why-item');
            items.forEach(function (it) {
                it.classList.remove('bc-why-item-open');
                var body = it.querySelector('.bc-why-body');
                if (body) {
                    body.style.maxHeight = '0px';
                }
            });
        }

        function openItem(item) {
            if (!item) return;
            var body = item.querySelector('.bc-why-body');
            item.classList.add('bc-why-item-open');
            if (body) {
                // set explicit maxHeight for CSS transition
                body.style.maxHeight = body.scrollHeight + 'px';
            }
        }

        headers.forEach(function (h) {
            // make header keyboard-focusable
            try { h.setAttribute('tabindex', '0'); h.setAttribute('role', 'button'); } catch (e) { }

            // avoid duplicate bindings
            if (h._bcWhyBound) return;

            h.addEventListener('click', function () {
                var item = h.closest('.bc-why-item');
                if (!item) return;
                var isOpen = item.classList.contains('bc-why-item-open');
                closeAll();
                if (!isOpen) openItem(item);
            });

            h.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    h.click();
                }
            });

            h._bcWhyBound = true;
        });

        // initialize any items already marked open in markup
        var openBodies = document.querySelectorAll('.bc-why .bc-why-item-open .bc-why-body');
        openBodies.forEach(function (b) {
            if (b) b.style.maxHeight = b.scrollHeight + 'px';
        });

        // on resize adjust opened item's maxHeight
        var rt;
        window.addEventListener('resize', function () {
            clearTimeout(rt);
            rt = setTimeout(function () {
                var opened = document.querySelectorAll('.bc-why .bc-why-item-open .bc-why-body');
                opened.forEach(function (b) {
                    if (b) b.style.maxHeight = b.scrollHeight + 'px';
                });
            }, 150);
        });
    }

    // Form validation error handling
    function initFormValidation() {
        // Add error class to fields with validation errors
        function markErrorFields() {
            var errorFields = document.querySelectorAll('.text-danger');
            errorFields.forEach(function (error) {
                var field = error.previousElementSibling;
                if (field && (field.classList.contains('bc-input') || field.classList.contains('bc-textarea'))) {
                    field.classList.add('error-field');
                }
                // Also check parent for form-row structure
                var formRow = error.closest('.bc-form-row');
                if (formRow) {
                    var input = formRow.querySelector('.bc-input, .bc-textarea');
                    if (input) {
                        input.classList.add('error-field');
                    }
                }
            });
        }

        // Remove error class when user starts typing
        function setupFieldListeners() {
            var fields = document.querySelectorAll('.bc-input, .bc-textarea');
            fields.forEach(function (field) {
                field.addEventListener('input', function () {
                    var error = field.parentElement.querySelector('.text-danger');
                    if (error && field.value.trim() !== '') {
                        field.classList.remove('error-field');
                    }
                });
            });
        }

        // Run on page load
        markErrorFields();
        setupFieldListeners();

        // Also run after form submission (for server-side validation)
        var forms = document.querySelectorAll('form');
        forms.forEach(function (form) {
            form.addEventListener('submit', function () {
                setTimeout(markErrorFields, 100);
            });
        });

        // Watch for dynamically added error messages (jQuery Validation)
        if (typeof jQuery !== 'undefined' && jQuery.fn.validate) {
            jQuery(document).on('DOMNodeInserted', function (e) {
                if (e.target.classList && e.target.classList.contains('text-danger')) {
                    setTimeout(markErrorFields, 50);
                }
            });
        }
    }

    // Init all
    function initAll() {
        initAnimations();
        initThanksPopup();
        initWhyAccordion();
        initFormValidation();
    }

    onReady(initAll);
    window.addEventListener('load', function () { setTimeout(initAll, 30); });

})();
