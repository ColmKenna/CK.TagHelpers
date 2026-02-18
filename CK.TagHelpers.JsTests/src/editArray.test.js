
const { fireEvent } = require('@testing-library/dom');

const {
    addNewItem,
    toggleEditMode,
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
} = require('../../CK.Taghelpers/wwwroot/js/editArray.js');

function setupFixture(options = {}) {
    const { reorderEnabled = false, maxItems = null } = options;
    const maxItemsAttribute = Number.isInteger(maxItems) ? `data-max-items="${maxItems}"` : '';
    document.body.innerHTML = `
        <form id="test-form">
            <div id="test-container" class="edit-array-container" data-delete-text="Delete" data-undelete-text="Undelete" data-reorder-enabled="${reorderEnabled ? 'true' : 'false'}" ${maxItemsAttribute}>
                <div id="test-container-items">
                    <div class="edit-array-placeholder">No items yet</div>
                </div>
                <template id="test-template">
                    <div class="edit-array-item">
                        <input type="hidden" data-is-deleted-marker="true" name="Input[__index__].IsDeleted" value="false" />
                        <div class="display-container">
                            <span data-display-for="Input___index__"></span>
                        </div>
                        <div class="edit-container ea-hidden">
                            <label for="Input___index__">Name</label>
                            <input type="text" id="Input___index__" name="Input[__index__]" data-id="input-__index__" data-display-for="Input___index__" value="" />
                            <span data-valmsg-for="Input[__index__]"></span>
                            <button type="button" class="done-item-btn" data-action="done" data-item-id="closest">Done</button>
                            <button type="button" class="cancel-item-btn" data-action="cancel" data-item-id="closest">Cancel</button>
                        </div>
                        <button type="button" class="edit-item-btn" data-action="edit" data-item-id="closest">Edit</button>
                        <button type="button" class="delete-item-btn" data-action="delete" data-item-id="closest">Delete</button>
                        <button type="button" class="move-up-btn" data-action="move" data-container-id="test-container" data-item-id="closest" data-direction="-1">Up</button>
                        <button type="button" class="move-down-btn" data-action="move" data-container-id="test-container" data-item-id="closest" data-direction="1">Down</button>
                    </div>
                </template>
                <button id="test-container-add" type="button" data-action="add" data-container-id="test-container" data-template-id="test-template">Add Item</button>
            </div>
        </form>
    `;
}

function addItemAndGet(idSuffix = '0') {
    addNewItem('test-container', 'test-template');
    return document.getElementById(`test-container-item-${idSuffix}`);
}

describe('editArray.js', () => {
    beforeEach(() => {
        jest.restoreAllMocks();
        delete window.onDoneCallback;
        delete window.onUpdateCallback;
        delete window.onDeleteCallback;
        delete global.$;
        setupFixture();
    });

    // Validates item creation updates ids/names/attributes and emits integration events.
    describe('add item flow', () => {
        // Validates token replacement and DOM updates for newly added rows.
        it('adds a new item, updates template tokens, disables Add, and hides placeholder', () => {
            // Arrange
            const eventSpy = jest.fn();
            document.addEventListener('editarray:item-added', eventSpy);

            // Act
            addNewItem('test-container', 'test-template');

            // Assert
            const itemsContainer = document.getElementById('test-container-items');
            const item = document.getElementById('test-container-item-0');
            const addButton = document.getElementById('test-container-add');
            const placeholder = itemsContainer.querySelector('.edit-array-placeholder');

            expect(item).toBeTruthy();
            expect(itemsContainer.querySelectorAll('.edit-array-item')).toHaveLength(1);
            expect(item.querySelector('input[type="text"]').id).toBe('Input_0');
            expect(item.querySelector('input[type="text"]').name).toBe('Input[0]');
            expect(item.querySelector('label').htmlFor).toBe('Input_0');
            expect(item.querySelector('[data-valmsg-for]').getAttribute('data-valmsg-for')).toBe('Input[0]');
            expect(item.querySelector('input[type="text"]').getAttribute('data-id')).toBe('input-0');
            expect(item.querySelector('input[type="text"]').getAttribute('data-display-for')).toBe('Input_0');
            expect(item.querySelector('.display-container').id).toBe('test-container-item-0-display');
            expect(item.querySelector('.edit-container').id).toBe('test-container-item-0-edit');
            expect(item.querySelector('.display-container').classList.contains('ea-hidden')).toBe(true);
            expect(item.querySelector('.edit-container').classList.contains('ea-hidden')).toBe(false);
            expect(item.querySelector('input[data-new-item-marker="true"]')).toBeTruthy();
            expect(item.querySelector('button[data-action="cancel"]')).toBeTruthy();
            expect(item.firstElementChild?.hasAttribute('data-is-deleted-marker')).toBe(true);
            expect(addButton.disabled).toBe(true);
            expect(placeholder.classList.contains('ea-hidden')).toBe(true);
            expect(document.activeElement).toBe(item.querySelector('input[type="text"]'));
            expect(eventSpy).toHaveBeenCalledTimes(1);
            expect(eventSpy.mock.calls[0][0].detail.itemId).toBe('test-container-item-0');
        });

        // Validates delegated click handling triggers add behavior without inline onclick.
        it('adds an item when clicking delegated Add button', () => {
            // Arrange
            const addButton = document.getElementById('test-container-add');

            // Act
            fireEvent.click(addButton);

            // Assert
            const items = document.querySelectorAll('#test-container-items .edit-array-item');
            expect(items).toHaveLength(1);
        });

        it('respects max-items limit and blocks additions beyond the configured maximum', () => {
            // Arrange
            setupFixture({ maxItems: 1 });

            // Act
            addNewItem('test-container', 'test-template');
            toggleEditMode('test-container-item-0');
            addNewItem('test-container', 'test-template');

            // Assert
            const items = document.querySelectorAll('#test-container-items .edit-array-item');
            const addButton = document.getElementById('test-container-add');
            expect(items).toHaveLength(1);
            expect(addButton.disabled).toBe(true);
        });

        it('disables add button on init when existing item count already reaches max-items', () => {
            // Arrange
            setupFixture({ maxItems: 1 });
            const itemsContainer = document.getElementById('test-container-items');
            itemsContainer.insertAdjacentHTML('beforeend', '<div class="edit-array-item" id="existing-item"></div>');
            const addButton = document.getElementById('test-container-add');
            addButton.disabled = false;

            // Act
            initEditArray();

            // Assert
            expect(addButton.disabled).toBe(true);
        });
    });

    // Validates edit/display transitions enforce callbacks and cancelation hooks.
    describe('edit mode transitions', () => {
        // Validates save path updates display values, clears new-item markers, and invokes callback.
        it('saves edit mode to display mode and calls onUpdate callback', () => {
            // Arrange
            const item = addItemAndGet('0');
            const input = item.querySelector('input[type="text"]');
            input.value = 'Alice';
            window.onUpdateCallback = jest.fn();
            item.dataset.onUpdate = 'onUpdateCallback';

            // Act
            toggleEditMode('test-container-item-0');

            // Assert
            expect(item.querySelector('.display-container').classList.contains('ea-hidden')).toBe(false);
            expect(item.querySelector('.edit-container').classList.contains('ea-hidden')).toBe(true);
            expect(item.querySelector('[data-display-for="Input_0"]').textContent).toBe('Alice');
            expect(window.onUpdateCallback).toHaveBeenCalledWith('test-container-item-0');
            expect(item.querySelector('#test-container-item-0-edit button[data-action="cancel"]')).toBeTruthy();
            expect(item.querySelector('input[data-new-item-marker]')).toBeFalsy();
            expect(document.getElementById('test-container-add').disabled).toBe(false);
        });

        // Validates onDone hook can veto save transition.
        it('does not leave edit mode when onDone returns false', () => {
            // Arrange
            const item = addItemAndGet('0');
            window.onDoneCallback = jest.fn(() => false);
            window.onUpdateCallback = jest.fn();
            item.dataset.onDone = 'onDoneCallback';
            item.dataset.onUpdate = 'onUpdateCallback';

            // Act
            toggleEditMode('test-container-item-0');

            // Assert
            expect(window.onDoneCallback).toHaveBeenCalledWith('test-container-item-0');
            expect(window.onUpdateCallback).not.toHaveBeenCalled();
            expect(item.querySelector('.display-container').classList.contains('ea-hidden')).toBe(true);
            expect(item.querySelector('.edit-container').classList.contains('ea-hidden')).toBe(false);
        });

        it('calls callbacks in order: onDone, edit-saving event, then onUpdate', () => {
            // Arrange
            const item = addItemAndGet('0');
            const callOrder = [];

            window.onDoneCallback = jest.fn(() => {
                callOrder.push('done');
                return true;
            });
            window.onUpdateCallback = jest.fn(() => {
                callOrder.push('update');
            });
            item.dataset.onDone = 'onDoneCallback';
            item.dataset.onUpdate = 'onUpdateCallback';

            const savingHandler = () => {
                callOrder.push('saving');
            };
            document.addEventListener('editarray:edit-saving', savingHandler);

            // Act
            toggleEditMode('test-container-item-0');

            // Assert
            expect(callOrder).toEqual(['done', 'saving', 'update']);
            document.removeEventListener('editarray:edit-saving', savingHandler);
        });

        // Validates cancelable save event lets external validators prevent invalid transitions.
        it('does not leave edit mode when edit-saving event is prevented', () => {
            // Arrange
            const item = addItemAndGet('0');
            const cancelHandler = (event) => event.preventDefault();
            document.addEventListener('editarray:edit-saving', cancelHandler);

            // Act
            toggleEditMode('test-container-item-0');

            // Assert
            expect(item.querySelector('.display-container').classList.contains('ea-hidden')).toBe(true);
            expect(item.querySelector('.edit-container').classList.contains('ea-hidden')).toBe(false);

            document.removeEventListener('editarray:edit-saving', cancelHandler);
        });

        // Validates entering edit mode emits event for consumers that wire dynamic validation.
        it('emits edit-entered when switching from display to edit mode', () => {
            // Arrange
            const item = addItemAndGet('0');
            toggleEditMode('test-container-item-0');
            const eventSpy = jest.fn();
            document.addEventListener('editarray:edit-entered', eventSpy);

            // Act
            toggleEditMode('test-container-item-0');

            // Assert
            expect(item.querySelector('.display-container').classList.contains('ea-hidden')).toBe(true);
            expect(item.querySelector('.edit-container').classList.contains('ea-hidden')).toBe(false);
            expect(eventSpy).toHaveBeenCalledTimes(1);
            expect(eventSpy.mock.calls[0][0].detail.itemId).toBe('test-container-item-0');
        });

        it('reverts row values and returns to display mode when cancel action is invoked', () => {
            // Arrange
            const item = addItemAndGet('0');
            const input = item.querySelector('input[type="text"]');
            input.value = 'Alice';
            toggleEditMode(item.id); // Save initial value to display mode

            toggleEditMode(item.id); // Enter edit mode
            input.value = 'Bob';

            const cancelButton = item.querySelector('#test-container-item-0-edit button[data-action="cancel"]');

            // Act
            handleEditArrayAction({ target: cancelButton });

            // Assert
            expect(item.querySelector('.display-container').classList.contains(HIDDEN_CLASS)).toBe(false);
            expect(item.querySelector('.edit-container').classList.contains(HIDDEN_CLASS)).toBe(true);
            expect(input.value).toBe('Alice');
            expect(item.querySelector('[data-display-for="Input_0"]').textContent).toBe('Alice');
        });
    });

    // Validates deletion paths differ for persisted items vs unsaved new items.
    describe('delete and undelete flow', () => {
        // Validates unsaved new rows are removed immediately and placeholder visibility is restored.
        it('removes unsaved new item entirely and re-enables Add', () => {
            // Arrange
            const item = addItemAndGet('0');
            const addButton = document.getElementById('test-container-add');

            // Act
            markForDeletion(item.id);

            // Assert
            const itemsContainer = document.getElementById('test-container-items');
            expect(document.getElementById('test-container-item-0')).toBeFalsy();
            expect(addButton.disabled).toBe(false);
            expect(itemsContainer.querySelector('.edit-array-placeholder').classList.contains('ea-hidden')).toBe(false);
            expect(document.activeElement).toBe(addButton);
        });

        // Validates persisted rows toggle deleted state, edit availability, and delete callback.
        it('marks existing item as deleted and can restore it', () => {
            // Arrange
            const item = addItemAndGet('0');
            window.onDeleteCallback = jest.fn();
            item.dataset.onDelete = 'onDeleteCallback';
            toggleEditMode(item.id);

            // Act
            markForDeletion(item.id);

            // Assert
            const deleteButton = item.querySelector('.delete-item-btn');
            const editButton = item.querySelector('.edit-item-btn');
            const isDeletedInput = item.querySelector('input[data-is-deleted-marker]');

            expect(item.getAttribute('data-deleted')).toBe('true');
            expect(item.classList.contains('deleted')).toBe(true);
            expect(isDeletedInput.value).toBe('true');
            expect(deleteButton.textContent).toBe('Undelete');
            expect(editButton.disabled).toBe(true);
            expect(window.onDeleteCallback).toHaveBeenCalledWith(item.id);

            // Act
            markForDeletion(item.id);

            // Assert
            expect(item.getAttribute('data-deleted')).toBeNull();
            expect(item.classList.contains('deleted')).toBe(false);
            expect(isDeletedInput.value).toBe('false');
            expect(deleteButton.textContent).toBe('Delete');
            expect(editButton.disabled).toBe(false);
        });
    });

    // Validates reordering only happens when explicitly enabled and keeps model binding indexes valid.
    describe('reorder and renumber behavior', () => {
        // Validates move operation changes DOM order and surfaces duplicate-ID renumber warnings.
        it('reorders items and keeps original ids when duplicate-id guard skips renumbering', () => {
            // Arrange
            setupFixture({ reorderEnabled: true });
            addNewItem('test-container', 'test-template');
            addNewItem('test-container', 'test-template');
            document.querySelector('#test-container-item-0 input[type="text"]').value = 'first';
            document.querySelector('#test-container-item-1 input[type="text"]').value = 'second';
            const warnSpy = jest.spyOn(console, 'warn').mockImplementation(() => {});

            // Act
            moveItem('test-container', 'test-container-item-0', 1);

            // Assert
            const items = document.querySelectorAll('#test-container-items .edit-array-item');
            expect(items).toHaveLength(2);
            expect(items[0].id).toBe('test-container-item-1');
            expect(items[1].id).toBe('test-container-item-0');
            expect(items[0].querySelector('input[type="text"]').value).toBe('second');
            expect(items[0].querySelector('input[type="text"]').id).toBe('Input_1');
            expect(items[0].querySelector('input[type="text"]').name).toBe('Input[1]');
            expect(items[0].querySelector('.delete-item-btn').dataset.itemId).toBe('test-container-item-1');
            expect(items[1].querySelector('.delete-item-btn').dataset.itemId).toBe('test-container-item-0');
            expect(warnSpy).toHaveBeenCalled();

            const movedItem = items[1];
            const expectedFocusedButton = movedItem.querySelector('[data-action="move"][data-direction="1"]');
            expect(document.activeElement).toBe(expectedFocusedButton);
        });

        // Validates guard clause blocks reordering when container has not opted in.
        it('does not reorder when reordering is disabled', () => {
            // Arrange
            addNewItem('test-container', 'test-template');
            addNewItem('test-container', 'test-template');
            const warnSpy = jest.spyOn(console, 'warn').mockImplementation(() => {});

            // Act
            moveItem('test-container', 'test-container-item-0', 1);

            // Assert
            const items = document.querySelectorAll('#test-container-items .edit-array-item');
            expect(items[0].id).toBe('test-container-item-0');
            expect(items[1].id).toBe('test-container-item-1');
            expect(warnSpy).toHaveBeenCalled();
        });
    });

    // Validates unobtrusive parsing integration behavior with and without jQuery present.
    describe('validation integration', () => {
        // Validates helper no-ops safely when jQuery validation is unavailable.
        it('no-ops when jQuery unobtrusive validation is not loaded', () => {
            // Arrange
            const element = document.getElementById('test-container');

            // Act / Assert
            expect(() => refreshUnobtrusiveValidation(element)).not.toThrow();
        });

        // Validates helper reparses form and wires eager onfocusout validation when available.
        it('reparses unobtrusive validation and sets onfocusout behavior when validator exists', () => {
            // Arrange
            const validator = {
                settings: {},
                checkable: jest.fn(() => false),
                element: jest.fn()
            };
            const mockForm = {
                length: 1,
                removeData: jest.fn().mockReturnThis(),
                data: jest.fn(() => validator)
            };
            const closest = jest.fn(() => mockForm);
            global.$ = jest.fn(() => ({ closest }));
            global.$.validator = {
                unobtrusive: {
                    parse: jest.fn()
                }
            };

            // Act
            refreshUnobtrusiveValidation(document.getElementById('test-container'));

            // Assert
            expect(global.$.validator.unobtrusive.parse).toHaveBeenCalledWith(mockForm);
            expect(typeof validator.settings.onfocusout).toBe('function');
            const fakeField = document.createElement('input');
            validator.settings.onfocusout.call(validator, fakeField);
            expect(validator.element).toHaveBeenCalledWith(fakeField);
        });
    });

    // Validates helper token replacement behavior used by renumbering.
    describe('replaceIndexTokens', () => {
        // Validates fast path avoids unnecessary processing when no markers are present.
        it('returns unchanged value when no markers or old id are present', () => {
            expect(replaceIndexTokens('plain-text', 3, null, null)).toBe('plain-text');
        });

        // Validates bracket-index pattern replacement used in name attributes.
        it('replaces [n] bracket patterns with the new index', () => {
            expect(replaceIndexTokens('Input[0]', 5, null, null)).toBe('Input[5]');
        });

        // Validates underscore-index pattern replacement used in id attributes.
        it('replaces _n__ underscore patterns with the new index', () => {
            expect(replaceIndexTokens('Input_0__Name', 3, null, null)).toBe('Input_3__Name');
        });

        // Validates __newItem__ marker pattern replacement.
        it('replaces __newItem__n patterns with the new index', () => {
            expect(replaceIndexTokens('__newItem__0', 7, null, null)).toBe('__newItem__7');
        });

        // Validates -item-n pattern replacement used in element IDs.
        it('replaces -item-n patterns with the new index', () => {
            expect(replaceIndexTokens('container-item-0', 2, null, null)).toBe('container-item-2');
        });

        // Validates oldId→newId substitution for renumbering container-scoped IDs.
        it('replaces oldId with newId when both are provided', () => {
            expect(replaceIndexTokens(
                'test-container-item-0',
                1,
                'test-container-item-0',
                'test-container-item-1'
            )).toBe('test-container-item-1');
        });

        // Validates compound values with multiple token patterns are all replaced.
        it('handles values with multiple patterns in a single string', () => {
            const value = 'Input[0].Items[0].Name';
            expect(replaceIndexTokens(value, 2, null, null)).toBe('Input[2].Items[2].Name');
        });
    });

    // Validates show/hide helpers manage the ea-hidden CSS class correctly.
    describe('showElement and hideElement', () => {
        it('showElement removes ea-hidden class from an element', () => {
            const el = document.createElement('div');
            el.classList.add(HIDDEN_CLASS);

            showElement(el);

            expect(el.classList.contains(HIDDEN_CLASS)).toBe(false);
        });

        it('hideElement adds ea-hidden class to an element', () => {
            const el = document.createElement('div');

            hideElement(el);

            expect(el.classList.contains(HIDDEN_CLASS)).toBe(true);
        });
    });

    // Validates exported constants match the values used in the DOM contract.
    describe('DOM contract constants', () => {
        it('exports expected suffix and selector constants', () => {
            expect(HIDDEN_CLASS).toBe('ea-hidden');
            expect(SUFFIX_ITEMS).toBe('-items');
            expect(SUFFIX_DISPLAY).toBe('-display');
            expect(SUFFIX_EDIT).toBe('-edit');
            expect(SUFFIX_ADD).toBe('-add');
            expect(SELECTOR_ITEM).toBe('.edit-array-item');
            expect(SELECTOR_PLACEHOLDER).toBe('.edit-array-placeholder');
            expect(SELECTOR_CONTAINER).toBe('.edit-array-container');
        });
    });

    // Validates container ID extraction from item IDs used by toggleEditMode.
    describe('getContainerIdFromItemId', () => {
        it('extracts container id from a well-formed item id', () => {
            expect(getContainerIdFromItemId('my-container-item-0')).toBe('my-container');
        });

        it('returns null for a malformed item id without -item-n suffix', () => {
            expect(getContainerIdFromItemId('no-suffix-here')).toBeNull();
        });

        it('returns null when given null or empty string', () => {
            expect(getContainerIdFromItemId(null)).toBeNull();
            expect(getContainerIdFromItemId('')).toBeNull();
        });

        it('handles nested container names with hyphens', () => {
            expect(getContainerIdFromItemId('outer-inner-container-item-5')).toBe('outer-inner-container');
        });
    });

    // Validates resolveItemId resolves "closest" ancestor or returns literal data-item-id.
    describe('resolveItemId', () => {
        it('returns the closest edit-array-item ancestor id when data-item-id is "closest"', () => {
            // Arrange
            const item = addItemAndGet('0');
            const button = item.querySelector('.edit-item-btn');
            button.dataset.itemId = 'closest';

            // Act
            const resolved = resolveItemId(button);

            // Assert
            expect(resolved).toBe('test-container-item-0');
        });

        it('returns the literal data-item-id value when it is not "closest"', () => {
            // Arrange
            const button = document.createElement('button');
            button.dataset.itemId = 'explicit-item-3';

            // Act / Assert
            expect(resolveItemId(button)).toBe('explicit-item-3');
        });

        it('returns null when data-item-id is absent', () => {
            const button = document.createElement('button');
            expect(resolveItemId(button)).toBeNull();
        });

        it('returns null when closest ancestor has no id', () => {
            // Arrange - button with "closest" but no .edit-array-item ancestor
            const button = document.createElement('button');
            button.dataset.itemId = 'closest';
            document.body.appendChild(button);

            // Act / Assert
            expect(resolveItemId(button)).toBeNull();
            button.remove();
        });
    });

    // Validates updateDisplayFromForm copies input values to matching display spans.
    describe('updateDisplayFromForm rendering', () => {
        it('copies input value to the matching display-for span', () => {
            // Arrange
            const item = addItemAndGet('0');
            const input = item.querySelector('input[type="text"]');
            input.value = 'Bob';

            // Act
            updateDisplayFromForm('test-container-item-0');

            // Assert
            const displaySpan = item.querySelector('[data-display-for="Input_0"]');
            expect(displaySpan.textContent).toBe('Bob');
        });

        it('no-ops safely when item does not exist', () => {
            expect(() => updateDisplayFromForm('nonexistent-item')).not.toThrow();
        });

        it('skips inputs that have no matching display element', () => {
            // Arrange
            const item = addItemAndGet('0');
            const editContainer = item.querySelector('.edit-container');
            const extraInput = document.createElement('input');
            extraInput.id = 'orphan-input';
            extraInput.value = 'should be ignored';
            editContainer.appendChild(extraInput);

            // Act / Assert - should not throw when display target is missing
            expect(() => updateDisplayFromForm('test-container-item-0')).not.toThrow();
        });
    });

    // Validates replaceTemplateTokens replaces __index__ tokens in cloned template fragments.
    describe('replaceTemplateTokens rendering', () => {
        it('replaces __index__ in name, id, data-id, data-display-for, for, and data-valmsg-for', () => {
            // Arrange
            const template = document.getElementById('test-template');
            const clone = template.content.cloneNode(true);

            // Act
            replaceTemplateTokens(clone, 4);

            // Assert
            const input = clone.querySelector('input[type="text"]');
            expect(input.name).toBe('Input[4]');
            expect(input.id).toBe('Input_4');
            expect(input.getAttribute('data-id')).toBe('input-4');
            expect(input.getAttribute('data-display-for')).toBe('Input_4');

            const label = clone.querySelector('label');
            expect(label.htmlFor).toBe('Input_4');

            const valMsg = clone.querySelector('[data-valmsg-for]');
            expect(valMsg.getAttribute('data-valmsg-for')).toBe('Input[4]');

            const hiddenInput = clone.querySelector('input[data-is-deleted-marker]');
            expect(hiddenInput.name).toBe('Input[4].IsDeleted');
        });
    });

    // Validates removeUnsavedItem directly removes the item and restores container state.
    describe('removeUnsavedItem', () => {
        it('removes the item from the DOM and re-enables the add button', () => {
            // Arrange
            addItemAndGet('0');
            const addButton = document.getElementById('test-container-add');
            expect(addButton.disabled).toBe(true);

            // Act
            removeUnsavedItem('test-container-item-0', 'test-container');

            // Assert
            expect(document.getElementById('test-container-item-0')).toBeNull();
            expect(addButton.disabled).toBe(false);
            expect(document.activeElement).toBe(addButton);
        });

        it('shows placeholder when last item is removed', () => {
            // Arrange
            addItemAndGet('0');
            const placeholder = document.querySelector('.edit-array-placeholder');
            expect(placeholder.classList.contains(HIDDEN_CLASS)).toBe(true);

            // Act
            removeUnsavedItem('test-container-item-0', 'test-container');

            // Assert
            expect(placeholder.classList.contains(HIDDEN_CLASS)).toBe(false);
        });

        it('does not show placeholder when other items remain', () => {
            // Arrange
            addItemAndGet('0');
            toggleEditMode('test-container-item-0');
            addItemAndGet('1');

            // Act
            removeUnsavedItem('test-container-item-1', 'test-container');

            // Assert
            const placeholder = document.querySelector('.edit-array-placeholder');
            expect(placeholder.classList.contains(HIDDEN_CLASS)).toBe(true);
        });

        it('no-ops safely when item does not exist', () => {
            expect(() => removeUnsavedItem('nonexistent', 'test-container')).not.toThrow();
        });

        it('no-ops safely when containerId is null', () => {
            // Arrange
            addItemAndGet('0');

            // Act / Assert
            expect(() => removeUnsavedItem('test-container-item-0', null)).not.toThrow();
            expect(document.getElementById('test-container-item-0')).toBeNull();
        });
    });

    // Validates updateAttributeWithIndex updates individual attributes with index tokens.
    describe('updateAttributeWithIndex', () => {
        it('updates an id attribute using replaceIndexTokens', () => {
            // Arrange
            const el = document.createElement('div');
            el.id = 'container-item-0';

            // Act
            updateAttributeWithIndex(el, 'id', 3, 'container-item-0', 'container-item-3');

            // Assert
            expect(el.id).toBe('container-item-3');
        });

        it('updates a name attribute with bracket index pattern', () => {
            // Arrange
            const input = document.createElement('input');
            input.name = 'Field[0].Value';

            // Act
            updateAttributeWithIndex(input, 'name', 2, null, null);

            // Assert
            expect(input.name).toBe('Field[2].Value');
        });

        it('updates htmlFor on label elements via the "for" attribute name', () => {
            // Arrange - use _n__ pattern that matches the underscore-index regex
            const label = document.createElement('label');
            label.htmlFor = 'Input_0__Name';

            // Act
            updateAttributeWithIndex(label, 'for', 5, null, null);

            // Assert
            expect(label.htmlFor).toBe('Input_5__Name');
        });

        it('updates a data-* attribute', () => {
            // Arrange
            const el = document.createElement('span');
            el.setAttribute('data-valmsg-for', 'Items[0].Name');

            // Act
            updateAttributeWithIndex(el, 'data-valmsg-for', 1, null, null);

            // Assert
            expect(el.getAttribute('data-valmsg-for')).toBe('Items[1].Name');
        });

        it('no-ops when element is null', () => {
            expect(() => updateAttributeWithIndex(null, 'id', 0, null, null)).not.toThrow();
        });

        it('no-ops when attribute value is empty', () => {
            // Arrange
            const el = document.createElement('div');

            // Act / Assert - element has no 'name' attribute
            expect(() => updateAttributeWithIndex(el, 'name', 0, null, null)).not.toThrow();
        });
    });

    // Validates renumberItems updates all item IDs and descendant attributes after DOM reorder.
    describe('renumberItems rendering', () => {
        it('skips renumbering and warns when manual swap causes duplicate IDs', () => {
            // Arrange - swap creates a conflict: position 0 wants id "item-0" but it already exists
            setupFixture({ reorderEnabled: true });
            addItemAndGet('0');
            toggleEditMode('test-container-item-0');
            addItemAndGet('1');
            toggleEditMode('test-container-item-1');

            const container = document.getElementById('test-container-items');
            const item0 = document.getElementById('test-container-item-0');
            const item1 = document.getElementById('test-container-item-1');
            container.insertBefore(item1, item0);
            const warnSpy = jest.spyOn(console, 'warn').mockImplementation(() => {});

            // Act
            renumberItems('test-container');

            // Assert - duplicate-ID guard fires, items keep their original IDs
            expect(warnSpy).toHaveBeenCalledWith(expect.stringContaining('Duplicate ID'));
            const items = container.querySelectorAll('.edit-array-item');
            expect(items[0].id).toBe('test-container-item-1');
            expect(items[1].id).toBe('test-container-item-0');
        });

        it('renumbers a single item to index 0', () => {
            // Arrange - single item, no duplicate conflict
            addItemAndGet('0');
            toggleEditMode('test-container-item-0');

            // Act
            renumberItems('test-container');

            // Assert
            const items = document.querySelectorAll('#test-container-items .edit-array-item');
            expect(items).toHaveLength(1);
            expect(items[0].id).toBe('test-container-item-0');
            expect(items[0].querySelector('input[type="text"]').name).toBe('Input[0]');
        });

        it('logs error when container element is not found', () => {
            // Arrange
            const errorSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

            // Act
            renumberItems('nonexistent-container');

            // Assert
            expect(errorSpy).toHaveBeenCalledWith(expect.stringContaining('nonexistent-container'));
        });

        it('no-ops when container has zero items', () => {
            // Act / Assert - should not throw with empty container
            expect(() => renumberItems('test-container')).not.toThrow();
        });
    });

    // Validates handleEditArrayAction dispatches actions based on data-action attribute.
    describe('handleEditArrayAction event delegation', () => {
        it('dispatches edit action for button with data-action="edit"', () => {
            // Arrange
            const item = addItemAndGet('0');
            toggleEditMode('test-container-item-0');
            const editButton = item.querySelector('.edit-item-btn');

            // Act
            handleEditArrayAction({ target: editButton });

            // Assert - should have entered edit mode (display hidden, edit visible)
            expect(item.querySelector('.display-container').classList.contains(HIDDEN_CLASS)).toBe(true);
            expect(item.querySelector('.edit-container').classList.contains(HIDDEN_CLASS)).toBe(false);
        });

        it('dispatches done action for button with data-action="done"', () => {
            // Arrange
            const item = addItemAndGet('0');
            const doneButton = item.querySelector('.done-item-btn');

            // Act
            handleEditArrayAction({ target: doneButton });

            // Assert - should have saved (display visible, edit hidden)
            expect(item.querySelector('.display-container').classList.contains(HIDDEN_CLASS)).toBe(false);
            expect(item.querySelector('.edit-container').classList.contains(HIDDEN_CLASS)).toBe(true);
        });

        it('dispatches delete action for button with data-action="delete"', () => {
            // Arrange
            addItemAndGet('0');
            const item = document.getElementById('test-container-item-0');
            const deleteButton = item.querySelector('.delete-item-btn');

            // Act - new item, so delete should remove it entirely
            handleEditArrayAction({ target: deleteButton });

            // Assert
            expect(document.getElementById('test-container-item-0')).toBeNull();
        });

        it('dispatches cancel action for unsaved rows and removes the item', () => {
            // Arrange
            addItemAndGet('0');
            const item = document.getElementById('test-container-item-0');
            const cancelButton = item.querySelector('#test-container-item-0-edit button[data-action="cancel"]');

            // Act
            handleEditArrayAction({ target: cancelButton });

            // Assert
            expect(document.getElementById('test-container-item-0')).toBeNull();
        });

        it('dispatches move action for button with data-action="move"', () => {
            // Arrange
            setupFixture({ reorderEnabled: true });
            addItemAndGet('0');
            toggleEditMode('test-container-item-0');
            addItemAndGet('1');
            toggleEditMode('test-container-item-1');
            const moveDownBtn = document.querySelector('#test-container-item-0 .move-down-btn');
            const warnSpy = jest.spyOn(console, 'warn').mockImplementation(() => {});

            // Act
            handleEditArrayAction({ target: moveDownBtn });

            // Assert
            const items = document.querySelectorAll('#test-container-items .edit-array-item');
            expect(items[0].id).not.toBe('test-container-item-0');
            warnSpy.mockRestore();
        });

        it('ignores clicks on non-action elements', () => {
            // Arrange
            const span = document.createElement('span');
            document.body.appendChild(span);

            // Act / Assert - should not throw
            expect(() => handleEditArrayAction({ target: span })).not.toThrow();
            span.remove();
        });
    });

    // Validates multi-item rendering produces correct sequential indexes.
    describe('multi-item rendering', () => {
        it('renders sequential items with incrementing indexes', () => {
            // Arrange & Act
            addItemAndGet('0');
            toggleEditMode('test-container-item-0');
            addItemAndGet('1');

            // Assert
            const item0 = document.getElementById('test-container-item-0');
            const item1 = document.getElementById('test-container-item-1');

            expect(item0.querySelector('input[type="text"]').name).toBe('Input[0]');
            expect(item1.querySelector('input[type="text"]').name).toBe('Input[1]');
            expect(item0.querySelector('input[type="text"]').id).toBe('Input_0');
            expect(item1.querySelector('input[type="text"]').id).toBe('Input_1');
            expect(item0.querySelector('.display-container').id).toBe('test-container-item-0-display');
            expect(item1.querySelector('.display-container').id).toBe('test-container-item-1-display');
        });

        it('renders hidden IsDeleted marker as first child for each item', () => {
            // Arrange & Act
            addItemAndGet('0');
            toggleEditMode('test-container-item-0');
            addItemAndGet('1');

            // Assert
            const item0 = document.getElementById('test-container-item-0');
            const item1 = document.getElementById('test-container-item-1');

            expect(item0.firstElementChild.getAttribute('data-is-deleted-marker')).toBe('true');
            expect(item0.firstElementChild.value).toBe('false');
            expect(item1.firstElementChild.getAttribute('data-is-deleted-marker')).toBe('true');
            expect(item1.firstElementChild.value).toBe('false');
        });

        it('cancel button is rendered in the edit section and remains available after save', () => {
            // Arrange
            addItemAndGet('0');
            const item = document.getElementById('test-container-item-0');
            const editContainer = item.querySelector('#test-container-item-0-edit');

            // Assert - cancel visible during new-item edit
            expect(editContainer.querySelector('button[data-action="cancel"]')).toBeTruthy();

            // Act - save
            toggleEditMode('test-container-item-0');

            // Assert - cancel still exists for subsequent edit sessions
            expect(editContainer.querySelector('button[data-action="cancel"]')).toBeTruthy();
            expect(editContainer.classList.contains('ea-hidden')).toBe(true);
        });

        it('new-item hidden marker is rendered during add and removed on save', () => {
            // Arrange
            addItemAndGet('0');
            const item = document.getElementById('test-container-item-0');

            // Assert - marker present during new-item edit
            const marker = item.querySelector('input[data-new-item-marker="true"]');
            expect(marker).toBeTruthy();
            expect(marker.type).toBe('hidden');

            // Act - save
            toggleEditMode('test-container-item-0');

            // Assert - marker removed after save
            expect(item.querySelector('input[data-new-item-marker]')).toBeNull();
        });
    });

    // Validates initEditArray wires event delegation and emits init events.
    describe('initEditArray rendering', () => {
        it('emits editarray:init event for each container on the page', () => {
            // Arrange
            const initSpy = jest.fn();
            document.addEventListener('editarray:init', initSpy);

            // Act
            initEditArray();

            // Assert
            expect(initSpy).toHaveBeenCalledTimes(1);
            expect(initSpy.mock.calls[0][0].detail.container.id).toBe('test-container');

            document.removeEventListener('editarray:init', initSpy);
        });

        it('wires delegated click handling so Add button works after init', () => {
            // Arrange
            initEditArray();
            const addButton = document.getElementById('test-container-add');

            // Act
            fireEvent.click(addButton);

            // Assert
            expect(document.querySelectorAll('#test-container-items .edit-array-item')).toHaveLength(1);
        });
    });
});

// ## Coverage Gaps
// - `initEditArray` can attach duplicate `document` click listeners if called multiple times; no idempotency guard exists.
// - Reordering silently skips an item when duplicate IDs are detected during `renumberItems`; there is no surfaced error state.
// - `replaceIndexTokens` relies on naming conventions (`[n]`, `_n__`, `-item-n`) and may miss atypical templates with different patterns.
// - Callback names (`data-on-*`) are looked up on `window` and non-functions are ignored; there is no warning for misconfiguration.
