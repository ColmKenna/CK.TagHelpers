(function () {
    // Initialize all dynamic editor dialogs on the page
    function initializeDynamicEditor(dialogId) {
        const dialog = document.getElementById(dialogId);
        if (!dialog) return;

        const eventName = (dialog.dataset.eventName || "").trim();
        if (!eventName) {
            console.warn("DynamicEditor: data-event-name is missing or empty.", { dialogId });
        }

        const confirmBtn = document.getElementById(`${dialogId}-confirm`);
        const cancelBtn = document.getElementById(`${dialogId}-cancel`);
        const form = document.getElementById(`${dialogId}-form`);

        if (!confirmBtn || !cancelBtn || !form) {
            console.error('DynamicEditor: Could not find required elements', { dialogId, confirmBtn, cancelBtn, form });
            return;
        }

        // Store initial form state
        let initialFormState = null;

        // Also capture on first show if using showModal()
        const originalShowModal = dialog.showModal.bind(dialog);
        dialog.showModal = function() {
            initialFormState = captureFormState();
            originalShowModal();
        };

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
                } else if (element.tagName === 'INPUT') {
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
                } else if (element.tagName === 'INPUT') {
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

        // Confirm Event
        confirmBtn.addEventListener('click', () => {
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
        });

        // Cancel Event
        cancelBtn.addEventListener('click', () => {
            // Capture the canceled values (what the user changed to before canceling)
            const canceledData = getFormData();

            // Restore form to initial state
            restoreFormState();

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
        });
    }

    // Expose initialization function globally
    window.DynamicEditor = {
        init: initializeDynamicEditor
    };
})();
