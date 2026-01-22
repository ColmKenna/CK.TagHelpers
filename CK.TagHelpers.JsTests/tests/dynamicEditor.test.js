/**
 * Tests for dynamicEditor.js
 */

const fs = require('fs');
const path = require('path');

/**
 * Helper to mock dialog element methods that jsdom doesn't implement
 */
function mockDialogElement(dialog) {
  if (dialog) {
    dialog.showModal = jest.fn(() => dialog.setAttribute('open', ''));
    dialog.close = jest.fn(() => dialog.removeAttribute('open'));
  }
  return dialog;
}

// Load the script before each test suite
beforeEach(() => {
  // Reset DOM
  document.body.innerHTML = '';

  // Clear window.DynamicEditor if it exists
  delete window.DynamicEditor;

  // Load the script
  const scriptPath = path.resolve(__dirname, '../../CK.TagHelpers/wwwroot/js/dynamicEditor.js');
  const scriptContent = fs.readFileSync(scriptPath, 'utf8');
  eval(scriptContent);
});

describe('DynamicEditor', () => {
  describe('initialization', () => {
    test('should expose init and destroy methods on window.DynamicEditor', () => {
      expect(window.DynamicEditor).toBeDefined();
      expect(typeof window.DynamicEditor.init).toBe('function');
      expect(typeof window.DynamicEditor.destroy).toBe('function');
    });

    test('should not initialize if dialog element does not exist', () => {
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation();

      window.DynamicEditor.init('non-existent-dialog');

      // Should not throw, just silently return
      expect(consoleSpy).not.toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    test('should warn if data-event-name is missing', () => {
      const consoleSpy = jest.spyOn(console, 'warn').mockImplementation();

      document.body.innerHTML = `
        <dialog id="test-dialog">
          <form id="test-dialog-form">
            <input name="field1" value="test" />
          </form>
          <button id="test-dialog-confirm">Confirm</button>
          <button id="test-dialog-cancel">Cancel</button>
        </dialog>
      `;
      mockDialogElement(document.getElementById('test-dialog'));

      window.DynamicEditor.init('test-dialog');

      expect(consoleSpy).toHaveBeenCalledWith(
        'DynamicEditor: data-event-name is missing or empty.',
        expect.objectContaining({ dialogId: 'test-dialog' })
      );
      consoleSpy.mockRestore();
    });

    test('should error if required elements are missing', () => {
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation();

      document.body.innerHTML = `
        <dialog id="test-dialog" data-event-name="test">
        </dialog>
      `;
      mockDialogElement(document.getElementById('test-dialog'));

      window.DynamicEditor.init('test-dialog');

      expect(consoleSpy).toHaveBeenCalledWith(
        'DynamicEditor: Could not find required elements',
        expect.any(Object)
      );
      consoleSpy.mockRestore();
    });

    test('should initialize successfully with all required elements', () => {
      document.body.innerHTML = `
        <dialog id="test-dialog" data-event-name="test">
          <form id="test-dialog-form">
            <input name="field1" value="test" />
          </form>
          <button id="test-dialog-confirm">Confirm</button>
          <button id="test-dialog-cancel">Cancel</button>
        </dialog>
      `;
      mockDialogElement(document.getElementById('test-dialog'));

      window.DynamicEditor.init('test-dialog');

      // Should not throw
      expect(true).toBe(true);
    });

    test('should prevent double initialization', () => {
      document.body.innerHTML = `
        <dialog id="test-dialog" data-event-name="test">
          <form id="test-dialog-form">
            <input name="field1" value="test" />
          </form>
          <button id="test-dialog-confirm">Confirm</button>
          <button id="test-dialog-cancel">Cancel</button>
        </dialog>
      `;
      mockDialogElement(document.getElementById('test-dialog'));

      window.DynamicEditor.init('test-dialog');
      window.DynamicEditor.init('test-dialog'); // Should not throw or cause issues

      expect(true).toBe(true);
    });
  });

  describe('form state management', () => {
    let dialog;

    beforeEach(() => {
      document.body.innerHTML = `
        <dialog id="test-dialog" data-event-name="testEvent">
          <form id="test-dialog-form">
            <input name="textField" type="text" value="initial" />
            <input name="checkField" type="checkbox" checked />
            <select name="selectField" multiple>
              <option value="a" selected>A</option>
              <option value="b">B</option>
              <option value="c" selected>C</option>
            </select>
          </form>
          <button id="test-dialog-confirm">Confirm</button>
          <button id="test-dialog-cancel">Cancel</button>
        </dialog>
      `;
      dialog = mockDialogElement(document.getElementById('test-dialog'));
      window.DynamicEditor.init('test-dialog');
    });

    test('should capture form state when dialog opens', async () => {
      // Simulate opening the dialog by setting the open attribute
      dialog.setAttribute('open', '');

      // Wait for MutationObserver to process
      await new Promise(resolve => setTimeout(resolve, 0));

      // The MutationObserver should have captured state
      // We verify this by changing values and then canceling
      const textInput = document.querySelector('input[name="textField"]');
      textInput.value = 'changed';

      // Click cancel to restore
      const cancelBtn = document.getElementById('test-dialog-cancel');
      cancelBtn.click();

      // Value should be restored to 'initial'
      expect(textInput.value).toBe('initial');
    });

    test('should restore checkbox state on cancel', async () => {
      const checkbox = document.querySelector('input[name="checkField"]');

      dialog.setAttribute('open', '');

      // Wait for MutationObserver to process
      await new Promise(resolve => setTimeout(resolve, 0));

      // Initially checked, uncheck it
      checkbox.checked = false;

      // Click cancel
      const cancelBtn = document.getElementById('test-dialog-cancel');
      cancelBtn.click();

      expect(checkbox.checked).toBe(true);
    });

    test('should restore multi-select state on cancel', async () => {
      const select = document.querySelector('select[name="selectField"]');

      dialog.setAttribute('open', '');

      // Wait for MutationObserver to process
      await new Promise(resolve => setTimeout(resolve, 0));

      // Initially a and c are selected, change to just b
      select.options[0].selected = false;
      select.options[1].selected = true;
      select.options[2].selected = false;

      // Click cancel
      const cancelBtn = document.getElementById('test-dialog-cancel');
      cancelBtn.click();

      expect(select.options[0].selected).toBe(true); // a
      expect(select.options[1].selected).toBe(false); // b
      expect(select.options[2].selected).toBe(true); // c
    });
  });

  describe('event dispatching', () => {
    let dialog;

    beforeEach(() => {
      document.body.innerHTML = `
        <dialog id="test-dialog" data-event-name="testEvent">
          <form id="test-dialog-form">
            <input name="field1" type="text" value="test value" />
          </form>
          <button id="test-dialog-confirm">Confirm</button>
          <button id="test-dialog-cancel">Cancel</button>
        </dialog>
      `;
      dialog = mockDialogElement(document.getElementById('test-dialog'));
      window.DynamicEditor.init('test-dialog');
    });

    test('should dispatch update event on confirm', () => {
      dialog.setAttribute('open', '');

      let receivedEvent = null;
      dialog.addEventListener('testEvent-update', (e) => {
        receivedEvent = e;
      });

      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      expect(receivedEvent).not.toBeNull();
      expect(receivedEvent.detail).toEqual({ field1: 'test value' });
    });

    test('should dispatch cancel event on cancel', async () => {
      dialog.setAttribute('open', '');

      // Wait for MutationObserver to process
      await new Promise(resolve => setTimeout(resolve, 0));

      let receivedEvent = null;
      dialog.addEventListener('testEvent-cancel', (e) => {
        receivedEvent = e;
      });

      // Change value before canceling
      const input = document.querySelector('input[name="field1"]');
      input.value = 'changed value';

      const cancelBtn = document.getElementById('test-dialog-cancel');
      cancelBtn.click();

      expect(receivedEvent).not.toBeNull();
      expect(receivedEvent.detail.originalValues).toEqual({ field1: 'test value' });
      expect(receivedEvent.detail.canceledValues).toEqual({ field1: 'changed value' });
    });

    test('should close dialog on confirm', () => {
      dialog.setAttribute('open', '');

      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      expect(dialog.close).toHaveBeenCalled();
    });

    test('should close dialog on cancel', () => {
      dialog.setAttribute('open', '');

      const cancelBtn = document.getElementById('test-dialog-cancel');
      cancelBtn.click();

      expect(dialog.close).toHaveBeenCalled();
    });
  });

  describe('form validation', () => {
    let dialog;

    beforeEach(() => {
      document.body.innerHTML = `
        <dialog id="test-dialog" data-event-name="testEvent">
          <div id="test-dialog-validation-summary"></div>
          <form id="test-dialog-form">
            <input name="requiredField" type="text" required class="form-control" />
            <span data-valmsg-for="requiredField" class="field-validation-error"></span>
          </form>
          <button id="test-dialog-confirm">Confirm</button>
          <button id="test-dialog-cancel">Cancel</button>
        </dialog>
      `;
      dialog = mockDialogElement(document.getElementById('test-dialog'));
      window.DynamicEditor.init('test-dialog');
    });

    test('should not dispatch update event if validation fails', () => {
      dialog.setAttribute('open', '');

      let eventReceived = false;
      dialog.addEventListener('testEvent-update', () => {
        eventReceived = true;
      });

      // Leave required field empty and try to confirm
      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      expect(eventReceived).toBe(false);
    });

    test('should show validation summary when validation fails', () => {
      dialog.setAttribute('open', '');

      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      const summary = document.getElementById('test-dialog-validation-summary');
      expect(summary.style.display).toBe('block');
      expect(summary.innerHTML).toContain('Please fix the following errors');
    });

    test('should add error class to invalid inputs', () => {
      dialog.setAttribute('open', '');

      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      const input = document.querySelector('input[name="requiredField"]');
      expect(input.classList.contains('input-validation-error')).toBe(true);
    });

    test('should clear validation errors when dialog opens', async () => {
      const summary = document.getElementById('test-dialog-validation-summary');
      const input = document.querySelector('input[name="requiredField"]');

      // First trigger validation error
      dialog.setAttribute('open', '');
      await new Promise(resolve => setTimeout(resolve, 0));
      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      // Close and reopen
      dialog.removeAttribute('open');
      dialog.setAttribute('open', '');

      // Wait for MutationObserver to process
      await new Promise(resolve => setTimeout(resolve, 0));

      expect(summary.style.display).toBe('none');
      expect(input.classList.contains('input-validation-error')).toBe(false);
    });

    test('should dispatch update event when validation passes', () => {
      dialog.setAttribute('open', '');

      let eventReceived = false;
      dialog.addEventListener('testEvent-update', () => {
        eventReceived = true;
      });

      // Fill in required field
      const input = document.querySelector('input[name="requiredField"]');
      input.value = 'valid value';

      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      expect(eventReceived).toBe(true);
    });
  });

  describe('cleanup', () => {
    test('should remove event listeners on destroy', () => {
      document.body.innerHTML = `
        <dialog id="test-dialog" data-event-name="testEvent">
          <form id="test-dialog-form">
            <input name="field1" value="test" />
          </form>
          <button id="test-dialog-confirm">Confirm</button>
          <button id="test-dialog-cancel">Cancel</button>
        </dialog>
      `;
      const dialog = mockDialogElement(document.getElementById('test-dialog'));

      window.DynamicEditor.init('test-dialog');
      dialog.setAttribute('open', '');

      let eventReceived = false;
      dialog.addEventListener('testEvent-update', () => {
        eventReceived = true;
      });

      // Destroy the editor
      window.DynamicEditor.destroy('test-dialog');

      // Clicking confirm should not trigger the event anymore
      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      expect(eventReceived).toBe(false);
    });

    test('should handle destroy for non-initialized dialog', () => {
      // Should not throw
      window.DynamicEditor.destroy('non-existent-dialog');
      expect(true).toBe(true);
    });
  });

  describe('getFormData', () => {
    test('should collect all form data correctly', () => {
      document.body.innerHTML = `
        <dialog id="test-dialog" data-event-name="testEvent">
          <form id="test-dialog-form">
            <input name="text" type="text" value="hello" />
            <input name="number" type="number" value="42" />
            <select name="multi" multiple>
              <option value="x" selected>X</option>
              <option value="y" selected>Y</option>
              <option value="z">Z</option>
            </select>
            <select name="emptyMulti" multiple>
              <option value="a">A</option>
            </select>
          </form>
          <button id="test-dialog-confirm">Confirm</button>
          <button id="test-dialog-cancel">Cancel</button>
        </dialog>
      `;
      const dialog = mockDialogElement(document.getElementById('test-dialog'));
      window.DynamicEditor.init('test-dialog');

      dialog.setAttribute('open', '');

      let receivedData = null;
      dialog.addEventListener('testEvent-update', (e) => {
        receivedData = e.detail;
      });

      const confirmBtn = document.getElementById('test-dialog-confirm');
      confirmBtn.click();

      expect(receivedData.text).toBe('hello');
      expect(receivedData.number).toBe('42');
      expect(receivedData.multi).toEqual(['x', 'y']);
      expect(receivedData.emptyMulti).toEqual([]);
    });
  });
});
