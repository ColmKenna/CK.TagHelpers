
const { fireEvent } = require('@testing-library/dom');

const {
    addNewItem,
    toggleEditMode,
    markForDeletion,
    moveItem,
    refreshUnobtrusiveValidation,
    initEditArray,
    replaceIndexTokens
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
                        </div>
                        <button type="button" class="edit-item-btn" data-action="edit" data-item-id="closest">Edit</button>
                        <button type="button" class="done-item-btn" data-action="done" data-item-id="closest">Done</button>
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
            expect(item.querySelector('button[data-cancel="cancel"]')).toBeTruthy();
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
            expect(item.querySelector('button[data-cancel]')).toBeFalsy();
            expect(item.querySelector('input[data-new-item-marker]')).toBeFalsy();
            expect(document.getElementById('test-container-add').disabled).toBe(false);
        });

        // Validates onDone hook can veto save transition.
        it('does not leave edit mode when onDone returns false', () => {
            // Arrange
            const item = addItemAndGet('0');
            window.onDoneCallback = jest.fn(() => false);
            item.dataset.onDone = 'onDoneCallback';

            // Act
            toggleEditMode('test-container-item-0');

            // Assert
            expect(window.onDoneCallback).toHaveBeenCalledWith('test-container-item-0');
            expect(item.querySelector('.display-container').classList.contains('ea-hidden')).toBe(true);
            expect(item.querySelector('.edit-container').classList.contains('ea-hidden')).toBe(false);
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
    });
});

// ## Coverage Gaps
// - `initEditArray` can attach duplicate `document` click listeners if called multiple times; no idempotency guard exists.
// - Reordering silently skips an item when duplicate IDs are detected during `renumberItems`; there is no surfaced error state.
// - `replaceIndexTokens` relies on naming conventions (`[n]`, `_n__`, `-item-n`) and may miss atypical templates with different patterns.
// - Callback names (`data-on-*`) are looked up on `window` and non-functions are ignored; there is no warning for misconfiguration.
