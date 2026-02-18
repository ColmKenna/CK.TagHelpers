/**
 * EditArray JavaScript Module
 *
 * Provides client-side functionality for dynamic array editing in ASP.NET Core applications.
 * Works as vanilla JavaScript. Emits custom events for validation integration.
 *
 * @requires None (vanilla JS)
 * @emits editarray:init - When the module initializes for each container
 * @emits editarray:item-added - After a new item is appended to the DOM
 * @emits editarray:edit-saving - Before switching from edit to display mode (cancelable)
 * @emits editarray:edit-entered - After switching from display to edit mode
 */

// J4: Shared DOM-ID constants — makes the C#↔JS contract explicit.
const HIDDEN_CLASS = 'ea-hidden';
const SUFFIX_ITEMS = '-items';
const SUFFIX_DISPLAY = '-display';
const SUFFIX_EDIT = '-edit';
const SUFFIX_ADD = '-add';
const SELECTOR_ITEM = '.edit-array-item';
const SELECTOR_PLACEHOLDER = '.edit-array-placeholder';
const SELECTOR_CONTAINER = '.edit-array-container';

// J2: Centralised show/hide helpers.
function showElement(el) { el.classList.remove(HIDDEN_CLASS); }
function hideElement(el) { el.classList.add(HIDDEN_CLASS); }

function captureEditSnapshot(item, editContainer) {
    const controls = Array.from(editContainer.querySelectorAll('input, select, textarea'));
    const snapshot = controls.map(control => {
        const isCheckable = control.type === 'checkbox' || control.type === 'radio';
        const isMultiSelect = control.tagName === 'SELECT' && control.multiple;
        const state = {};

        if (isCheckable) {
            state.checked = control.checked;
        } else if (isMultiSelect) {
            state.selectedValues = Array.from(control.selectedOptions).map(option => option.value);
        } else {
            state.value = control.value;
        }

        return state;
    });

    item.dataset.editSnapshot = JSON.stringify(snapshot);
}

function restoreEditSnapshot(item, editContainer) {
    const snapshotRaw = item.dataset.editSnapshot;
    if (!snapshotRaw) return;

    let snapshot;
    try {
        snapshot = JSON.parse(snapshotRaw);
    } catch {
        delete item.dataset.editSnapshot;
        return;
    }

    const controls = Array.from(editContainer.querySelectorAll('input, select, textarea'));
    snapshot.forEach((state, index) => {
        const control = controls[index];
        if (!control) return;

        const isCheckable = control.type === 'checkbox' || control.type === 'radio';
        const isMultiSelect = control.tagName === 'SELECT' && control.multiple;

        if (isCheckable && typeof state.checked === 'boolean') {
            control.checked = state.checked;
            return;
        }

        if (isMultiSelect && Array.isArray(state.selectedValues)) {
            const selected = new Set(state.selectedValues);
            Array.from(control.options).forEach(option => {
                option.selected = selected.has(option.value);
            });
            return;
        }

        if (typeof state.value === 'string') {
            control.value = state.value;
        }
    });
}

function clearEditSnapshot(item) {
    delete item.dataset.editSnapshot;
}

/**
 * Replace '__index__' tokens in cloned template elements
 * @param {DocumentFragment} clone - The cloned template fragment
 * @param {number} newIndex - The index to replace tokens with
 */
function replaceTemplateTokens(clone, newIndex) {
    const allElements = clone.querySelectorAll(
        '[name],[id],[data-id],[data-display-for],label[for],[data-valmsg-for]'
    );
    allElements.forEach(element => {
        if (element.name) {
            element.name = element.name.replace('__index__', newIndex);
        }
        if (element.id) {
            element.id = element.id.replace('__index__', newIndex);
        }
        if (element.hasAttribute('data-id')) {
            const value = element.getAttribute('data-id');
            if (value) {
                element.setAttribute('data-id', value.replace('__index__', newIndex));
            }
        }
        if (element.hasAttribute('data-display-for')) {
            const value = element.getAttribute('data-display-for');
            if (value) {
                element.setAttribute('data-display-for', value.replace('__index__', newIndex));
            }
        }
        if (element.htmlFor) {
            element.htmlFor = element.htmlFor.replace('__index__', newIndex);
        }
        if (element.hasAttribute('data-valmsg-for')) {
            const value = element.getAttribute('data-valmsg-for');
            if (value) {
                element.setAttribute('data-valmsg-for', value.replace('__index__', newIndex));
            }
        }
    });
}

/**
 * Add a new item to an edit-array container
 * @param {string} containerId - The ID of the edit-array container
 * @param {string} templateId - The ID of the template to clone
 */
function addNewItem(containerId, templateId) {
    const container = document.getElementById(containerId + SUFFIX_ITEMS);
    const template = document.getElementById(templateId);
    const clone = template.content.cloneNode(true);

    // Get current count of items to use as new index (only count actual items, not placeholders)
    const newIndex = container.querySelectorAll(SELECTOR_ITEM).length;

    // Enforce max-items limit if configured
    const maxItems = getMaxItems(containerId);
    if (maxItems !== null && newIndex >= maxItems) {
        syncAddButtonState(containerId);
        return;
    }

    // Replace '__index__' tokens in one pass to reduce DOM traversals.
    replaceTemplateTokens(clone, newIndex);

    // Set the ID for the new item
    const itemDiv = clone.querySelector(SELECTOR_ITEM);
    const itemId = `${containerId}-item-${newIndex}`;
    if (itemDiv) {
        itemDiv.id = itemId;

        // Update data-item-id attributes from "closest" to the actual item ID
        // This allows event delegation to resolve the correct item
        const buttonsWithClosest = itemDiv.querySelectorAll('button[data-item-id="closest"]');
        buttonsWithClosest.forEach(btn => {
            btn.dataset.itemId = itemId;
        });

        // For newly added items in display mode, show edit container first and hide display container
        const displayContainer = itemDiv.querySelector('.display-container');
        const editContainer = itemDiv.querySelector('.edit-container');

        if (displayContainer && editContainer) {
            displayContainer.id = `${itemId}${SUFFIX_DISPLAY}`;
            editContainer.id = `${itemId}${SUFFIX_EDIT}`;
            hideElement(displayContainer);
            showElement(editContainer);

            const hiddenInput = document.createElement('input');
            hiddenInput.type = 'hidden';
            hiddenInput.name = `__newItem__${newIndex}`;
            hiddenInput.value = 'true';
            hiddenInput.setAttribute('data-new-item-marker', 'true');
            hiddenInput.setAttribute('data-id', `__newItem__${newIndex}`);
            hiddenInput.setAttribute('data-display-for', `__newItem__${newIndex}`);

            // Append the hidden input to the edit container
            editContainer.appendChild(hiddenInput);
        }
    }

    container.appendChild(clone);

    // Re-parse jQuery unobtrusive validation for the new inputs if available
    refreshUnobtrusiveValidation(container);

    // Hide the placeholder if it exists
    const placeholder = container.querySelector(SELECTOR_PLACEHOLDER);
    if (placeholder) {
        hideElement(placeholder);
    }

    // disable the add button
    const addButton = document.getElementById(containerId + SUFFIX_ADD);
    if (addButton) {
        addButton.disabled = true;
    }

    // Notify listeners that a new item was added (e.g. for validation wiring)
    document.dispatchEvent(new CustomEvent('editarray:item-added', {
        detail: { containerId, itemId: itemDiv?.id, container }
    }));

    // Keep keyboard users in context by moving focus into the new item editor.
    const newItem = document.getElementById(itemId);
    if (newItem) {
        const firstInput = newItem.querySelector('input:not([type="hidden"]), select, textarea');
        if (firstInput) {
            firstInput.focus();
        }
    }
}

/**
 * Toggle between display and edit modes for an item
 * @param {string} itemId - The ID of the item to toggle
 */
function toggleEditMode(itemId) {
    const item = document.getElementById(itemId);
    if (!item) return;

    const containerId = getContainerIdFromItemId(itemId);
    const displayContainer = document.getElementById(`${itemId}${SUFFIX_DISPLAY}`);
    const editContainer = document.getElementById(`${itemId}${SUFFIX_EDIT}`);

    if (displayContainer && editContainer) {
        if (displayContainer.classList.contains(HIDDEN_CLASS)) {
            // Optional dedicated hook for Done button; returning false cancels the transition
            const onDone = item.dataset.onDone;
            if (onDone && typeof window[onDone] === 'function') {
                const shouldContinue = window[onDone](itemId);
                if (shouldContinue === false) {
                    return;
                }
            }

            // Allow listeners (e.g. validator) to cancel the save
            const savingEvent = new CustomEvent('editarray:edit-saving', {
                cancelable: true,
                detail: { itemId, editContainer }
            });
            document.dispatchEvent(savingEvent);
            if (savingEvent.defaultPrevented) {
                return;
            }

            // Update display with current values
            updateDisplayFromForm(itemId);

            // Switch from edit to display
            showElement(displayContainer);
            hideElement(editContainer);
            clearEditSnapshot(item);

            // Safely invoke OnUpdate callback if defined (XSS prevention: validate function exists)
            const onUpdate = item.dataset.onUpdate;
            if (onUpdate && typeof window[onUpdate] === 'function') {
                window[onUpdate](itemId);
            }
        } else {
            captureEditSnapshot(item, editContainer);

            // Switch from display to edit
            hideElement(displayContainer);
            showElement(editContainer);

            // Notify listeners that edit mode was entered (e.g. for validation wiring)
            document.dispatchEvent(new CustomEvent('editarray:edit-entered', {
                detail: { itemId, editContainer }
            }));
        }
    }

    // re-enable the add button
    if (containerId) {
        const addButton = document.getElementById(containerId + SUFFIX_ADD);
        if (addButton) {
            addButton.disabled = false;
            syncAddButtonState(containerId);
        }
    }

    // remove the hidden input for new items if it exists
    const hiddenInput = item.querySelector('input[data-new-item-marker]');
    if (hiddenInput) {
        hiddenInput.remove();
    }
}

function cancelEdit(itemId) {
    const item = document.getElementById(itemId);
    if (!item) return;

    const containerId = getContainerIdFromItemId(itemId);
    const newItemInput = item.querySelector('input[data-new-item-marker]');
    if (newItemInput) {
        removeUnsavedItem(itemId, containerId);
        return;
    }

    const displayContainer = document.getElementById(`${itemId}${SUFFIX_DISPLAY}`);
    const editContainer = document.getElementById(`${itemId}${SUFFIX_EDIT}`);
    if (!displayContainer || !editContainer) return;

    restoreEditSnapshot(item, editContainer);
    clearEditSnapshot(item);

    showElement(displayContainer);
    hideElement(editContainer);

    if (containerId) {
        const addButton = document.getElementById(containerId + SUFFIX_ADD);
        if (addButton) {
            addButton.disabled = false;
            syncAddButtonState(containerId);
        }
    }
}

/**
 * Remove an unsaved new item entirely and restore UI state
 * @param {string} itemId - The ID of the unsaved item to remove
 * @param {string|null} containerId - The ID of the parent edit-array container
 */
function removeUnsavedItem(itemId, containerId) {
    const item = document.getElementById(itemId);
    if (!item) return;

    item.remove();
    if (containerId) {
        const addButton = document.getElementById(containerId + SUFFIX_ADD);
        if (addButton) {
            addButton.disabled = false;
            syncAddButtonState(containerId);
            addButton.focus();
        }

        // Show placeholder if there are no items left
        const itemsContainer = document.getElementById(containerId + SUFFIX_ITEMS);
        if (itemsContainer) {
            const itemCount = itemsContainer.querySelectorAll(SELECTOR_ITEM).length;
            if (itemCount === 0) {
                const placeholder = itemsContainer.querySelector(SELECTOR_PLACEHOLDER);
                if (placeholder) {
                    showElement(placeholder);
                }
            }
        }
    }
}

function markForDeletion(itemId) {
    const item = document.getElementById(itemId);
    if (!item) return;

    const deleteButton = item.querySelector('.delete-item-btn');
    const editButton = item.querySelector('.edit-item-btn');
    const isDeletedInput = item.querySelector('input[data-is-deleted-marker]');

    // Find the container by looking for parent with id ending in '-items' or containing 'edit-array'
    let container = item.closest(SELECTOR_CONTAINER);
    if (!container) {
        // Fallback: look for parent with id containing 'items'
        let current = item.parentElement;
        while (current) {
            if (current.id && current.id.includes(SUFFIX_ITEMS)) {
                container = current.parentElement;
                break;
            }
            current = current.parentElement;
        }
    }

    const containerId = container ? container.id : null;
    const newItemInput = item.querySelector('input[data-new-item-marker]');

    // don't mark for deletion, remove it all together
    if (newItemInput) {
        removeUnsavedItem(itemId, containerId);
        return;
    }

    // Get button text from container data attributes, with fallback defaults
    const deleteText = container?.dataset?.deleteText || 'Delete';
    const undeleteText = container?.dataset?.undeleteText || 'Undelete';

    if (item.getAttribute('data-deleted') === 'true') {
        // Undo deletion
        item.removeAttribute('data-deleted');
        item.classList.remove('deleted');
        if (isDeletedInput) {
            isDeletedInput.value = 'false';
        }
        if (deleteButton) {
            deleteButton.textContent = deleteText;
        }
        if (editButton) {
            editButton.disabled = false;
        }
    } else {
        // Mark as deleted
        item.setAttribute('data-deleted', 'true');
        item.classList.add('deleted');
        if (isDeletedInput) {
            isDeletedInput.value = 'true';
        }
        if (deleteButton) {
            deleteButton.textContent = undeleteText;
        }
        if (editButton) {
            editButton.disabled = true;
        }

        // Safely invoke OnDelete callback if defined (XSS prevention: validate function exists)
        const onDelete = item.dataset.onDelete;
        if (onDelete && typeof window[onDelete] === 'function') {
            window[onDelete](itemId);
        }
    }
}

/**
 * Update display values based on the current form input values
 * @param {string} itemId - The ID of the item to update
 */
function updateDisplayFromForm(itemId) {
    const item = document.getElementById(itemId);
    if (!item) return;

    const displayContainer = document.getElementById(`${itemId}${SUFFIX_DISPLAY}`);
    const editContainer = document.getElementById(`${itemId}${SUFFIX_EDIT}`);

    if (displayContainer && editContainer) {
        const inputs = editContainer.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            const displayElement = displayContainer.querySelector(`[data-display-for="${input.id}"]`);
            if (displayElement) {
                displayElement.textContent = input.value;
            }
        });
    }
}

function moveItem(containerId, itemId, offset) {
    const mainContainer = document.getElementById(containerId);
    const container = document.getElementById(`${containerId}${SUFFIX_ITEMS}`);
    const item = document.getElementById(itemId);
    if (!container || !item || offset === 0) return;

    // Guard reorder operations - only allow if reordering is enabled on the container
    if (!mainContainer || mainContainer.getAttribute('data-reorder-enabled') !== 'true') {
        console.warn(`Reordering is not enabled for container: ${containerId}`);
        return;
    }

    const items = Array.from(container.querySelectorAll(SELECTOR_ITEM));
    const currentIndex = items.indexOf(item);
    if (currentIndex === -1) return;

    const targetIndex = currentIndex + offset;
    if (targetIndex < 0 || targetIndex >= items.length) return;

    const referenceItem = items[targetIndex];
    const insertBeforeNode = offset > 0 ? referenceItem.nextSibling : referenceItem;
    container.insertBefore(item, insertBeforeNode);

    renumberItems(containerId);

    // Keep focus on the same logical move direction after reordering.
    const updatedItems = container.querySelectorAll(SELECTOR_ITEM);
    const movedItem = updatedItems[targetIndex];
    if (!movedItem) return;

    const moveButton = movedItem.querySelector(
        `[data-action="move"][data-direction="${offset < 0 ? '-1' : '1'}"]`
    );
    if (moveButton) {
        moveButton.focus();
    }
}

function renumberItems(containerId) {
    const container = document.getElementById(`${containerId}${SUFFIX_ITEMS}`);
    if (!container) {
        console.error(`Container not found: ${containerId}${SUFFIX_ITEMS}`);
        return;
    }

    const items = Array.from(container.querySelectorAll(SELECTOR_ITEM));
    if (items.length === 0) return;

    items.forEach((item, newIndex) => {
        const oldId = item.id;
        const newId = `${containerId}-item-${newIndex}`;

        // Validate uniqueness - skip if new ID already exists and isn't the current item
        if (document.getElementById(newId) && newId !== oldId) {
            console.warn(`Duplicate ID detected: ${newId}. Skipping renumbering for this item.`);
            return;
        }

        updateAttributeWithIndex(item, 'id', newIndex, oldId, newId);

        const descendants = item.querySelectorAll(
            '[name],[id],[for],[data-id],[data-display-for],[data-valmsg-for],' +
            '[data-new-item-marker],[aria-describedby],[data-item-id]'
        );
        descendants.forEach(child => {
            updateAttributeWithIndex(child, 'id', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'name', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'for', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'data-id', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'data-display-for', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'data-valmsg-for', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'data-new-item-marker', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'aria-describedby', newIndex, oldId, newId);
            // Update data-item-id for event delegation (no longer using onclick)
            updateAttributeWithIndex(child, 'data-item-id', newIndex, oldId, newId);
        });
    });
}

function updateAttributeWithIndex(element, attributeName, newIndex, oldId, newId) {
    if (!element) return;

    const currentValue = attributeName === 'for'
        ? element.htmlFor || element.getAttribute('for')
        : element.getAttribute(attributeName);

    if (!currentValue) return;

    const updatedValue = replaceIndexTokens(currentValue, newIndex, oldId, newId);
    if (updatedValue === currentValue) return;

    if (attributeName === 'for') {
        element.htmlFor = updatedValue;
    } else if (attributeName === 'id') {
        element.id = updatedValue;
    } else {
        element.setAttribute(attributeName, updatedValue);
    }
}

function replaceIndexTokens(value, newIndex, oldId, newId) {
    // Fast path when no token patterns are present.
    if (!value.includes('[') && !value.includes('_') && !value.includes('-item-')) {
        if (!oldId || !value.includes(oldId)) {
            return value;
        }
    }

    let updated = value;
    if (oldId && newId && updated.includes(oldId)) {
        updated = updated.replaceAll(oldId, newId);
    }

    updated = updated.replace(/\[\d+\]/g, `[${newIndex}]`);
    updated = updated.replace(/_(\d+)__/g, `_${newIndex}__`);
    updated = updated.replace(/__newItem__\d+/g, `__newItem__${newIndex}`);
    updated = updated.replace(/-item-\d+/g, `-item-${newIndex}`);

    return updated;
}

function getContainerIdFromItemId(itemId) {
    if (!itemId) return null;
    const match = itemId.match(/^(.*)-item-\d+$/);
    return match && match[1] ? match[1] : null;
}

function getMaxItems(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return null;

    const maxItemsAttr = container.dataset.maxItems;
    if (!maxItemsAttr) return null;

    const parsed = parseInt(maxItemsAttr, 10);
    if (Number.isNaN(parsed) || parsed <= 0) {
        return null;
    }
    return parsed;
}

function syncAddButtonState(containerId) {
    const addButton = document.getElementById(containerId + SUFFIX_ADD);
    if (!addButton) return;

    const maxItems = getMaxItems(containerId);
    if (maxItems === null) return;

    const itemsContainer = document.getElementById(containerId + SUFFIX_ITEMS);
    const currentCount = itemsContainer
        ? itemsContainer.querySelectorAll(SELECTOR_ITEM).length
        : 0;
    addButton.disabled = currentCount >= maxItems;
}

/**
 * Resolves the item ID from a button's data-item-id attribute.
 * If the value is "closest", finds the nearest .edit-array-item ancestor's ID.
 * @param {HTMLElement} button - The button element
 * @returns {string|null} The resolved item ID
 */
function resolveItemId(button) {
    const itemIdAttr = button.dataset.itemId;
    if (itemIdAttr === 'closest') {
        const item = button.closest(SELECTOR_ITEM);
        return item ? item.id : null;
    }
    return itemIdAttr || null;
}

/**
 * Event delegation handler for edit-array button actions.
 * Handles all button clicks via data-action attributes instead of inline onclick handlers.
 * This improves security (CSP compliance), testability, and maintainability.
 */
function handleEditArrayAction(event) {
    const button = event.target.closest('button[data-action]');
    if (!button) return;

    const action = button.dataset.action;
    const containerId = button.dataset.containerId;
    const templateId = button.dataset.templateId;
    const itemId = resolveItemId(button);
    const direction = button.dataset.direction ? parseInt(button.dataset.direction, 10) : 0;

    switch (action) {
        case 'add':
            if (containerId && templateId) {
                addNewItem(containerId, templateId);
            }
            break;
        case 'edit':
        case 'done':
            if (itemId) {
                toggleEditMode(itemId);
            }
            break;
        case 'delete':
            if (itemId) {
                markForDeletion(itemId);
            }
            break;
        case 'cancel':
            if (itemId) {
                cancelEdit(itemId);
            }
            break;
        case 'move':
            if (containerId && itemId && direction !== 0) {
                moveItem(containerId, itemId, direction);
            }
            break;
        default:
            // Unknown action, ignore
            break;
    }
}

/**
 * Re-parse jQuery unobtrusive validation for dynamically added inputs.
 * Removes stale validator data so $.validator.unobtrusive.parse() picks up new fields.
 * No-ops silently when jQuery validation is not loaded.
 * @param {HTMLElement} element - An element inside the form to re-parse
 */
function refreshUnobtrusiveValidation(element) {
    if (typeof $ === 'undefined' || !$.validator || !$.validator.unobtrusive) return;
    var $form = $(element).closest('form');
    if (!$form.length) return;

    $form.removeData('validator').removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse($form);

    // Enable eager validation so fields validate on blur, not just on submit
    var validator = $form.data('validator');
    if (validator) {
        validator.settings.onfocusout = function (el) {
            if (!this.checkable(el)) {
                this.element(el);
            }
        };
    }
}

// Initialize event delegation and validation when DOM is ready
function initEditArray() {
    document.addEventListener('click', handleEditArrayAction);

    // Wire up unobtrusive validation and notify listeners for each container
    document.querySelectorAll(SELECTOR_CONTAINER).forEach(function(container) {
        refreshUnobtrusiveValidation(container);
        if (container.id) {
            syncAddButtonState(container.id);
        }
        document.dispatchEvent(new CustomEvent('editarray:init', {
            detail: { container }
        }));
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initEditArray);
} else {
    initEditArray();
}

// Export for test environments (CommonJS); ignored in browsers.
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        addNewItem,
        toggleEditMode,
        cancelEdit,
        markForDeletion,
        removeUnsavedItem,
        updateDisplayFromForm,
        moveItem,
        renumberItems,
        updateAttributeWithIndex,
        replaceIndexTokens,
        replaceTemplateTokens,
        getContainerIdFromItemId,
        resolveItemId,
        handleEditArrayAction,
        refreshUnobtrusiveValidation,
        initEditArray,
        showElement,
        hideElement,
        HIDDEN_CLASS,
        SUFFIX_ITEMS,
        SUFFIX_DISPLAY,
        SUFFIX_EDIT,
        SUFFIX_ADD,
        SELECTOR_ITEM,
        SELECTOR_PLACEHOLDER,
        SELECTOR_CONTAINER
    };
}
