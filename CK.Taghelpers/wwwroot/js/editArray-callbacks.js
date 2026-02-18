/**
 * EditArray Callbacks — Companion library for editArray.js
 *
 * Provides ready-made, composable callback factories for the
 * asp-on-done, asp-on-update, and asp-on-delete hooks.
 *
 * Usage:
 *   <script src="~/js/editArray.js"></script>
 *   <script src="~/js/editArray-callbacks.js"></script>
 *   <script>
 *     editArrayCallbacks.register('guardDone',
 *       editArrayCallbacks.validationGuard({ statusElementId: 'status' })
 *     );
 *   </script>
 *
 * @requires editArray.js (loaded first)
 */
(function (root) {
    'use strict';

    var SAFE_NAME = /^[a-zA-Z_$][a-zA-Z0-9_$]*$/;

    /**
     * Register a callback function on `window` so editArray.js can find it
     * via `window[name]`. Validates the name matches the same regex the C#
     * TagHelper enforces.
     *
     * @param {string} name - A valid JavaScript identifier
     * @param {Function} fn - The callback function
     */
    function register(name, fn) {
        if (typeof name !== 'string' || !SAFE_NAME.test(name)) {
            throw new Error(
                'editArrayCallbacks.register: "' + name + '" is not a valid JavaScript identifier.'
            );
        }
        if (typeof fn !== 'function') {
            throw new Error(
                'editArrayCallbacks.register: second argument must be a function.'
            );
        }
        root[name] = fn;
    }

    /**
     * Create a validation guard for asp-on-done.
     *
     * Checks jQuery unobtrusive validation when available, otherwise falls
     * back to inspecting `span[data-valmsg-for]` elements for error text.
     * Returns `false` to block the edit-to-display transition.
     *
     * @param {Object} [options]
     * @param {string} [options.statusElementId] - DOM id to update with pass/fail message
     * @param {string} [options.validMessage='Validation passed.']
     * @param {string} [options.invalidMessage='Please fix validation errors.']
     * @param {string} [options.validClass='alert alert-success mt-2']
     * @param {string} [options.invalidClass='alert alert-danger mt-2']
     * @returns {Function} An onDone callback that returns boolean
     */
    function validationGuard(options) {
        var opts = options || {};
        var statusElementId = opts.statusElementId || null;
        var validMessage = opts.validMessage || 'Validation passed.';
        var invalidMessage = opts.invalidMessage || 'Please fix validation errors.';
        var validClass = opts.validClass || 'alert alert-success mt-2';
        var invalidClass = opts.invalidClass || 'alert alert-danger mt-2';

        function updateStatus(isValid) {
            if (!statusElementId) return;
            var el = document.getElementById(statusElementId);
            if (!el) return;
            el.textContent = isValid ? validMessage : invalidMessage;
            el.className = isValid ? validClass : invalidClass;
        }

        return function guardDone(itemId) {
            var editScope = document.getElementById(itemId + '-edit');
            var form = editScope ? editScope.closest('form') : null;

            // jQuery unobtrusive validation path
            if (form && typeof root.$ === 'function' &&
                root.$.validator && root.$.validator.unobtrusive) {
                root.$.validator.unobtrusive.parse(editScope);
                var validator = root.$(form).data('validator') || root.$(form).validate();
                var inputs = root.$(editScope).find(':input');
                var isValid = inputs.toArray().every(function (input) {
                    return validator.element(input);
                });
                updateStatus(isValid);
                return isValid;
            }

            // Fallback: inspect validation message spans
            var spans = editScope
                ? editScope.querySelectorAll('span[data-valmsg-for]')
                : [];
            var hasErrors = Array.from(spans).some(function (s) {
                return (s.textContent && s.textContent.trim()) ||
                    s.classList.contains('field-validation-error');
            });
            updateStatus(!hasErrors);
            return !hasErrors;
        };
    }

    /**
     * Create a callback that updates a DOM element's text (and optionally class).
     *
     * @param {Object} options
     * @param {string} options.elementId - The DOM element id to update
     * @param {string|Function} options.message - Static string or `(itemId) => string`
     * @param {string} [options.className] - If provided, replaces element.className
     * @returns {Function} A callback suitable for onUpdate / onDelete
     */
    function statusUpdater(options) {
        var opts = options || {};
        var elementId = opts.elementId;
        var message = opts.message || '';
        var className = opts.className || null;

        return function updateStatus(itemId) {
            var el = elementId ? document.getElementById(elementId) : null;
            if (!el) return;
            el.textContent = typeof message === 'function' ? message(itemId) : message;
            if (className) {
                el.className = className;
            }
        };
    }

    /**
     * Create a callback that logs to the console and optionally calls a toast function.
     *
     * @param {Object} [options]
     * @param {string} [options.prefix='EditArray'] - Console log prefix
     * @param {string|Function} [options.message='Callback fired'] - Static string or `(itemId) => string`
     * @param {string} [options.level='info'] - Toast level passed to the toast function
     * @param {Function} [options.toast] - Optional `(message, level) => void` toast function
     * @returns {Function} A callback suitable for any hook
     */
    function notifier(options) {
        var opts = options || {};
        var prefix = opts.prefix || 'EditArray';
        var message = opts.message || 'Callback fired';
        var level = opts.level || 'info';
        var toastFn = opts.toast || null;

        return function notify(itemId) {
            var text = typeof message === 'function' ? message(itemId) : message;
            console.log('[' + prefix + '] ' + text + ' (item: ' + itemId + ')');
            if (typeof toastFn === 'function') {
                toastFn(text, level);
            }
        };
    }

    /**
     * Compose multiple callback functions into a single callback.
     * Functions run in order. If ANY function returns `false` (strict),
     * execution stops and the composed callback returns `false`.
     * This makes compose safe for onDone guards.
     *
     * @param {...Function} fns - Functions to compose
     * @returns {Function} A composed callback
     */
    function compose() {
        var fns = Array.from(arguments);
        return function composed(itemId) {
            for (var i = 0; i < fns.length; i++) {
                var result = fns[i](itemId);
                if (result === false) {
                    return false;
                }
            }
        };
    }

    var editArrayCallbacks = {
        register: register,
        validationGuard: validationGuard,
        statusUpdater: statusUpdater,
        notifier: notifier,
        compose: compose
    };

    root.editArrayCallbacks = editArrayCallbacks;

    // CommonJS export for test environments
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = editArrayCallbacks;
    }

})(typeof window !== 'undefined' ? window : global);
