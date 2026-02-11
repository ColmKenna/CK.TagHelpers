/**
 * EditArray Validator Module
 *
 * Provides jQuery Unobtrusive Validation integration for the EditArray component.
 * Subscribes to custom events emitted by editArray.js to wire up and enforce
 * validation without coupling the core module to jQuery.
 *
 * @requires editArray.js (emits editarray:* events)
 * @optional jQuery - When available with jQuery Validation Unobtrusive, enables
 *                    automatic form re-validation after adding new items.
 */
class EditArrayValidator {
    constructor() {
        this.#bind();
    }

    #bind() {
        document.addEventListener('editarray:init', (e) => this.#onInit(e));
        document.addEventListener('editarray:item-added', (e) => this.#onItemAdded(e));
        document.addEventListener('editarray:edit-saving', (e) => this.#onEditSaving(e));
        document.addEventListener('editarray:edit-entered', (e) => this.#onEditEntered(e));
    }

    /**
     * Wire up validation for existing containers on page load.
     */
    #onInit(event) {
        const { container } = event.detail;
        this.#wireUpValidation(container);
    }

    /**
     * Wire up validation after a new item is added.
     */
    #onItemAdded(event) {
        const { container } = event.detail;
        this.#wireUpValidation(container);
    }

    /**
     * Validate inputs before saving (edit → display transition).
     * Calls event.preventDefault() if validation fails to cancel the transition.
     */
    #onEditSaving(event) {
        const { editContainer } = event.detail;
        if (!this.#validateInputs(editContainer)) {
            event.preventDefault();
        }
    }

    /**
     * Re-wire validation when entering edit mode (display → edit transition).
     */
    #onEditEntered(event) {
        const { editContainer } = event.detail;
        this.#wireUpValidation(editContainer);
    }

    /**
     * Re-parse jQuery unobtrusive validation and attach blur handlers for a container.
     * Call this after adding new DOM elements or toggling visibility of form inputs
     * to ensure validation is wired up correctly.
     * @param {HTMLElement} container - A DOM element inside a form
     */
    #wireUpValidation(container) {
        if (!container) return;

        const $jq = window.jQuery;
        if (!$jq || !$jq.validator || !$jq.validator.unobtrusive) return;

        const form = container.closest('form');
        if (!form) return;

        const $form = $jq(form);
        $form.removeData('validator');
        $form.removeData('unobtrusiveValidator');
        $jq.validator.unobtrusive.parse($form);

        // Attach blur validation handlers to all inputs within the container
        $jq(container).find('input, select, textarea').off('blur.validate').on('blur.validate', function () {
            $jq(this).valid();
        });
    }

    /**
     * Validate all inputs within an edit container using jQuery Validation.
     * @param {HTMLElement} editContainer - The edit container element
     * @returns {boolean} true if all inputs are valid (or jQuery is unavailable), false otherwise
     */
    #validateInputs(editContainer) {
        const $jq = window.jQuery;
        if (!$jq || !$jq.validator) return true;

        const form = editContainer.closest('form');
        if (!form) return true;

        const $inputs = $jq(editContainer).find('input, select, textarea');
        let isValid = true;
        $inputs.each(function () {
            if (!$jq(this).valid()) {
                isValid = false;
            }
        });
        return isValid;
    }
}

// Self-initialize singleton
new EditArrayValidator();
