(function () {
    // Registry to store initialized dialogs and their handlers for cleanup
    const initializedDialogs = new Map();

    // Initialize all dynamic editor dialogs on the page
    function initializeDynamicEditor(dialogId) {
        // Prevent double initialization
        if (initializedDialogs.has(dialogId)) {
            return;
        }
        const dialog = document.getElementById(dialogId);
        if (!dialog) return;

        const eventName = (dialog.dataset.eventName || "").trim();
        if (!eventName) {
            console.warn("DynamicEditor: data-event-name is missing or empty.", { dialogId });
        }

        const confirmBtn = document.getElementById(`${dialogId}-confirm`);
        const cancelBtn = document.getElementById(`${dialogId}-cancel`);
        const form = document.getElementById(`${dialogId}-form`);
        const validationSummary = document.getElementById(`${dialogId}-validation-summary`);

        if (!confirmBtn || !cancelBtn || !form) {
            console.error('DynamicEditor: Could not find required elements', { dialogId, confirmBtn, cancelBtn, form });
            return;
        }

        // Store initial form state
        let initialFormState = null;

        // Use MutationObserver to detect when dialog opens (via 'open' attribute)
        const observer = new MutationObserver((mutations) => {
            for (const mutation of mutations) {
                if (mutation.attributeName === 'open' && dialog.hasAttribute('open')) {
                    // Dialog was just opened - capture state and clear errors
                    initialFormState = captureFormState();
                    clearValidationErrors();
                    break;
                }
            }
        });

        // Observe the dialog for 'open' attribute changes
        observer.observe(dialog, {
            attributes: true,
            attributeFilter: ['open']
        });

        // Capture the current form state
        function captureFormState() {
            const state = {};
            const formElements = form.elements;

            for (let element of formElements) {
                if (!element.name) continue;

                if (element.type === 'checkbox') {
                    state[element.name] = element.checked;
                } else if (element.type === 'select-multiple') {
                    state[element.name] = Array.from(element.options)
                        .filter(opt => opt.selected)
                        .map(opt => opt.value);
                } else if (element.tagName === 'SELECT') {
                    state[element.name] = element.value;
                } else if (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA') {
                    state[element.name] = element.value;
                }
            }

            return state;
        }

        // Restore form to its initial state
        function restoreFormState() {
            if (!initialFormState) return;

            const formElements = form.elements;

            for (let element of formElements) {
                if (!element.name || !(element.name in initialFormState)) continue;

                if (element.type === 'checkbox') {
                    element.checked = initialFormState[element.name];
                } else if (element.type === 'select-multiple') {
                    const selectedValues = initialFormState[element.name];
                    Array.from(element.options).forEach(opt => {
                        opt.selected = selectedValues.includes(opt.value);
                    });
                } else if (element.tagName === 'SELECT') {
                    element.value = initialFormState[element.name];
                } else if (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA') {
                    element.value = initialFormState[element.name];
                }
            }
        }

        // Helper to get form data
        function getFormData() {
            const formData = new FormData(form);
            const data = {};

            // Handle multi-select elements
            const multiSelects = form.querySelectorAll('select[multiple]');
            const multiSelectNames = new Set(
                Array.from(multiSelects).map(select => select.name)
            );

            // Build the data object
            for (const [key, value] of formData.entries()) {
                if (multiSelectNames.has(key)) {
                    // For multi-select, collect all values as array
                    if (!data[key]) {
                        data[key] = [];
                    }
                    data[key].push(value);
                } else {
                    data[key] = value;
                }
            }

            // Ensure multi-select fields exist even if empty
            multiSelectNames.forEach(name => {
                if (!data[name]) {
                    data[name] = [];
                }
            });

            return data;
        }

        // Clear all validation error displays
        function clearValidationErrors() {
            // Clear validation summary
            if (validationSummary) {
                validationSummary.style.display = 'none';
                validationSummary.innerHTML = '';
            }
            
            // Clear individual field errors
            const errorSpans = form.querySelectorAll('.field-validation-error');
            errorSpans.forEach(span => {
                span.textContent = '';
                span.style.display = 'none';
            });
            
            // Remove invalid class from inputs
            const inputs = form.querySelectorAll('.form-control');
            inputs.forEach(input => {
                input.classList.remove('input-validation-error');
            });
        }

        // Validate the form using HTML5 validation API and data attributes
        function validateForm() {
            clearValidationErrors();
            const errors = [];
            const formElements = form.elements;

            for (let element of formElements) {
                if (!element.name || element.type === 'button') continue;

                const fieldErrors = [];
                
                // Check HTML5 validity
                if (!element.checkValidity()) {
                    // Get custom error message from data attributes or use default
                    if (element.validity.valueMissing) {
                        const customMsg = element.dataset.valRequired;
                        fieldErrors.push(customMsg || `${element.name} is required`);
                    }
                    if (element.validity.tooShort) {
                        const customMsg = element.dataset.valMinlength;
                        fieldErrors.push(customMsg || `${element.name} must be at least ${element.minLength} characters`);
                    }
                    if (element.validity.tooLong) {
                        const customMsg = element.dataset.valMaxlength;
                        fieldErrors.push(customMsg || `${element.name} must not exceed ${element.maxLength} characters`);
                    }
                    if (element.validity.rangeUnderflow) {
                        const customMsg = element.dataset.valRange;
                        fieldErrors.push(customMsg || `${element.name} must be at least ${element.min}`);
                    }
                    if (element.validity.rangeOverflow) {
                        const customMsg = element.dataset.valRange;
                        fieldErrors.push(customMsg || `${element.name} must not exceed ${element.max}`);
                    }
                    if (element.validity.typeMismatch) {
                        fieldErrors.push(`Please enter a valid ${element.type}`);
                    }
                    if (element.validity.patternMismatch) {
                        const customMsg = element.dataset.valRegex;
                        fieldErrors.push(customMsg || `${element.name} format is invalid`);
                    }
                    
                    // If no specific error was identified, use generic message
                    if (fieldErrors.length === 0) {
                        fieldErrors.push(element.validationMessage || `${element.name} is invalid`);
                    }
                }

                // Show field-level errors
                if (fieldErrors.length > 0) {
                    element.classList.add('input-validation-error');
                    const errorSpan = form.querySelector(`[data-valmsg-for="${element.name}"]`);
                    if (errorSpan) {
                        errorSpan.textContent = fieldErrors[0];
                        errorSpan.style.display = 'block';
                    }
                    errors.push(...fieldErrors);
                }
            }

            // Show validation summary if there are errors
            if (errors.length > 0 && validationSummary) {
                validationSummary.innerHTML = '<strong>Please fix the following errors:</strong><ul>' +
                    errors.map(err => `<li>${err}</li>`).join('') +
                    '</ul>';
                validationSummary.style.display = 'block';
            }

            return errors.length === 0;
        }

        // Confirm Event handler
        function handleConfirm() {
            // Validate before confirming
            if (!validateForm()) {
                return;
            }

            const data = getFormData();

            // Dispatch custom "update" event
            const updateEvent = new CustomEvent(`${eventName}-update`, {
                detail: data,
                bubbles: true
            });
            dialog.dispatchEvent(updateEvent);

            // Update initial state to current values after confirming (before closing)
            initialFormState = captureFormState();

            dialog.close();
        }

        // Cancel Event handler
        function handleCancel() {
            // Capture the canceled values (what the user changed to before canceling)
            const canceledData = getFormData();

            // Restore form to initial state
            restoreFormState();
            clearValidationErrors();

            // Dispatch custom "cancel" event with both original and canceled values
            const cancelEvent = new CustomEvent(`${eventName}-cancel`, {
                detail: {
                    originalValues: initialFormState,
                    canceledValues: canceledData
                },
                bubbles: true
            });
            dialog.dispatchEvent(cancelEvent);
            dialog.close();
        }

        // Add event listeners
        confirmBtn.addEventListener('click', handleConfirm);
        cancelBtn.addEventListener('click', handleCancel);

        // Store references for cleanup
        initializedDialogs.set(dialogId, {
            dialog,
            confirmBtn,
            cancelBtn,
            handleConfirm,
            handleCancel,
            observer
        });
    }

    // Cleanup function to remove event listeners and restore original state
    function destroyDynamicEditor(dialogId) {
        const registration = initializedDialogs.get(dialogId);
        if (!registration) {
            return;
        }

        const { confirmBtn, cancelBtn, handleConfirm, handleCancel, observer } = registration;

        // Remove event listeners
        confirmBtn.removeEventListener('click', handleConfirm);
        cancelBtn.removeEventListener('click', handleCancel);

        // Disconnect the MutationObserver
        if (observer) {
            observer.disconnect();
        }

        // Remove from registry
        initializedDialogs.delete(dialogId);
    }

    // Expose initialization and cleanup functions globally
    window.DynamicEditor = {
        init: initializeDynamicEditor,
        destroy: destroyDynamicEditor
    };
})();
