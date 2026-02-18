const {
    register,
    validationGuard,
    statusUpdater,
    notifier,
    compose
} = require('../../CK.Taghelpers/wwwroot/js/editArray-callbacks.js');

// Also pull in editArray.js so we can test integration with toggleEditMode
const {
    addNewItem,
    toggleEditMode,
    HIDDEN_CLASS
} = require('../../CK.Taghelpers/wwwroot/js/editArray.js');

function setupFixture() {
    document.body.innerHTML = `
        <form id="test-form">
            <div id="test-container" class="edit-array-container"
                 data-delete-text="Delete" data-undelete-text="Undelete"
                 data-reorder-enabled="false">
                <div id="test-container-items">
                    <div class="edit-array-placeholder">No items yet</div>
                </div>
                <template id="test-template">
                    <div class="edit-array-item">
                        <input type="hidden" data-is-deleted-marker="true"
                               name="Input[__index__].IsDeleted" value="false" />
                        <div class="display-container">
                            <span data-display-for="Input___index__"></span>
                        </div>
                        <div class="edit-container ea-hidden">
                            <label for="Input___index__">Name</label>
                            <input type="text" id="Input___index__"
                                   name="Input[__index__]" data-id="input-__index__"
                                   data-display-for="Input___index__" value="" />
                            <span data-valmsg-for="Input[__index__]"></span>
                        </div>
                        <button type="button" class="edit-item-btn"
                                data-action="edit" data-item-id="closest">Edit</button>
                        <button type="button" class="done-item-btn"
                                data-action="done" data-item-id="closest">Done</button>
                        <button type="button" class="delete-item-btn"
                                data-action="delete" data-item-id="closest">Delete</button>
                    </div>
                </template>
                <button id="test-container-add" type="button"
                        data-action="add" data-container-id="test-container"
                        data-template-id="test-template">Add Item</button>
            </div>
        </form>
        <div id="status-el" class="alert alert-info">Waiting</div>
    `;
}

function addItemAndGet(idSuffix = '0') {
    addNewItem('test-container', 'test-template');
    return document.getElementById(`test-container-item-${idSuffix}`);
}

describe('editArray-callbacks.js', () => {
    beforeEach(() => {
        jest.restoreAllMocks();
        delete global.$;
        // Clean up any registered callbacks
        delete window.testCallback;
        delete window.myGuard;
        delete window.myNotifier;
        setupFixture();
    });

    // ------------------------------------------------------------------
    // register
    // ------------------------------------------------------------------
    describe('register', () => {
        it('assigns function to window[name]', () => {
            const fn = jest.fn();
            register('testCallback', fn);
            expect(window.testCallback).toBe(fn);
        });

        it('throws on invalid identifier name', () => {
            expect(() => register('alert("xss")', jest.fn()))
                .toThrow('not a valid JavaScript identifier');
        });

        it('throws on name starting with digit', () => {
            expect(() => register('123bad', jest.fn()))
                .toThrow('not a valid JavaScript identifier');
        });

        it('throws on hyphenated name', () => {
            expect(() => register('my-callback', jest.fn()))
                .toThrow('not a valid JavaScript identifier');
        });

        it('throws on dotted name', () => {
            expect(() => register('on.done', jest.fn()))
                .toThrow('not a valid JavaScript identifier');
        });

        it('allows underscore-prefixed name', () => {
            const fn = jest.fn();
            register('_private', fn);
            expect(window._private).toBe(fn);
            delete window._private;
        });

        it('allows dollar-prefixed name', () => {
            const fn = jest.fn();
            register('$handler', fn);
            expect(window.$handler).toBe(fn);
            delete window.$handler;
        });

        it('throws when fn is not a function', () => {
            expect(() => register('validName', 'not-a-function'))
                .toThrow('must be a function');
        });
    });

    // ------------------------------------------------------------------
    // validationGuard
    // ------------------------------------------------------------------
    describe('validationGuard', () => {
        it('returns true when no validation errors exist', () => {
            const guard = validationGuard();
            const item = addItemAndGet('0');
            // Switch to display to set up state, then back to edit
            toggleEditMode(item.id);

            // Item is now in display mode; re-enter edit
            toggleEditMode(item.id);

            // Now call guard — no errors in spans
            const result = guard(item.id);
            expect(result).toBe(true);
        });

        it('returns false when validation spans have error text', () => {
            const guard = validationGuard();
            const item = addItemAndGet('0');

            // Add error text to the validation span
            const editContainer = document.getElementById(item.id + '-edit');
            const span = editContainer.querySelector('span[data-valmsg-for]');
            span.textContent = 'Name is required';

            const result = guard(item.id);
            expect(result).toBe(false);
        });

        it('returns false when validation spans have error class', () => {
            const guard = validationGuard();
            const item = addItemAndGet('0');

            const editContainer = document.getElementById(item.id + '-edit');
            const span = editContainer.querySelector('span[data-valmsg-for]');
            span.classList.add('field-validation-error');

            const result = guard(item.id);
            expect(result).toBe(false);
        });

        it('updates status element on valid result', () => {
            const guard = validationGuard({
                statusElementId: 'status-el',
                validMessage: 'OK',
                validClass: 'pass'
            });
            const item = addItemAndGet('0');

            guard(item.id);

            const statusEl = document.getElementById('status-el');
            expect(statusEl.textContent).toBe('OK');
            expect(statusEl.className).toBe('pass');
        });

        it('updates status element on invalid result', () => {
            const guard = validationGuard({
                statusElementId: 'status-el',
                invalidMessage: 'Fix errors',
                invalidClass: 'fail'
            });
            const item = addItemAndGet('0');

            const editContainer = document.getElementById(item.id + '-edit');
            const span = editContainer.querySelector('span[data-valmsg-for]');
            span.textContent = 'Required';

            guard(item.id);

            const statusEl = document.getElementById('status-el');
            expect(statusEl.textContent).toBe('Fix errors');
            expect(statusEl.className).toBe('fail');
        });

        it('uses jQuery unobtrusive validation when available', () => {
            const guard = validationGuard();
            const item = addItemAndGet('0');

            // Mock jQuery unobtrusive validation
            const mockValidator = {
                element: jest.fn(() => true)
            };
            const mockJQuery = jest.fn((selector) => ({
                data: jest.fn(() => mockValidator),
                validate: jest.fn(() => mockValidator),
                find: jest.fn(() => ({
                    toArray: () => {
                        const editContainer = document.getElementById(item.id + '-edit');
                        return Array.from(editContainer.querySelectorAll('input, select, textarea'));
                    }
                }))
            }));
            mockJQuery.validator = {
                unobtrusive: {
                    parse: jest.fn()
                }
            };
            global.$ = mockJQuery;

            const result = guard(item.id);

            expect(result).toBe(true);
            expect(mockJQuery.validator.unobtrusive.parse).toHaveBeenCalled();
        });

        it('blocks toggleEditMode when registered as onDone and validation fails', () => {
            const guard = validationGuard();
            register('myGuard', guard);

            const item = addItemAndGet('0');
            item.dataset.onDone = 'myGuard';

            // Add validation error
            const editContainer = document.getElementById(item.id + '-edit');
            const span = editContainer.querySelector('span[data-valmsg-for]');
            span.textContent = 'Error';

            // Try to switch from edit to display — should be blocked
            toggleEditMode(item.id);

            // Display container should still be hidden (edit mode stays)
            const displayContainer = document.getElementById(item.id + '-display');
            expect(displayContainer.classList.contains(HIDDEN_CLASS)).toBe(true);
        });
    });

    // ------------------------------------------------------------------
    // statusUpdater
    // ------------------------------------------------------------------
    describe('statusUpdater', () => {
        it('updates element text with static message', () => {
            const updater = statusUpdater({
                elementId: 'status-el',
                message: 'Item saved'
            });
            updater('test-container-item-0');

            const el = document.getElementById('status-el');
            expect(el.textContent).toBe('Item saved');
        });

        it('updates element text with function message', () => {
            const updater = statusUpdater({
                elementId: 'status-el',
                message: (itemId) => `Updated: ${itemId}`
            });
            updater('test-container-item-0');

            const el = document.getElementById('status-el');
            expect(el.textContent).toBe('Updated: test-container-item-0');
        });

        it('replaces className when provided', () => {
            const updater = statusUpdater({
                elementId: 'status-el',
                message: 'Done',
                className: 'alert alert-success'
            });
            updater('test-container-item-0');

            const el = document.getElementById('status-el');
            expect(el.className).toBe('alert alert-success');
        });

        it('does not replace className when not provided', () => {
            const updater = statusUpdater({
                elementId: 'status-el',
                message: 'Done'
            });
            updater('test-container-item-0');

            const el = document.getElementById('status-el');
            expect(el.className).toBe('alert alert-info'); // original class
        });

        it('no-ops when element does not exist', () => {
            const updater = statusUpdater({
                elementId: 'nonexistent',
                message: 'test'
            });
            expect(() => updater('item-0')).not.toThrow();
        });
    });

    // ------------------------------------------------------------------
    // notifier
    // ------------------------------------------------------------------
    describe('notifier', () => {
        it('logs to console with default prefix', () => {
            const spy = jest.spyOn(console, 'log').mockImplementation();
            const notify = notifier({ message: 'Item saved' });

            notify('item-0');

            expect(spy).toHaveBeenCalledWith(
                '[EditArray] Item saved (item: item-0)'
            );
        });

        it('logs with custom prefix', () => {
            const spy = jest.spyOn(console, 'log').mockImplementation();
            const notify = notifier({ prefix: 'MyApp', message: 'Updated' });

            notify('item-0');

            expect(spy).toHaveBeenCalledWith(
                '[MyApp] Updated (item: item-0)'
            );
        });

        it('supports function message', () => {
            const spy = jest.spyOn(console, 'log').mockImplementation();
            const notify = notifier({
                message: (id) => `Saved ${id}`
            });

            notify('item-0');

            expect(spy).toHaveBeenCalledWith(
                '[EditArray] Saved item-0 (item: item-0)'
            );
        });

        it('calls toast function when provided', () => {
            jest.spyOn(console, 'log').mockImplementation();
            const toastFn = jest.fn();
            const notify = notifier({
                message: 'Saved',
                level: 'success',
                toast: toastFn
            });

            notify('item-0');

            expect(toastFn).toHaveBeenCalledWith('Saved', 'success');
        });

        it('uses default level "info" when not specified', () => {
            jest.spyOn(console, 'log').mockImplementation();
            const toastFn = jest.fn();
            const notify = notifier({
                message: 'test',
                toast: toastFn
            });

            notify('item-0');

            expect(toastFn).toHaveBeenCalledWith('test', 'info');
        });

        it('does not call toast when not provided', () => {
            jest.spyOn(console, 'log').mockImplementation();
            const notify = notifier({ message: 'test' });

            // Should not throw
            expect(() => notify('item-0')).not.toThrow();
        });
    });

    // ------------------------------------------------------------------
    // compose
    // ------------------------------------------------------------------
    describe('compose', () => {
        it('runs all functions in order', () => {
            const order = [];
            const fn1 = jest.fn(() => order.push('first'));
            const fn2 = jest.fn(() => order.push('second'));
            const fn3 = jest.fn(() => order.push('third'));

            const composed = compose(fn1, fn2, fn3);
            composed('item-0');

            expect(order).toEqual(['first', 'second', 'third']);
            expect(fn1).toHaveBeenCalledWith('item-0');
            expect(fn2).toHaveBeenCalledWith('item-0');
            expect(fn3).toHaveBeenCalledWith('item-0');
        });

        it('stops and returns false when a function returns false', () => {
            const fn1 = jest.fn(() => true);
            const fn2 = jest.fn(() => false);
            const fn3 = jest.fn();

            const composed = compose(fn1, fn2, fn3);
            const result = composed('item-0');

            expect(result).toBe(false);
            expect(fn1).toHaveBeenCalled();
            expect(fn2).toHaveBeenCalled();
            expect(fn3).not.toHaveBeenCalled();
        });

        it('does not stop on undefined return (only strict false)', () => {
            const fn1 = jest.fn(); // returns undefined
            const fn2 = jest.fn(() => null);
            const fn3 = jest.fn();

            const composed = compose(fn1, fn2, fn3);
            composed('item-0');

            expect(fn3).toHaveBeenCalled();
        });

        it('works with validationGuard and notifier together', () => {
            jest.spyOn(console, 'log').mockImplementation();
            const item = addItemAndGet('0');

            const composed = compose(
                validationGuard(),
                notifier({ message: 'Validation passed' })
            );

            const result = composed(item.id);

            // No validation errors, so guard passes and notifier runs
            expect(result).not.toBe(false);
            expect(console.log).toHaveBeenCalled();
        });

        it('stops at guard when validation fails', () => {
            jest.spyOn(console, 'log').mockImplementation();
            const item = addItemAndGet('0');

            // Add validation error
            const editContainer = document.getElementById(item.id + '-edit');
            const span = editContainer.querySelector('span[data-valmsg-for]');
            span.textContent = 'Required';

            const composed = compose(
                validationGuard(),
                notifier({ message: 'Should not fire' })
            );

            const result = composed(item.id);

            expect(result).toBe(false);
            expect(console.log).not.toHaveBeenCalled();
        });
    });

    // ------------------------------------------------------------------
    // Integration: registered callbacks work with editArray.js
    // ------------------------------------------------------------------
    describe('integration with editArray.js', () => {
        it('registered statusUpdater fires on onUpdate', () => {
            register('testCallback', statusUpdater({
                elementId: 'status-el',
                message: (id) => `Saved: ${id}`
            }));

            const item = addItemAndGet('0');
            item.dataset.onUpdate = 'testCallback';

            // Toggle from edit → display to trigger onUpdate
            toggleEditMode(item.id);

            expect(document.getElementById('status-el').textContent)
                .toBe('Saved: test-container-item-0');
        });

        it('registered composed guard+notifier blocks invalid transition', () => {
            jest.spyOn(console, 'log').mockImplementation();

            register('myGuard', compose(
                validationGuard(),
                notifier({ message: 'Passed' })
            ));

            const item = addItemAndGet('0');
            item.dataset.onDone = 'myGuard';

            // Add validation error
            const editContainer = document.getElementById(item.id + '-edit');
            const span = editContainer.querySelector('span[data-valmsg-for]');
            span.textContent = 'Error';

            toggleEditMode(item.id);

            // Should still be in edit mode
            const displayContainer = document.getElementById(item.id + '-display');
            expect(displayContainer.classList.contains(HIDDEN_CLASS)).toBe(true);
            // Notifier should NOT have been called
            expect(console.log).not.toHaveBeenCalled();
        });
    });

    // ------------------------------------------------------------------
    // CommonJS export
    // ------------------------------------------------------------------
    describe('module exports', () => {
        it('exports all public members', () => {
            const mod = require('../../CK.Taghelpers/wwwroot/js/editArray-callbacks.js');
            expect(typeof mod.register).toBe('function');
            expect(typeof mod.validationGuard).toBe('function');
            expect(typeof mod.statusUpdater).toBe('function');
            expect(typeof mod.notifier).toBe('function');
            expect(typeof mod.compose).toBe('function');
        });
    });
});
